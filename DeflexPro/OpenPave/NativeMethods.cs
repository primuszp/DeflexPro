using System.Runtime.InteropServices;

namespace DeflexPro.OpenPave;

internal static class NativeMethods
{
    private const string LibraryName = "libop64.dll";

    [DllImport(LibraryName, EntryPoint = "OP_LE_Calc", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Calculate(
        uint flags,
        uint layerCount,
        double[] thicknesses,
        double[] moduli,
        double[] poissonRatios,
        double[] frictions,
        uint loadCount,
        double[] loadX,
        double[] loadY,
        double[] loadForces,
        double[] loadPressures,
        double[] loadRadii,
        uint pointCount,
        double[] pointX,
        double[] pointY,
        double[] pointDepths,
        uint[] pointLayers,
        [Out] double[] results);

    [DllImport(LibraryName, EntryPoint = "OP_LE_BackCalc", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Backcalculate(
        double precision,
        double noise,
        double tolerance,
        uint maximumIterations,
        uint layerCount,
        double[] thicknesses,
        [In, Out] double[] moduli,
        double[] poissonRatios,
        double[] frictions,
        uint loadCount,
        double[] loadX,
        double[] loadY,
        double[] loadForces,
        double[] loadPressures,
        double[] loadRadii,
        uint pointCount,
        uint dropCount,
        double[] pointX,
        double[] pointY,
        double[] deflections);

    [DllImport(LibraryName, EntryPoint = "OP_HT_Init", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint HeatInitialize(
        uint layerCount,
        double[] thicknesses,
        double[] diffusivities,
        uint nodeCount,
        double[] nodeDepths,
        double[] nodeTemperatures,
        uint bandwidth,
        double timeStep);

    [DllImport(LibraryName, EntryPoint = "OP_HT_Step", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int HeatStep(
        nint token,
        uint topTemperatureCount,
        double[] topTemperatures,
        double bottomTemperature);

    [DllImport(LibraryName, EntryPoint = "OP_HT_Interpolate", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int HeatInterpolate(
        nint token,
        uint pointCount,
        double[] depths,
        [Out] double[] temperatures);

    [DllImport(LibraryName, EntryPoint = "OP_HT_Reset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int HeatReset(nint token);
}
