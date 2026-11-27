using System.Runtime.InteropServices;

namespace FootballCareer;

/// <summary>
/// Bridge to the C/C++ library (career.dll on Windows, libcareer.so on Linux).
/// If the library has not been built, Available is false and the game uses the C# code instead.
/// </summary>
public static class Native
{
    [DllImport("career", EntryPoint = "cr_version", CallingConvention = CallingConvention.Cdecl)]
    private static extern int cr_version();

    [DllImport("career", EntryPoint = "cr_market_value", CallingConvention = CallingConvention.Cdecl)]
    private static extern double cr_market_value(
        int overall, int potential, int age,
        [MarshalAs(UnmanagedType.LPStr)] string position,
        int contractYearsLeft, double form);

    public static bool Available { get; }

    static Native()
    {
        try { Available = cr_version() > 0; }
        catch (Exception) { Available = false; } // library missing, wrong CPU type, or entry point not found
    }

    public static double MarketValue(int overall, int potential, int age, string position, int contractYearsLeft, double form)
        => cr_market_value(overall, potential, age, position, contractYearsLeft, form);
}
