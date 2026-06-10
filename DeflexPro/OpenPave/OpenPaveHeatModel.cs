using Microsoft.Win32.SafeHandles;

namespace DeflexPro.OpenPave;

public sealed class OpenPaveHeatModel : IDisposable
{
    private readonly HeatModelHandle handle;
    private bool disposed;

    internal OpenPaveHeatModel(
        IReadOnlyList<double> layerThicknesses,
        IReadOnlyList<double> layerDiffusivities,
        IReadOnlyList<double> nodeDepths,
        IReadOnlyList<double> initialTemperatures,
        int bandwidth,
        double timeStepSeconds)
    {
        Validate(layerThicknesses, layerDiffusivities, nodeDepths, initialTemperatures, bandwidth, timeStepSeconds);
        nint token;
        lock (OpenPaveService.NativeLock)
        {
            token = NativeMethods.HeatInitialize(
                (uint)layerThicknesses.Count, layerThicknesses.ToArray(), layerDiffusivities.ToArray(),
                (uint)nodeDepths.Count, nodeDepths.ToArray(), initialTemperatures.ToArray(),
                checked((uint)bandwidth), timeStepSeconds);
        }
        if (token == 0)
            throw new OpenPaveException("OP_HT_Init", -1);
        handle = new HeatModelHandle(token);
    }

    public void Step(IReadOnlyList<double> topTemperatures, double bottomTemperature)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(topTemperatures);
        if (topTemperatures.Count == 0 || topTemperatures.Any(x => !double.IsFinite(x)) || !double.IsFinite(bottomTemperature))
            throw new ArgumentException("Boundary temperatures must be finite.");
        lock (OpenPaveService.NativeLock)
        {
            OpenPaveService.ThrowOnError(
                NativeMethods.HeatStep(handle.DangerousGetHandle(), (uint)topTemperatures.Count, topTemperatures.ToArray(), bottomTemperature),
                "OP_HT_Step");
        }
    }

    public IReadOnlyList<double> Interpolate(IReadOnlyList<double> depths)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(depths);
        if (depths.Count == 0 || depths.Any(x => x < 0 || !double.IsFinite(x)) || !IsNonDecreasing(depths))
            throw new ArgumentException("Depths must be finite, non-negative, and sorted.", nameof(depths));
        var temperatures = new double[depths.Count];
        lock (OpenPaveService.NativeLock)
        {
            OpenPaveService.ThrowOnError(
                NativeMethods.HeatInterpolate(handle.DangerousGetHandle(), (uint)depths.Count, depths.ToArray(), temperatures),
                "OP_HT_Interpolate");
        }
        return temperatures;
    }

    public void Dispose()
    {
        if (disposed) return;
        handle.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    private static void Validate(
        IReadOnlyList<double> thicknesses,
        IReadOnlyList<double> diffusivities,
        IReadOnlyList<double> depths,
        IReadOnlyList<double> temperatures,
        int bandwidth,
        double timeStep)
    {
        if (thicknesses.Count == 0 || thicknesses.Count != diffusivities.Count)
            throw new ArgumentException("Thermal layer thickness and diffusivity counts must match.");
        if (depths.Count != temperatures.Count || depths.Count < bandwidth + 1)
            throw new ArgumentException("Node depth and temperature counts must match the selected bandwidth.");
        if (bandwidth is not (1 or 2 or 4) || (depths.Count - 1) % bandwidth != 0)
            throw new ArgumentException("Bandwidth must be 1, 2, or 4 and divide the element count.", nameof(bandwidth));
        if (thicknesses.Any(x => x <= 0) || diffusivities.Any(x => x <= 0) ||
            !IsNonDecreasing(depths) || !double.IsFinite(timeStep) || timeStep <= 0)
            throw new ArgumentException("Invalid thermal model values.");

        var layerBottoms = new double[thicknesses.Count];
        var bottom = depths[0];
        for (var i = 0; i < thicknesses.Count; i++)
        {
            bottom += thicknesses[i];
            layerBottoms[i] = bottom;
        }

        var layerIndex = 0;
        for (var start = 0; start < depths.Count - bandwidth; start += bandwidth)
        {
            var elementTop = depths[start];
            var elementBottom = depths[start + bandwidth];
            while (layerIndex < layerBottoms.Length && layerBottoms[layerIndex] <= elementTop)
                layerIndex++;
            if (layerIndex >= layerBottoms.Length ||
                layerBottoms[layerIndex] - thicknesses[layerIndex] > elementTop ||
                layerBottoms[layerIndex] < elementBottom)
                throw new ArgumentException("Thermal elements cannot cross layer boundaries.");
        }
    }

    private static bool IsNonDecreasing(IReadOnlyList<double> values) =>
        Enumerable.Range(0, values.Count - 1).All(i => values[i] <= values[i + 1]);

    private sealed class HeatModelHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal HeatModelHandle(nint token) : base(true) => SetHandle(token);
        protected override bool ReleaseHandle()
        {
            lock (OpenPaveService.NativeLock)
                return NativeMethods.HeatReset(handle) == 0;
        }
    }
}
