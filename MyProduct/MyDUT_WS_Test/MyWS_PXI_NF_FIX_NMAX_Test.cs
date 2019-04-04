using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Avago.ATF.StandardLibrary;
using Broadcom.Tests;
using EqmtLibrary.NI5646R;

namespace MyProduct
{
    public class MyWS_PXI_NF_FIX_NMAX_Test : MyDUT_WS_Test
    {
        public NoiseFloorTest[] nfsite;
        public PowerServoTest[] pwrServoSite;

        #region Test Variables
        public int istep = 0;
        public int NumberOfRuns;        
        public double SAReferenceLevel;
        public double SoakTime;
        public double SoakFrequency;
        public double vBW_Hz;
        public double RBW_Hz;
        public bool preSoakSweep;
        public int preSoakSweepTemp;
        public double stepFreqMHz;
        public double tmpRXFreqHz;
        public int sweepPts;

        #endregion

        /// <summary>
        /// Constructor of WS_PXI_NF_FIX_NMAX test. Base constructor will be called prior to this.
        /// </summary>
        /// <param name="MyDUT"></param>
        /// <param name="TestPara"></param>
        public MyWS_PXI_NF_FIX_NMAX_Test(MyDUT_WS MyDUT, Dictionary<string, string> TestPara)
            : base(MyDUT, TestPara)
        {
            MyDUT.PreConfigureTest(this, TestPara);
        }

        /// <summary>
        /// Test execution. Call this from RunTest
        /// </summary>
        /// <param name="results"></param>
        public override void Initiate()
        {
            ElapsedMilliseconds = mydut.ExecuteTest(this);            
        }

    }


    partial class MyDUT_WS
    {
        /// <summary>
        /// Pre-configuration particular for WS_PXI_NF_FIX_NMAX test
        /// </summary>
        /// <param name="T"></param>
        /// <param name="TestPara"></param>
        public void PreConfigureTest(MyWS_PXI_NF_FIX_NMAX_Test T, Dictionary<string, string> TestPara)
        {
            // This function is similar to PXI_NF_NONCA_NDIAG, but the TX frequency range is fixed while RX band is swept
            
            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
            //Variable use in VST Measure Function
            T.NumberOfRuns = 5;
            T.SAReferenceLevel = -20;
            T.SoakTime = 450e-3;
            T.SoakFrequency = T.ºStartTXFreq1 * 1e6;
            T.vBW_Hz = 300;
            T.RBW_Hz = 1e6;
            T.preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
            T.preSoakSweepTemp = T.preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
            T.stepFreqMHz = 0.1;
            T.tmpRXFreqHz = T.ºStartRXFreq1 * 1e6;
            T.sweepPts = (Convert.ToInt32((T.ºStopRXFreq1 - T.ºStartRXFreq1) / T.stepFreqMHz)) + 1; //Determine sweep points according to RX frequency range
            //----

            T.status = false;
            T.Index = 0;
            T.tx1_span = 0;
            T.tx1_noPoints = 0;
            T.rx1_span = 0;
            T.rx1_cntrfreq = 0;

            T.tolerancePwr = Convert.ToDouble(T.ºPoutTolerance);
            if (T.tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                T.tolerancePwr = 0.5;

            if (T.ºPXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                T.NumberOfRuns = 1;
            else
                T.NumberOfRuns = T.ºPXI_NoOfSweep;

            //use for searching previous result - to get the DUT LNA gain from previous result
            if (Convert.ToInt16(T.ºTestUsePrev) > 0)
            {
                T.usePrevRslt = true;
                T.resultTag = (int)e_ResultTag.NF1_AMPL;
                //prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
            }

            #region Decode Calibration Path and Segment Data
            T.CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", T.ºSwBand.ToUpper());
            myUtility.Decode_CalSegm_Setting(T.CalSegmData);
            #endregion

            #region PowerSensor Offset, MXG and MXA1 configuration

            //Calculate PAPR offset for PXI SG
            EqVST.Get_s_SignalType(T.ºModulation, T.ºWaveFormName, out EqVST.SignalType);
            T.papr_dB = EqVST.SignalType.SG_papr_dB;

            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
            T.tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
            T.tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

            //Get average pathloss base on start and stop freq
            T.count = Convert.ToInt16((T.ºStopTXFreq1 - T.ºStartTXFreq1) / T.ºStepTXFreq1);
            T.ºTXFreq = T.ºStartTXFreq1;
            for (int i = 0; i <= T.count; i++)
            {
                multiSite_Pathloss(TestSite, LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, T.ºTXFreq, ref T.ºLossInputPathSG1, ref T.StrError);
                multiSite_Pathloss(TestSite, LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, T.ºTXFreq, ref T.ºLossCouplerPath, ref T.StrError);

                for (int j = 0; j < TestSite; j++)
                {
                    T.tmpInputLoss[j] = Math.Round(T.tmpInputLoss[j] + (float)T.ºLossInputPathSG1[j], 3);         //need to use round function because of C# float and double floating point bug/error
                    T.tmpCouplerLoss[j] = Math.Round(T.tmpCouplerLoss[j] + (float)T.ºLossCouplerPath[j], 3);      //need to use round function because of C# float and double floating point bug/error
                }

                T.ºTXFreq = Convert.ToSingle(Math.Round(T.ºTXFreq + T.ºStepTXFreq1, 3));      //need to use round function because of C# float and double floating point bug/error  
            }
            for (int j = 0; j < TestSite; j++)
            {
                T.tmpAveInputLoss[j] = Math.Round(T.tmpInputLoss[j] / (T.count + 1), 3);
                T.tmpAveCouplerLoss[j] = Math.Round(T.tmpCouplerLoss[j] / (T.count + 1), 3);
                T.totalInputLoss[j] = Math.Round(T.tmpAveInputLoss[j] - T.tbInputLoss, 3);

                //change PowerSensor, MXG setting
                //normalize the coupler pathloss (power sensor) to ensure same reading at TX_IN port because power tuning will use Coupler Power as Ref of TX IN power level
                //assumption made that both port (coupler and TX_IN) are same power level when we have normalize the power sensor offset
                T.normalizeCPLLoss[j] = Math.Round(T.tmpAveInputLoss[j] - T.tmpAveCouplerLoss[j], 3);

                //Find actual SG Power Level
                T.SGTargetPin[j] = Math.Round(T.ºPin1 - (T.totalInputLoss[j] - T.papr_dB), 3);
                if (T.SGTargetPin[j] > T.ºSG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                {
                    return;
                }
            }

            EqPwrMeter.set_offsets(T.normalizeCPLLoss);
            T.MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", T.ºSwBand.ToUpper());
            myUtility.Decode_MXA_Setting(T.MXA_Config);

            T.SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
            T.vBW_Hz = myUtility.MXA_Setting.VBW;
            

            #endregion

            #region Populate Noise Test Config

            #region decode and re-arrange multiple bandwidth (Ascending)
            T.bw_cnt = 0;
            T.tmpRBW_Hz = Array.ConvertAll(T.ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array

            T.multiRBW_Hz = new double[T.tmpRBW_Hz.Length];

            Array.Sort(T.tmpRBW_Hz);
            foreach (double key in T.tmpRBW_Hz)
            {
                T.multiRBW_Hz[T.bw_cnt] = Convert.ToDouble(key);
                T.bw_cnt++;

            }

            T.multiRBW_cnt = T.multiRBW_Hz.Length;
            T.RBW_Hz = T.multiRBW_Hz[T.multiRBW_cnt - 1];   //the largest RBW is the last in array 
            #endregion

            //pass configuration to NF global config
            var nfconfig = new Broadcom.Tests.NoiseFloor.S_NoiseConfig[VST_Address.Length];

            for (int i = 0; i < VST_Address.Length; i++)
            {
                nfconfig[i].Band = T.ºTX1Band;
                nfconfig[i].Modulation = T.ºModulation;
                nfconfig[i].WaveformName = T.ºWaveFormName;

                nfconfig[i].NumberOfRuns = T.NumberOfRuns;
                nfconfig[i].DwellTime = (T.ºDwellT1 - 0.03) / 1000;
                nfconfig[i].SoakTime = T.SoakTime;
                nfconfig[i].preSoakSweep = T.preSoakSweep;

                nfconfig[i].TXFrequencyStart = T.ºStartTXFreq1 * 1e6;
                nfconfig[i].TXFrequencyStop = T.ºStopTXFreq1 * 1e6;
                nfconfig[i].TXFrequencyStep = T.ºStepTXFreq1 * 1e6;

                nfconfig[i].RXFrequencyStart = T.ºStartRXFreq1 * 1e6;
                nfconfig[i].RXFrequencyStop = T.ºStopRXFreq1 * 1e6;

                nfconfig[i].SGPowerLevel = T.SGTargetPin[i];
                nfconfig[i].SAReferenceLevel = T.SAReferenceLevel;
                nfconfig[i].Vbw = T.vBW_Hz;
                nfconfig[i].Rbw = T.RBW_Hz;

                nfconfig[i].Bandwidths = new double[T.multiRBW_cnt];
                nfconfig[i].Bandwidths = T.multiRBW_Hz;
            }

            #endregion
            
            
            T.pwrServoSite = EqVST.BuildPwrServoTest(T.ºModulation, T.ºWaveFormName, nfconfig);
            T.nfsite = EqVST.BuildNFTest(T.ºModulation, T.ºWaveFormName, nfconfig);
        }

        /// <summary>
        /// Execution of WS_PXI_NF_FIX_NMAX test
        /// </summary>
        /// <param name="T"></param>
        /// <param name="results"></param>
        public long ExecuteTest(MyWS_PXI_NF_FIX_NMAX_Test T)
        {
            Stopwatch tTime = new Stopwatch();

            tTime.Reset();
            tTime.Start();

            #region measure contact power (Pout1)

            multiSite_PowerServo(T.pwrServoSite,
                T.ºTunePwr_TX1, false, 
                T.SGTargetPin, T.ºSG1MaxPwr, T.ºPout1, T.tolerancePwr, T.totalInputLoss,
                T.papr_dB, T.ºRdPwr_Delay, out T.R_Pin, out T.R_Pout, out T.PwrSearch);

            //total test time for each parameter will include the soak time
            T.paramTestTime = tTime.ElapsedMilliseconds;
            if (T.paramTestTime < (long)T.ºEstimate_TestTime)
            {
                T.syncTest_Delay = (long)T.ºEstimate_TestTime - T.paramTestTime;
                T.SoakTime = T.syncTest_Delay * 1e-3;       //convert to second
            }
            else
            {
                T.SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
            }

            for (int i = 0; i < VST_Address.Length; i++)
            {
                //Calculate back the Ref level for FBAR Noise base on the Tune Power from power servo
                T.SGPowerLevel[i] = Math.Round(T.R_Pin[i] - (T.totalInputLoss[i] - T.papr_dB), 3);                
            }
            #endregion

            #region Measure FBAR Noise
            MultiSite_MultiTrace = new Broadcom.Tests.NoiseFloor.S_Multisite_TrData[TestSite];

            T.pwrServoSite.ChangePowerLevel(T.SGPowerLevel, T.Index, true);          //Abort power servo script
            Parallel.ForEach(T.nfsite, nf =>
            {
                nf.Initialize();
                nf.Configure();
                nf.Initiate();
                nf.WaitUntilMeasurementComplete();
            });

            for (int i = 0; i < T.nfsite.Length; i++)
            {
                // Get NF result
                var r = T.nfsite[i].results.noiseFloorTraces;
            }

            #endregion

            #region Sort and Store Trace Data
            //Store multi trace from PXI to global array
            for (T.rbw_counter = 0; T.rbw_counter < T.multiRBW_cnt; T.rbw_counter++)
            {
                for (int n = 0; n < T.NumberOfRuns + T.preSoakSweepTemp; n++)
                {
                    //temp trace array storage use for MAX , MIN etc calculation 

                }
            }

            #endregion
            
            //for test time checking
            tTime.Stop();
            return tTime.ElapsedMilliseconds;

        }

        private void multiSite_PowerServo(PowerServoTest[] T,
            bool doPwrServo, bool disableTune,  
            double[] refLvl, double sgMaxRefLvl, double targetPout, double tolerancePwr, double[] totalInputLoss,
            double papr_dB, int delay, out double[] tunePin, out double[] tunePout, out bool[] tuneStatus)
        {
            double[] initialrefLvl = new double[refLvl.Length];     //temp storage for reference
            bool[] doPwrTune = new bool[refLvl.Length];             //temp flag for power tuning

            tunePin = new double[refLvl.Length];
            tunePout = new double[refLvl.Length];
            tuneStatus = new bool[refLvl.Length];

            //init to fail state as default
            StopOnFail.TestFail = true;
            int passCount = 0;
            int testcounter = 0;
            initialrefLvl = refLvl;
            int maxLoop = 10;

            for (int i = 0; i < refLvl.Length; i++)
            {
                doPwrTune[i] = true;
                tuneStatus[i] = false;
                tunePin[i] = Math.Round(refLvl[i] - papr_dB + totalInputLoss[i], 3);       //set to default value
            }

            if (!doPwrServo)
            {
                T.ChangePowerLevel(refLvl, testcounter, disableTune);
                DelayMs(delay);
                tunePout = EqPwrMeter.MeasureAll();

                for (int i = 0; i < refLvl.Length; i++)
                {
                    if (Math.Abs(targetPout - tunePout[i]) <= (tolerancePwr + 3.5))
                    {
                        passCount++;
                        tuneStatus[i] = true;
                        StopOnFail.TestFail = false;
                    }

                    tunePin[i] = Math.Round(refLvl[i] - papr_dB + totalInputLoss[i], 3);
                }
            }
            else
            {
                do
                {
                    T.ChangePowerLevel(refLvl, testcounter, disableTune);
                    DelayMs(delay);
                    tunePout = EqPwrMeter.MeasureAll();

                    for (int i = 0; i < refLvl.Length; i++)
                    {
                        if (!tuneStatus[i] && doPwrTune[i])         //only calculate reflevel (i.e. do pwr tune) if tuneStatus = false/fail
                        {
                            if (Math.Abs(targetPout - tunePout[i]) >= tolerancePwr)
                            {
                                if (tunePin[i] < sgMaxRefLvl)   //preset to initial target power for 1st measurement count
                                {
                                    refLvl[i] = Math.Round(refLvl[i] + (targetPout - tunePout[i]), 3);
                                    doPwrTune[i] = true;
                                }
                                if (tunePin[i] > sgMaxRefLvl)      //if input sig gen exit limit , exit pwr search loop
                                {
                                    refLvl[i] = initialrefLvl[i];    //reset target Sig Gen to initial setting
                                    tunePin[i] = Math.Round(refLvl[i] - papr_dB + totalInputLoss[i], 3);
                                    doPwrTune[i] = false;
                                }
                            }
                            else
                            {
                                //if tunePout within Target Pout tolerance
                                tunePin[i] = Math.Round(refLvl[i] - papr_dB + totalInputLoss[i], 3);
                                doPwrTune[i] = false;           //do not tune i.e maintain sg ref level
                                tuneStatus[i] = true;           //set tune status to pass
                                passCount++;
                            }
                        }
                    }

                    if (passCount == refLvl.Length)
                    {
                        //force the loop to exit if all is pass
                        testcounter = maxLoop;
                    }

                    testcounter++;
                }
                while (testcounter < maxLoop);     // max power search loop
            }
        }


    }
}
