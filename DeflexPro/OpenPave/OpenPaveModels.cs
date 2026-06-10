namespace DeflexPro.OpenPave;

/// <param name="Thickness">Layer thickness in mm; zero only for the final half-space.</param>
/// <param name="Modulus">Elastic modulus in MPa.</param>
public sealed record OpenPaveLayer(
    double Thickness,
    double Modulus,
    double PoissonRatio = 0.35,
    double InterfaceFriction = 1.0);

/// <param name="X">Load center X coordinate in mm.</param>
/// <param name="Y">Load center Y coordinate in mm.</param>
/// <param name="Force">Total load in N.</param>
/// <param name="Pressure">Contact pressure in MPa.</param>
/// <param name="Radius">Load radius in mm.</param>
public sealed record OpenPaveLoad(
    double X,
    double Y,
    double Force,
    double Pressure,
    double Radius);

/// <param name="X">X coordinate in mm.</param>
/// <param name="Y">Y coordinate in mm.</param>
/// <param name="Depth">Depth in mm.</param>
public sealed record OpenPavePoint(
    double X,
    double Y = 0,
    double Depth = 0,
    int Layer = 0);

public sealed record OpenPaveResponse(
    double StressX,
    double StressY,
    double StressZ,
    double DeflectionX,
    double DeflectionY,
    double DeflectionZ,
    double StrainX,
    double StrainY,
    double StrainZ,
    IReadOnlyList<double> Raw);

public sealed record OpenPaveBackcalculationResult(
    IReadOnlyList<double> Moduli,
    IReadOnlyList<double> CalculatedDeflections,
    double RmseMillimeters,
    double RmsePercent);

public sealed record OpenPaveBackcalculationIteration(
    int Iteration,
    IReadOnlyList<double> Moduli,
    double RmseMillimeters,
    double RmsePercent,
    bool Converged);
