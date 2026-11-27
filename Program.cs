// Program.cs - the C# host. Runs game rules, league simulation, interactive match endpoints,
// serves wwwroot, calls the C++ library, and talks to optional Java (news) and PHP (saves).
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FootballCareer;

const int MaxAge = 38;

var builder = WebApplication.CreateBuilder(args);
var javaUrl = Environment.GetEnvironmentVariable("JAVA_URL") ?? "http://127.0.0.1:8081";
var phpUrl = Environment.GetEnvironmentVariable("PHP_URL") ?? "http://127.0.0.1:8082";
builder.Services.AddHttpClient("java", c => { c.BaseAddress = new Uri(javaUrl); c.Timeout = TimeSpan.FromSeconds(2); });
builder.Services.AddHttpClient("php", c => { c.BaseAddress = new Uri(phpUrl); c.Timeout = TimeSpan.FromSeconds(3); });
if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") is null)
    builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
var careers = new ConcurrentDictionary<string, Career>();

// ---------- catalog & status ----------

app.MapGet("/api/info", async (IHttpClientFactory f) => Results.Json(new
{
    native = Native.Available,
    java = await Ping(f, "java", "/health"),
    php = await Ping(f, "php", "/index.php?action=health")
}));

app.MapGet("/api/catalog", () => Results.Json(new
{
    leagues = LeagueCatalog.Leagues,
    clubs = LeagueCatalog.Clubs,
    nationalities = LeagueCatalog.Nationalities
}));

app.MapGet("/api/selfcheck", () =>
{
    if (!Native.Available)
        return Results.Json(new { native = false, message = "The C/C++ library is not loaded. Build it first (see README)." });

    var rng = new Random(12345);
    int total = 5000, mismatches = 0;
    decimal worst = 0;
    for (int i = 0; i < total; i++)
    {
        var p = Player.Generate(rng, "Check", (Position)rng.Next(13), rng.Next(17, 38));
        p.Form = 0.8 + rng.NextDouble() * 0.4;
        p.ContractYearsLeft = rng.Next(0, 5);
        p.Potential = Math.Clamp(p.Overall + rng.Next(0, 25), p.Overall, 99);
        var diff = Math.Abs(MarketValueCalculator.CalculateManaged(p) - MarketValueCalculator.CalculateNative(p));
        if (diff != 0) { mismatches++; worst = Math.Max(worst, diff); }
    }
    return Results.Json(new { native = true, checkedPlayers = total, mismatches, worstDifference = worst });
});

// ---------- careers ----------

app.MapPost("/api/career", (NewCareerIn req) =>
{
    if (!Enum.TryParse<Position>(req.Position, true, out var pos) || !Enum.IsDefined(pos))
        return Results.BadRequest(new { error = "Unknown position." });

    var club = LeagueCatalog.GetClub(req.ClubId);
    string nat = LeagueCatalog.Nationalities.FirstOrDefault(n => n.Equals(req.Nationality, StringComparison.OrdinalIgnoreCase)) ?? "England";

    var look = new Appearance
    {
        SkinTone = CleanColor(req.Look?.SkinTone, "#e0a986"),
        HairStyle = CleanToken(req.Look?.HairStyle, "short"),
        HairColor = CleanColor(req.Look?.HairColor, "#1c1613"),
        FacialHair = CleanToken(req.Look?.FacialHair, "none"),
        BootColor = CleanColor(req.Look?.BootColor, "#4db8ff"),
        ShirtNumber = Math.Clamp(req.Look?.ShirtNumber ?? 9, 1, 99)
    };

    var rng = new Random();
    var player = Player.Generate(rng, CleanName(req.Name), pos, 17, nat, club, req.PreferredFoot ?? "Right", req.HeightCm ?? 180, look);
    var league = LeagueSimulator.CreateSeason(club.LeagueId, rng);

    var career = new Career(Guid.NewGuid().ToString("N")[..12], player, league, rng);
    career.Values.Add(player.MarketValue);
    career.PeakValue = career.Values[0];
    career.PeakAge = player.Age;
    careers[career.Id] = career;
    return Results.Json(ToDto(career));
});

// Play a single matchday (either auto-simmed or with user's interactive match result)
app.MapPost("/api/career/{id}/matchday", (string id, MatchdayIn? req) =>
{
    if (!careers.TryGetValue(id, out var career))
        return Results.NotFound(new { error = "Career not found." });

    lock (career)
    {
        var p = career.Player;
        if (p.Age >= MaxAge)
            return Results.BadRequest(new { error = "This player has retired." });

        if (career.League.CurrentMatchday > career.League.TotalMatchdays)
            return Results.BadRequest(new { error = "All league matches have been played. Finish the season to advance." });

        (int, int, double, int)? interactive = null;
        if (req is { Interactive: true })
        {
            interactive = (req.UserGoals, req.UserAssists, req.UserRating > 0 ? req.UserRating : 7.0, req.ExtraTeamGoals);
        }

        var match = LeagueSimulator.PlayMatchday(career.League, p, career.Rng, interactive);
        return Results.Json(new { career = ToDto(career), match });
    }
});

// Finish the current league season (simulates remaining matchdays, ages player, updates attributes & starts next league season)
app.MapPost("/api/career/{id}/season", (string id) =>
{
    if (!careers.TryGetValue(id, out var career))
        return Results.NotFound(new { error = "Career not found. The server may have been restarted. Load a save or start again." });

    lock (career)
    {
        var p = career.Player;
        if (p.Age >= MaxAge)
            return Results.BadRequest(new { error = "This player has retired." });

        var row = Progression.CompleteSeason(p, career.League, career.Rng);
        career.History.Add(row);
        career.Values.Add(row.Value);
        career.Apps += row.Apps; career.Goals += row.Goals; career.Assists += row.Assists;
        if (row.Value > career.PeakValue) { career.PeakValue = row.Value; career.PeakAge = row.Age; }

        // Start next season's league fixtures if not retired
        if (p.Age < MaxAge)
        {
            career.League = LeagueSimulator.CreateSeason(p.LeagueId, career.Rng);
        }

        return Results.Json(new { career = ToDto(career), row });
    }
});

// Transfer to another real club
app.MapPost("/api/career/{id}/transfer", (string id, TransferIn req) =>
{
    if (!careers.TryGetValue(id, out var career))
        return Results.NotFound(new { error = "Career not found." });

    lock (career)
    {
        if (career.League.CurrentMatchday > 1)
            return Results.BadRequest(new { error = "Transfers are only open before Matchday 1 of a season." });

        var club = LeagueCatalog.GetClub(req.ClubId);
        career.Player.ClubId = club.Id;
        career.Player.ClubName = club.Name;
        career.Player.LeagueId = club.LeagueId;
        career.Player.ContractYearsLeft = 4;
        career.League = LeagueSimulator.CreateSeason(club.LeagueId, career.Rng);
        return Results.Json(ToDto(career));
    }
});

// ---------- Java: news ----------

app.MapGet("/api/career/{id}/news", async (string id, IHttpClientFactory f) =>
{
    if (!careers.TryGetValue(id, out var career))
        return Results.NotFound(new { error = "Career not found." });

    var p = career.Player;
    var query = $"/news?name={Uri.EscapeDataString(p.Name)}&position={p.Position}&age={p.Age}&ovr={p.Overall}" +
                $"&goals={p.Goals}&assists={p.Assists}&value={(long)p.MarketValue}&season={p.Seasons}" +
                $"&club={Uri.EscapeDataString(p.ClubName)}&nat={Uri.EscapeDataString(p.Nationality)}";
    try
    {
        using var res = await f.CreateClient("java").GetAsync(query);
        if (!res.IsSuccessStatusCode) return Results.Json(new { error = "News service error." }, statusCode: 502);
        return Results.Content(await res.Content.ReadAsStringAsync(), "application/json; charset=utf-8");
    }
    catch (Exception)
    {
        return Results.Json(new { error = "The Java news service is not running." }, statusCode: 503);
    }
});

// ---------- PHP: saves ----------

app.MapPost("/api/career/{id}/save", async (string id, IHttpClientFactory f) =>
{
    if (!careers.TryGetValue(id, out var career))
        return Results.NotFound(new { error = "Career not found." });

    try
    {
        string body;
        lock (career) body = JsonSerializer.Serialize(ToDto(career), json);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var res = await f.CreateClient("php").PostAsync($"/index.php?action=save&slot={career.Id}", content);
        if (!res.IsSuccessStatusCode) return Results.Json(new { error = "The save service refused the save." }, statusCode: 502);
        return Results.Json(new { ok = true, slot = career.Id });
    }
    catch (Exception)
    {
        return Results.Json(new { error = "The PHP save service is not running." }, statusCode: 503);
    }
});

app.MapGet("/api/saves", async (IHttpClientFactory f) =>
{
    try
    {
        using var res = await f.CreateClient("php").GetAsync("/index.php?action=list");
        if (!res.IsSuccessStatusCode) return Results.Json(new { error = "Could not list saves." }, statusCode: 502);
        return Results.Content(await res.Content.ReadAsStringAsync(), "application/json; charset=utf-8");
    }
    catch (Exception)
    {
        return Results.Json(new { error = "The PHP save service is not running." }, statusCode: 503);
    }
});

app.MapPost("/api/load/{slot}", async (string slot, IHttpClientFactory f) =>
{
    if (!System.Text.RegularExpressions.Regex.IsMatch(slot, "^[A-Za-z0-9_-]{1,40}$"))
        return Results.BadRequest(new { error = "Invalid save slot." });

    try
    {
        using var res = await f.CreateClient("php").GetAsync($"/index.php?action=load&slot={slot}");
        if (!res.IsSuccessStatusCode) return Results.Json(new { error = "Save not found." }, statusCode: 404);

        var wrapper = JsonSerializer.Deserialize<LoadWrapper>(await res.Content.ReadAsStringAsync(), json);
        var career = wrapper?.Data is null ? null : Rebuild(wrapper.Data, slot);
        if (career is null) return Results.BadRequest(new { error = "This save file is not a valid career." });

        careers[career.Id] = career;
        return Results.Json(ToDto(career));
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "This save file is damaged." });
    }
    catch (HttpRequestException)
    {
        return Results.Json(new { error = "The PHP save service is not running." }, statusCode: 503);
    }
    catch (TaskCanceledException)
    {
        return Results.Json(new { error = "The PHP save service is not running." }, statusCode: 503);
    }
});

app.Run();

// ---------- helpers ----------

static async Task<bool> Ping(IHttpClientFactory f, string client, string path)
{
    try { using var r = await f.CreateClient(client).GetAsync(path); return r.IsSuccessStatusCode; }
    catch (Exception) { return false; }
}

static string CleanName(string? name)
{
    var cleaned = new string((name ?? "").Where(c => !char.IsControl(c)).ToArray()).Trim();
    if (cleaned.Length > 24) cleaned = cleaned[..24].Trim();
    return cleaned.Length == 0 ? "Alex Novak" : cleaned;
}

static string CleanColor(string? c, string fallback)
{
    if (!string.IsNullOrWhiteSpace(c) && System.Text.RegularExpressions.Regex.IsMatch(c, "^#[0-9A-Fa-f]{6}$"))
        return c;
    return fallback;
}

static string CleanToken(string? t, string fallback)
{
    if (!string.IsNullOrWhiteSpace(t) && System.Text.RegularExpressions.Regex.IsMatch(t, "^[a-z0-9_-]{1,20}$"))
        return t;
    return fallback;
}

static object ToDto(Career c)
{
    var p = c.Player;
    var club = LeagueCatalog.GetClub(p.ClubId);
    var nextFix = LeagueSimulator.GetNextUserFixture(c.League, p.ClubId);
    object? nextOpponent = null;
    if (nextFix.HasValue)
    {
        bool isHome = nextFix.Value.HomeId == p.ClubId;
        var opp = LeagueCatalog.GetClub(isHome ? nextFix.Value.AwayId : nextFix.Value.HomeId);
        nextOpponent = new
        {
            matchday = c.League.CurrentMatchday,
            isHome,
            clubId = opp.Id,
            clubName = opp.Name,
            shortName = opp.ShortName,
            strength = opp.Strength,
            primaryColor = opp.PrimaryColor,
            secondaryColor = opp.SecondaryColor
        };
    }

    return new
    {
        id = c.Id,
        player = new
        {
            name = p.Name,
            nationality = p.Nationality,
            clubId = p.ClubId,
            clubName = p.ClubName,
            leagueId = p.LeagueId,
            preferredFoot = p.PreferredFoot,
            heightCm = p.HeightCm,
            look = p.Look,
            age = p.Age,
            position = p.Position.ToString(),
            attr = p.Attr.ToArray(),
            potential = p.Potential,
            form = p.Form,
            contractYearsLeft = p.ContractYearsLeft,
            seasons = p.Seasons,
            goals = p.Goals,
            assists = p.Assists,
            apps = p.Apps
        },
        club = new
        {
            id = club.Id,
            name = club.Name,
            shortName = club.ShortName,
            strength = club.Strength,
            primaryColor = club.PrimaryColor,
            secondaryColor = club.SecondaryColor
        },
        league = new
        {
            leagueId = c.League.LeagueId,
            leagueName = c.League.LeagueName,
            currentMatchday = c.League.CurrentMatchday,
            totalMatchdays = c.League.TotalMatchdays,
            table = c.League.Table,
            recentResults = c.League.RecentResults,
            seasonGoals = c.League.SeasonGoals,
            seasonAssists = c.League.SeasonAssists,
            seasonApps = c.League.SeasonApps,
            nextFixture = nextOpponent
        },
        overall = p.Overall,
        marketValue = p.MarketValue,
        history = c.History,
        values = c.Values,
        totals = new { apps = c.Apps, goals = c.Goals, assists = c.Assists },
        peak = new { value = c.PeakValue, age = c.PeakAge }
    };
}

static Career? Rebuild(SaveIn s, string id)
{
    var d = s.Player;
    if (d?.Attr is null || d.Attr.Length != 7) return null;
    if (!Enum.TryParse<Position>(d.Position, true, out var pos) || !Enum.IsDefined(pos)) return null;

    var club = LeagueCatalog.GetClub(d.ClubId);
    var a = d.Attr.Select(v => Math.Clamp(v, 10, 99)).ToArray();
    var player = new Player
    {
        Name = CleanName(d.Name),
        Nationality = string.IsNullOrWhiteSpace(d.Nationality) ? "England" : d.Nationality,
        ClubId = club.Id,
        ClubName = club.Name,
        LeagueId = club.LeagueId,
        PreferredFoot = d.PreferredFoot == "Left" ? "Left" : "Right",
        HeightCm = Math.Clamp(d.HeightCm <= 0 ? 180 : d.HeightCm, 160, 208),
        Look = d.Look ?? new Appearance(),
        Position = pos,
        Age = Math.Clamp(d.Age, 17, MaxAge),
        Attr = new Attributes { Pace = a[0], Shooting = a[1], Passing = a[2], Dribbling = a[3], Defending = a[4], Physical = a[5], Goalkeeping = a[6] },
        Potential = Math.Clamp(d.Potential, 1, 99),
        Form = Math.Clamp(d.Form, 0.8, 1.2),
        ContractYearsLeft = Math.Clamp(d.ContractYearsLeft, 0, 6),
        Seasons = Math.Clamp(d.Seasons, 0, 40),
        Goals = Math.Max(0, d.Goals), Assists = Math.Max(0, d.Assists), Apps = Math.Max(0, d.Apps)
    };

    var rng = new Random();
    var league = LeagueSimulator.CreateSeason(club.LeagueId, rng);
    var career = new Career(id, player, league, rng);
    career.History.AddRange((s.History ?? new()).TakeLast(40));
    career.Values.AddRange((s.Values ?? new()).TakeLast(41).Select(v => Math.Max(0, v)));
    if (career.Values.Count == 0) career.Values.Add(player.MarketValue);
    career.Apps = Math.Max(0, s.Totals?.Apps ?? 0);
    career.Goals = Math.Max(0, s.Totals?.Goals ?? 0);
    career.Assists = Math.Max(0, s.Totals?.Assists ?? 0);
    career.PeakValue = Math.Max(career.Values.Max(), s.Peak?.Value ?? 0);
    career.PeakAge = Math.Clamp(s.Peak?.Age ?? player.Age, 17, MaxAge);
    return career;
}

// ---------- types ----------

record NewCareerIn(string? Name, string? Position, string? Nationality, string? ClubId, string? PreferredFoot, int? HeightCm, Appearance? Look);
record MatchdayIn(bool Interactive, int UserGoals, int UserAssists, double UserRating, int ExtraTeamGoals);
record TransferIn(string? ClubId);
public record SeasonRow(int Season, int Age, int Ovr, int Pot, int Apps, int Goals, int Assists, double Form, decimal Value, string? ClubName = null, int LeaguePos = 0);
record PlayerIn(string? Name, string? Nationality, string? ClubId, string? PreferredFoot, int HeightCm, Appearance? Look, int Age, string? Position, int[]? Attr, int Potential, double Form, int ContractYearsLeft, int Seasons, int Goals, int Assists, int Apps);
record TotalsIn(int Apps, int Goals, int Assists);
record PeakIn(decimal Value, int Age);
record SaveIn(PlayerIn? Player, List<SeasonRow>? History, List<decimal>? Values, TotalsIn? Totals, PeakIn? Peak);
record LoadWrapper(SaveIn? Data);

class Career
{
    public string Id { get; }
    public Player Player { get; }
    public LeagueState League { get; set; }
    public Random Rng { get; }
    public List<SeasonRow> History { get; } = new();
    public List<decimal> Values { get; } = new();
    public int Apps, Goals, Assists, PeakAge;
    public decimal PeakValue;

    public Career(string id, Player player, LeagueState league, Random rng)
    {
        Id = id;
        Player = player;
        League = league;
        Rng = rng;
    }
}