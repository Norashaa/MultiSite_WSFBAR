using System.Linq;

namespace NationalInstruments.SystemsEngineering.SignalProcessing
{
    class SignalGeneration
    {
        public static double[] Ramp(double start, double stop, int steps)
        {
            double delta = (stop - start) / (steps - 1);
            double[] ramp = new double[steps];
            for (int i = 0; i < steps; i++)
            {
                ramp[i] = start + delta * i;
            }
            return ramp;
        }

        public static double[] Ramp(double start, double stop, double delta)
        {
            int steps = (int)((stop - start) / delta + 1);
            double[] ramp = new double[steps];
            for(int i = 0; i < steps; i++)
            {
                ramp[i] = start + delta * i;
            }
            return ramp;
        }

        public static ComplexDouble[] CarrierWave(int length)
        {
            ComplexDouble[] cw = new ComplexDouble[length];
            for (int i = 0; i < length; i++)
                cw[i] = ComplexDouble.FromPolar(1, 0);
            return cw;
        }
    }
}
