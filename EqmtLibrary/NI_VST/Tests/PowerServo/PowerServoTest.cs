using System;
using System.Collections.Generic;
using NationalInstruments;
using NationalInstruments.SystemsEngineering.Hardware;
using NationalInstruments.ModularInstruments.NIRfsg;


namespace Broadcom.Tests
{
    public class PowerServoTest : ITest
    {

        /// <summary>
        /// Name of the waveform written to the generator.
        /// </summary>
        public string waveformName;
        /// <summary>
        /// Waveform to generate.  Defined by the waveformName field.
        /// </summary>
        public ComplexWaveform<ComplexDouble> waveform;
        /// <summary>
        /// Get IQ rate of the waveform
        /// </summary>
        public double IqRate
        {
            get
            {
                return 1 / waveform.PrecisionTiming.SampleInterval.FractionalSeconds;
            }
        }
        /// <summary>
        /// Center frequency
        /// </summary>
        public double centerFrequency;
        /// <summary>
        /// Target average power level for power servoing
        /// </summary>
        public double powerLevel;

        private bool isConfigFirstTime = true;


        private Transceiver transceiver;

        public PowerServoTest(
            Transceiver transceiver,
            string waveformName,
            ComplexWaveform<ComplexDouble> waveform,
            double centerFrequency = 1E9,
            double powerLevel = -10
            )
        {
            this.transceiver = transceiver;
            this.waveformName = waveformName;
            this.waveform = waveform;
            this.centerFrequency = centerFrequency;
            this.powerLevel = powerLevel;
        }

        public static PowerServoTest[] Multisite(
            Transceiver[] sites,
            string waveformName,
            ComplexWaveform<ComplexDouble> waveform,
            double centerFrequency = 1E9,
            double powerLevel = -10)
        {
            PowerServoTest[] testArr = new PowerServoTest[sites.Length];
            for (int i = 0; i < sites.Length; i++)
            {
                testArr[i] = new PowerServoTest(
                    sites[i],
                    waveformName,
                    waveform,
                    centerFrequency,
                    powerLevel);
            }
            return testArr;
        }

        public void Initialize()
        {
            transceiver.Reset(); // prevents the use of residual settings
            transceiver.RfsgHandle.FrequencyReference.Configure("PXI_CLK", 10E6);
            transceiver.RfsgHandle.Arb.GenerationMode = RfsgWaveformGenerationMode.ArbitraryWaveform;
            transceiver.RfsgHandle.RF.PowerLevelType = RfsgRFPowerLevelType.AveragePower;
            transceiver.RfsgHandle.Arb.PreFilterGain = -2; // the lowest value this property can have is -2
        }

        public void ApplyPowerLevel()
        {
            transceiver.RfsgHandle.RF.PowerLevel = powerLevel;

            if (isConfigFirstTime)
            {
                transceiver.ConditionalWriteWaveform(waveformName, waveform);
                transceiver.RfsgHandle.RF.Frequency = centerFrequency;
                transceiver.RfsgHandle.Arb.IQRate = IqRate;
                transceiver.RfsgHandle.Initiate();

                isConfigFirstTime = false;
            }
        }

        public void Abort()
        {
            transceiver.RfsgHandle.Abort();
        }

        public void Configure()
        {
            throw new NotImplementedException();
        }

        public void Initiate()
        {
            throw new NotImplementedException();
        }

        public void WaitUntilAcquisitionComplete()
        {
            throw new NotImplementedException();
        }

        public void WaitUntilMeasurementComplete()
        {
            throw new NotImplementedException();
        }
    }
}
