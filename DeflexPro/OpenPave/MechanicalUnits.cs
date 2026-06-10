namespace DeflexPro.OpenPave;

public static class MechanicalUnits
{
    public const double MillimetersPerMicrometer = 0.001;
    public const double NewtonsPerCentiKilonewton = 10.0;

    public static double MicrometersToMillimeters(double value) =>
        value * MillimetersPerMicrometer;

    public static double CentiKilonewtonsToNewtons(double value) =>
        value * NewtonsPerCentiKilonewton;
}
