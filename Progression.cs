namespace FootballCareer;

public static class Progression
{
    /// <summary>Finishes the current season: updates form, attributes, age, and generates a fresh league table.</summary>
    public static SeasonRow CompleteSeason(Player p, LeagueState league, Random rng)
    {
        // If user hit "Sim Full Season" mid-way through, finish remaining matchdays first
        while (league.CurrentMatchday <= league.TotalMatchdays)
        {
            LeagueSimulator.PlayMatchday(league, p, rng, null);
        }

        int rank = league.Table.FindIndex(r => r.ClubId == p.ClubId) + 1;
        if (rank <= 0) rank = 6;

        double avgRating = league.SeasonApps > 0 ? league.RatingSum / league.SeasonApps : 6.6;
        double rankBonus = rank == 1 ? 0.08 : rank <= 4 ? 0.04 : 0.0;
        p.Form = Math.Clamp(0.85 + (avgRating - 6.0) * 0.12 + rankBonus, 0.8, 1.2);

        // Attribute growth / decline
        var a = p.Attr.ToArray();
        for (int i = 0; i < a.Length; i++)
        {
            if (i == 6 && p.Position != Position.GK) continue;
            a[i] = Math.Clamp(a[i] + AttributeDelta(p, i, avgRating, rng), 10, 99);
        }
        p.Attr.Pace = a[0]; p.Attr.Shooting = a[1]; p.Attr.Passing = a[2]; p.Attr.Dribbling = a[3];
        p.Attr.Defending = a[4]; p.Attr.Physical = a[5]; p.Attr.Goalkeeping = a[6];

        if (p.Overall > p.Potential) p.Potential = p.Overall;

        int sApps = league.SeasonApps;
        int sGoals = league.SeasonGoals;
        int sAssists = league.SeasonAssists;

        p.Age++;
        p.Seasons++;
        p.ContractYearsLeft--;
        if (p.ContractYearsLeft <= 0) p.ContractYearsLeft = rng.Next(2, 5);

        return new SeasonRow(
            p.Seasons,
            p.Age,
            p.Overall,
            p.Potential,
            sApps,
            sGoals,
            sAssists,
            p.Form,
            p.MarketValue,
            p.ClubName,
            rank
        );
    }

    static int AttributeDelta(Player p, int attrIndex, double avgRating, Random rng)
    {
        double gap = Math.Max(0, p.Potential - p.Overall);
        double growth = p.Age switch { <= 20 => 3.5, <= 23 => 2.5, <= 26 => 1.2, <= 29 => 0.2, _ => 0.0 };
        growth *= Math.Min(1.0, 0.3 + gap / 15.0);
        if (avgRating >= 7.4) growth += 0.6; // strong match performances accelerate development

        double decline = p.Age switch { <= 29 => 0.0, 30 => 0.8, 31 => 1.3, 32 => 1.8, 33 => 2.3, _ => 3.0 };
        if (attrIndex == 0 || attrIndex == 5) decline *= 1.5;
        if (attrIndex == 1 || attrIndex == 2) decline *= 0.6;

        double noise = (rng.NextDouble() - 0.5) * 2.0;
        return (int)Math.Round(growth - decline + noise);
    }
}
