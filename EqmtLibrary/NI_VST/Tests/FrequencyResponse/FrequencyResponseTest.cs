using NationalInstruments;
using NationalInstruments.ModularInstruments.NIRfsa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.SystemsEngineering.Hardware;
using NationalInstruments.SystemsEngineering.SignalProcessing;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Broadcom.Tests
{
    public class FrequencyResponseTest : ITest
    {
        private Transceiver transceiver;
        private ManualResetEvent acquisitionCompleteEvent = new ManualResetEvent(true);
        private ManualResetEvent measurementCompleteEvent = new ManualResetEvent(false);
        private BlockingCollection<ComplexDouble[]> queue =
            new BlockingCollection<ComplexDouble[]>(new ConcurrentQueue<ComplexDouble[]>());
        private double[] frequencyRamp;

        public double startFrequency;
        public double stopFrequency;
        /// <summary>
        /// Number of steps in the sweep.
        /// </summary>
        public int steps;
        /// <summary>
        /// Power level of the RF generator.
        /// </summary>
        public double sgPowerLevel;
        /// <summary>
        /// Reference level of the RF analyzer.
        /// </summary>
        public double saReferenceLevel;
        /// <summary>
        /// Resolution bandwidth for each point in the sweep.
        /// </summary>
        public double bandwidth;
        /// <summary>
        /// Amount of time the analyzer spends acquiring a record.
        /// Increasing this value will result in higher accuracy at the expense of increased test time.
        /// </summary>
        public double dwellTime;
        /// <summary>
        /// The amount of time the analyzer waits before initiating it's dwell time.
        /// This value can be used to accomodate for propogation delay or extra settling time.
        /// </summary>
        public double deskew;
        /// <summary>
        /// PXI backplane line to use for device triggering.
        /// </summary>
        public string triggerLine;
        /// <summary>
        /// Amount of time to wait for an analyzer record to be available.
        /// </summary>
        public PrecisionTimeSpan timeout;

        /// <summary>
        /// Structure that organizes the results of the test.
        /// </summary> 
        public struct FrequencyResponseResults
        {
            /// <summary>
            /// Array of frequency points in the sweep.
            /// </summary>
            public double[] frequencyRamp;
            /// <summary>
            /// The power measured by the analyzer.
            /// </summary>
            public double[] measuredPower;
            /// <summary>
            /// Power measured by the analyzer minus the generator power level.
            /// </summary>
            public double[] measuredGain;
        }

        public FrequencyResponseResults results;

        public FrequencyResponseTest(
            Transceiver transceiver,
            double startFrequency = 995E6,
            double stopFrequency = 1.005E9,
            int steps = 60,
            double sgPowerLevel = -10,
            double saReferenceLevel = 0,
            double bandwidth = 1E6, //10E3,
            double dwellTime = 1E-3,
            double deskew = 0,
            string triggerLine = "PXI_Trig0",
            double timeout = 10)
        {
            this.transceiver = transceiver;
            this.startFrequency = startFrequency;
            this.stopFrequency = stopFrequency;
            this.steps = steps;
            this.sgPowerLevel = sgPowerLevel;
            this.saReferenceLevel = saReferenceLevel;
            this.bandwidth = bandwidth;
            this.dwellTime = dwellTime;
            this.deskew = deskew;
            this.triggerLine = triggerLine;
            this.timeout = new PrecisionTimeSpan(timeout);

            transceiver.Initialize(); // initializing the hardware ahead of time can dramatically speed up execution of the caller
        }

        public static FrequencyResponseTest[] Multisite(
            Transceiver[] sites,
            double startFrequency = 995E6,
            double stopFrequency = 1.005E9,
            int steps = 60,
            double sgPowerLevel = -10,
            double saReferenceLevel = 0,
            double bandwidth = 1E6, //10E3,
            double dwellTime = 1E-3,
            double deskew = 0,
            string triggerLine = "PXI_Trig0",
            double timeout = 10)
        {
            FrequencyResponseTest[] testArr = new FrequencyResponseTest[sites.Length];
            for (int i = 0; i < testArr.Length; i++)
            {
                testArr[i] = new FrequencyResponseTest(
                    sites[i],
                    startFrequency,
                    stopFrequency,
                    steps,
                    sgPowerLevel,
                    saReferenceLevel,
                    bandwidth,
                    dwellTime,
                    deskew,
                    triggerLine,
                    timeout);
            }
            return testArr;
        }

        /// <summary>
        /// Resets the hardware configuration and sets all static test parameters.
        /// </summary>
        public void Initialize()
        {
            transceiver.Reset();
            transceiver.RfsaHandle.Configuration.AcquisitionType = RfsaAcquisitionType.IQ;
            transceiver.RfsaHandle.Configuration.ReferenceClock.Configure(RfsaReferenceClockSource.PxiClock, 10E6);
            transceiver.RfsgHandle.Triggers.ConfigurationListStepTrigger.TriggerType = RfsgConfigurationListStepTriggerType.DigitalEdge;
            transceiver.RfsgHandle.FrequencyReference.Configure(RfsgFrequencyReferenceSource.PxiClock, 10E6);
        }

        private void ConfigureSA()
        {
            transceiver.RfsaHandle.Configuration.Vertical.ReferenceLevel = saReferenceLevel;
            transceiver.RfsaHandle.Configuration.IQ.IQRate = 1.25 * bandwidth;
            transceiver.RfsaHandle.Configuration.IQ.NumberOfRecords = frequencyRamp.Length;
            transceiver.RfsaHandle.Configuration.IQ.NumberOfSamples = (int)Math.Round(dwellTime * transceiver.RfsaHandle.Configuration.IQ.IQRate);
            transceiver.RfsaHandle.Configuration.Events.EndOfRecordEvent.OutputTerminal = triggerLine;
            transceiver.RfsaHandle.Configuration.Triggers.ReferenceTrigger.Advanced.TriggerDelay = deskew; // reference trigger delay works even if reference trigger is not configured
            transceiver.RfsaHandle.Acquisition.Advanced.DownconverterCenterFrequency = startFrequency - bandwidth; // inject to the left of the sweep band
            RfsaConfigurationListProperties[] configurationListProperties = new RfsaConfigurationListProperties[] { RfsaConfigurationListProperties.IQCarrierFrequency };
            transceiver.RfsaHandle.Configuration.BasicConfigurationList.CreateConfigurationList("AnalyzerSweep", configurationListProperties, true);
            for (int i = 0; i < frequencyRamp.Length; i++)
            {
                transceiver.RfsaHandle.Configuration.BasicConfigurationList.CreateStep(true);
                transceiver.RfsaHandle.Configuration.IQ.CarrierFrequency = frequencyRamp[i];
            }
            transceiver.RfsaHandle.Utility.Commit();
        }

        private void ConfigureSG()
        {
            transceiver.RfsgHandle.RF.PowerLevel = sgPowerLevel;
            transceiver.RfsgHandle.Triggers.ConfigurationListStepTrigger.DigitalEdge.Source = triggerLine;
            transceiver.RfsgHandle.RF.Upconverter.CenterFrequency = stopFrequency + bandwidth; // inject to the right of the sweep band
            RfsgConfigurationListProperties[] configurationListProperties = new RfsgConfigurationListProperties[] { RfsgConfigurationListProperties.Frequency };
            transceiver.RfsgHandle.BasicConfigurationList.CreateConfigurationList("GeneratorSweep", configurationListProperties, true);
            foreach (double frequency in frequencyRamp)
            {
                transceiver.RfsgHandle.BasicConfigurationList.CreateStep(true);
                transceiver.RfsgHandle.RF.Frequency = frequency;
            }
            transceiver.RfsgHandle.Utility.Commit();
        }

        /// <summary>
        /// Configures the test.
        /// </summary>
        public void Configure()
        {
            frequencyRamp = SignalGeneration.Ramp(startFrequency, stopFrequency, steps);
            ConfigureSA();
            ConfigureSG();
        }

        /// <summary>
        /// Starts the test.
        /// </summary>
        public void Initiate()
        {
            transceiver.RfsgHandle.Initiate();
            transceiver.RfsaHandle.Acquisition.IQ.Initiate();
            acquisitionCompleteEvent.Reset();
            measurementCompleteEvent.Reset();
            ThreadPool.QueueUserWorkItem(o => Acquire());
            ThreadPool.QueueUserWorkItem(o => Measure());
        }

        private void Acquire()
        {
            long numberOfSamples = transceiver.RfsaHandle.Configuration.IQ.NumberOfSamples;
            double sgPowerLevel = transceiver.RfsgHandle.RF.PowerLevel;
            for (int i = 0; i < frequencyRamp.Length; i++)
            {
                ComplexDouble[] rawIQ; // backward compatibility with c# versions less than 7
                transceiver.RfsaHandle.Acquisition.IQ.FetchIQSingleRecordComplex(i, numberOfSamples, timeout, out rawIQ);
                queue.Add(rawIQ);
            }
            transceiver.RfsgHandle.Abort();
            acquisitionCompleteEvent.Set();
        }

        private void Measure()
        {
            results.frequencyRamp = new double[frequencyRamp.Length];
            results.measuredPower = new double[frequencyRamp.Length];
            results.measuredGain = new double[frequencyRamp.Length];
            for (int i = 0; i < frequencyRamp.Length; i++)
            {
                results.frequencyRamp[i] = frequencyRamp[i]; // copying the frequency ramp array prevents modification by the user
                ComplexDouble[] rawIQ;
                if (queue.TryTake(out rawIQ, timeout.ToTimeSpan()))
                {
                    results.measuredPower[i] = SignalAnalysis.CalculatePower(rawIQ).Average();
                    results.measuredGain[i] = results.measuredPower[i] - sgPowerLevel;
                }
            }
            measurementCompleteEvent.Set();
        }

        /// <summary>
        /// Waits for the acquisition thread of the test class to complete.
        /// </summary>
        public void WaitUntilAcquisitionComplete()
        {
            acquisitionCompleteEvent.WaitOne();
        }

        /// <summary>
        /// Waits for the measurement thread of the test class to complete.
        /// </summary>
        public void WaitUntilMeasurementComplete()
        {
            measurementCompleteEvent.WaitOne();
        }

        /// <summary>
        /// Stops the test.
        /// </summary>
        public void Abort()
        {
            transceiver.RfsgHandle.Abort();
            transceiver.RfsaHandle.Acquisition.IQ.Abort();
        }

        public override string ToString()
        {
            return "FrequencyResponseTest_" + transceiver;
        }

    }
}