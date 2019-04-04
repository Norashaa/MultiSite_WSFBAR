using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Avago.ATF.StandardLibrary;
using Broadcom.Tests;
using System.Diagnostics;

namespace MyProduct
{
    public abstract class MyDUT_WS_Test
    {
        /// <summary>
        /// A reference copy of caller class MyDUT_WS for internal used
        /// </summary>
        protected MyDUT_WS mydut;

        #region TCF Setting
        //Read TCF Setting
        public string StrError = string.Empty;

        public int ºTestNum;
        public string ºTestMode;
        public string ºTestParam;
        public string ºTestParaName;
        public string ºTestUsePrev;

        //Single Freq Condition
        public float ºTXFreq;
        public float ºRXFreq;
        public float ºPout;
        public float ºPin;
        public string ºTXBand;
        public string ºRXBand;
        public bool ºTunePwr_TX;

        //Sweep TX1/RX1 Freq Condition
        public float ºPout1;
        public float ºPin1;
        public float ºStartTXFreq1;
        public float ºStopTXFreq1;
        public float ºStepTXFreq1;
        public float ºDwellT1;
        public bool ºTunePwr_TX1;

        public float ºStartRXFreq1;
        public float ºStopRXFreq1;
        public float ºStepRXFreq1;
        public float ºRX1SweepT;

        public string ºTX1Band;
        public string ºRX1Band;
        public bool ºSetRX1NDiag;

        //Sweep TX2/RX2 Freq Condition
        public float ºPout2;
        public float ºPin2;
        public float ºStartTXFreq2;
        public float ºStopTXFreq2;
        public float ºStepTXFreq2;
        public float ºDwellT2;
        public bool ºTunePwr_TX2;

        public float ºStartRXFreq2;
        public float ºStopRXFreq2;
        public float ºStepRXFreq2;
        public float ºRX2SweepT;

        public string ºTX2Band;
        public string ºRX2Band;
        public bool ºSetRX2NDiag;

        //Misc
        public string ºPXI_MultiRBW;
        public int ºPXI_NoOfSweep;
        public string ºPoutTolerance;
        public string ºPinTolerance;
        public string ºPowerMode;
        public string ºCalTag;
        public string ºSwBand;
        public string ºModulation;
        public string ºWaveFormName;

        //Read TCF SMU Setting
        public float[] ºSMUVCh;
        public float[] ºSMUILimitCh;

        public string ºSMUSetCh;
        public string ºSMUMeasCh;

        //Read TCF DC Setting
        public float[] ºDCVCh;
        public float[] ºDCILimitCh;

        public string ºDCSetCh;
        public string ºDCMeasCh;

        //Read Set Equipment Flag
        public bool ºSetSA1;
        public bool ºSetSA2;
        public bool ºSetSG1;
        public bool ºSetSG2;
        public bool ºSetSMU;

        //Read Off State Flag
        public bool ºOffSG1;
        public bool ºOffSG2;
        public bool ºOffSMU;
        public bool ºOffDC;

        //Read Require test parameter
        public bool ºTest_Pin;
        public bool ºTest_Pout;
        public bool ºTest_Pin1;
        public bool ºTest_Pout1;
        public bool ºTest_Pin2;
        public bool ºTest_Pout2;
        public bool ºTest_NF1;
        public bool ºTest_NF2;
        public bool ºTest_MXATrace;
        public bool ºTest_MXATraceFreq;
        public bool ºTest_Harmonic;
        public bool ºTest_IMD;
        public bool ºTest_MIPI;
        public bool ºTest_SMU;
        public bool ºTest_DCSupply;
        public bool ºTest_Switch;
        public bool ºTest_TestTime;

        //Read SA & SG setting
        public string ºSA1att;
        public string ºSA2att;
        public double ºSG1MaxPwr;
        public double ºSG2MaxPwr;
        public double ºSG1_DefaultFreq;
        public double ºSG2_DefaultFreq;
        public double ºPXI_Multiplier_RXIQRate;

        //Read Delay Setting
        public int ºTrig_Delay;
        public int ºGeneric_Delay;
        public int ºRdCurr_Delay;
        public int ºRdPwr_Delay;
        public int ºSetup_Delay;
        public int ºStartSync_Delay;
        public int ºStopSync_Delay;
        public int ºEstimate_TestTime;

        //Misc Setting
        public string ºSearch_Method;
        public float ºSearch_Value;
        public bool ºInterpolation;
        public bool ºAbs_Value;
        public bool ºSave_MXATrace;

        public string[] SetSMU;
        public string[] MeasSMU;
        public double[] R_SMU_ICh;
        public string[] R_SMULabel_ICh;

        public string[] SetDC;
        public string[] MeasDC;
        public double[] R_DC_ICh;
        public string[] R_DCLabel_ICh;
        public string[] SetSMUSelect;

        public bool MIPI_Read_Successful = false;


        #endregion

        #region Test Variable
        //Test Variable
        public bool status = false;

        public int tx1_noPoints = 0;
        public float tx1_span = 0;
        public double rx1_span = 0;
        public double rx1_cntrfreq = 0;
        public double rx2_span = 0;
        public double rx2_cntrfreq = 0;
        public double tolerancePwr = 0;

        public int Index = 0;
        public int NoOfPts;
        public double[] RXContactdBm;
        public double[] RXContactFreq;

        //mxa#1 and mxa#2 setting variable
        public int rx1_mxa_nopts = 0;
        public double rx1_mxa_nopts_step = 0.1;        //step 0.1MHz , example mxa_nopts (601) , every points = 0.1MHz
        public int rx2_mxa_nopts = 0;
        public double rx2_mxa_nopts_step = 0.1;        //step 0.1MHz , example mxa_nopts (601) , every points = 0.1MHz

        public string CalSegmData = null;
        public double tbInputLoss = 0;
        public double tbOutputLoss = 0;
        public string MXA_Config = null;
        public int markerNo = 1;

        public int count;
        public long paramTestTime = 0;
        public long syncTest_Delay = 0;
        public decimal trigDelay = 0;

        //COMMON case variable
        public int resultTag;
        public int arrayVal;
        public double result;
        public bool usePrevRslt = false;
        public double prevRslt = 0;

        //VST Variable            
        public double SG_IQRate = 0;
        public double papr_dB = 0;

        #region Misc Variable
        public float dummyData;
        public int bw_cnt = 0;
        public int multiRBW_cnt;
        public int rbw_counter;
        public double[] tmpRBW_Hz;
        public double[] multiRBW_Hz;
        #endregion

        #endregion

        #region Result Variable
        public double[] R_NF1_Ampl;
        public double[] R_NF2_Ampl;
        public double[] R_NF1_Freq;
        public double[] R_NF2_Freq;
        public double[] R_H2_Ampl;
        public double[] R_H2_Freq;
        public double[] R_Pin;
        public double[] R_Pout;
        public double[] R_ITotal;
        public double[] R_MIPI;
        public double[] R_DCSupply;
        public double[] R_Switch;
        public double[] R_RFCalStatus;

        protected long ElapsedMilliseconds = 0;
        #endregion

        #region Misc Variable
        public double [] SGTargetPin;           //Global variable for SG input power tuning
        public double[] SGPowerLevel;          //Global variable for SG input power testing
        public bool[] PwrSearch; 
        
        #endregion

        #region Cal Factor
        //Load cal factor
        public double[] ºLossCouplerPath ;
        public double[] ºLossOutputPathRX1 ;
        public double[] ºLossOutputPathRX2 ;
        public double[] ºLossInputPathSG1 ;
        public double[] ºLossInputPathSG2 ;

        public double[] tmpInputLoss ;
        public double[] tmpCouplerLoss ;
        public double[] tmpAveInputLoss ;
        public double[] tmpAveCouplerLoss ;
        public double[] tmpRxLoss ;
        public double[] tmpAveRxLoss ;
        public double[] normalizeCPLLoss;

        public double[] totalInputLoss ;      //Input Pathloss + Testboard Loss
        public double[] totalOutputLoss ;     //Output Pathloss + Testboard Loss
        #endregion

        /// <summary>
        /// Construction of new test. Pre-configuration of new test is done here.
        /// This parent constructor implement the pre-configuration common to all tests
        /// </summary>
        /// <param name="MyDUT"></param>
        /// <param name="TestPara"></param>
        public MyDUT_WS_Test(MyDUT_WS MyDUT, Dictionary<string, string> TestPara)
        {
            mydut = MyDUT;
            MyDUT.PreConfigureTest(this, TestPara);            
        }

        
        /// <summary>
        /// Execute test 
        /// </summary>
        /// <param name="results"></param>
        public abstract void Initiate();

        /// <summary>
        /// Build results
        /// </summary>
        /// <param name="results"></param>
        public void BuildResult(ref ATFReturnResult results)
        {
            if (ºTest_TestTime)
            {
                mydut.BuildResults(ref results, ºTestParaName + "_TestTime", "mS", ElapsedMilliseconds);
            }
            
            mydut.BuildResult(this);
        }

    }
    


    public partial class MyDUT_WS
    {
        /// <summary>
        /// Pre-configuration of a test. This configuration is common for all tests
        /// </summary>
        /// <param name="T"></param>
        /// <param name="TestPara"></param>
        public void PreConfigureTest(MyDUT_WS_Test T, Dictionary<string, string> TestPara)
        {
            #region Read TCF Setting
            //Read TCF Setting

            string StrError = string.Empty;

            T.ºTestNum = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestNum));       //use as array number for data store
            T.ºTestMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestMode);
            T.ºTestParam = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestParam);
            T.ºTestParaName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstParaName);
            T.ºTestUsePrev = myUtility.ReadTcfData(TestPara, TCF_Header.ConstUsePrev);

            //Single Freq Condition
            T.ºTXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXFreq));
            T.ºRXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXFreq));
            T.ºPout = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout));
            T.ºPin = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin));
            T.ºTXBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXBand).ToUpper();
            T.ºRXBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXBand).ToUpper();
            T.ºTunePwr_TX = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX).ToUpper() == "V" ? true : false);

            //Sweep TX1/RX1 Freq Condition
            T.ºPout1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout1));
            T.ºPin1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin1));
            T.ºStartTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq1));
            T.ºStopTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq1));
            T.ºStepTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq1));
            T.ºDwellT1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDwellTime1));
            T.ºTunePwr_TX1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX1).ToUpper() == "V" ? true : false);

            T.ºStartRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq1));
            T.ºStopRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq1));
            T.ºStepRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq1));
            T.ºRX1SweepT = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1SweepT));

            T.ºTX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX1Band).ToUpper();
            T.ºRX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1Band).ToUpper();
            T.ºSetRX1NDiag = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetRX1NDiag).ToUpper() == "V" ? true : false);

            //Sweep TX2/RX2 Freq Condition
            T.ºPout2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout2));
            T.ºPin2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin2));
            T.ºStartTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq2));
            T.ºStopTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq2));
            T.ºStepTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq2));
            T.ºDwellT2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDwellTime2));
            T.ºTunePwr_TX2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX2).ToUpper() == "V" ? true : false);

            T.ºStartRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq2));
            T.ºStopRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq2));
            T.ºStepRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq2));
            T.ºRX2SweepT = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX2SweepT));

            T.ºTX2Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX2Band).ToUpper();
            T.ºRX2Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX2Band).ToUpper();
            T.ºSetRX2NDiag = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetRX2NDiag).ToUpper() == "V" ? true : false);

            //Misc
            T.ºPXI_MultiRBW = myUtility.ReadTcfData(TestPara, TCF_Header.PXI_MultiRBW);
            T.ºPXI_NoOfSweep = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.PXI_NoOfSweep));
            T.ºPoutTolerance = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPoutTolerance);
            T.ºPinTolerance = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPinTolerance);
            T.ºPowerMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPowerMode);
            T.ºCalTag = myUtility.ReadTcfData(TestPara, TCF_Header.ConstCalTag);
            T.ºSwBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwBand).ToUpper();
            T.ºModulation = myUtility.ReadTcfData(TestPara, TCF_Header.ConstModulation).ToUpper();
            T.ºWaveFormName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstWaveformName).ToUpper();

            //Read TCF SMU Setting
            T.ºSMUVCh = new float[9];
            T.ºSMUILimitCh = new float[9];

            T.ºSMUSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUSetCh);
            T.ºSMUMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUMeasCh);
            T.ºSMUVCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh0));
            T.ºSMUVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh1));
            T.ºSMUVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh2));
            T.ºSMUVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh3));
            T.ºSMUVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh4));
            T.ºSMUVCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh5));
            T.ºSMUVCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh6));
            T.ºSMUVCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh7));
            T.ºSMUVCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh8));

            T.ºSMUILimitCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh0Limit));
            T.ºSMUILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh1Limit));
            T.ºSMUILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh2Limit));
            T.ºSMUILimitCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh3Limit));
            T.ºSMUILimitCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh4Limit));
            T.ºSMUILimitCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh5Limit));
            T.ºSMUILimitCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh6Limit));
            T.ºSMUILimitCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh7Limit));
            T.ºSMUILimitCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh8Limit));

            //Read TCF DC Setting
            T.ºDCVCh = new float[5];
            T.ºDCILimitCh = new float[5];

            T.ºDCSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCSetCh);
            T.ºDCMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCMeasCh);
            T.ºDCVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh1));
            T.ºDCVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh2));
            T.ºDCVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh3));
            T.ºDCVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh4));
            T.ºDCILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh1Limit));
            T.ºDCILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh2Limit));
            T.ºDCILimitCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh3Limit));
            T.ºDCILimitCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh4Limit));

            //Read Set Equipment Flag
            T.ºSetSA1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSA1).ToUpper() == "V" ? true : false);
            T.ºSetSA2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSA2).ToUpper() == "V" ? true : false);
            T.ºSetSG1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSG1).ToUpper() == "V" ? true : false);
            T.ºSetSG2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSG2).ToUpper() == "V" ? true : false);
            T.ºSetSMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSMU).ToUpper() == "V" ? true : false);

            //Read Off State Flag
            T.ºOffSG1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSG1).ToUpper() == "V" ? true : false);
            T.ºOffSG2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSG2).ToUpper() == "V" ? true : false);
            T.ºOffSMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSMU).ToUpper() == "V" ? true : false);
            T.ºOffDC = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffDC).ToUpper() == "V" ? true : false);

            //Read Require test parameter
            T.ºTest_Pin = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin).ToUpper() == "V" ? true : false);
            T.ºTest_Pout = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout).ToUpper() == "V" ? true : false);
            T.ºTest_Pin1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin1).ToUpper() == "V" ? true : false);
            T.ºTest_Pout1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout1).ToUpper() == "V" ? true : false);
            T.ºTest_Pin2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin2).ToUpper() == "V" ? true : false);
            T.ºTest_Pout2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout2).ToUpper() == "V" ? true : false);
            T.ºTest_NF1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF1).ToUpper() == "V" ? true : false);
            T.ºTest_NF2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF2).ToUpper() == "V" ? true : false);
            T.ºTest_MXATrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MXATrace).ToUpper() == "V" ? true : false);
            T.ºTest_MXATraceFreq = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MXATraceFreq).ToUpper() == "V" ? true : false);
            T.ºTest_Harmonic = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Harmonic).ToUpper() == "V" ? true : false);
            T.ºTest_IMD = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_IMD).ToUpper() == "V" ? true : false);
            T.ºTest_MIPI = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MIPI).ToUpper() == "V" ? true : false);
            T.ºTest_SMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_SMU).ToUpper() == "V" ? true : false);
            T.ºTest_DCSupply = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_DCSupply).ToUpper() == "V" ? true : false);
            T.ºTest_Switch = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Switch).ToUpper() == "V" ? true : false);
            T.ºTest_TestTime = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_TestTime).ToUpper() == "V" ? true : false);

            //Read SA & SG setting
            T.ºSA1att = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSA1att);
            T.ºSA2att = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSA2att);
            T.ºSG1MaxPwr = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG1MaxPwr));
            T.ºSG2MaxPwr = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG2MaxPwr));
            T.ºSG1_DefaultFreq = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG1_DefaultFreq));
            T.ºSG2_DefaultFreq = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG2_DefaultFreq));
            T.ºPXI_Multiplier_RXIQRate = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMultiplier_RXIQRate));

            //Read Delay Setting
            T.ºTrig_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTrig_Delay));
            T.ºGeneric_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstGeneric_Delay));
            T.ºRdCurr_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRdCurr_Delay));
            T.ºRdPwr_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRdPwr_Delay));
            T.ºSetup_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetup_Delay));
            T.ºStartSync_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartSync_Delay));
            T.ºStopSync_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopSync_Delay));
            T.ºEstimate_TestTime = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstEstimate_TestTime));

            //Misc Setting
            T.ºSearch_Method = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Method).ToUpper();
            T.ºSearch_Value = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Value));
            T.ºInterpolation = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstInterpolation).ToUpper() == "V" ? true : false);
            T.ºAbs_Value = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstAbs_Value).ToUpper() == "V" ? true : false);
            T.ºSave_MXATrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSave_MXATrace).ToUpper() == "V" ? true : false);

            T.R_SMU_ICh = new double[9];
            T.R_SMULabel_ICh = new string[9];

            T.R_DC_ICh = new double[5];
            T.R_DCLabel_ICh = new string[5];

            bool MIPI_Read_Successful = false;

            ////temp result storage use for MAX , MIN etc calculation 
            //Results[TestCount].Multi_Results = new s_mRslt[15];      //default to 15 , need to check total enum of e_ResultTag
            //Results[TestCount].TestNumber = ºTestNum;
            //Results[TestCount].Enable = true;

            #endregion

            //Initialize result & pathloss data before every test cycle (done when Test initialized)            
            InitResultVariable(T, TestSite);
            InitCalFactor(T, TestSite);
            InitMiscVariable(T, TestSite);
        }

        private void InitResultVariable(MyDUT_WS_Test T, int testSite)
        {
            T.R_NF1_Ampl = new double[testSite];
            T.R_NF2_Ampl = new double[testSite];
            T.R_NF1_Freq = new double[testSite];
            T.R_NF2_Freq = new double[testSite];
            T.R_H2_Ampl = new double[testSite];
            T.R_H2_Freq = new double[testSite];
            T.R_Pin = new double[testSite];
            T.R_Pout = new double[testSite];
            T.R_ITotal = new double[testSite];
            T.R_MIPI = new double[testSite];
            T.R_DCSupply = new double[testSite];
            T.R_Switch = new double[testSite];
            T.R_RFCalStatus = new double[testSite];

            for (int i = 0; i < testSite; i++)
            {
                T.R_NF1_Ampl[i] = 99999;
                T.R_NF2_Ampl[i] = 99999;
                T.R_NF1_Freq[i] = -99999;
                T.R_NF2_Freq[i] = -99999;
                T.R_H2_Ampl[i] = 99999;
                T.R_H2_Freq[i] = -99999;
                T.R_Pin[i] = 99999;
                T.R_Pout[i] = -99999;
                T.R_ITotal[i] = 99999;
                T.R_MIPI[i] = -99999;
                T.R_DCSupply[i] = 99999;
                T.R_Switch[i] = -99999;
                T.R_RFCalStatus[i] = -99999;
            }
        }
        private void InitCalFactor(MyDUT_WS_Test T, int testSite)
        {
            T.ºLossCouplerPath = new double[testSite];
            T.ºLossOutputPathRX1 = new double[testSite];
            T.ºLossOutputPathRX2 = new double[testSite];
            T.ºLossInputPathSG1 = new double[testSite];
            T.ºLossInputPathSG2 = new double[testSite];

            T.tmpInputLoss = new double[testSite];
            T.tmpCouplerLoss = new double[testSite];
            T.tmpAveInputLoss = new double[testSite];
            T.tmpAveCouplerLoss = new double[testSite];
            T.tmpRxLoss = new double[testSite];
            T.tmpAveRxLoss = new double[testSite];
            T.normalizeCPLLoss = new double[testSite];

            T.totalInputLoss = new double[testSite];
            T.totalOutputLoss = new double[testSite];

            for (int i = 0; i < testSite; i++)
            {
                T.ºLossCouplerPath[i] = 0;
                T.ºLossOutputPathRX1[i] = 0;
                T.ºLossOutputPathRX2[i] = 0;
                T.ºLossInputPathSG1[i] = 0;
                T.ºLossInputPathSG2[i] = 0;

                T.tmpInputLoss[i] = 0;
                T.tmpCouplerLoss[i] = 0;
                T.tmpAveInputLoss[i] = 0;
                T.tmpAveCouplerLoss[i] = 0;
                T.tmpRxLoss[i] = 0;
                T.tmpAveRxLoss[i] = 0;
                T.normalizeCPLLoss[i] = 0;

                T.totalInputLoss[i] = 0;
                T.totalOutputLoss[i] = 0;
            }
        }
        private void InitMiscVariable(MyDUT_WS_Test T, int testSite)
        {
            T.SGTargetPin = new double[testSite];
            T.PwrSearch = new bool[testSite];
            T.SGPowerLevel = new double[testSite];
            for (int i = 0; i < testSite; i++)
            {
                T.SGTargetPin[i] = -99999;
                T.SGPowerLevel[i] = -99999;
                T.PwrSearch[i] = false;
            }
        }
        


        /// <summary>
        /// Build result stage common to all tests
        /// </summary>
        /// <param name="T"></param>
        public void BuildResult(MyDUT_WS_Test T)
        {
            #region Build Result
            if (T.ºTest_Pin)
            {
                //BuildResults(ref results, ºTestParaName + "_Pin", "dBm", R_Pin);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Header = T.ºTestParaName + "_Pin";
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Data = T.R_Pin[0];
            }
            if (T.ºTest_Pout)
            {
                //BuildResults(ref results, ºTestParaName + "_Pout", "dBm", R_Pout);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Header = T.ºTestParaName + "_Pout";
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Data = T.R_Pout[0];
            }
            #endregion
        }
    }

}
