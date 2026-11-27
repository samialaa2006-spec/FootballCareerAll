namespace FootballCareer;

public class LeagueState
{
    public string LeagueId { get; set; } = "epl";
    public string LeagueName { get; set; } = "Premier League";
    public int CurrentMatchday { get; set; } = 1;
    public int TotalMatchdays { get; set; } = 22;
    public List<LeagueRow> Table { get; set; } = new();
    public List<List<(string HomeId, string AwayId)>> Fixtures { get; set; } = new();
    public List<MatchResult> RecentResults { get; set; } = new();
    public int SeasonGoals { get; set; }
    public int SeasonAssists { get; set; }
    public int SeasonApps { get; set; }
    public double RatingSum { get; set; }
}

public static class LeagueSimulator
{
    public static LeagueState CreateSeason(string leagueId, Random rng)
    {
        var league = LeagueCatalog.Leagues.FirstOrDefault(l => l.Id == leagueId) ?? LeagueCatalog.Leagues[0];
        var clubs = LeagueCatalog.ClubsInLeague(league.Id);

        var state = new LeagueState
        {
            LeagueId = league.Id,
            LeagueName = league.Name,
            CurrentMatchday = 1,
            Table = clubs.Select(c => new LeagueRow
            {
                ClubId = c.Id,
                ClubName = c.Name,
                ShortName = c.ShortName,
                PrimaryColor = c.PrimaryColor,
                SecondaryColor = c.SecondaryColor
            }).ToList()
        };

        // Round-robin double schedule (home & away)
        var ids = clubs.Select(c => c.Id).OrderBy(_ => rng.Next()).ToList();
        int n = ids.Count;
        var firstHalf = new List<List<(string, string)>>();
        var rotating = new List<string>(ids);

        for (int round = 0; round < n - 1; round++)
        {
            var day = new List<(string, string)>();
            for (int i = 0; i < n / 2; i++)
            {
                string a = rotating[i];
                string b = rotating[n - 1 - i];
                day.Add(round % 2 == 0 ? (a, b) : (b, a));
            }
            firstHalf.Add(day);
            var last = rotating[^1];
            rotating.RemoveAt(n - 1);
            rotating.Insert(1, last);
        }

        foreach (var day in firstHalf) state.Fixtures.Add(day);
        foreach (var day in firstHalf)
            state.Fixtures.Add(day.Select(m => (m.Item2, m.Item1)).ToList());

        state.TotalMatchdays = state.Fixtures.Count;
        return state;
    }

    public static (string HomeId, string AwayId)? GetNextUserFixture(LeagueState state, string userClubId)
    {
        if (state.CurrentMatchday > state.TotalMatchdays) return null;
        var day = state.Fixtures[state.CurrentMatchday - 1];
        foreach (var m in day)
        {
            if (m.HomeId == userClubId || m.AwayId == userClubId) return m;
        }
        return null;
    }

    /// <summary>
    /// Simulates one matchday across the league.
    /// If userContribution is provided (from interactive play), those goals/assists/rating are used for the user's match!
    /// </summary>
    public static MatchResult? PlayMatchday(
        LeagueState state,
        Player player,
        Random rng,
        (int UserGoals, int UserAssists, double UserRating, int ExtraTeamGoals)? interactiveInput = null)
    {
        if (state.CurrentMatchday > state.TotalMatchdays) return null;

        int md = state.CurrentMatchday;
        var matches = state.Fixtures[md - 1];
        MatchResult? userResult = null;

        foreach (var (homeId, awayId) in matches)
        {
            var homeClub = LeagueCatalog.GetClub(homeId);
            var awayClub = LeagueCatalog.GetClub(awayId);
            bool isUserMatch = homeId == player.ClubId || awayId == player.ClubId;
            bool userIsHome = homeId == player.ClubId;

            double homeStr = homeClub.Strength + 2.5; // home advantage
            double awayStr = awayClub.Strength;

            if (isUserMatch)
            {
                double playerBoost = (player.Overall - 72) * 0.14;
                if (userIsHome) homeStr += playerBoost;
                else awayStr += playerBoost;
            }

            int homeGoals = SampleGoals(homeStr, awayStr, rng);
            int awayGoals = SampleGoals(awayStr, homeStr, rng);

            int uGoals = 0, uAssists = 0;
            double uRating = 6.5;
            string summary = $"{homeClub.ShortName} {homeGoals} - {awayGoals} {awayClub.ShortName}";

            if (isUserMatch)
            {
                if (interactiveInput.HasValue)
                {
                    var inp = interactiveInput.Value;
                    uGoals = Math.Clamp(inp.UserGoals, 0, 6);
                    uAssists = Math.Clamp(inp.UserAssists, 0, 6);
                    uRating = Math.Clamp(inp.UserRating, 4.0, 10.0);

                    if (userIsHome)
                        homeGoals = Math.Max(homeGoals, uGoals + uAssists + inp.ExtraTeamGoals);
                    else
                        awayGoals = Math.Max(awayGoals, uGoals + uAssists + inp.ExtraTeamGoals);

                    summary = $"Interactive match: {homeClub.Name} {homeGoals} - {awayGoals} {awayClub.Name} " +
                              (uGoals > 0 || uAssists > 0 ? $"({uGoals}G, {uAssists}A, {uRating:0.0} rtg)" : $"({uRating:0.0} rtg)");
                }
                else
                {
                    // Auto-sim user performance
                    int teamGoals = userIsHome ? homeGoals : awayGoals;
                    int teamConceded = userIsHome ? awayGoals : homeGoals;
                    double quality = Math.Pow(player.Overall / 72.0, 2.2);

                    double goalChance = player.Position switch
                    {
                        Position.ST => 0.45, Position.CF => 0.38,
                        Position.LW or Position.RW => 0.28, Position.CAM => 0.25,
                        Position.LM or Position.RM => 0.16, Position.CM => 0.11,
                        Position.CDM or Position.CB or Position.LB or Position.RB => 0.05,
                        _ => 0.0
                    } * quality;

                    for (int g = 0; g < teamGoals; g++)
                    {
                        double roll = rng.NextDouble();
                        if (roll < goalChance) uGoals++;
                        else if (roll < goalChance + 0.24 * quality) uAssists++;
                    }

                    double baseRtg = 6.4 + (teamGoals - teamConceded) * 0.25 + uGoals * 0.85 + uAssists * 0.55 + (rng.NextDouble() - 0.5) * 0.8;
                    if (player.Position == Position.GK && teamConceded == 0) baseRtg += 0.9;
                    uRating = Math.Clamp(Math.Round(baseRtg, 1), 4.5, 10.0);

                    summary = $"{homeClub.Name} {homeGoals} - {awayGoals} {awayClub.Name}" +
                              (uGoals > 0 || uAssists > 0 ? $" • {player.Name}: {uGoals}G {uAssists}A ({uRating:0.0})" : $" • Rating {uRating:0.0}");
                }

                state.SeasonApps++;
                state.SeasonGoals += uGoals;
                state.SeasonAssists += uAssists;
                state.RatingSum += uRating;

                player.Apps++;
                player.Goals += uGoals;
                player.Assists += uAssists;

                userResult = new MatchResult(
                    md, homeId, homeClub.Name, awayId, awayClub.Name,
                    homeGoals, awayGoals, true, uGoals, uAssists, uRating, summary
                );
            }

            UpdateTable(state, homeId, awayId, homeGoals, awayGoals);
        }

        SortTable(state);
        if (userResult != null)
        {
            state.RecentResults.Insert(0, userResult);
            if (state.RecentResults.Count > 10) state.RecentResults.RemoveAt(state.RecentResults.Count - 1);
        }

        state.CurrentMatchday++;
        return userResult;
    }

    static int SampleGoals(double attackStr, double defenseStr, Random rng)
    {
        double diff = (attackStr - defenseStr) * 0.065;
        double lambda = Math.Clamp(1.35 + diff, 0.25, 3.8);
        // Poisson sampling
        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;
        do { k++; p *= rng.NextDouble(); } while (p > L && k < 8);
        return k - 1;
    }

    static void UpdateTable(LeagueState state, string homeId, string awayId, int hg, int ag)
    {
        var h = state.Table.First(r => r.ClubId == homeId);
        var a = state.Table.First(r => r.ClubId == awayId);
        h.Played++; a.Played++;
        h.GF += hg; h.GA += ag;
        a.GF += ag; a.GA += hg;
        if (hg > ag) { h.Won++; a.Lost++; }
        else if (hg < ag) { a.Won++; h.Lost++; }
        else { h.Drawn++; a.Drawn++; }
    }

    public static void SortTable(LeagueState state)
    {
        state.Table = state.Table
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.GD)
            .ThenByDescending(r => r.GF)
            .ThenBy(r => r.ClubName)
            .ToList();
    }
}
