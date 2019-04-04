using System;
using System.Linq;

namespace NationalInstruments.SystemsEngineering.SignalProcessing
{
    class SignalAnalysis
    {
        public static double[] CalculatePower(ComplexDouble[] iqData)
        {
            double[] power = new double[iqData.Length];
            for (int i = 0; i < iqData.Length; i++)
            {
                power[i] = 20 * Math.Log10(iqData[i].Magnitude) + 10;
            }
            return power;
        }

        public static double[] DeltaPhase(ComplexDouble[] iqData)
        {
            ComplexDouble[] dataNeighbors = iqData.Skip(1).ToArray();
            double[] deltaPhase = new double[dataNeighbors.Length];
            for (int i = 0; i < dataNeighbors.Length; i++)
            {
                deltaPhase[i] = iqData[i].ComplexConjugate.Multiply(dataNeighbors[i]).Phase;
            }
            return deltaPhase;
        }

        public static double PAPR(ComplexDouble[] iqData)
        {
            double[] voltsSquared = new double[iqData.Length];
            for (int i = 0; i < voltsSquared.Length; i++)
            {
                voltsSquared[i] = iqData[i].Magnitude * iqData[i].Magnitude;
            }
            return 10 * Math.Log10(voltsSquared.Max() / voltsSquared.Average());
        }
    }
}
