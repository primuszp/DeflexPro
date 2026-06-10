namespace DeflexPro.OpenPave;

public sealed class OpenPaveService
{
    private const int ResultWidth = 27;
    internal static readonly object NativeLock = new();

    public IReadOnlyList<OpenPaveResponse> Calculate(
        IReadOnlyList<OpenPaveLayer> layers,
        IReadOnlyList<OpenPaveLoad> loads,
        IReadOnlyList<OpenPavePoint> points,
        int flags = 0x100)
    {
        ValidateMechanicalModel(layers, loads, points);
        var layerData = BuildLayerData(layers);
        var loadData = BuildLoadData(loads);
        var pointData = BuildPointData(points);
        var raw = new double[points.Count * ResultWidth];

        int status;
        lock (NativeLock)
        {
            status = NativeMethods.Calculate(
                checked((uint)flags), (uint)layers.Count,
                layerData.Thicknesses, layerData.Moduli, layerData.PoissonRatios, layerData.Frictions,
                (uint)loads.Count,
                loadData.X, loadData.Y, loadData.Forces, loadData.Pressures, loadData.Radii,
                (uint)points.Count,
                pointData.X, pointData.Y, pointData.Depths, pointData.Layers,
                raw);
        }
        ThrowOnError(status, "OP_LE_Calc");

        return Enumerable.Range(0, points.Count).Select(index =>
        {
            var values = raw.Skip(index * ResultWidth).Take(ResultWidth).ToArray();
            return new OpenPaveResponse(
                values[0], values[1], values[2],
                values[12], values[13], values[14],
                values[15], values[16], values[17],
                values);
        }).ToArray();
    }

    public OpenPaveBackcalculationResult Backcalculate(
        IReadOnlyList<OpenPaveLayer> layers,
        OpenPaveLoad load,
        // Offsets and deflections use the mechanical model's length unit (mm).
        IReadOnlyList<(double Offset, double Deflection)> measuredDeflections,
        double precision = 1e-4,
        double noise = 5e-4,
        double tolerance = 1e-12,
        int maximumIterations = 50,
        IProgress<OpenPaveBackcalculationIteration>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(measuredDeflections);
        if (measuredDeflections.Count < 3)
            throw new ArgumentException("At least three measured deflections are required.", nameof(measuredDeflections));
        if (maximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumIterations));
        if (measuredDeflections.Any(x => !double.IsFinite(x.Offset) || x.Offset < 0 || !IsPositiveFinite(x.Deflection)))
            throw new ArgumentException("Offsets must be non-negative and deflections must be positive finite values.", nameof(measuredDeflections));

        var points = measuredDeflections.Select(x => new OpenPavePoint(x.Offset)).ToArray();
        ValidateMechanicalModel(layers, [load], points);
        var layerData = BuildLayerData(layers);
        var loadData = BuildLoadData([load]);
        var pointData = BuildPointData(points);
        var measured = measuredDeflections.Select(x => x.Deflection).ToArray();

        OpenPaveBackcalculationResult? result = null;
        for (var iteration = 1; iteration <= maximumIterations; iteration++)
        {
            int status;
            lock (NativeLock)
            {
                status = NativeMethods.Backcalculate(
                    precision, noise, tolerance, 1,
                    (uint)layers.Count,
                    layerData.Thicknesses, layerData.Moduli, layerData.PoissonRatios, layerData.Frictions,
                    1,
                    loadData.X, loadData.Y, loadData.Forces, loadData.Pressures, loadData.Radii,
                    (uint)points.Length, 1,
                    pointData.X, pointData.Y, measured);
            }

            // OpenPave returns 1 when the requested iteration limit is reached.
            if (status != 0 && status != 1)
                ThrowOnError(status, "OP_LE_BackCalc");

            result = BuildBackcalculationResult(layers, load, points, measuredDeflections, layerData.Moduli);
            progress?.Report(new OpenPaveBackcalculationIteration(
                iteration,
                result.Moduli.ToArray(),
                result.RmseMillimeters,
                result.RmsePercent,
                status == 0));

            if (status == 0)
                break;
        }

        return result ?? throw new InvalidOperationException("OpenPave backcalculation produced no result.");
    }

    private OpenPaveBackcalculationResult BuildBackcalculationResult(
        IReadOnlyList<OpenPaveLayer> layers,
        OpenPaveLoad load,
        IReadOnlyList<OpenPavePoint> points,
        IReadOnlyList<(double Offset, double Deflection)> measuredDeflections,
        IReadOnlyList<double> moduli)
    {
        var solvedLayers = layers.Select((layer, index) => layer with { Modulus = moduli[index] }).ToArray();
        var calculated = Calculate(solvedLayers, [load], points).Select(x => Math.Abs(x.DeflectionZ)).ToArray();
        var squaredErrors = measuredDeflections.Select((x, index) => Math.Pow(x.Deflection - calculated[index], 2)).ToArray();
        var rmse = Math.Sqrt(squaredErrors.Average());
        var mean = measuredDeflections.Average(x => x.Deflection);

        return new OpenPaveBackcalculationResult(
            moduli.ToArray(),
            calculated,
            rmse,
            mean > 0 ? rmse / mean * 100 : 0);
    }

    public OpenPaveHeatModel CreateHeatModel(
        IReadOnlyList<double> layerThicknesses,
        IReadOnlyList<double> layerDiffusivities,
        IReadOnlyList<double> nodeDepths,
        IReadOnlyList<double> initialTemperatures,
        int bandwidth = 1,
        double timeStepSeconds = 3600)
        => new(layerThicknesses, layerDiffusivities, nodeDepths, initialTemperatures, bandwidth, timeStepSeconds);

    private static void ValidateMechanicalModel(
        IReadOnlyList<OpenPaveLayer> layers,
        IReadOnlyList<OpenPaveLoad> loads,
        IReadOnlyList<OpenPavePoint> points)
    {
        ArgumentNullException.ThrowIfNull(layers);
        ArgumentNullException.ThrowIfNull(loads);
        ArgumentNullException.ThrowIfNull(points);
        if (layers.Count == 0 || loads.Count == 0 || points.Count == 0)
            throw new ArgumentException("At least one layer, load, and calculation point are required.");
        if (points.Any(x => x.Layer < 0 || !double.IsFinite(x.X) || !double.IsFinite(x.Y) || !double.IsFinite(x.Depth)))
            throw new ArgumentException("Point coordinates must be finite and layer numbers must be non-negative.", nameof(points));
        if (layers[^1].Thickness != 0 || layers.Take(layers.Count - 1).Any(x => x.Thickness <= 0))
            throw new ArgumentException("Only the last layer may be a half-space with zero thickness.", nameof(layers));
        if (layers.Any(x => !IsPositiveFinite(x.Modulus) || x.PoissonRatio <= 0 || x.PoissonRatio >= 0.5 ||
                            x.InterfaceFriction < 0 || x.InterfaceFriction > 1))
            throw new ArgumentException("Layer properties are outside the supported ranges.", nameof(layers));
        if (loads.Any(x => !double.IsFinite(x.X) || !double.IsFinite(x.Y) || !IsPositiveFinite(x.Radius) ||
                           (!IsPositiveFinite(x.Force) && !IsPositiveFinite(x.Pressure))))
            throw new ArgumentException("Each load requires a positive radius and force or pressure.", nameof(loads));
    }

    private static (double[] Thicknesses, double[] Moduli, double[] PoissonRatios, double[] Frictions) BuildLayerData(
        IReadOnlyList<OpenPaveLayer> layers) =>
        (layers.Select(x => x.Thickness).ToArray(),
         layers.Select(x => x.Modulus).ToArray(),
         layers.Select(x => x.PoissonRatio).ToArray(),
         layers.Select(x => x.InterfaceFriction).ToArray());

    private static (double[] X, double[] Y, double[] Forces, double[] Pressures, double[] Radii) BuildLoadData(
        IReadOnlyList<OpenPaveLoad> loads) =>
        (loads.Select(x => x.X).ToArray(),
         loads.Select(x => x.Y).ToArray(),
         loads.Select(x => x.Force).ToArray(),
         loads.Select(x => x.Pressure).ToArray(),
         loads.Select(x => x.Radius).ToArray());

    private static (double[] X, double[] Y, double[] Depths, uint[] Layers) BuildPointData(
        IReadOnlyList<OpenPavePoint> points) =>
        (points.Select(x => x.X).ToArray(),
         points.Select(x => x.Y).ToArray(),
         points.Select(x => x.Depth).ToArray(),
         points.Select(x => checked((uint)x.Layer)).ToArray());

    private static bool IsPositiveFinite(double value) => double.IsFinite(value) && value > 0;

    internal static void ThrowOnError(int status, string operation)
    {
        if (status != 0)
            throw new OpenPaveException(operation, status);
    }
}
