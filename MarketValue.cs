namespace FootballCareer;

public static class MarketValueCalculator
{
    /// <summary>Set to false to force the pure C# version (used by the self-check).</summary>
    public static bool UseNative = true;

    /// <summary>Market value in euros. Uses the C++ library when it is available, otherwise C#.</summary>
    public static decimal Calculate(Player p)
        => UseNative && Native.Available ? CalculateNative(p) : CalculateManaged(p);

    public static decimal CalculateNative(Player p)
        => (decimal)Native.MarketValue(p.Overall, p.Potential, p.Age, p.Position.ToString(), p.ContractYearsLeft, p.Form);

    /// <summary>The same formula in C#. Keep it identical to native/market_value.cpp and native/curves.c.</summary>
    public static decimal CalculateManaged(Player p)
    {
        int ovr = p.Overall;

        // 1) Base value grows exponentially with rating: 60 ovr ~ 0.8M, 75 ~ 10M, 85 ~ 56M, 90 ~ 130M
        double baseValue = 26_700 * Math.Exp(0.17 * (ovr - 40));

        // 2) Young players are valued on potential, not just current ability
        double effective = ovr + Math.Max(0, p.Potential - ovr) * PotentialWeight(p.Age);
        double ratingValue = 26_700 * Math.Exp(0.17 * (effective - 40));
        double value = Math.Max(baseValue, ratingValue);

        // 3) Age multiplier
        value *= AgeFactor(p.Age);

        // 4) Position multiplier (attackers cost more, keepers/defenders less)
        value *= p.Position switch
        {
            Position.ST => 1.20, Position.CF => 1.15,
            Position.LW or Position.RW => 1.10, Position.CAM => 1.10,
            Position.LM or Position.RM => 1.00, Position.CM => 1.00,
            Position.CDM => 0.95, Position.CB => 0.95,
            Position.LB or Position.RB => 0.90, Position.GK => 0.75, _ => 1.0
        };

        // 5) Contract: value drops sharply when the contract is nearly over
        value *= p.ContractYearsLeft switch { <= 0 => 0.35, 1 => 0.70, 2 => 0.90, _ => 1.0 };

        // 6) Recent form
        value *= p.Form;

        return Math.Round((decimal)value / 10_000m) * 10_000m;
    }

    static double PotentialWeight(int age) => age switch
    {
        <= 18 => 0.75, <= 20 => 0.60, <= 22 => 0.40, <= 24 => 0.20, _ => 0.0
    };

    static double AgeFactor(int age) => age switch
    {
        <= 17 => 0.85, <= 21 => 1.10, <= 25 => 1.15, <= 28 => 1.05,
        29 => 0.90, 30 => 0.75, 31 => 0.60, 32 => 0.45, 33 => 0.32, 34 => 0.22, _ => 0.12
    };
}
