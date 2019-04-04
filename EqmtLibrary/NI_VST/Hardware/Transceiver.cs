using NationalInstruments.ModularInstruments.NIRfsa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments;
using System.Collections.Generic;

namespace NationalInstruments.SystemsEngineering.Hardware
{
    public class Transceiver : Instrument
    {
        private HashSet<string> waveformCache = new HashSet<string>();

        public NIRfsa RfsaHandle { get; private set; }
        public NIRfsg RfsgHandle { get; private set; }

        public Transceiver(string IOAddress) : base(IOAddress) { }

        /// <summary>
        /// Initializes both the SA and the SG.
        /// </summary>
        public override void Initialize()
        {
            if (RfsaHandle == null)
                RfsaHandle = new NIRfsa(IOAddress, true, true);
            if (RfsgHandle == null)
                RfsgHandle = new NIRfsg(IOAddress, true, true);
            RfsaHandle.Configuration.ReferenceClock.Configure(RfsaReferenceClockSource.PxiClock, 10E6);
            RfsgHandle.FrequencyReference.Configure(RfsgFrequencyReferenceSource.PxiClock, 10E6);
            RfsaHandle.Utility.Commit();
            RfsgHandle.Utility.Commit();
        }

        /// <summary>
        /// Writes the specified waveform to the generator only if the waveform does not already exist in the generator's onboard memory.
        /// </summary>
        public void ConditionalWriteWaveform(string waveformName, ComplexWaveform<ComplexDouble> waveform)
        {
            if (!waveformCache.Contains(waveformName))
            {
                RfsgHandle.Arb.WriteWaveform(waveformName, waveform);
                waveformCache.Add(waveformName);
            }
        }

        /// <summary>
        /// Resets both the SA and the SG to their default states.
        /// </summary>
        public override void Reset()
        {
            RfsaHandle.Utility.Reset();
            RfsgHandle.Utility.Reset();
            waveformCache.Clear();
        }

        /// <summary>
        /// Closes both the SA and the SG.
        /// </summary>
        public override void Close()
        {
            RfsaHandle.Close();
            RfsaHandle = null;
            RfsgHandle.Close();
            RfsgHandle = null;
        }

        /// <summary>
        /// Ensures the the device sessions are closed if the object is garbage collected.
        /// </summary>
        ~Transceiver()
        {
            if (RfsaHandle != null)
                RfsaHandle.Close();
            if (RfsgHandle != null)
                RfsgHandle.Close();
        }
    }
}
