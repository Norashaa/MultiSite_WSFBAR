#define ni5644R

using NationalInstruments;
using NationalInstruments.ModularInstruments.NIRfsa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.SystemsEngineering.Hardware;
using NationalInstruments.SystemsEngineering.SignalProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Broadcom.Tests
{
    public class NoiseFloorTest : ITest
    {
        private Transceiver transceiver;
        private ManualResetEvent acquisitionCompleteEvent = new ManualResetEvent(true);
        private ManualResetEvent measurementCompleteEvent = new ManualResetEvent(false);
        private BlockingCollection<ComplexWaveform<ComplexDouble>> queue =
            new BlockingCollection<ComplexWaveform<ComplexDouble>>(new ConcurrentQueue<ComplexWaveform<ComplexDouble>>());
        private double[] txFrequencyRamp;
        private double[] rxFrequencyRamp;

        /// <summary>
        /// Provides a unique name for the script and the configuration lists of the SA and SG.
        /// </summary>
        public string band;
        /// <summary>
        /// Name of the waveform written to the generator.
        /// </summary>
        public string waveformName;
        /// <summary>
        /// Waveform to generate.  Defined by the waveformName field.
        /// </summary>
        public ComplexWaveform<ComplexDouble> waveform;
        /// <summary>
        /// Start frequency of the TX band.
        /// Set this value equal to the txStopFrequency for fixed TX.
        /// </summary>
        public double txStartFrequency;
        /// <summary>
        /// Stop frequency of the RX band.
        /// Set this value equal to the txStartFrequency for fixed TX.
        /// </summary>
        public double txStopFrequency;
        /// <summary>
        /// Start frequency of the RX band.
        /// </summary>
        public double rxStartFrequency;
        /// <summary>
        /// Stop frequency of the RX band.
        /// </summary>
        public double rxStopFrequency;
        /// <summary>
        /// Resolution of each step in the frequency sweep.
        /// Use the TX frequency step here.
        /// </summary>
        public double frequencyStep;
        /// <summary>
        /// Number of times to run the sweep.
        /// The max hold trace is calculated over the number of runs specified.
        /// </summary>
        public int numberOfRuns;
        /// <summary>
        /// PXI backplane trigger line driven by the generator and used by the analyzer to capture a record.
        /// </summary>
        public string referenceTriggerLine;
        /// <summary>
        /// Peak power level of the IQ waveform.
        /// </summary>
        public double sgPowerLevel;
        /// <summary>
        /// Analyzer reference level.
        /// </summary>
        public double saReferenceLevel;
        /// <summary>
        /// Array of resolution bandwidth values to use at each sweep point.
        /// </summary>
        public double[] bandwidths = new double[] { 10E3, 100E3, 1E6 };
        /// <summary>
        /// Video bandwidth value to use at each sweep point.
        /// </summary>
        public double vbw;
        /// <summary>
        /// Specifies whether to perform a presoak sweep.
        /// </summary>
        public bool preSoakSweep;
        /// <summary>
        /// Frequency at which the generator will soak for the specified soak time.
        /// </summary>
        public double soakFrequency;
        /// <summary>
        /// Time in seconds to soak at the frequency specified.
        /// </summary>
        public double soakTime;
        /// <summary>
        /// Acquisition time of each point in the sweep.
        /// </summary>
        public double dwellTime;
        /// <summary>
        /// Time in which the idle waveform is generated between points.
        /// </summary>
        public double idleTime;
        /// <summary>
        /// Specifies how long the analyzer waits before dwelling at a particular frequency.
        /// </summary>
        public double referenceTriggerDelay;
        /// <summary>
        /// Amount of time to wait for a record to become available.
        /// </summary>
        public PrecisionTimeSpan timeout;

        public struct NoiseFloorResults
        {
            public int numberOfTracePoints;
            public double[][][] noiseFloorTraces;
            public double[][] maxHoldTraces;
        }

        public NoiseFloorResults results;

        public NoiseFloorTest(
            Transceiver transceiver,
            string band,
            string waveformName,
            ComplexWaveform<ComplexDouble> waveform,
            double txStartFrequency = 1920E6,
            double txStopFrequency = 1980E6,
            double rxStartFrequency = 2110E6,
            double rxStopFrequency = 2170E6,
            double frequencyStep = 1E6,
            int numberOfRuns = 1,
            string referenceTriggerLine = "PXI_Trig0",
            double sgPowerLevel = -10,
            double saReferenceLevel = 10,
            double vbw = 10000,
            bool preSoakSweep = false,
            double soakFrequency = 1955E6,
            double soakTime = 0,
            double dwellTime = 1E-3,
            double idleTime = 300E-6,
            double referenceTriggerDelay = 15E-6,
            double timeout = 10)
        {
            this.transceiver = transceiver;
            this.band = band;
            this.waveformName = waveformName;
            this.waveform = waveform;
            this.txStartFrequency = txStartFrequency;
            this.txStopFrequency = txStopFrequency;
            this.rxStartFrequency = rxStartFrequency;
            this.rxStopFrequency = rxStopFrequency;
            this.frequencyStep = frequencyStep;
            this.numberOfRuns = numberOfRuns;
            this.referenceTriggerLine = referenceTriggerLine;
            this.sgPowerLevel = sgPowerLevel;
            this.saReferenceLevel = saReferenceLevel;
            this.vbw = vbw;
            this.preSoakSweep = preSoakSweep;
            this.soakFrequency = soakFrequency;
            this.soakTime = soakTime;
            this.dwellTime = dwellTime;
            this.idleTime = idleTime;
            this.referenceTriggerDelay = referenceTriggerDelay;
            this.timeout = new PrecisionTimeSpan(timeout);

            ThreadPool.QueueUserWorkItem(o => LVFilters.Initialize()); // this is launched asynchronously and will never be waited on

            transceiver.Initialize();
        }

        public static NoiseFloorTest[] Multisite(
            Transceiver[] sites,
            string band,
            string waveformName,
            ComplexWaveform<ComplexDouble> waveform,
            double txStartFrequency = 1920E6,
            double txStopFrequency = 1980E6,
            double rxStartFrequency = 2110E6,
            double rxStopFrequency = 2170E6,
            double frequencyStep = 1E6,
            int numberOfRuns = 1,
            string referenceTriggerLine = "PXI_Trig0",
            double sgPowerLevel = -10,
            double saReferenceLevel = 10,
            double vbw = 10000,
            bool preSoakSweep = false,
            double soakFrequency = 1955E6,
            double soakTime = 0,
            double dwellTime = 1E-3,
            double idleTime = 300E-6,
            double referenceTriggerDelay = 15E-6,
            double timeout = 10)
        {
            NoiseFloorTest[] testArr = new NoiseFloorTest[sites.Length];
            for (int i = 0; i < testArr.Length; i++)
            {
                testArr[i] = new NoiseFloorTest(
                    sites[i],
                    band,
                    waveformName,
                    waveform,
                    txStartFrequency,
                    txStopFrequency,
                    rxStartFrequency,
                    rxStopFrequency,
                    frequencyStep,
                    numberOfRuns,
                    referenceTriggerLine,
                    sgPowerLevel,
                    saReferenceLevel,
                    vbw,
                    preSoakSweep,
                    soakFrequency,
                    soakTime,
                    dwellTime,
                    idleTime,
                    referenceTriggerDelay,
                    timeout);
            }
            return testArr;
        }

        /// <summary>
        /// Resets the hardware configuration and sets all static test parameters.
        /// </summary>
        public void Initialize()
        {
            transceiver.Reset(); // prevents the use of residual settings
            // rfsa properties
            transceiver.RfsaHandle.Configuration.ReferenceClock.Configure(RfsaReferenceClockSource.PxiClock, 10E6);
            transceiver.RfsaHandle.Configuration.AcquisitionType = RfsaAcquisitionType.IQ;
            // rfsg properties
            transceiver.RfsgHandle.FrequencyReference.Configure(RfsgFrequencyReferenceSource.PxiClock, 10E6);
            transceiver.RfsgHandle.Arb.GenerationMode = RfsgWaveformGenerationMode.Script;
            transceiver.RfsgHandle.RF.PowerLevelType = RfsgRFPowerLevelType.PeakPower;
            transceiver.RfsgHandle.Arb.PreFilterGain = -2;
            transceiver.RfsgHandle.Triggers.ConfigurationListStepTrigger.TriggerType = RfsgConfigurationListStepTriggerType.DigitalEdge;
            transceiver.RfsgHandle.Triggers.ConfigurationListStepTrigger.DigitalEdge.Source = RfsgDigitalEdgeConfigurationListStepTriggerSource.Marker0Event;
        }

        private void ConfigureSA()
        {
            transceiver.RfsaHandle.Configuration.Vertical.ReferenceLevel = saReferenceLevel;
            double maxBandwidth = bandwidths.Max();
            transceiver.RfsaHandle.Configuration.IQ.IQRate = 1.25 * maxBandwidth;
            double actualIqRate = transceiver.RfsaHandle.Configuration.IQ.IQRate; // need to pull the actual iq rate from the driver for accurate calculation
            transceiver.RfsaHandle.Configuration.IQ.NumberOfSamples = (long)Math.Ceiling(actualIqRate * dwellTime);
            transceiver.RfsaHandle.Configuration.IQ.NumberOfRecords = rxFrequencyRamp.Length * numberOfRuns;
            { // automatically offset the LO outside of the band
                // the DownconverterCenterFrequency property will remain constant for each step in the configuration list
                if (txStartFrequency > rxStopFrequency)
                    transceiver.RfsaHandle.Acquisition.Advanced.DownconverterCenterFrequency = rxStartFrequency - maxBandwidth; // inject to the left if the rx band is below the tx band
                else if (rxStartFrequency > txStopFrequency)
                    transceiver.RfsaHandle.Acquisition.Advanced.DownconverterCenterFrequency = rxStopFrequency + maxBandwidth; // inject to the right if the rx band is above the tx band
                else // if none of the above then there is some band overlap
                { // find the side that is easier to inject on
                    double upperMargin = Math.Abs(rxStopFrequency - txStopFrequency);
                    double lowerMargin = Math.Abs(rxStartFrequency - txStartFrequency);
                    if (upperMargin < lowerMargin)
                        transceiver.RfsaHandle.Acquisition.Advanced.DownconverterCenterFrequency = rxStopFrequency + upperMargin + maxBandwidth;
                    else // prefer low side injection if the upper and lower margins are equal
                        transceiver.RfsaHandle.Acquisition.Advanced.DownconverterCenterFrequency = rxStartFrequency - lowerMargin - maxBandwidth;
                } // the driver will throw an error if it is unable to satify the calculated downconverter center frequency
            }
#if ni5644R
            { // comment out if using 5646R
                transceiver.RfsaHandle.Acquisition.Advanced.DownconverterCenterFrequency = (rxStartFrequency + rxStopFrequency) / 2;
            }
#endif
            { // set configuration list to loop through the carrier frequencies
                RfsaConfigurationListProperties[] configurationListProperties = new RfsaConfigurationListProperties[] { RfsaConfigurationListProperties.IQCarrierFrequency };
                transceiver.RfsaHandle.Configuration.BasicConfigurationList.CreateConfigurationList(band, configurationListProperties, true);
                foreach (double frequency in rxFrequencyRamp)
                { // a configuration list executes in the order in which it is configured
                    transceiver.RfsaHandle.Configuration.BasicConfigurationList.CreateStep(true);
                    transceiver.RfsaHandle.Configuration.IQ.CarrierFrequency = frequency;
                }
            }
            transceiver.RfsaHandle.Configuration.Triggers.ReferenceTrigger.DigitalEdge.Configure(referenceTriggerLine, RfsaTriggerEdge.Rising, 0);
            transceiver.RfsaHandle.Utility.Commit();
        }

        private string BuildScript()
        {
            double actualIqRate = transceiver.RfsgHandle.Arb.IQRate; // get the actual iqRate of the generator for sample calculations

            // remove initial sweep time from overall soak time #NJK
            if (soakTime - ((dwellTime + idleTime) * rxFrequencyRamp.Length) > 0)
                soakTime = soakTime - ((dwellTime + idleTime) * rxFrequencyRamp.Length);
            int soakTimeSignalRepeat = (int)(actualIqRate * soakTime / waveform.SampleCount);
            int soakTimeResidualSamples = (int)((actualIqRate * soakTime) % waveform.SampleCount);
            soakTimeResidualSamples -= soakTimeResidualSamples % 2;

            int activeWaveformSubset = (int)Math.Round(actualIqRate * (dwellTime + referenceTriggerDelay));
            activeWaveformSubset -= activeWaveformSubset % 2;
            int idleWaveformSubset = (int)Math.Round(actualIqRate * idleTime);
            idleWaveformSubset -= idleWaveformSubset % 2;
            int referenceTriggerDelayLength = (int)(actualIqRate * referenceTriggerDelay); // truncate towards 0
            referenceTriggerDelayLength -= referenceTriggerDelayLength % 2;

            /// marker0 is for advancing through the configuration list and is not exported
            /// marker1 is for the rfsa reference trigger
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendFormat("script {0}\r\n", band);

            // string to repeat during script flattening for each sweep step
            string repeatingSignal = string.Format("generate {0} subset(0, {1}) marker1({2})\r\n", waveformName, activeWaveformSubset, referenceTriggerDelayLength);
            repeatingSignal += string.Format("generate {0} subset(0, {1}) marker0(0)\r\n", waveformName, idleWaveformSubset); // advances configuration list

            scriptBuilder.AppendFormat("generate {0} subset(0, {1})", waveformName, idleWaveformSubset);
            if (soakTime > 0)
                scriptBuilder.Append(" marker0(0)\r\n"); // tune out of soak if it is present
            else
                scriptBuilder.Append("\r\n");

            if (preSoakSweep)
            {
                scriptBuilder.AppendFormat("repeat {0}\r\n", rxFrequencyRamp.Length);
                scriptBuilder.Append(repeatingSignal);
                scriptBuilder.Append("end repeat\r\n");
            }
            // will be at soak frequency now if there is one
            if (soakTime > 0)
            {
                if (soakTimeSignalRepeat > 0)
                {
                    scriptBuilder.AppendFormat("repeat {0}\r\n", soakTimeSignalRepeat);
                    scriptBuilder.AppendFormat("generate {0}\r\n", waveformName);
                    scriptBuilder.Append("end repeat\r\n");
                }
                if (soakTimeResidualSamples > 0)
                    scriptBuilder.AppendFormat("generate {0} subset(0, {1})\r\n", waveformName, soakTimeResidualSamples);
                // retune time from soak frequency to first sweep frequency
                scriptBuilder.AppendFormat("generate {0} subset(0, {1}) marker0(0)\r\n", waveformName, idleWaveformSubset);
            }
            // build flattened script
            if (soakTime > 0)
            {
                for (int i = 0; i < numberOfRuns; i++)
                {
                    scriptBuilder.AppendFormat("repeat {0}\r\n", rxFrequencyRamp.Length);
                    scriptBuilder.Append(repeatingSignal);
                    scriptBuilder.Append("end repeat\r\n");
                    // string to call at the end of each sweep to index past the soak frequency (element 0 in the frequency list)
                    scriptBuilder.AppendFormat("generate {0} subset(0, {1}) marker0(0)\r\n", waveformName, idleWaveformSubset);
                }
            }
            else // simplifies script if there is no soak time
            {
                scriptBuilder.AppendFormat("repeat {0}\r\n", numberOfRuns * rxFrequencyRamp.Length);
                scriptBuilder.Append(repeatingSignal);
                scriptBuilder.Append("end repeat\r\n");
            }
            scriptBuilder.Append("end script");
            return scriptBuilder.ToString();
        }

        private void ConfigureSG()
        {
            transceiver.RfsgHandle.RF.PowerLevel = sgPowerLevel;
            // set IQ rate of the generator
            transceiver.RfsgHandle.Arb.IQRate = 1 / waveform.PrecisionTiming.SampleInterval.TotalSeconds;
            { // automatically offset the LO outside of the band
                // the Upconverter.CenterFrequency property will remain constant for each step in the configuration list
                double maxBandwidth = bandwidths.Max();
                if (txStartFrequency > rxStopFrequency)
                    transceiver.RfsgHandle.RF.Upconverter.CenterFrequency = txStopFrequency + maxBandwidth; // inject to the right if the rx band is below the tx band
                else if (rxStartFrequency > txStopFrequency)
                    transceiver.RfsgHandle.RF.Upconverter.CenterFrequency = txStartFrequency - maxBandwidth; // inject to the left if the rx band is above the tx band
                else // if none of the above then there is some band overlap
                { // find the side that is easier to inject on
                    double upperMargin = Math.Abs(rxStopFrequency - txStopFrequency);
                    double lowerMargin = Math.Abs(rxStartFrequency - txStartFrequency);
                    if (lowerMargin < upperMargin)
                        transceiver.RfsgHandle.RF.Upconverter.CenterFrequency = txStartFrequency - lowerMargin - maxBandwidth;
                    else // prefer high side injection if the upper and lower margins are equal
                        transceiver.RfsgHandle.RF.Upconverter.CenterFrequency = txStopFrequency + upperMargin + maxBandwidth;
                } // the driver will throw an error if it is unable to satify the calculated downconverter center frequency
            }
#if ni5644R
            { // comment out if using 5646R
                transceiver.RfsgHandle.RF.Upconverter.CenterFrequency = (txStartFrequency + txStopFrequency) / 2;
            }
#endif
            { // set configuration list to loop through the carrier frequencies
                // transceiver.RfsgHandle.BasicConfigurationList.CheckIfConfigurationListExists(Band); // possible speed improvement can be made here.
                RfsgConfigurationListProperties[] configurationListProperties = new RfsgConfigurationListProperties[] { RfsgConfigurationListProperties.Frequency };
                transceiver.RfsgHandle.BasicConfigurationList.CreateConfigurationList(band, configurationListProperties, true);
                if (soakTime > 0)
                {
                    transceiver.RfsgHandle.BasicConfigurationList.CreateStep(true);
                    transceiver.RfsgHandle.RF.Frequency = soakFrequency;
                }
                foreach (double frequency in txFrequencyRamp)
                { // create steps for sweep
                    transceiver.RfsgHandle.BasicConfigurationList.CreateStep(true);
                    transceiver.RfsgHandle.RF.Frequency = frequency;
                }
            }
            transceiver.ConditionalWriteWaveform(waveformName, waveform);
            transceiver.RfsgHandle.Arb.Scripting.WriteScript(BuildScript());
            transceiver.RfsgHandle.Arb.Scripting.SelectedScriptName = band;
            transceiver.RfsgHandle.DeviceEvents.MarkerEvents[1].ExportedOutputTerminal = referenceTriggerLine;
            transceiver.RfsgHandle.Utility.Commit();
        }

        public void Configure()
        {
            { // configuring the ramps before calling wait function leaves flexibility to multithread the configurations in the future if desired
                double rxStartFrequency = this.rxStartFrequency + frequencyStep / 2; // place the tuning points in the middle of the sweep points
                double rxStopFrequency = this.rxStopFrequency - frequencyStep / 2;
                rxFrequencyRamp = SignalGeneration.Ramp(rxStartFrequency, rxStopFrequency, frequencyStep);
                if (txStartFrequency != txStopFrequency) // check for fixed TX case
                { // create independent frequency ramp if not fixed tx
                    double txTmpStartFrequency = this.txStartFrequency + frequencyStep / 2;
                    double txTmpStopFrequency = this.txStopFrequency - frequencyStep / 2;
                    txFrequencyRamp = SignalGeneration.Ramp(txTmpStartFrequency, txTmpStopFrequency, frequencyStep);
                }
                else // build array of equal values that are all the fixed tx frequency
                    txFrequencyRamp = SignalGeneration.Ramp(txStartFrequency, txStopFrequency, rxFrequencyRamp.Length);
            }
            ConfigureSA();
            ConfigureSG();
        }

        private void Acquire()
        {
            long numberOfSamples = transceiver.RfsaHandle.Configuration.IQ.NumberOfSamples;
            for (int i = 0; i < rxFrequencyRamp.Length * numberOfRuns; i++)
            {
                ComplexWaveform<ComplexDouble> iqData = transceiver.RfsaHandle.Acquisition.IQ.FetchIQSingleRecordComplexWaveform<ComplexDouble>(i, numberOfSamples, timeout);
                queue.Add(iqData);
            }
            transceiver.RfsgHandle.Abort();
            acquisitionCompleteEvent.Set();
        }

        private void Measure()
        {
            // allocate space for the results
            int tracePointsPerStep = (int)(frequencyStep / 100E3); // truncates towards 0
            results.numberOfTracePoints = rxFrequencyRamp.Length * tracePointsPerStep + 1;
            results.noiseFloorTraces = new double[bandwidths.Length][][];
            results.maxHoldTraces = new double[bandwidths.Length][];
            for (int i = 0; i < bandwidths.Length; i++)
            {
                results.noiseFloorTraces[i] = new double[numberOfRuns][];
                for (int j = 0; j < numberOfRuns; j++)
                    results.noiseFloorTraces[i][j] = Enumerable.Repeat(double.NegativeInfinity, results.numberOfTracePoints).ToArray();
                results.maxHoldTraces[i] = Enumerable.Repeat(double.NegativeInfinity, results.numberOfTracePoints).ToArray();
            }
            // process data from producer
            for (int i = 0; i < numberOfRuns; i++)
            {
                for (int j = 0; j < rxFrequencyRamp.Length; j++)
                {
                    ComplexWaveform<ComplexDouble> iqData; // for compatibility with pre c# 7 code
                    if (queue.TryTake(out iqData, timeout.ToTimeSpan()))
                    {
                        double iqRate = 1 / iqData.PrecisionTiming.SampleInterval.TotalSeconds;
                        /// Call LabVIEW built DLL for filtering the data.
                        /// The VI is set to shared clone re-entrant, therefore we can manage execution thread from C#.
                        Parallel.For(0, bandwidths.Length, bandwidthIndex =>
                        {
                            double[] powerTrace = LVFilters.ComplexFilter(iqRate, vbw, 1000, bandwidths[bandwidthIndex], 70, iqData.GetScaledData());
                            // sort the trace from high to low
                            int[] indexes = Enumerable.Range(0, powerTrace.Length).ToArray();
                            Array.Sort(powerTrace, indexes, Comparer<double>.Create(new Comparison<double>((i1, i2) => i2.CompareTo(i1)))); // sort decending
                            // take the first (max) values of the sorted power trace and sort again according to index
                            int recordTracePoints = tracePointsPerStep;
                            if (j == rxFrequencyRamp.Length - 1)
                                recordTracePoints++;
                            Array.Sort(indexes, powerTrace, 0, recordTracePoints); // sorts just the points we are interested in
                            // evaluate the max hold of the result
                            for (int k = 0; k < recordTracePoints; k++)
                            {
                                int traceIndex = j * tracePointsPerStep + k;
                                results.noiseFloorTraces[bandwidthIndex][i][traceIndex] = powerTrace[k];
                                results.maxHoldTraces[bandwidthIndex][traceIndex] = Math.Max(results.maxHoldTraces[bandwidthIndex][traceIndex], powerTrace[k]);
                            }
                        });
                    }
                }
            }
            measurementCompleteEvent.Set();
        }

        /// <summary>
        /// Starts the test.
        /// </summary>
        public void Initiate()
        {
            transceiver.RfsaHandle.Acquisition.IQ.Initiate();
            transceiver.RfsgHandle.Initiate();
            acquisitionCompleteEvent.Reset();
            measurementCompleteEvent.Reset();
            ThreadPool.QueueUserWorkItem(o => Acquire());
            ThreadPool.QueueUserWorkItem(o => Measure());
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
            return "NoiseFloorTest_" + transceiver;
        }

    }
}
