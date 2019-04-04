using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Broadcom.Tests;
using NationalInstruments;
using NationalInstruments.SystemsEngineering.Hardware;
using Broadcom.Tests.NoiseFloor;

namespace EqmtLibrary.NI5646R
{
    public partial class VST_NI5646R
    {
        public NoiseFloorTest[] BuildNFTest(string strWaveform, string strWaveformName, S_NoiseConfig[] noiseConfig)
        {
            var nfSite = new NoiseFloorTest[testSite];

            for (int i = 0; i < testSite; i++)
			{
                #region decode and re-arrange multiple bandwidth (Ascending)
                int multiRBW_cnt = 0;
                int bw_cnt = 0;
                double[] multiRBW_Hz = new double[noiseConfig[i].Bandwidths.Length];

                Array.Sort(noiseConfig[i].Bandwidths);
                foreach (double key in noiseConfig[i].Bandwidths)
                {
                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                    bw_cnt++;
                }

                multiRBW_cnt = multiRBW_Hz.Length;
                noiseConfig[i].Rbw = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                #endregion


                #region Load waveform (CW or from file)
                ComplexDouble[] iqDataArr;
                s_SignalType value = new s_SignalType();

                Get_s_SignalType(strWaveform, strWaveformName, out value);
                if (value.signalMode == "CW")
                {
                    iqDataCW_Array(out iqDataArr);
                }
                else
                {
                    iqData_Array(value.SG_IPath, value.SG_QPath, out iqDataArr);
                }

                var iqWaveform = ComplexWaveform<ComplexDouble>.FromArray1D(iqDataArr);
                iqWaveform.PrecisionTiming = PrecisionWaveformTiming.CreateWithRegularInterval(new PrecisionTimeSpan(1 / value.SG_IQRate));

                #endregion

                //pass configuration to Contact global config
                nfSite[i] = new NoiseFloorTest(
                    VST[i],
                    waveformName : strWaveformName,
                    waveform : iqWaveform,
                    numberOfRuns : noiseConfig[i].NumberOfRuns,
                    band : noiseConfig[i].Band,

                    dwellTime : noiseConfig[i].DwellTime,
                    soakTime : noiseConfig[i].SoakTime,
                    soakFrequency : noiseConfig[i].TXFrequencyStart,
                    preSoakSweep : noiseConfig[i].preSoakSweep,

                    txStartFrequency : noiseConfig[i].TXFrequencyStart,
                    txStopFrequency : noiseConfig[i].TXFrequencyStop,
                    frequencyStep : noiseConfig[i].TXFrequencyStep,

                    rxStartFrequency : noiseConfig[i].RXFrequencyStart,
                    rxStopFrequency : noiseConfig[i].RXFrequencyStop,
                    
                    saReferenceLevel : noiseConfig[i].SAReferenceLevel,
                    sgPowerLevel : noiseConfig[i].SGPowerLevel,
                    vbw : noiseConfig[i].Vbw                    
			        );

                nfSite[i].bandwidths = multiRBW_Hz;                
			}

            return nfSite;
        }

    }
}