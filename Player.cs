namespace FootballCareer;

public enum Position { GK, CB, LB, RB, CDM, CM, CAM, LM, RM, LW, RW, CF, ST }

public class Appearance
{
    public string SkinTone { get; set; } = "#e0a986";
    public string HairStyle { get; set; } = "short";     // short, fade, curly, afro, buzz, long, ponytail
    public string HairColor { get; set; } = "#1c1613";
    public string FacialHair { get; set; } = "none";     // none, stubble, beard, mustache
    public string BootColor { get; set; } = "#4db8ff";
    public int ShirtNumber { get; set; } = 9;
}

public class Attributes
{
    public int Pace, Shooting, Passing, Dribbling, Defending, Physical, Goalkeeping;

    public int[] ToArray() => new[] { Pace, Shooting, Passing, Dribbling, Defending, Physical, Goalkeeping };

    public static readonly string[] ShortNames = { "PAC", "SHO", "PAS", "DRI", "DEF", "PHY", "GK" };
    public static readonly string[] FullNames = { "Pace", "Shooting", "Passing", "Dribbling", "Defending", "Physical", "Goalkeeping" };

    public string Card(bool isGoalkeeper)
    {
        var a = ToArray();
        var parts = new List<string>();
        for (int i = 0; i < a.Length; i++)
        {
            if (i == 6 && !isGoalkeeper) continue;
            parts.Add($"{ShortNames[i]} {a[i],2}");
        }
        return string.Join(" | ", parts);
    }
}

public class Player
{
    public string Name = "";
    public string Nationality = "England";
    public string ClubId = "arsenal";
    public string ClubName = "Arsenal";
    public string LeagueId = "epl";
    public string PreferredFoot = "Right";
    public int HeightCm = 180;
    public Appearance Look = new();

    public int Age;
    public Position Position;
    public Attributes Attr = new();
    public int Potential;           // ceiling for Overall (40-99)
    public double Form = 1.0;       // 0.8 (bad) .. 1.2 (great), last season's performance
    public int ContractYearsLeft = 3;
    public int Seasons;             // career length
    public int Goals, Assists, Apps;

    // Weight of each attribute for each position (Pace, Shoot, Pass, Drib, Def, Phys, GK).
    static readonly Dictionary<Position, double[]> Weights = new()
    {
        [Position.GK] = new[] { 0.00, 0.00, 0.05, 0.00, 0.00, 0.05, 0.90 },
        [Position.CB] = new[] { 0.10, 0.00, 0.10, 0.05, 0.50, 0.25, 0.00 },
        [Position.LB]  = new[] { 0.25, 0.00, 0.20, 0.15, 0.30, 0.10, 0.00 },
        [Position.RB]  = new[] { 0.25, 0.00, 0.20, 0.15, 0.30, 0.10, 0.00 },
        [Position.CDM] = new[] { 0.05, 0.05, 0.30, 0.10, 0.35, 0.15, 0.00 },
        [Position.CM]  = new[] { 0.10, 0.10, 0.35, 0.20, 0.15, 0.10, 0.00 },
        [Position.CAM] = new[] { 0.10, 0.20, 0.30, 0.30, 0.00, 0.10, 0.00 },
        [Position.LM]  = new[] { 0.25, 0.10, 0.25, 0.30, 0.00, 0.10, 0.00 },
        [Position.RM]  = new[] { 0.25, 0.10, 0.25, 0.30, 0.00, 0.10, 0.00 },
        [Position.LW]  = new[] { 0.30, 0.20, 0.15, 0.30, 0.00, 0.05, 0.00 },
        [Position.RW]  = new[] { 0.30, 0.20, 0.15, 0.30, 0.00, 0.05, 0.00 },
        [Position.CF]  = new[] { 0.15, 0.30, 0.20, 0.25, 0.00, 0.10, 0.00 },
        [Position.ST]  = new[] { 0.20, 0.45, 0.05, 0.15, 0.00, 0.15, 0.00 },
    };

    public int Overall
    {
        get
        {
            var a = Attr.ToArray();
            var w = Weights[Position];
            double sum = 0;
            for (int i = 0; i < a.Length; i++) sum += a[i] * w[i];
            return (int)Math.Round(sum);
        }
    }

    public decimal MarketValue => MarketValueCalculator.Calculate(this);

    public static Player Generate(Random rng, string name, Position pos, int age, string nationality = "England", ClubDef? club = null, string foot = "Right", int height = 180, Appearance? look = null)
    {
        var p = new Player
        {
            Name = name,
            Position = pos,
            Age = age,
            Nationality = nationality,
            PreferredFoot = foot == "Left" ? "Left" : "Right",
            HeightCm = Math.Clamp(height, 160, 208),
            Look = look ?? new Appearance()
        };

        if (club != null)
        {
            p.ClubId = club.Id;
            p.ClubName = club.Name;
            p.LeagueId = club.LeagueId;
        }

        // Slightly higher starting ability if joining an elite club academy, balanced for 17yo
        int clubBonus = club != null ? Math.Clamp((club.Strength - 78) / 3, 0, 5) : 0;
        int baseSkill = rng.Next(48, 64) + clubBonus;
        int Roll() => Math.Clamp(baseSkill + rng.Next(-11, 12), 25, 95);
        p.Attr = new Attributes
        {
            Pace = Roll(), Shooting = Roll(), Passing = Roll(), Dribbling = Roll(),
            Defending = Roll(), Physical = Roll(),
            Goalkeeping = pos == Position.GK ? Roll() : 10
        };
        // Boost the attributes that matter for this position
        var w = Weights[pos];
        var a = p.Attr.ToArray();
        for (int i = 0; i < a.Length; i++) if (w[i] >= 0.25) a[i] = Math.Min(95, a[i] + 8);
        p.Attr.Pace = a[0]; p.Attr.Shooting = a[1]; p.Attr.Passing = a[2]; p.Attr.Dribbling = a[3];
        p.Attr.Defending = a[4]; p.Attr.Physical = a[5]; p.Attr.Goalkeeping = a[6];

        int youthBonus = Math.Max(0, 23 - age);
        p.Potential = Math.Clamp(p.Overall + rng.Next(2, 9) + youthBonus * 3 + clubBonus, p.Overall, 99);
        p.ContractYearsLeft = rng.Next(2, 5);
        return p;
    }
}
