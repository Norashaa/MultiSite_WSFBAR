using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using Broadcom.Tests;

namespace MyProduct
{
    public class MyWS_PXI_RXPATH_CONTACT_Test : MyDUT_WS_Test
    {
        public FrequencyResponseTest[] freqRespSite;

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
        /// Constructor of WS_PXI_RXPATH_CONTACT test. Base constructor will be called prior to this.
        /// </summary>
        /// <param name="MyDUT"></param>
        /// <param name="TestPara"></param>
        public MyWS_PXI_RXPATH_CONTACT_Test(MyDUT_WS MyDUT, Dictionary<string, string> TestPara)
            : base(MyDUT, TestPara)
        {
            MyDUT.PreConfigureTest(this, TestPara);
        }

        /// <summary>
        /// Execute test. Call this from RunTest
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
        /// Pre-configuration particular for WS_PXI_RXPATH_CONTACT test
        /// </summary>
        /// <param name="T"></param>
        /// <param name="TestPara"></param>
        public void PreConfigureTest(MyWS_PXI_RXPATH_CONTACT_Test T, Dictionary<string, string> TestPara)
        {
            //this function is checking the pathloss/pathgain from antenna port to rx port

            #region PXI_RXPATH_CONTACT
            T.NoOfPts = (Convert.ToInt32((T.ºStopRXFreq1 - T.ºStartRXFreq1) / T.ºStepRXFreq1)) + 1;

            T.RXContactdBm = new double[T.NoOfPts];
            T.RXContactFreq = new double[T.NoOfPts];

            T.SGTargetPin = new double[TestSite];

            #region Decode Calibration Path and Segment Data
            T.CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", T.ºSwBand.ToUpper()); myUtility.Decode_CalSegm_Setting(T.CalSegmData);
            myUtility.Decode_CalSegm_Setting(T.CalSegmData);
            #endregion

            #region Pathloss Offset

            //Calculate PAPR offset for PXI SG
            EqVST.Get_s_SignalType(T.ºModulation, T.ºWaveFormName, out EqVST.SignalType);
            T.papr_dB = EqVST.SignalType.SG_papr_dB;

            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
            T.tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
            T.tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

            //Get average pathloss base on start and stop freq
            T.count = Convert.ToInt16((T.ºStopRXFreq1 - T.ºStartRXFreq1) / T.ºStepRXFreq1);
            T.ºRXFreq = T.ºStartRXFreq1;
            for (int i = 0; i <= T.count; i++)
            {
                T.RXContactFreq[i] = T.ºRXFreq;
                multiSite_Pathloss(TestSite, LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, T.ºRXFreq, ref T.ºLossOutputPathRX1, ref T.StrError);
                multiSite_Pathloss(TestSite, LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, T.ºRXFreq, ref T.ºLossCouplerPath, ref T.StrError);
                for (int j = 0; j < TestSite; j++)
                {
                    T.tmpRxLoss[j] = Math.Round(T.tmpRxLoss[j] + (float)T.ºLossOutputPathRX1[j], 3);   //need to use round function because of C# float and double floating point bug/error
                    T.tmpCouplerLoss[j] = Math.Round(T.tmpCouplerLoss[j] + (float)T.ºLossCouplerPath[j], 3);   //need to use round function because of C# float and double floating point bug/error
                }

                T.ºRXFreq = Convert.ToSingle(Math.Round(T.ºRXFreq + T.ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
            }
            for (int j = 0; j < TestSite; j++)
            {
                T.tmpAveRxLoss[j] = T.tmpRxLoss[j] / (T.count + 1);
                T.tmpAveCouplerLoss[j] = T.tmpCouplerLoss[j] / (T.count + 1);
                T.totalInputLoss[j] = T.tmpAveCouplerLoss[j] - T.tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                T.totalOutputLoss[j] = T.tmpAveRxLoss[j] - T.tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                //Find actual SG Power Level
                T.SGTargetPin[j] = T.ºPin1 - (T.totalInputLoss[j] - T.papr_dB);
                if (T.SGTargetPin[j] > T.ºSG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                {
                    return;
                }
            }

            #region Decode MXA Config
            T.MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", T.ºSwBand.ToUpper());
            myUtility.Decode_MXA_Setting(T.MXA_Config);

            T.SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
            T.vBW_Hz = myUtility.MXA_Setting.VBW;
            #endregion

            #endregion

            #region Test RX Path
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
                nfconfig[i].Band = T.ºRX1Band;
                nfconfig[i].Modulation = T.ºModulation;
                nfconfig[i].WaveformName = T.ºWaveFormName;

                nfconfig[i].DwellTime = T.ºDwellT1 / 1000;
                nfconfig[i].RXFrequencyStart = T.ºStartRXFreq1 * 1e6;
                nfconfig[i].RXFrequencyStop = T.ºStopRXFreq1 * 1e6;
                nfconfig[i].RXFrequencyStep = T.ºStepRXFreq1 * 1e6;

                nfconfig[i].SGPowerLevel = T.SGTargetPin[i];
                nfconfig[i].SAReferenceLevel = T.SAReferenceLevel;
                nfconfig[i].Vbw = T.vBW_Hz;
                nfconfig[i].Rbw = T.RBW_Hz;

                nfconfig[i].Bandwidths = new double[T.multiRBW_cnt];
                nfconfig[i].Bandwidths = T.multiRBW_Hz;
            }
            #endregion

            //Config RX Path
            T.freqRespSite = EqVST.BuildContactTest(nfconfig);

            #endregion
            
            #endregion
        }

        /// <summary>
        /// Execution of WS_PXI_RXPATH_CONTACT test
        /// </summary>
        /// <param name="T"></param>
        /// <param name="results"></param>
        public long ExecuteTest(MyWS_PXI_RXPATH_CONTACT_Test T)
        {
            Stopwatch tTime = new Stopwatch();

            tTime.Reset();
            tTime.Start();

            #region Test RX Path
            //Measure RX Path
            Parallel.ForEach(T.freqRespSite, site =>
                {
                    site.Initialize();
                    site.Configure();
                    site.Initiate();
                    site.WaitUntilMeasurementComplete();
                });

            //Sort out test result
            switch (T.ºSearch_Method.ToUpper())
            {
                case "MAX":
                    //R_NF1_Ampl = RXContactdBm.Max();
                    //R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactdBm, R_NF1_Ampl)];
                    break;

                case "MIN":
                    //R_NF1_Ampl = RXContactdBm.Min();
                    //R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactdBm, R_NF1_Ampl)];
                    break;

                case "AVE":
                case "AVERAGE":
                    //R_NF1_Ampl = RXContactdBm.Average();
                    //R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                    break;

                case "USER":
                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                    if ((T.ºSearch_Value >= T.ºStartRXFreq1) && (T.ºSearch_Value <= T.ºStopRXFreq1))
                    {
                        try
                        {
                            //R_NF1_Ampl = RXContactdBm[Array.IndexOf(RXContactFreq, ºSearch_Value)];     //return contact power from same array number(of index number associated with 'USER' Freq)
                            //R_NF1_Freq = ºSearch_Value;
                        }
                        catch       //if ºSearch_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                        {
                            //R_NF1_Freq = ºSearch_Value;
                            //R_NF1_Ampl = 99999;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Test Parameter : " + T.ºTestParam + "(SEARCH METHOD : " + T.ºSearch_Method + ", USER DEFINE : " + T.ºSearch_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    }
                    break;

                default:
                    MessageBox.Show("Test Parameter : " + T.ºTestParam + "(" + T.ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;
            }

            //R_NF1_Ampl = (R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss) - ºPin1;      //return DUT only pathgain/loss result while excluding pathloss cal
            #endregion

            //for test time checking
            tTime.Stop();
            return tTime.ElapsedMilliseconds;
        }


    }
}
