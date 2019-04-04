using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Avago.ATF.StandardLibrary;
using Ivi.Visa.Interop;
using EqmtLibrary;
using Broadcom.Tests;

namespace MyProduct
{
    public partial class MyDUT_WS
    {
        #region Driver Declaration
        public static IO_TextFile IO_TxtFile = new IO_TextFile();
        MyUtility myUtility = new MyUtility();
        Stopwatch Speedo = new Stopwatch();

        Dictionary<string, string>[] DicTestPA;
        Dictionary<string, string> DicCalInfo;
        Dictionary<string, string> DicWaveForm;
        Dictionary<string, string> DicWaveFormMutate;
        Dictionary<string, string> DicTestLabel;
        Dictionary<string, string>[] DicMipiKey;

        List<string> FileContains = new List<string>();

        #endregion

        #region Instrument Static Variable
        public static s_EqmtStatus EqmtStatus;
        public static s_OffBias_AfterTest BiasStatus;

        public static int TestSite = 2;            //Max test site = 2
        public static int TotalDCSupply = 4;       //max DC Supply 1 Channel is 4 (equal 4 channel in tcf)

        public string[] VST_Address = new string[TestSite];
        public string[] PwrMeter_Address = new string[TestSite];
        public string[] DCSupply_Address = new string[TestSite];

        EqmtLibrary.NI5646R.VST_NI5646R EqVST;
        EqmtLibrary.RSNRPZ.cRSNRPZ EqPwrMeter;

        #endregion

        #region Control Flag
        string Previous_SWMode = "";
        string Previous_MXAMode = "";
        static bool FirstDut = true;
        #endregion

        #region Result Variable
        public s_SNPFile SNPFile;
        public s_SNPDatalog SNPDatalog;
        public s_StopOnFail StopOnFail;

        public s_Result[] Results;
        public int TestCount;

        #endregion

        #region Misc Variable
        public Broadcom.Tests.NoiseFloor.S_Multisite_TrData[] MultiSite_MultiTrace;

        #endregion

        /// <summary>
        /// List of DUT tests arrange in sequence
        /// </summary>
        List<MyDUT_WS_Test> DutTests;
        

        public MyDUT_WS(ref StringBuilder sb)
        {
            Init(ref sb);
        }
        ~MyDUT_WS()
        {
            UnInit();
        }

        #region Run Testing - Main
        /// <summary>
        /// Call this to run the tests which initialized in the DutTests list.
        /// </summary>
        /// <param name="results"></param>
        public void RunTest(ref ATFReturnResult results)
        {
            Results = new s_Result[DicTestPA.Length];
            //MXATrace = new s_TraceData[DicTestPA.Length];
            //PXITrace = new s_TraceData[DicTestPA.Length];

            string StrError = string.Empty;
            long TestTimeFBar, TestTimePA;

            TestCount = 0; //reset to start
            Speedo.Reset();
            Speedo.Start();
            StopOnFail.TestFail = false;

            foreach (var t in DutTests)
            {
                t.Initiate();
                t.BuildResult(ref results);
                TestCount++;

                //ATFResultBuilder.AddResultToDict("aaa", 12, ref StrError);              
            }

            Speedo.Stop();
            TestTimePA = Speedo.ElapsedMilliseconds;

            ATFResultBuilder.AddResult(ref results, "PATestTime", "mS", TestTimePA);
            ATFResultBuilder.AddResult(ref results, "TotalTestTime", "mS", TestTimePA);
        }
        
        #endregion

        private void Init(ref StringBuilder sb)
        {
            #region Load TCF
            ManualResetEvent[] DoneEvents = new ManualResetEvent[4];
            DoneEvents[0] = new ManualResetEvent(false);
            DoneEvents[1] = new ManualResetEvent(false);
            DoneEvents[2] = new ManualResetEvent(false);
            DoneEvents[3] = new ManualResetEvent(false);

            ThreadWithDelegate ThLoadPaTCF = new ThreadWithDelegate(DoneEvents[0]);
            ThLoadPaTCF.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadPaTCF);
            ThreadPool.QueueUserWorkItem(ThLoadPaTCF.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadWaveForm = new ThreadWithDelegate(DoneEvents[1]);
            ThLoadWaveForm.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadWafeForm);
            ThreadPool.QueueUserWorkItem(ThLoadWaveForm.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadCalTCF = new ThreadWithDelegate(DoneEvents[2]);
            ThLoadCalTCF.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadCalTCF);
            ThreadPool.QueueUserWorkItem(ThLoadCalTCF.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadMipiReg = new ThreadWithDelegate(DoneEvents[3]);
            ThLoadMipiReg.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadMipiReg);
            ThreadPool.QueueUserWorkItem(ThLoadMipiReg.ThreadPoolCallback, 0);

            WaitHandle.WaitAll(DoneEvents);

            #endregion

            #region Retrieve Cal Sheet Info
            string CalFilePath = Convert.ToString(DicCalInfo[DataFilePath.CalPathRF]);
            string LocSetFilePath = Convert.ToString(DicCalInfo[DataFilePath.LocSettingPath]);
            #endregion

            #region Read Local Setting File

            string CalEnable = myUtility.ReadTextFile(LocSetFilePath, LocalSetting.HeaderFilePath, LocalSetting.keyCalEnable);

            //Read & Set DC & SMU biasing status - OFF/ON for every DUT after complete test
            BiasStatus.DC = Convert.ToBoolean(myUtility.ReadTextFile(LocSetFilePath, "OFF_AfterTest", "DC"));
            BiasStatus.SMU = Convert.ToBoolean(myUtility.ReadTextFile(LocSetFilePath, "OFF_AfterTest", "SMU"));

            //Read Stop On Failure status mode - True (program will stop testing if failure happen) , false (proceed per normal)
            StopOnFail.TestFail = false;      //init to default 
            StopOnFail.Enable = Convert.ToBoolean(myUtility.ReadTextFile(LocSetFilePath, "STOP_ON_FAIL", "ENABLE"));

            #endregion

            #region Instrument Init

            InstrInit(LocSetFilePath);

            #endregion

            #region Load Cal

            try
            {
                ATFCrossDomainWrapper.Cal_SwitchInterpolationFlag(true);
                ATFCrossDomainWrapper.Cal_LoadCalData(LocalSetting.CalTag, CalFilePath);
            }
            catch (Exception ex)
            {
                if (DicTestPA[0].ContainsValue("Calibration"))
                {
                    //Do Nothing
                }
                else
                {
                    sb.AppendFormat("Fail to Load 1D Cal Data from {0}: {1}\n", CalFilePath, ex.Message);
                }
            }

            #endregion
            
            #region Initialize WS Dut Tests
            
            TestsInit(); 
            
            #endregion
        }

        private void UnInit()
        {
            var processes = from p in System.Diagnostics.Process.GetProcessesByName("EXCEL") select p;

            foreach (var process in processes)
            {
                // All those background un-release process will be closed
                if (process.MainWindowTitle == "")
                    process.Kill();
            }

            InstrUnInit();
            TestsUnInit();
        }


        #region Instrument Init/Uninit  
        private void InstrInit(string LocSetFilePath)
        {
            #region Switch Init
            string SWmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "Switch");
            string SWaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "Switch");

            switch (SWmodel.ToUpper())
            {
                case "3499A":
                    EqmtStatus.Switch = true;
                    break;
                case "AEM_WOLFER":
                    EqmtStatus.Switch = true;
                    break;
                case "SW_NI6509":
                    EqmtStatus.Switch = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.Switch = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment SWITCH Model : " + SWmodel.ToUpper(), "Pls ignore if Equipment Switch not require.");
                    EqmtStatus.Switch = false;
                    break;
            }
            #endregion

            #region Multiple 1CH DC Supply
            //This initilaization will also work with a single 4 Channel Power Supply like N6700B
            //For example N6700B, all address will be same. Software will create 4 instance for each channel

            EqmtStatus.DCSupply = new bool[TotalDCSupply];

            for (int i = 0; i < TotalDCSupply; i++)
            {
                string DCSupplymodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "DCSUPPLY0" + (i + 1));
                string DCSupplyaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "DCSUPPLY0" + (i + 1));

                switch (DCSupplymodel.ToUpper())
                {
                    case "E3633A":
                    case "E3644A":
                        EqmtStatus.DCSupply[i] = true;
                        break;
                    case "N6700B":
                        EqmtStatus.DCSupply[i] = true;
                        break;
                    case "NONE":
                    case "NA":
                        EqmtStatus.DCSupply[i] = false;
                        // Do Nothing , equipment not present
                        break;
                    default:
                        MessageBox.Show("Equipment DC Supply Model(DCSUPPLY0" + (i + 1) + ") : " + DCSupplymodel.ToUpper(), "Pls ignore if Equipment DC not require.");
                        EqmtStatus.DCSupply[i] = false;
                        break;
                }
            }
            #endregion

            #region VST Init
            string VSTmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "PXI_VST");
            string VSTaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "PXI_VST");

            for (int i = 0; i < VST_Address.Length; i++)
            {
                VST_Address[i] = "VST_0" + (i + 1).ToString();           //VST address format must be in this "VST_01" or "VST_02" etc ..
            }

            switch (VSTmodel.ToUpper())
            {
                case "NI5646R":
                case "PXIE-5646R":
                    EqVST = new EqmtLibrary.NI5646R.VST_NI5646R(VST_Address);
                    //EqVST.Initialize();
                    foreach (string key in DicWaveForm.Keys)
                    {
                        EqVST.Mod_FormatCheck(key.ToString(), DicWaveForm[key].ToString(), DicWaveFormMutate[key].ToString(), true);
                    }                    
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.PXI_VST = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment PXI VST Model : " + VSTmodel.ToUpper(), "Pls ignore if Equipment PXI_VST not required");
                    EqmtStatus.TuneFilter = false;
                    break;
            }
            #endregion

            #region Power Sensor Init
            string PMmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "PWRMETER");
            string PMaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "PWRMETER");

            //temporary solution for power meter address - need to change to more flexible method
            string[] tmpAddress = new string[2];
            tmpAddress[0] = "USB::0x0aad::0x000c::" + "121133";
            tmpAddress[1] = "USB::0x0aad::0x00e2::" + "101542";

            for (int i = 0; i < PwrMeter_Address.Length; i++)
            {
                PwrMeter_Address[i] = tmpAddress[i];
            }

            switch (PMmodel.ToUpper())
            {
                case "E4416A":
                case "E4417A":
                    //EqPwrMeter = new LibEqmtDriver.PS.E4417A(PMaddr);
                    //EqPwrMeter.Initialize(1);
                    EqmtStatus.PM = true;
                    break;
                case "NRPZ11":
                    EqPwrMeter = new EqmtLibrary.RSNRPZ.cRSNRPZ(PwrMeter_Address);
                    EqmtStatus.PM = true;
                    break;
                case "NRP8S":
                    EqPwrMeter = new EqmtLibrary.RSNRPZ.cRSNRPZ(PwrMeter_Address);
                    EqmtStatus.PM = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.PM = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment POWERSENSOR Model : " + PMmodel.ToUpper(), "Pls ignore if Equipment Power Sensor not require.");
                    EqmtStatus.PM = false;
                    break;
            }
            #endregion
        }

        public void InstrUnInit()
        {
            for (int i = 0; i < TotalDCSupply; i++)
            {
                if (EqmtStatus.DCSupply[i])
                {
                    //EqDCSupply[i].Init();
                    //EqDCSupply[i] = null;
                }
            }

            if (EqmtStatus.Switch)
            {
                //EqSwitch = null;
            }

            if (EqmtStatus.PXI_VST)
            {
                //EqVST = null;
            }

            if (EqmtStatus.PM)
            {
                EqPwrMeter.Close();
            }
        }

        #endregion

        #region Tests Init/Uninit
        /// <summary>
        /// Initialize new DutTests according to test plan. Call this during Init() stage
        /// Please add the declaration of a new test in the switch-case block here
        /// </summary>
        public void TestsInit()
        {
            DutTests = new List<MyDUT_WS_Test>();
            foreach (Dictionary<string, string> currTestCond in DicTestPA)
            {

                string TestMode = myUtility.ReadTcfData(currTestCond, TCF_Header.ConstTestMode);
                string TestParam = myUtility.ReadTcfData(currTestCond, TCF_Header.ConstTestParam);


                switch (TestMode.ToUpper())
                {
                    case "WS-NF":

                        switch (TestParam.ToUpper())
                        {
                            case "WS_PXI_NF_NONCA_NDIAG":
                                DutTests.Add(new MyWS_PXI_NF_NONCA_NDIAG_Test(this, currTestCond));
                                break;
                            case "WS_PXI_NF_FIX_NMAX":
                                DutTests.Add(new MyWS_PXI_NF_FIX_NMAX_Test(this, currTestCond));
                                break;
                            case "WS_PXI_RXPATH_CONTACT":
                                DutTests.Add(new MyWS_PXI_RXPATH_CONTACT_Test(this, currTestCond));
                                break;
                            default:
                                MessageBox.Show("WaferSort NF Test Parameter : " + TestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                break;
                        }
                        break;
                    default:
                        MessageBox.Show("Test Mode " + TestMode + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                        break;
                }
            }
        } 
        
        /// <summary>
        /// Clear the tests in DutTests list
        /// </summary>
        public void TestsUnInit()
        {
            DutTests.Clear();
        }
        #endregion


        #region Read TCF File
        private void ReadPaTCF()
        {
            //myUtility.ReadTCF(ConstPASheetNo, ConstPAIndexColumnNo, ConstPATestParaColumnNo, ref DicTestPA);
            myUtility.ReadTCF(TCF_Sheet.ConstPASheetNo, TCF_Sheet.ConstPAIndexColumnNo, TCF_Sheet.ConstPATestParaColumnNo, ref DicTestPA, ref DicTestLabel);
        }
        private void ReadCalTCF()
        {
            myUtility.ReadCalSheet(TCF_Sheet.ConstCalSheetNo, TCF_Sheet.ConstCalIndexColumnNo, TCF_Sheet.ConstCalParaColumnNo, ref DicCalInfo);
        }
        private void ReadWafeForm()
        {
            //myUtility.ReadWaveformFilePath(TCF_Sheet.ConstKeyWordSheetNo, TCF_Sheet.ConstWaveFormColumnNo, ref DicWaveForm);  //remark and replace by additional dic for mutateWaveform (Shaz - 12/05/2016)
            myUtility.ReadWaveformFilePath(TCF_Sheet.ConstKeyWordSheetNo, TCF_Sheet.ConstWaveFormColumnNo, ref DicWaveForm, ref DicWaveFormMutate);
        }
        private void ReadMipiReg()
        {
            myUtility.ReadMipiReg(TCF_Sheet.ConstMipiRegSheetNo, TCF_Sheet.ConstPAIndexColumnNo, TCF_Sheet.ConstMipiRegColumnNo, ref DicMipiKey);
        }
        #endregion

        public void BuildResults(ref ATFReturnResult results, string paraName, string unit, double value)
        {
            ATFResultBuilder.AddResult(ref results, paraName, unit, value);
        }

        #region misc function
        // Delay routine to avoid using Thread.Sleep()
        public void DelayMs(int mSec)
        {
            EqmtLibrary.Utility.HiPerfTimer timer = new EqmtLibrary.Utility.HiPerfTimer();
            timer.wait(mSec);
        }
        public void DelayUs(int uSec)
        {
            EqmtLibrary.Utility.HiPerfTimer timer = new EqmtLibrary.Utility.HiPerfTimer();
            timer.wait_us(uSec);
        }

        //multi site function 
        public void multiSite_Pathloss(int testsite, string calTag, string calSegm, double refFreq, ref double[] refloss, ref string error)
        { 
            for (int i = 0; i < testsite; i++)
            {
                string tmpcalSegm = null;
                tmpcalSegm = "SITE" + (i + 1) + "_" + calSegm;      //append 'SITEx_' to generic calSegment Tag    
                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(calTag, tmpcalSegm, refFreq, ref refloss[i], ref error);
            }               
        }        

        private double[] CalculatePowerRamp(double minPower, double maxPower, int sampleCount)
        {
            double[] ramp = new double[sampleCount];

            double step = (maxPower - minPower) / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                ramp[i] = minPower + i * step;
            }

            return ramp;
        }

        #endregion
    }
}
