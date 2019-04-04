
#region COMMNET and Copyright SECTION (NO MANUAL TOUCH!)
// This is AUTOMATIC generated template Test Plan (.cs) file for ATF (Clotho) of WSD, AvagoTech: V2.2.1.0
// Any Questions or Comments, Please Contact: YU HAN, yu.han@avagotech.com
// NOTE 1: Test Plan template .cs has 'FIXED' Sections which should NEVER be Manually Touched 
// NOTE 2: Starting from V2.2.0, Clotho follows new Package style test plan management:
//       (a) Requires valid integer Version defined for TestPlan, TestLimit, and ExcelUI
//               For TestPlan.cs, refer to header item 'TestPlanVersion=1'
//               For TestLiimit.csv, refer to row #7 'SpecVersion,1'
//               For ExcelUI.xlsx, refer to sheet #1, row #1 'VER	1'
//       Note TestPlanTemplateGenerator generated items holds default version as '1'
//       (b) About ExcelUI file and TestLimit file:
//               Always load from same parent folder as Test Plan .cs, @ root level
//       (c) About Correlation File:
//               When Development mode, loaded from  C:\Avago.ATF.Common.x64\CorrelationFiles\Development\
//               When Production mode, loaded from package folder within C:\Avago.ATF.Common.x64\CorrelationFiles\Production\
#endregion COMMNET and Copyright SECTION

#region Test Plan Properties Section (NO MANUAL TOUCH)
////<TestPlanVersion>TestPlanVersion=1<TestPlanVersion/>
////<ExcelBuddyConfig>BuddyExcel = NPI_2MM6CI1D_8055_NF_TCF.xlsx;ExcelDisplay = 1<ExcelBuddyConfig/>
////<TestLimitBuddyConfig>BuddyTestLimit = NPI_2MM6CI1D_8055_NF_TSF.csv<TestLimitBuddyConfig/>
////<CorrelationBuddyConfig>BuddyCorrelaton = NPI_2MM6CI1D_8055_NF_COR.csv<CorrelationBuddyConfig/>
#endregion Test Plan Properties Section


#region Test Plan Hardware Configuration Section
#endregion Test Plan Hardware Configuration Section


#region Test Plan Parameters Section
////<TestParameter>Name="SimHW";Type="IntType";Unit=""<TestParameter/>
#endregion Test Plan Parameters Section


#region Singel Value Parameters Section
////<SingelValueParameter>Name="SimHW";Value="1";Type="IntType";Unit=""<SingelValueParameter/>
#endregion Singel Value Parameters Section


#region Test Plan Sweep Control Section (NO MANUAL TOUCH!)
#endregion Test Plan Sweep Control Section


#region 'FIXED' Reference Section (NO MANUAL TOUCH!)
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Ivi.Visa.Interop;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
#endregion 'FIXED' Reference Section


#region Custom Reference Section
//////////////////////////////////////////////////////////////////////////////////
// ----------- ONLY provide your Custom Reference 'Usings' here --------------- //
using MyProduct;

using System.Net;
using System.Messaging;
using System.Runtime.InteropServices;
using System.Drawing;

using cTestResultsReader;

using cWaferDatabase_XML;
using cWaferProbeTip_XML;
// ----------- END of Custom Reference 'Usings' --------------- //
//////////////////////////////////////////////////////////////////////////////////
#endregion Custom Reference Section


public class VORTEX_WSPXI_NF_REV105 : MarshalByRefObject, IATFTest
{
    MyDUT myDUT;
    MyUtility myUtility = new MyUtility();
 
    #region WaferSort Variable
    public System.Messaging.MessageQueue mq;
    private System.Messaging.MessageQueue mqTest;

    private string Cassette_Wafer_List = @"C:\Scripts\WaferList_Info.ini";

    private int TestSite = 1;
    private int MaxData = 0;
    private int CurrentData = 1;

    private bool A_Map = true;
    private bool b_AMapSetting = false;
    private bool b_Retest = true;
    private bool b_RetestInit = true;
    private bool b_Test = false;
    private bool b_site = false;
    private bool SecondProbeTest = false;
    private bool startTest = false;

    private bool b_TouchStone = false;
    private StringBuilder[] sHeader;

    private cTestResultsReader.s_Results ResultFile;
    private cWaferProbeTip_XML.cXMLWaferProbeTip Tip = new cWaferProbeTip_XML.cXMLWaferProbeTip();
    private cWaferDatabase_XML.cXMLWaferDatabase DB = new cWaferDatabase_XML.cXMLWaferDatabase();

    private string CurrentWafer_File = @"C:\Scripts\CurrentWafer.txt";
    private string WaferList_File = @"C:\Scripts\WaferList_Info.ini";
    private string Local_Setting_File = @"C:\Avago.ATF.Common\Production\Local_Config\Local_Setting.txt";

    private bool b_SiteNumber = false;
    private int ref_XDie = -1;
    private int ref_YDie = -1;
    private int Max_XDie = -1;
    private int Max_YDie = -1;
    private int Centre_XDie = -1;
    private int Centre_YDie = -1;
    private int Offset_XDie = -1;
    private int Offset_YDie = -1;

    private char[] delimiter = new char[] { ',', ';' };
    private bool b_FTC_Coor = false;

    private string User = "";
    //private bool b_Std_Chk = false;

    private bool b_HarmonicHeader = false;

    private bool b_H2_Test = true;

    private System.Diagnostics.Stopwatch watch_h2 = new System.Diagnostics.Stopwatch();

    private StringBuilder sbHarmonic = new StringBuilder();
    private StringBuilder sbHarmonicHeader = new StringBuilder();

    Random random = new Random();
    bool DataSent = false;
    private double[,] Previous_Data;
    bool b_FirstProbe = true;
    #endregion

    #region  SNP (Datalog) variable
    IPHostEntry ipEntry = null;
    DateTime DT = new DateTime();

    bool InitSNP;

    string
    tPVersion = "",
    ProductTag = "",
    lotId = "",
    SublotId = "",
    WaferId = "",
    OpId = "",
    HandlerSN = "",
    newPath = "",
    FileName = "",
    TesterHostName = "",
    TesterIP = "",
    activeDir = @"C:\\Avago.ATF.Common\\DataLog\\";

    //Temp string for current Lot and SubLot ID - to solve Inari issue when using Tally Generator without unload testplan
    //This will cause the datalog for current lot been copied to previous lot folder
    string previous_LotSubLotID = "",
        current_LotSubLotID = "",
        tempWaferId = "",
        tempOpId = "",
        tempHandlerSN = "";

    #endregion

    //GUI ENTRY Variable flag
    bool FirstTest;

    //CY temperature setting
    private double temperatureReference;
    private double temperatureSpecification;
    private bool temperatureControlEnable;
    private int temperatureReadingFrequency;

    public string DoATFInit(string args)
    {
        Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFInit: {0}\nDo Minimum HW Init:\n{1}\n", args, ATFInitializer.DoMinimumHWInit());


        #region Custom Init Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom Init Coding here --------------- //
        
        #region WaferSort - TestSite Init
        if (System.IO.File.Exists(Cassette_Wafer_List))
        {
            User = myUtility.ReadTextFile(Cassette_Wafer_List, "GENERAL", "USER").ToUpper().Trim();
        }
        #endregion

        myDUT = new MyDUT(ref sb);
        myDUT.tmpUnit_No = 0;

        #region WaferSort - Map Init
        //MessageBox.Show("DoATFInit() called", "To Approve"); 
        #region "Initializing Wafer Map"
        #region Message Queue"
        //Setting Message Queue
        if (MessageQueue.Exists(@".\Private$\WaferQueue"))
        {
            mq = new System.Messaging.MessageQueue(@".\Private$\WaferQueue");
        }
        else
        {
            mq = MessageQueue.Create(@".\Private$\WaferQueue");
        }
        if (MessageQueue.Exists(@".\Private$\WaferQueueTest"))
        {
            mqTest = new System.Messaging.MessageQueue(@".\Private$\WaferQueueTest");
        }
        else
        {
            mqTest = MessageQueue.Create(@".\Private$\WaferQueueTest");
        }
        #endregion
        // Initializing Settings

        string ProbeType = "SINGLE"; // SINGLE, DUAL, DUAL_VERTICAL, TRIPPLE, QUAD or USER
        if (TestSite == 2)
        {
            ProbeType = "DUAL_VERTICAL";
        }
        string StripID = "NA";

        string s_WafefSettingFile;

        s_WafefSettingFile = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 2, 2);
        //StripID = "H2";
        StripID = "NS";
        A_Map = true;
        //}
        if (System.IO.File.Exists(s_WafefSettingFile + "2"))
        {
            s_WafefSettingFile = s_WafefSettingFile + "2";
        }
        else if (System.IO.File.Exists(myUtility.ReadTextFile(Local_Setting_File, "SERVER", "FILESERVER").TrimEnd('\\') + "\\" + myUtility.ReadTextFile(Local_Setting_File, "SERVER", "WAFERSETTING").Trim('\\') + "\\" + s_WafefSettingFile + "2"))
        {
            s_WafefSettingFile = s_WafefSettingFile + "2";
        }

        string ProbeAddress = myUtility.ReadTextFile(Local_Setting_File, "PROBER", "ADDRESS").ToUpper().Trim(); ; //Not Required, Parse from Handler Plugin
        string ProberType = Extract_ProberType("PROBER", "TYPE", "EG"); //"EG"; //EG, TSK or Simulation
        string WaferBinFile = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 1, 2);
        string WaferSettingFile = s_WafefSettingFile; //Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 2, 2);
        //string tlimitFileName = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_TESTLIMIT_FILE, "");
        string Spec_LimitFilename = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TL_FULLPATH, "");
        string RetestBin = Parse_RetestBin(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 28, 2)); // not use for DC
        string WaferID = ""; //Not Used here
        bool b_Cleaning_Enable = Convert.ToBoolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 7, 2));//false;
        int Cleaning_Frequency = Convert.ToInt32(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 8, 2));
        string Cleaning_Location = "0,0"; //Location X, Y for Cleaning
        bool b_AutoCleaning = false;

        string ProbeFilename = ""; // if probe type is USER
        string SpecLimitFileName_2ndPass = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 9, 2);
        bool b_EnableSkipPassedDie = Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 11, 2)); // to Skip Tested Pass Die if reprobe during testing
        bool b_EnableAdvanceFailureCheck = Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 12, 2));
        double AdvanceFailure_TriggerYield = Parse_Double(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 13, 2));
        int AdvanceFailure_TriggerPeriod = Parse_Int(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 14, 2));
        int AdvanceFailure_SkipInitialDieCount = Parse_Int(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 15, 2));

        bool b_FC_Data = Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 20, 2));

        //CY temperature setting
        temperatureControlEnable = Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 42, 2));
        temperatureReference = Parse_Double(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 43, 2));
        temperatureSpecification = Parse_Double(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 44, 2));
        temperatureReadingFrequency = Parse_Int(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 45, 2));

        string H2_LimitFileName = Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 51, 2);

        //WaferDatabase.cWaferDatabase DB = new WaferDatabase.cWaferDatabase();
        //DB.Set_Database_Path = @"\\192.168.5.10\fbar\Clotho-FBAR_Database\DB";
        Tip.DB_Path = myUtility.ReadTextFile(Local_Setting_File, "SERVER", "FILESERVER").TrimEnd('\\') + "\\" + myUtility.ReadTextFile(Local_Setting_File, "SERVER", "DATABASE").TrimEnd('\\'); //@"C:\Wafer_Temp\DB\";
        Tip.ProbeTipLimit = 10000000;
        DB.DB_Path = myUtility.ReadTextFile(Local_Setting_File, "SERVER", "FILESERVER").TrimEnd('\\') + "\\" + myUtility.ReadTextFile(Local_Setting_File, "SERVER", "DATABASE").Trim('\\');
        string WaferLotFile = "";
        long LinkID = 0;

        bool ACP_Probing = true;

        //DialogResult Rslt = MessageBox.Show("Second Probe Test?", "2 Pass Testing", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //if (Rslt == DialogResult.Yes)
        {
            bool chkID = false;
            string CurrentWafer_Position = "";
            if (System.IO.File.Exists(CurrentWafer_File))
            {
                string CurrWaferPos = System.IO.File.ReadAllText(@"C:\Scripts\CurrentWafer.txt");

                if (CurrWaferPos.Trim().ToUpper() == "DONE")
                {
                    if (User == "SUSER")
                    {
                        chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper());
                    }
                    else
                    {
                        chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper());// + "-v" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "")));
                    }
                }
                else
                {
                    WaferID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
                    if (User == "SUSER")
                    {
                        chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), WaferID);
                    }
                    else
                    {
                        chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), WaferID);// + "-v" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, ""))
                    }
                }
            }
            else
            {

                WaferID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");

                try
                {
                    if (WaferID != "")
                    {
                        if (User == "SUSER")
                        {
                            chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), WaferID);
                        }
                        else
                        {
                            chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), WaferID);// + "-v" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "")), WaferID);
                            //MessageBox.Show(chkID.ToString() + Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper() + WaferID);
                            //chkID = DB.Check_WaferLotID_Exist("2LV7-8030", WaferID);
                        }
                    }
                    else
                    {
                        if (User == "SUSER")
                        {
                            chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper());
                        }
                        else
                        {
                            chkID = DB.Check_WaferLotID_Exist(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper());// + "-v" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "")));
                            //chkID = DB.Check_WaferLotID_Exist("2LV7-8030");
                            //MessageBox.Show(chkID.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            {

                //bool ChkSubID = DB.Check_SubLotID_Exist(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PRODUCT_TAG, ""));
                if (chkID)
                {
                    DialogResult Rslt = MessageBox.Show("Second Probe Test?", "2 Pass Testing", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (Rslt == DialogResult.Yes)
                    {

                        //WaferLotFile = DB.Get_FirstProbe_Filename(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PRODUCT_TAG, ""), DB.WaferLotID, DB.SubLotID, true, ref LinkID);
                        if (User == "SUSER")
                        {
                            WaferLotFile = DB.Get_FirstProbe_Filename(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), DB.WaferLotID, true, ref LinkID);
                        }
                        else
                        {
                            //WaferLotFile = DB.Get_FirstProbe_Filename(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "") + "-v" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "")), DB.WaferLotID, true, ref LinkID);
                            //string dddd = Append_Filename(DB.Get_SecondProbe_Filename(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), DB.WaferLotID, true, ref LinkID), "DELTA");

                            //WaferLotFile = DB.Get_FirstProbe_Filename(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), DB.WaferLotID, true, ref LinkID);
                            //WaferLotFile = DB.Get_FirstProbe_Filename("2LV7-8030", DB.WaferLotID, true, ref LinkID);

                            WaferLotFile = Append_Filename(DB.Get_SecondProbe_Filename(Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper(), DB.WaferLotID, true, ref LinkID), "DELTA");
                        }
                        b_FirstProbe = false;
                        b_H2_Test = true;
                        StripID += "-2PASS";
                        SecondProbeTest = true;
                    }
                    else
                    {
                        b_H2_Test = false;
                        StripID += "-1PASS";
                        SecondProbeTest = false;
                    }
                }
                else
                {
                    b_H2_Test = false;
                    StripID += "-1PASS";
                    SecondProbeTest = false;
                }
            }
        }

        try
        {
            //b_TouchStone = Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 39, 2)); // parse in the Touch Stone Settings

            StringBuilder SP = new StringBuilder("");
            SP.AppendFormat("{0};", ProbeAddress);                  //0 - Prober Address - Not Required Set from Handler Plug In
            SP.AppendFormat("{0};", ProberType);                    //1 - Prober Type EG, TSK, SIMULATION
            SP.AppendFormat("{0};", "");                            //2 - Binning File - Not Requred for Auto Assign
            SP.AppendFormat("{0};", Spec_LimitFilename);            //3 - Spec Filename
            SP.AppendFormat("{0};", RetestBin);                     //4 - 2nd Pass Binning
            SP.AppendFormat("{0};", WaferSettingFile);              //5 - Wafer Map Filename (.wst)
            SP.AppendFormat("{0};", WaferID);                       //6 - Wafer ID
            SP.AppendFormat("{0};", b_Cleaning_Enable.ToString());  //7 - Cleaning Function (Manual Trigger)
            SP.AppendFormat("{0};", Cleaning_Frequency.ToString()); //8 - Cleaning Frequency
            SP.AppendFormat("{0};", Cleaning_Location);             //9 - Location (Optional - future used)
            SP.AppendFormat("{0};", ProbeType);                     //10 - Probe Tip Assignment (SINGLE, DUAL, DUAL-VERTICAL, TRIPPLE, QUAD
            SP.AppendFormat("{0};", ProbeFilename);                 //11 - Probe Tip Assignment, for special probe configuration
            SP.AppendFormat("{0};", StripID);                       //12 - STRIP ID - Not Required
            SP.AppendFormat("{0};", b_AutoCleaning.ToString());     //13 - Auto Cleaning Function

            SP.AppendFormat("{0};", b_EnableSkipPassedDie.ToString());                  //14 - Skip Passed Die Function for Retest
            SP.AppendFormat("{0};", b_EnableAdvanceFailureCheck.ToString());            //15 - Advance Failure Triggering Function
            SP.AppendFormat("{0};", AdvanceFailure_TriggerYield.ToString());            //16 - Advance Failure Yield Trigger
            SP.AppendFormat("{0};", AdvanceFailure_TriggerPeriod.ToString());           //17 - Advance Failure Periodic Alerts
            SP.AppendFormat("{0};", AdvanceFailure_SkipInitialDieCount.ToString());     //18 - Advance Failure Skip Initial Alerts

            SP.AppendFormat("{0};", SecondProbeTest.ToString());    //19 - Enable 2 Pass Probing Function
            SP.AppendFormat("{0};", WaferLotFile);                  //20 - Parsing 1st Pass Probe Data for 2 Pass
            SP.AppendFormat("{0};", LinkID.ToString());             //21 - Linking ID for 1st Pass Probe Data
            SP.AppendFormat("{0};", "");                            //22 - Not Used

            SP.AppendFormat("{0};", ACP_Probing.ToString());        //23 - ACP Probing Function (Not Used)
            SP.AppendFormat("{0};", SpecLimitFileName_2ndPass);     //24 - 2nd Pass Spec Filename
            SP.AppendFormat("{0};", "");                            //25 - Not Used
            SP.AppendFormat("{0};", b_FC_Data.ToString());          //26 - FTC data - do not use
            SP.AppendFormat("{0};", Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 22, 2)));   //27 - Auto Retest Function Enable
            SP.AppendFormat("{0};", Parse_Int(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 23, 2)));       //28 - Auto Retest Count
            SP.AppendFormat("{0};", Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 24, 2)));   //29 - Probe All Die on 2 Pass - Not Used
            SP.AppendFormat("{0};", "FALSE");                       //30 - Pattern Filtering (Maverick Function - not used for FBAR)
            SP.AppendFormat("{0};", "FALSE");                       //31 - Pattern Filtering (Maverick Function - not used for FBAR)
            SP.AppendFormat("{0};", Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 30, 2)));   //32 - Enhance Screening Enable
            SP.AppendFormat("{0};", Parse_Double(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 31, 2)));    //33 - Enhance SCreening Lower Spec
            SP.AppendFormat("{0};", Parse_Double(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 32, 2)));    //34 - Enhance Screening Upper Spec
            SP.AppendFormat("{0};", (Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 33, 2)));                //35 - Enhance Screening Frequency
            SP.AppendFormat("{0};", Parse_Boolean(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 24, 2)));   //36 - Probe All Die on 2 Pass - Not Used
            SP.AppendFormat("{0};", Parse_RejectVal(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 35, 2))); //37 - Set Continuous Failure Count
            SP.AppendFormat("{0};", "");                            //38 Save Picture File (not for Inari)
            SP.AppendFormat("{0};", Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 40, 2));                  //39 Ignore 1 Pass Spec
            SP.AppendFormat("{0};", false);                         //40 Bimodal Failure Message - Set False for Cassette Loader
            SP.AppendFormat("{0};", H2_LimitFileName);              //41 H2 Test Spec Filename;

            //Blank
            SP.AppendFormat("{0};", "");   						 	//42 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//43 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//44 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//45 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//46 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//47 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//48 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//49 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//50 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//51 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//52 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//53 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//54 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//55 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//56 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//57 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//58 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//59 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//60 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//61 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//62 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//63 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//64 empty - for future changes if needed

            //CY temperature setting
            SP.AppendFormat("{0};", "");   						 	//65 empty - for future changes if needed
            SP.AppendFormat("{0};", "");   						 	//66 empty - for future changes if needed
            SP.AppendFormat("{0};", temperatureReference);       	//67 temperature reference
            SP.AppendFormat("{0};", temperatureSpecification);   	//68 temperature specification
            SP.AppendFormat("{0};", temperatureControlEnable);   	//69 temperature control enable
            SP.AppendFormat("{0};", temperatureReadingFrequency);   //70 temperature reading frequency

            SendMsg(SP.ToString());

        }
        catch
        {
            MessageBox.Show("Error1234");
        }
        #endregion
        try
        {

            string correlationFilePath = "";
            correlationFilePath = Extract_Filename(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, ""));

            if (User == "SUSER")
            {
                if (System.IO.File.Exists(@"C:\Avago.ATF.Common.x64\CorrelationFiles\Development\" + correlationFilePath))
                {
                    correlationFilePath = @"C:\Avago.ATF.Common.x64\CorrelationFiles\Development\" + correlationFilePath;
                }
            }
            else
            {
                if (System.IO.File.Exists(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "")))
                {
                    correlationFilePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");
                    //MessageBox.Show(correlationFilePath);
                }
            }
            if (System.IO.File.Exists(correlationFilePath))
            {
                //MessageBox.Show("Corr Ok : " + correlationFilePath);
                string[] CorrData = System.IO.File.ReadAllLines(correlationFilePath);
                for (int iCorr = 0; iCorr < 4; iCorr++)
                {
                    //MessageBox.Show(iCorr.ToString());
                    if (CorrData[iCorr].ToUpper().StartsWith("SITE_NO"))
                    {
                        //MessageBox.Show("Corr Ok");
                        b_SiteNumber = true;
                    }
                    else
                    {
                        b_SiteNumber = false;
                    }
                }
            }

            if (System.IO.File.Exists(s_WafefSettingFile))
            {
                string[] WaferSettingData = System.IO.File.ReadAllLines(s_WafefSettingFile);
                if (s_WafefSettingFile.EndsWith("2"))
                {
                    string[] tmpCoor = WaferSettingData[6].Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    ref_XDie = int.Parse(tmpCoor[0]);
                    Centre_XDie = int.Parse(tmpCoor[1]);

                    tmpCoor = WaferSettingData[7].Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    ref_YDie = int.Parse(tmpCoor[0]);
                    Centre_YDie = int.Parse(tmpCoor[1]);

                    Max_XDie = int.Parse(WaferSettingData[10]);
                    Max_XDie = int.Parse(WaferSettingData[11]);
                    Offset_XDie = ref_XDie - Centre_XDie;
                    Offset_YDie = ref_YDie - Centre_YDie;
                    b_FTC_Coor = true;
                    //MessageBox.Show("FTC Map1");
                }
                else
                {
                    ref_XDie = int.Parse(WaferSettingData[6]);
                    ref_YDie = int.Parse(WaferSettingData[7]);
                    Max_XDie = int.Parse(WaferSettingData[10]);
                    Max_XDie = int.Parse(WaferSettingData[11]);
                    Centre_XDie = -1;
                    Centre_YDie = -1;
                    Offset_XDie = 0;
                    Offset_YDie = 0;
                    b_FTC_Coor = false;
                }
            }
            else
            {
                string WaferSetting_Path = myUtility.ReadTextFile(Local_Setting_File, "SERVER", "FILESERVER").TrimEnd('\\') + myUtility.ReadTextFile(Local_Setting_File, "SERVER", "WAFERSETTING").TrimEnd('\\');
                string[] WaferSettingData = System.IO.File.ReadAllLines(WaferSetting_Path + "\\" + s_WafefSettingFile);

                if (s_WafefSettingFile.EndsWith("2"))
                {
                    string[] tmpCoor = WaferSettingData[6].Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    ref_XDie = int.Parse(tmpCoor[0]);
                    Centre_XDie = int.Parse(tmpCoor[1]);

                    tmpCoor = WaferSettingData[7].Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    ref_YDie = int.Parse(tmpCoor[0]);
                    Centre_YDie = int.Parse(tmpCoor[1]);

                    Max_XDie = int.Parse(WaferSettingData[10]);
                    Max_XDie = int.Parse(WaferSettingData[11]);
                    Offset_XDie = ref_XDie - Centre_XDie;
                    Offset_YDie = ref_YDie - Centre_YDie;
                    b_FTC_Coor = true;
                    //MessageBox.Show("FTC Map2");
                }
                else
                {
                    ref_XDie = int.Parse(WaferSettingData[6]);
                    ref_YDie = int.Parse(WaferSettingData[7]);
                    Max_XDie = int.Parse(WaferSettingData[10]);
                    Max_XDie = int.Parse(WaferSettingData[11]);
                    Centre_XDie = -1;
                    Centre_YDie = -1;
                    Offset_XDie = 0;
                    Offset_YDie = 0;
                    b_FTC_Coor = false;
                }
            }
        }
        catch
        {
            MessageBox.Show("Error Reading Map");
        }
        if (System.IO.File.Exists(WaferList_File))
        {
            string PCB_ID_Str = ReadTextFile(WaferList_File, "GENERAL", "RunCard").Trim() + ";"
                                + ReadTextFile(WaferList_File, "GENERAL", "Cassette").Trim() + ";"
                                + ReadTextFile(WaferList_File, "GENERAL", "Setup").Trim();

            ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, PCB_ID_Str);
        }
        ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, Parse_NA(Avago.ATF.StandardLibrary.ATFCrossDomainWrapper.Excel_Get_Input("Common", 51, 2)));
        #endregion

        // ----------- END of Custom Init Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom Init Coding Section

        return sb.ToString();
    }

    public string DoATFUnInit(string args)
    {
        Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFUnInit: {0}\n", args);


        #region Custom UnInit Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom UnInit Coding here --------------- //

        myDUT.InstrUnInit();


        // ----------- END of Custom UnInit Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom UnInit Coding Section

        return sb.ToString();
    }


    public string DoATFLot(string args)
    {
        Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFLot: {0}\n", args);


        #region Custom CloseLot Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom CloseLot Coding here --------------- //




        // ----------- END of Custom CloseLot Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom CloseLot Coding Section

        return sb.ToString();
    }


    public ATFReturnResult DoATFTest(string args)
    {
        //Debugger.Break();

        string err = "";
        StringBuilder sb = new StringBuilder();
        ATFReturnResult result = new ATFReturnResult();

        // ----------- Example for Argument Parsing --------------- //
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (!ArgParser.parseArgString(args, ref dict))
        {
            err = "Invalid Argument String" + args;
            MessageBox.Show(err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new ATFReturnResult(err);
        }


        int simHW;
        try
        {
            simHW = ArgParser.getIntItem(ArgParser.TagSimMode, dict);
        }
        catch (Exception ex)
        {
            err = ex.Message;
            MessageBox.Show(err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new ATFReturnResult(err);
        }
        // ----------- END of Argument Parsing Example --------------- //


        #region Custom Test Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom Test Coding here --------------- //
        // Example for build TestPlan Result (Single Site)

#if (!DEBUG)
    myDUT.tmpUnit_No = Convert.ToInt32(ATFCrossDomainWrapper.GetClothoCurrentSN());
#else
        myDUT.tmpUnit_No++;      // Need to enable this during debug mode
#endif

        ATFResultBuilder.Reset();
        FirstTest = false;

        #region Retrieve lot ID# (for Datalog)
        //Retrieve lot ID#
        tPVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TP_VER, "");
        ProductTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "").ToUpper();
        lotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "").ToUpper();
        SublotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "").ToUpper();
        WaferId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_WAFER_ID, "");
        OpId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "");
        HandlerSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");
        TesterHostName = System.Net.Dns.GetHostName();
        ipEntry = System.Net.Dns.GetHostEntry(TesterHostName);
        TesterIP = ipEntry.AddressList[0].ToString().Replace(".", ""); //Always default to the 1st network card. This is because for Result FileName , clotho always take the 1st nework id return by system

        if (myDUT.tmpUnit_No == 0)      //do this for the 1st unit only
        {
            DT = DateTime.Now;

            if (ProductTag != "" && lotId != "")
            {
                //// SnP file Dir generation            
                newPath = System.IO.Path.Combine(activeDir, ProductTag + "_" + lotId + "_" + SublotId + "_" + TesterIP + "\\");
                //System.IO.Directory.CreateDirectory(newPath);
                //FileName = System.IO.Path.Combine(activeDir, ProductTag + "_" + lotId + "_" + SublotId + "_" + TesterIP + "\\" + lotId + ".txt");
            }
            else
            {
                string tempname = "DebugMode_" + DT.ToString("yyyyMMdd" + "_" + "HHmmss");
                newPath = System.IO.Path.Combine(activeDir, tempname + "\\");
                //System.IO.Directory.CreateDirectory(newPath);
                ProductTag = "Debug";
                //FileName = System.IO.Path.Combine(activeDir, tempname + "\\" + "DebugMode" + ".txt");
            }

            //Parse information to LibFbar
            myDUT.SNPFile.FileOutput_Path = newPath;
            myDUT.SNPFile.FileOutput_FileName = ProductTag;
            InitSNP = true;

            // Added variable to solve issue with datalog when Inari operator using 
            //Tally Generator to close lot instead of unload test plan
            //WaferId,OpId and HandlerSN are null when 2nd Lot started - make assumption that this 3 param are similar 1st Lot
            tempWaferId = WaferId;
            tempOpId = OpId;
            tempHandlerSN = HandlerSN;
            previous_LotSubLotID = current_LotSubLotID;
        }
        #endregion

        #region WaferSort - Routine
        StringBuilder XYLoc;
        string XCoor = "";
        string YCoor = "";
        bool b_ResetEna = false;
        int iMatch_Data = 0;
        int iTest_Data = 0;
        double RtnVal = 0;

        {       //start this part

            Pair<double, double>[] validLocations = ATFCrossDomainWrapper.GetValidLocations().ToArray();

            #region "Regen and Wafer Initializing"
            if (b_Retest)
            {
                if ((validLocations[0].First == 50000) || (validLocations[0].Second == 50000))
                {
                    b_Test = true;
                }
                else
                {
                    bool b_Flag = true;
                    if (b_RetestInit)
                    {

                        string tmp = Get_QueueMsg();
                        string[] Msg = tmp.Split(';');

                        if (Msg.Length > 1)
                        {
                            string StrFile = Msg[1];

                            cTestResultsReader.cTestResultsReader Result = new cTestResultsReader.cTestResultsReader();
                            ResultFile = new cTestResultsReader.s_Results();
                            Result.Result_FileName = StrFile;

                            if (Result.Read_File())
                            {
                                ResultFile = Result.parse_Results;
                                MaxData = ResultFile.ResultData.Length;

                            }
                            if (Msg[0].Trim() == "SECONDPROBE")
                            {
                                b_Retest = false;
                                b_Test = true;
                                b_Flag = false;
                            }
                            b_RetestInit = false;
                        }
                        else
                        {
                            b_Retest = false;
                            b_Test = true;
                            b_Flag = false;
                        }
                    }

                    if (!b_AMapSetting)
                    {
                        string WaferID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
                        string ProbeID = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "");
                        ProductTag = "";
                        if (User == "SUSER")
                        {
                            ProductTag = Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "")).ToUpper();
                        }
                        else
                        {
                            ProductTag = Extract_WaferID(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "") + "-v" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "")).ToUpper();
                        }
                        string TesterID = ATFConfig.Instance.getSpecificItem(ATFConfigConstants.TagToolTesterType).Value;
                        bool b_AutoAMapGenerate = false;        // to generate .A file for Production Pick and Place / AVI
                        ShareInfoCommon cc = ATFCrossDomainWrapper.GetShareInfoCommon();

                        StringBuilder SP1 = new StringBuilder("");
                        SP1.AppendFormat("{0};", WaferID);                          //0
                        SP1.AppendFormat("{0};", ProbeID);                          //1
                        SP1.AppendFormat("{0};", ProductTag);                       //2
                        SP1.AppendFormat("{0};", TesterID);                         //3
                        SP1.AppendFormat("{0};", b_AutoAMapGenerate.ToString());    //4
                        SP1.AppendFormat("{0};", cc.CurResultFileName);             //5
                        //SP1.AppendFormat("{0};", A_Map.ToString());
                        if (A_Map)
                        {
                            SP1.AppendFormat("{0};", "true");                       //6
                        }
                        else
                        {
                            SP1.AppendFormat("{0};", "false");                      //6
                        }
                        //SendMsg(WaferID + ProbeID+ProductTag+TesterID+b_AMapSetting.ToString());


                        SP1.AppendFormat("{0};", "INARI");                          //7
                        SP1.AppendFormat("{0};", "FBAR");                           //8
                        SP1.AppendFormat("{0};", "FILTER");                         //9
                        SP1.AppendFormat("{0};", ProductTag);                       //10
                        SP1.AppendFormat("{0};", WaferID);                          //11
                        SP1.AppendFormat("{0};", ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, ""));   //12
                        SP1.AppendFormat("{0};", TesterID);                         //13
                        SP1.AppendFormat("{0};", "");                               //14
                        SP1.AppendFormat("{0};", "");                               //15
                        SP1.AppendFormat("{0};", ProductTag);                       //16
                        SP1.AppendFormat("{0};", "C:\\Avago.ATF.Common\\Clotho_SDI_Data\\FBAR_" + Modify_output_filename(Remove_Ext(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, ""))) + "\\"); //17

                        SP1.AppendFormat("{0};", 0);                                //18
                        //SP1.AppendFormat("{0};", "");
                        SP1.AppendFormat("{0};", b_H2_Test);                        //19
                        SendMsg(SP1.ToString());
                        b_AMapSetting = true;

                        TestSite = validLocations.Length;
                    }

                    if (b_Flag)
                    {
                        // Regen Function
                        sHeader = new StringBuilder[ResultFile.ResultHeader.TestParameter_Name.Length];

                        for (int iH = 0; iH < ResultFile.ResultHeader.TestParameter_Name.Length; iH++)
                        {
                            sHeader[iH] = new StringBuilder(string.Format("{0}=", ResultFile.ResultHeader.TestParameter_Name[iH]));
                        }

                        for (int i = 0; i < validLocations.Length; i++)
                        {
                            sHeader[0].AppendFormat("{0},", validLocations[i].First);
                            sHeader[1].AppendFormat("{0},", validLocations[i].Second);
                            if ((validLocations[i].First == -99999) || (validLocations[i].Second == -99999))
                            {
                                for (int iR = 2; iR < ResultFile.ResultData[0].Data.Length; iR++)
                                {
                                    sHeader[iR].AppendFormat("{0},", "0");
                                }
                            }
                            else
                            {
                                int MatchLoc = ResultFile.XY_Info.Match_Position[truncate_XY(validLocations[i].First), truncate_XY(validLocations[i].Second)];
                                if (MatchLoc < 0)
                                {
                                    for (int iR = 2; iR < ResultFile.ResultData[0].Data.Length; iR++)
                                    {
                                        sHeader[iR].AppendFormat("{0},", "0");
                                    }
                                }
                                else
                                {
                                    for (int iR = 2; iR < ResultFile.ResultData[MatchLoc].Data.Length; iR++)
                                    {
                                        sHeader[iR].AppendFormat("{0},", ResultFile.ResultData[MatchLoc].Data[iR]);
                                    }
                                }
                                CurrentData++;
                            }
                        }

                        for (int iRR = 0; iRR < ResultFile.ResultData[0].Data.Length; iRR++)
                        {
                            //sb.AppendFormat("{0};", sHeader[iRR].ToString().Trim(','));
                        }
                        Thread.Sleep(15); // Make sure is > 15 ms or else Clotho will not be stable.
                        if (Get_QueueMsg_Quick() != "")
                        {
                            b_Retest = false;
                            b_Test = true;
                            //return sb.ToString();
                        }
                        if (CurrentData == MaxData)
                        {
                            b_Retest = false;
                            b_Test = true;
                            b_Retest = false;
                            //return sb.ToString();
                        }
                    }
                }
            }

            #endregion

            #region XY Coordinate

            if (b_Test)
            {
                
                for (int iSite = 0; iSite < TestSite; iSite++)
                {
                    if (!startTest)
                    {
                        //Drivers[iSite].SNPFile.FileOutput_Enable = true;
                        //Drivers[iSite].SNPFile.FileOutput_Path = "C:\\Avago.ATF.Common\\Clotho_SDI_Data\\FBAR_" + Modify_output_filename(Remove_Ext(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, ""))) + "\\"; //ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PRODUCT_TAG, "") + "_" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "") + "_" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "") + "\\";
                        //Drivers[iSite].SNPFile.FileOutput_FileName = "FBAR_" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "").ToUpper() + "_" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "") + "_" + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "");
                        //Drivers[iSite].SNPFile.TouchStone_Enable = false;
                        //Drivers[iSite].SNPFile.TouchStone_FileOutput_Path = "C:\\Avago.ATF.Common\\TouchStone_Data\\" + Modify_output_filename(Remove_Ext(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_CUR_RESULT_FILE, ""))) + "\\";
                    }
                    if ((validLocations[iSite].First == -99999) || (validLocations[iSite].Second == -99999) || (validLocations[iSite].First == 50000) || (validLocations[iSite].Second == 50000))
                    {
                        XYLoc = new StringBuilder("SKIP");
                    }
                    else
                    {
                        //XYLoc = new StringBuilder();
                        //XYLoc.AppendFormat("X{0}Y{1}", validLocations[iSite].First, validLocations[iSite].Second);
                        if (b_FTC_Coor)
                        {
                            XCoor = ((int)validLocations[iSite].First + Offset_XDie + 324).ToString("000");
                            YCoor = (Offset_YDie - (int)validLocations[iSite].Second + 324).ToString("000");
                        }
                        else
                        {
                            XCoor = ((int)validLocations[iSite].First).ToString();
                            YCoor = ((int)validLocations[iSite].Second).ToString();
                        }

                        XYLoc = new StringBuilder();
                        sbHarmonic = new StringBuilder();
                        //XYLoc.AppendFormat("X{0}Y{1}", validLocations[iSite].First, validLocations[iSite].Second);
                        XYLoc.AppendFormat("X{0}Y{1}", XCoor, YCoor);
                        sbHarmonic.AppendFormat("X{0},Y{1},", ((int)validLocations[iSite].First).ToString(), ((int)validLocations[iSite].Second).ToString());
                    }
                }
            }

            #endregion

            #region Test and Result

            double[] XYCorr = new double[2];
            for (int iSite = 0; iSite < validLocations.Length; iSite++)
            {
                if (b_FTC_Coor)
                {
                    XYCorr[0] = ((double)((int)validLocations[iSite].First + Offset_XDie + 324));
                    XYCorr[1] = ((double)(Offset_YDie - (int)validLocations[iSite].Second + 324));
                }
                else
                {
                    XYCorr[0] = ((double)(ref_XDie + (int)(validLocations[iSite].First)));
                    XYCorr[1] = ((double)(ref_YDie - (int)(validLocations[iSite].Second)));
                }
                if (b_SiteNumber)
                {
                    ATFResultBuilder.AddResult(ref result, "SITE_NO", "", (double)(iSite + 1));
                }
            }

            ATFResultBuilder.AddResult(ref result, "X", "", XYCorr[0]);
            ATFResultBuilder.AddResult(ref result, "Y", "", XYCorr[1]);
   
            try
            {
                myDUT.RunTest(ref result);
            }
            catch(Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
            if (!startTest) startTest = true;
            if ((validLocations[0].First == 50000) || (validLocations[0].Second == 50000))
            {
                b_Test = false;
            }

            #endregion
        }       //end this part

        #endregion


        // ----------- END of Custom Test Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom Test Coding Section

        //ATFReturnResult result = new ATFReturnResult();
        //ATFResultBuilder.AddResult(ref result, "PARAM", "X", 0.01);
        return result;
    }

    #region WaferSort Function
    public void SendMsg(string Message)
    {
        System.Messaging.Message mm = new System.Messaging.Message();
        mm.Body = Message;
        mm.Label = "Msg";
        mq.Send(mm);
    }
    private int truncate_XY(double value)
    {
        if (value < 0)
        {
            return 5000 + (int)value;
        }
        else
        {
            return (int)value;
        }
    }
    private string Get_QueueMsg()
    {
        System.Messaging.Message mes;
        string m;

        try
        {
            mes = mq.Receive(new TimeSpan(0, 0, 3));
            mes.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
            m = mes.Body.ToString();
        }
        catch
        {
            m = "No Message";
        }
        return m;
    }
    private string Get_QueueMsg_Quick()
    {
        System.Messaging.Message mes;
        string m;

        try
        {
            mes = mqTest.Receive(new TimeSpan(0, 0, 0, 0, 5));
            mes.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
            m = mes.Body.ToString();
        }
        catch
        {
            m = "";
        }
        return m;
    }
    private string Extract_Filename(string FullFilename)
    {
        string[] tmpStr = FullFilename.Trim().Split('\\');
        return tmpStr[tmpStr.Length - 1];
    }
    private bool Parse_Boolean(string Data)
    {
        if (Data.Trim() == "")
        {
            return false;
        }
        return bool.Parse(Data);
    }
    private int Parse_Int(string Data)
    {
        if (Data.Trim() == "")
        {
            return 0;
        }
        else
        {
            return int.Parse(Data);
        }
    }
    private double Parse_Double(string Data)
    {
        if (Data.Trim() == "")
        {
            return 0.0f;
        }
        else
        {
            return double.Parse(Data);
        }
    }
    private string Remove_Ext(string filename)
    {
        string[] info = filename.Split('.');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < info.Length - 1; i++)
        {
            sb.AppendFormat("{0}.", info[i]);
        }
        return sb.ToString().TrimEnd('.');
    }
    private string Extract_WaferID(string Input)
    {
        string[] info = Input.Split('-');
        if (info.Length > 1)
        {
            return info[0] + "-" + info[1];
        }
        else
        {
            return Input.ToUpper();
        }
    }
    private string Parse_RetestBin(string Input)
    {
        if (Input.Trim() == "")
        {
            return "PASS";
        }
        else
        {
            return Input;
        }
    }
    private int Parse_RejectVal(string Input)
    {
        if (Input.Trim() == "")
        {
            return 25;
        }
        else
        {
            return int.Parse(Input);
        }
    }
    private string ReadTextFile(string dirpath, string groupName, string targetName)
    {
        string tempSingleString;
        try
        {
            if (!System.IO.File.Exists(@dirpath))
            {
                throw new System.IO.FileNotFoundException("{0} does not exist."
                    , @dirpath);
            }
            else
            {
                using (System.IO.StreamReader reader = System.IO.File.OpenText(@dirpath))
                {
                    string line = "";
                    string[] templine;
                    tempSingleString = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == "[" + groupName + "]")
                        {
                            char[] temp = { };
                            line = reader.ReadLine();
                            while (line != null && line != "")
                            {
                                templine = line.ToString()
                                    .Split(new char[] { '=' });
                                temp = line.ToCharArray();
                                if (temp[0] == '[' && temp[temp.Length - 1] == ']')
                                    break;
                                if (templine[0].TrimEnd() == targetName)
                                {
                                    tempSingleString = templine[templine.Length - 1].ToString().TrimStart();
                                    break;
                                }
                                line = reader.ReadLine();
                            }
                            break;
                        }
                    }
                    reader.Close();
                }
            }
            return tempSingleString;
        }
        catch (System.IO.FileNotFoundException)
        {
            throw new System.IO.FileNotFoundException(dirpath + " " + groupName + " " +
                targetName + " Cannot Read from the file!");
        }
    }
    private string Extract_ProberType(string HeaderSection, string SubSection, string DefaultValue)
    {
        string[] Data = System.IO.File.ReadAllLines(@"C:\Avago.ATF.Common\Production\Local_Config\Local_Setting.txt");

        int StartInfo = -1;
        int Info = -1;
        for (int i = 0; i < Data.Length; i++)
        {
            if (Data[i].Contains(HeaderSection.Trim().ToUpper()))
            {
                StartInfo = i;
                break;
            }
        }
        if (StartInfo > 0)
        {
            for (int i = StartInfo; i < Data.Length; i++)
            {
                if (Data[i].Contains(SubSection.Trim().ToUpper()))
                {
                    Info = i;
                    break;
                }
            }
        }

        if (StartInfo == -1)
        {
            return DefaultValue;
        }
        if (Info == -1)
        {
            return DefaultValue;
        }

        string[] tmp = Data[Info].Split('=');

        return tmp[1].Trim().ToUpper();
    }

    private string Parse_NA(string Input)
    {
        if (Input.Trim() == "")
        {
            return "NA";
        }
        else
        {
            return Input;
        }
    }
    private string Modify_output_filename(string inputStr)
    {

        if (!inputStr.ToUpper().Contains(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "").ToUpper()))
        {
            string[] tmpStr = inputStr.Split('_');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tmpStr.Length; i++)
            {
                if (i == 1)
                {
                    sb.AppendFormat("{0}_{1}_", ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "").ToUpper(), tmpStr[i]);
                }
                else
                {
                    sb.AppendFormat("{0}_", tmpStr[i]);
                }
            }
            //MessageBox.Show(sb.ToString().TrimEnd('_') + ", " + inputStr);
            return sb.ToString().TrimEnd('_');
        }
        else
        {
            // MessageBox.Show(inputStr);
            return inputStr;
        }
    }
    private string Append_Filename(string FileName, string AppendStr)
    {
        string[] tmpStr = FileName.Split('.');
        if (AppendStr != "")
        {
            return (FileName.Substring(0, (FileName.Length - (tmpStr[tmpStr.Length - 1]).Length - 1)) + "_" + AppendStr + "." + tmpStr[tmpStr.Length - 1]);
        }
        else
        {
            return FileName;
        }
    }
    #endregion
}
