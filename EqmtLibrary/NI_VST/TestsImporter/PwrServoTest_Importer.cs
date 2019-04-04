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
        public PowerServoTest[] BuildPwrServoTest(string strWaveform, string strWaveformName, S_NoiseConfig[] noiseConfig)
        {
            var pwrServoSite = new PowerServoTest[testSite];

            for (int i = 0; i < testSite; i++)
            {
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
                pwrServoSite[i] = new PowerServoTest(
                    VST[i],
                    waveformName: strWaveformName,
                    waveform: iqWaveform,
                    centerFrequency : noiseConfig[i].TXFrequencyStart,
                    powerLevel : noiseConfig[i].SGPowerLevel
                    );
            }

            return pwrServoSite;
        }

    }

    public static class PwrServoTestExtensions
    {
        /// <summary>
        /// Extension method of multisite PowerServoTest in order to change power level during test running
        /// </summary>
        /// <param name="tests">PowerServoTest</param>
        /// <param name="refLvl">Desired output power level </param>
        /// <param name="counter">Loop counter. When 1, initialize the test. </param>
        /// <param name="disableTune">Set this true when want to abort the test</param>
        public static void ChangePowerLevel(this PowerServoTest[] tests, double[] refLvl, int counter, bool disableTune)
        {
            if (!disableTune)
            {
                if (counter < 1)        //Do this routine during initial power servo - Complete configuration and SG setting
                {
                    for (int i = 0; i < tests.Length; i++)
                    {
                        tests[i].Initialize();

                        tests[i].powerLevel = refLvl[i];
                        tests[i].ApplyPowerLevel(); 
                    }


                }
                else                    //Only do this routine during power servo search loop -> counter more than 1 
                {
                    for (int i = 0; i < tests.Length; i++)
                    {
                        tests[i].powerLevel = refLvl[i];
                        tests[i].ApplyPowerLevel(); 
                    }

                }
            }
            else
            {
                for (int i = 0; i < tests.Length; i++)
                {
                    //when bool set to true -> will abort power servo routine
                    tests[i].Abort(); 
                }
            }
        }
    }
}