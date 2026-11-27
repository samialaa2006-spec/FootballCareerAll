namespace FootballCareer;

public record LeagueDef(string Id, string Name, string Country, int TeamsCount);

public record ClubDef(
    string Id,
    string Name,
    string ShortName,
    string LeagueId,
    int Strength,        // 68..92 overall team rating
    string PrimaryColor,
    string SecondaryColor
);

public class LeagueRow
{
    public string ClubId { get; set; } = "";
    public string ClubName { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string PrimaryColor { get; set; } = "#ffffff";
    public string SecondaryColor { get; set; } = "#000000";
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GF { get; set; }
    public int GA { get; set; }
    public int GD => GF - GA;
    public int Points => Won * 3 + Drawn;
}

public record MatchResult(
    int Matchday,
    string HomeId,
    string HomeName,
    string AwayId,
    string AwayName,
    int HomeGoals,
    int AwayGoals,
    bool IsUserMatch,
    int UserGoals,
    int UserAssists,
    double UserRating,
    string Summary
);

public static class LeagueCatalog
{
    public static readonly List<LeagueDef> Leagues = new()
    {
        new("epl", "Premier League", "England", 12),
        new("laliga", "LaLiga", "Spain", 12),
        new("seriea", "Serie A", "Italy", 12),
        new("bundesliga", "Bundesliga", "Germany", 12),
        new("ligue1", "Ligue 1", "France", 12)
    };

    public static readonly List<ClubDef> Clubs = new()
    {
        // Premier League (12 top clubs -> 22 matchdays home & away)
        new("mci", "Manchester City", "MCI", "epl", 90, "#6CABDD", "#1C2C5B"),
        new("ars", "Arsenal", "ARS", "epl", 89, "#EF0107", "#FFFFFF"),
        new("liv", "Liverpool", "LIV", "epl", 89, "#C8102E", "#F6EB61"),
        new("che", "Chelsea", "CHE", "epl", 85, "#034694", "#FFFFFF"),
        new("tot", "Tottenham Hotspur", "TOT", "epl", 84, "#FFFFFF", "#132257"),
        new("mun", "Manchester United", "MUN", "epl", 83, "#DA291C", "#FBE122"),
        new("new", "Newcastle United", "NEW", "epl", 84, "#241F20", "#FFFFFF"),
        new("avl", "Aston Villa", "AVL", "epl", 83, "#670E36", "#95BFE5"),
        new("bha", "Brighton & Hove Albion", "BHA", "epl", 80, "#0057B8", "#FFFFFF"),
        new("whu", "West Ham United", "WHU", "epl", 79, "#7A263A", "#1BB1E7"),
        new("eve", "Everton", "EVE", "epl", 77, "#003399", "#FFFFFF"),
        new("ful", "Fulham", "FUL", "epl", 78, "#FFFFFF", "#111111"),

        // LaLiga (12 clubs)
        new("rma", "Real Madrid", "RMA", "laliga", 91, "#FFFFFF", "#FEBE10"),
        new("fcb", "FC Barcelona", "BAR", "laliga", 89, "#A50044", "#004D98"),
        new("atm", "Atletico Madrid", "ATM", "laliga", 86, "#CB3524", "#272E61"),
        new("ath", "Athletic Club", "ATH", "laliga", 83, "#EE2523", "#FFFFFF"),
        new("rso", "Real Sociedad", "RSO", "laliga", 82, "#0067B1", "#FFFFFF"),
        new("vil", "Villarreal CF", "VIL", "laliga", 82, "#FFE667", "#005187"),
        new("bet", "Real Betis", "BET", "laliga", 81, "#0BB363", "#FFFFFF"),
        new("sev", "Sevilla FC", "SEV", "laliga", 79, "#FFFFFF", "#D4021D"),
        new("val", "Valencia CF", "VAL", "laliga", 78, "#FFFFFF", "#FF7F00"),
        new("gir", "Girona FC", "GIR", "laliga", 80, "#CE1126", "#FFFFFF"),
        new("cel", "Celta Vigo", "CEL", "laliga", 77, "#8AC3EE", "#FFFFFF"),
        new("osa", "CA Osasuna", "OSA", "laliga", 77, "#D91A21", "#0A346F"),

        // Serie A (12 clubs)
        new("int", "Inter Milan", "INT", "seriea", 89, "#010E80", "#000000"),
        new("juv", "Juventus", "JUV", "seriea", 86, "#FFFFFF", "#111111"),
        new("acm", "AC Milan", "MIL", "seriea", 85, "#FB090B", "#111111"),
        new("nap", "SSC Napoli", "NAP", "seriea", 86, "#12A0D7", "#FFFFFF"),
        new("ata", "Atalanta", "ATA", "seriea", 85, "#1E71B8", "#000000"),
        new("rom", "AS Roma", "ROM", "seriea", 82, "#8E1F2F", "#F0BC42"),
        new("laz", "SS Lazio", "LAZ", "seriea", 82, "#87D8F7", "#FFFFFF"),
        new("fio", "ACF Fiorentina", "FIO", "seriea", 81, "#482E92", "#FFFFFF"),
        new("bol", "Bologna FC", "BOL", "seriea", 80, "#1A2F48", "#A21C26"),
        new("tor", "Torino FC", "TOR", "seriea", 78, "#8A1E03", "#FFFFFF"),
        new("udi", "Udinese", "UDI", "seriea", 76, "#FFFFFF", "#111111"),
        new("gen", "Genoa CFC", "GEN", "seriea", 76, "#AD1927", "#002B49"),

        // Bundesliga (12 clubs)
        new("bay", "Bayern Munich", "BAY", "bundesliga", 90, "#DC052D", "#FFFFFF"),
        new("b04", "Bayer Leverkusen", "B04", "bundesliga", 88, "#E32221", "#111111"),
        new("bvb", "Borussia Dortmund", "BVB", "bundesliga", 85, "#FDE100", "#111111"),
        new("rbl", "RB Leipzig", "RBL", "bundesliga", 84, "#DD0741", "#FFFFFF"),
        new("vfb", "VfB Stuttgart", "VFB", "bundesliga", 82, "#FFFFFF", "#E32219"),
        new("sge", "Eintracht Frankfurt", "SGE", "bundesliga", 82, "#E1000F", "#111111"),
        new("scf", "SC Freiburg", "SCF", "bundesliga", 79, "#E30613", "#111111"),
        new("wob", "VfL Wolfsburg", "WOB", "bundesliga", 79, "#65B32E", "#FFFFFF"),
        new("bmg", "Borussia Monchengladbach", "BMG", "bundesliga", 78, "#FFFFFF", "#000000"),
        new("tsg", "TSG Hoffenheim", "TSG", "bundesliga", 77, "#1961B5", "#FFFFFF"),
        new("svw", "Werder Bremen", "SVW", "bundesliga", 77, "#1D9053", "#FFFFFF"),
        new("fcu", "Union Berlin", "FCU", "bundesliga", 76, "#EB1923", "#F6E500"),

        // Ligue 1 (12 clubs)
        new("psg", "Paris Saint-Germain", "PSG", "ligue1", 89, "#004170", "#DA291C"),
        new("asm", "AS Monaco", "ASM", "ligue1", 83, "#E51B23", "#FFFFFF"),
        new("om",  "Olympique Marseille", "OM", "ligue1", 82, "#2FAEE0", "#FFFFFF"),
        new("los", "LOSC Lille", "LOSC", "ligue1", 82, "#E01E13", "#233058"),
        new("ol",  "Olympique Lyonnais", "OL", "ligue1", 81, "#FFFFFF", "#14387F"),
        new("ogc", "OGC Nice", "NIC", "ligue1", 80, "#CC0000", "#111111"),
        new("rcl", "RC Lens", "RCL", "ligue1", 79, "#EC1C24", "#FFF200"),
        new("srfc","Stade Rennais", "REN", "ligue1", 79, "#E4002B", "#111111"),
        new("sb29","Stade Brestois", "BRE", "ligue1", 78, "#E30613", "#FFFFFF"),
        new("str", "RC Strasbourg", "STR", "ligue1", 76, "#009FE3", "#FFFFFF"),
        new("fcn", "FC Nantes", "FCN", "ligue1", 76, "#FFE600", "#009A44"),
        new("tfc", "Toulouse FC", "TFC", "ligue1", 76, "#5C2D91", "#FFFFFF")
    };

    public static readonly List<string> Nationalities = new()
    {
        "England", "Spain", "France", "Germany", "Italy", "Brazil", "Argentina", "Portugal",
        "Netherlands", "Belgium", "Croatia", "Hungary", "Poland", "Norway", "Sweden", "Denmark",
        "USA", "Canada", "Mexico", "Colombia", "Uruguay", "Japan", "South Korea", "Morocco",
        "Nigeria", "Senegal", "Egypt", "Turkey", "Serbia", "Switzerland", "Austria", "Scotland"
    };

    public static ClubDef GetClub(string? id)
        => Clubs.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? Clubs[1]; // default Arsenal

    public static List<ClubDef> ClubsInLeague(string leagueId)
        => Clubs.Where(c => c.LeagueId.Equals(leagueId, StringComparison.OrdinalIgnoreCase)).ToList();
}
