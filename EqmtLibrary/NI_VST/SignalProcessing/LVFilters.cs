using System.Runtime.InteropServices;

namespace NationalInstruments.SystemsEngineering.SignalProcessing
{
    static class LVFilters
    {
        [DllImport("lvfilters.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Initialize();

        public static double[] ComplexFilter(double iqRate, double vbw, int leadingFirstPointSamples, double bandwidth, double minSelectivity,
            ComplexDouble[] iqData)
        {
            double[] real, imaginary;
            ComplexDouble.DecomposeArray(iqData, out real, out imaginary);
            double[] filteredData = new double[iqData.Length];
            ComplexFilter(iqRate, vbw, leadingFirstPointSamples, bandwidth, minSelectivity, real, imaginary, filteredData, iqData.Length);
            return filteredData;
        }

        [DllImport("lvfilters.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ComplexFilter(double iqRate, double vbw, int leadingFirstPointSamples, double bandwidth, double minSelectivity,
            double[] real, double[] imaginary, double[] filteredData, int len);
    }
}
