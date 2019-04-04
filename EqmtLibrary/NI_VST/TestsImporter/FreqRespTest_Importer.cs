using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Broadcom.Tests;
using NationalInstruments.SystemsEngineering.Hardware;
using Broadcom.Tests.NoiseFloor;

namespace EqmtLibrary.NI5646R
{
    public partial class VST_NI5646R
    {
        public FrequencyResponseTest[] BuildContactTest(S_NoiseConfig[] noiseConfig)
        {
            var freqRespSite = new FrequencyResponseTest[testSite];

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

                //pass configuration to Contact global config
                freqRespSite[i] = new FrequencyResponseTest(
                    VST[i],
                    dwellTime: noiseConfig[i].DwellTime,
                    startFrequency: noiseConfig[i].RXFrequencyStart,
                    stopFrequency: noiseConfig[i].RXFrequencyStop,
                    steps: (Convert.ToInt32((noiseConfig[i].RXFrequencyStop - noiseConfig[i].RXFrequencyStart) / noiseConfig[i].RXFrequencyStep)) + 1,
                    sgPowerLevel: noiseConfig[i].SGPowerLevel,
                    saReferenceLevel: noiseConfig[i].SAReferenceLevel
                    //bandwidth = noiseConfig[i].Rbw;        //disable currently due to using CW only , just use as default 10E3 in defaultconfiguration
                    );
            }

            return freqRespSite;
        }
    }
}