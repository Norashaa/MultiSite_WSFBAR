
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
////<ExcelBuddyConfig>BuddyExcel = 2MM6-8055_NF_TCF_Rev1.xlsx;ExcelDisplay = 1<ExcelBuddyConfig/>
////<xTestLimitBuddyConfig>BuddyTestLimit = AFEM-8055-AP1-NF_PXI_TSF_Rev4.0d.csv<TestLimitBuddyConfig/>
////<xCorrelationBuddyConfig>BuddyCorrelaton = AFEM-8055-AP1-NF_PXI_Corr_Rev4.0d.csv<CorrelationBuddyConfig/>
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

using ProductionControl_x86;

// ----------- END of Custom Reference 'Usings' --------------- //
//////////////////////////////////////////////////////////////////////////////////
#endregion Custom Reference Section


public class TestPlan : MarshalByRefObject, IATFTest
{
    MyDUT myDUT;

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

    public string DoATFInit(string args)
    {
        Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFInit: {0}\nDo Minimum HW Init:\n{1}\n", args, ATFInitializer.DoMinimumHWInit());


        #region Custom Init Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom Init Coding here --------------- //

        myDUT = new MyDUT(ref sb);
        myDUT.tmpUnit_No = 0;

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

#if (!DEBUG)
    myDUT.tmpUnit_No = Convert.ToInt32(ATFCrossDomainWrapper.GetClothoCurrentSN());
#else
        myDUT.tmpUnit_No++;      // Need to enable this during debug mode
#endif

        ATFResultBuilder.Reset();
        FirstTest = false;
        myDUT.RunTest(ref result);

        // ----------- END of Custom Test Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom Test Coding Section

        //ATFReturnResult result = new ATFReturnResult();
        //ATFResultBuilder.AddResult(ref result, "PARAM", "X", 0.01);
        return result;
    }

    private void LockClothoInputUI()
    {
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(5);
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(10);
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(15);
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(20);
        Clotho.EnableClothoTextBoxes(false);
    }

}
