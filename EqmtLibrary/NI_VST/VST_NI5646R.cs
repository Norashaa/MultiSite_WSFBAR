using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Broadcom.Tests.NoiseFloor;
using NationalInstruments;
using NationalInstruments.SystemsEngineering.Hardware;
using NationalInstruments.SystemsEngineering.SignalProcessing;
using System.IO;
using LabVIEWFilters;
using Broadcom.Tests;

namespace EqmtLibrary.NI5646R
{
    public partial class VST_NI5646R
    {
        private static int testSite;
        private static Transceiver[] VST;        

        public S_MultiRBW_Data[] MultiRBW_Data;
        public S_MutSignal_Setting MutSignal_Setting;
        public S_NoiseConfig[] NoiseConfig;
        public s_SignalType SignalType;

        #region Constructor

        //Constructor
        public VST_NI5646R(string[] vst_address)
        {
            testSite = vst_address.Length;
            VST = new Transceiver[testSite];
            NoiseConfig = new S_NoiseConfig[testSite];

            NF_VSTDriver.SignalType = new s_SignalType[Enum.GetNames(typeof(NoiseFloorWaveformMode)).Length];

            // Initialize hardware - Transceiver
            for (int i = 0; i < vst_address.Length; i++)
            {
                VST[i] = new Transceiver(vst_address[i]);
                VST[i].Initialize();
            }
        }       

        #endregion

        #region Config and Init MultiSite

        public void Mod_FormatCheck(string strWaveform, string strWaveformName, string strmutateCond, bool WaveformInitalLoad)
        {

            #region Variable
            double papr_dB;
            ComplexDouble[] iqDataArr;

            string org_SG_IPath = NF_VSTDriver.SG_Path + strWaveform + @"\I_" + strWaveformName + ".txt";
            string org_SG_QPath = NF_VSTDriver.SG_Path + strWaveform + @"\Q_" + strWaveformName + ".txt";
            string mut_SG_IPath = NF_VSTDriver.SG_Path + strWaveform + @"\MUTSIG\I_" + strWaveformName + ".txt";
            string mut_SG_QPath = NF_VSTDriver.SG_Path + strWaveform + @"\MUTSIG\Q_" + strWaveformName + ".txt";
            //check mutate signal folder
            if (!Directory.Exists(NF_VSTDriver.SG_Path + strWaveform + @"\MUTSIG\"))
                Directory.CreateDirectory(NF_VSTDriver.SG_Path + strWaveform + @"\MUTSIG\");
            #endregion

            #region set IQ Rate and status
            NoiseFloorWaveformMode ModulationType;
            ModulationType = (NoiseFloorWaveformMode)Enum.Parse(typeof(NoiseFloorWaveformMode), strWaveform.ToUpper());
            int arrayNo = (int)Enum.Parse(ModulationType.GetType(), ModulationType.ToString());         //to get the int value from System.Enum

            NF_VSTDriver.SignalType[arrayNo].signalMode = ModulationType.ToString();

            switch (ModulationType)
            {
                case NoiseFloorWaveformMode.CW:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 1e6;        //set to default CW Rate
                    break;
                case NoiseFloorWaveformMode.CDMA2K:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 4.9152e6;
                    break;
                case NoiseFloorWaveformMode.CDMA2KRC1:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 4.9152e6;
                    break;
                case NoiseFloorWaveformMode.GSM850:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM900:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM1800:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM1900:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM850A:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM900A:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM1800A:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.GSM1900A:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    break;
                case NoiseFloorWaveformMode.IS95A:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 4.9152e6;
                    break;
                case NoiseFloorWaveformMode.WCDMA:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;
                case NoiseFloorWaveformMode.WCDMAUL:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;
                case NoiseFloorWaveformMode.WCDMAGTC1:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;
                case NoiseFloorWaveformMode.LTE10M1RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 15.36e6;
                    break;
                case NoiseFloorWaveformMode.LTE10M12RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 15.36e6;
                    break;
                case NoiseFloorWaveformMode.LTE10M20RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 15.36e6;
                    break;
                case NoiseFloorWaveformMode.LTE10M48RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 15.36e6;
                    break;
                case NoiseFloorWaveformMode.LTE10M50RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 15.36e6;
                    break;
                case NoiseFloorWaveformMode.LTE15M75RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 23.04e6;
                    break;
                case NoiseFloorWaveformMode.LTE5M25RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;
                case NoiseFloorWaveformMode.LTE5M8RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;
                case NoiseFloorWaveformMode.LTE20M100RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 30.72e6;
                    break;
                case NoiseFloorWaveformMode.LTE20M18RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 30.72e6;
                    break;
                case NoiseFloorWaveformMode.LTE20M48RB:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 30.72e6;
                    break;
                case NoiseFloorWaveformMode.LTE5MCUSTOM:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;
                case NoiseFloorWaveformMode.LTE10MCUSTOM:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 15.36e6;
                    break;
                case NoiseFloorWaveformMode.LTE15MCUSTOM:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 23.04e6;
                    break;
                case NoiseFloorWaveformMode.LTE20MCUSTOM:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 30.72e6;
                    break;
                case NoiseFloorWaveformMode.CDMA2KCUSTOM:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 4.9152e6;
                    break;
                case NoiseFloorWaveformMode.WCDMACUSTOM:
                    NF_VSTDriver.SignalType[arrayNo].status = true;
                    NF_VSTDriver.SignalType[arrayNo].SG_IQRate = 7.68e6;
                    break;

                default: throw new Exception("Not such a waveform!");
            }
            #endregion

            #region Normal or Mutate Modulation Signal Generation

            if (NF_VSTDriver.SignalType[arrayNo].status)
            {

                if (NF_VSTDriver.SignalType[arrayNo].signalMode == "CW")
                {
                    NF_VSTDriver.SignalType[arrayNo].SG_papr_dB = 0;
                }
                else
                {
                    // Read IQ data and calculate PAPR offset for given modulation
                    //Read original waveform
                    iqData_Array(org_SG_IPath, org_SG_IPath, out iqDataArr);
                    papr_dB = SignalAnalysis.PAPR(iqDataArr);
                    NF_VSTDriver.SignalType[arrayNo].SG_papr_dB = Math.Round(papr_dB, 3);

                    // Read IQ data and calculate PAPR offset for given modulation
                    //double[] SG_Idata, SG_Qdata;
                    //ComplexDouble.DecomposeArray(iqDataArr, out SG_Idata, out SG_Qdata);
                    //Filters.PAPR(SG_Idata, SG_Qdata, out papr_dB);
                    //NF_VSTDriver.SignalType[arrayNo].SG_papr_dB = papr_dB;

                    if (MutSignal_Setting.enable)
                    {
                        //NF_VSTDriver.SignalType[arrayNo].SG_IPath = mut_SG_IPath;
                        //NF_VSTDriver.SignalType[arrayNo].SG_QPath = mut_SG_QPath;

                        //var mutSG_Idata = new double[iqDataArr.Length];
                        //var mutSG_Qdata = new double[iqDataArr.Length];

                        //Filters.MutateWaveform(SG_Idata.ToArray(), SG_Qdata.ToArray(), NF_VSTDriver.SignalType[arrayNo].SG_IQRate, total_time_sec, mod_time_sec, mod_offset_sec, freq_offset_hz, f_off_delay_sec, out mutSG_Idata, out mutSG_Qdata);

                        //string[] tempIdata = Array.ConvertAll(mutSG_Idata, Convert.ToString);
                        //System.IO.File.WriteAllLines(mut_SG_IPath, tempIdata);

                        //string[] tempQdata = Array.ConvertAll(mutSG_Qdata, Convert.ToString);
                        //System.IO.File.WriteAllLines(mut_SG_QPath, tempQdata);
                    }
                    else
                    {
                        // Set the modulation file path to default if mutate signal no required
                        NF_VSTDriver.SignalType[arrayNo].SG_IPath = org_SG_IPath;
                        NF_VSTDriver.SignalType[arrayNo].SG_QPath = org_SG_QPath;
                    }
                }
            }

            #endregion

        }

        #endregion

        #region utilities

        public void iqDataCW_Array(out ComplexDouble[] iqDataArray)
        {
            List<ComplexDouble> iqData = new List<ComplexDouble>();

            //Generate CW waveform
            for (int x = 0; x < 10000; x++)
            {
                iqData.Add(new ComplexDouble(1, 0));
            }

            //convert from list to Complex Double Array
            iqDataArray = new ComplexDouble[iqData.Count];
            iqDataArray = iqData.ToArray();
        }
        public void iqData_Array(string IPath, string QPath, out ComplexDouble[] iqDataArray)
        {
            List<ComplexDouble> iqData = new List<ComplexDouble>();

            // read iq data from file
            StreamReader iReader = new StreamReader(File.OpenRead(IPath));
            StreamReader qReader = new StreamReader(File.OpenRead(QPath));

            while (!(iReader.EndOfStream || qReader.EndOfStream))
            {
                double i = double.Parse(iReader.ReadLine());
                double q = double.Parse(qReader.ReadLine());
                iqData.Add(new ComplexDouble(i, q));
            }

            //convert from list to Complex Double Array
            iqDataArray = new ComplexDouble[iqData.Count];
            iqDataArray = iqData.ToArray();
        }

        public static ComplexDouble[] normalizeIQ(ComplexDouble[] wfm)
        {
            // Calculate PAPR and Normalize IQ
            double vmax = ComplexDouble.GetMagnitudes(wfm.ToArray()).Max();
            
            return ComplexDouble.ComposeArrayPolar(
                ComplexDouble.GetMagnitudes(wfm.ToArray()).Select(v => v / vmax).ToArray(),
                ComplexDouble.GetPhases(wfm.ToArray()));
            
        }

        public static List<double> File_ReadData(string FilePath)
        {
            try
            {
                var Data = new List<double>();

                var SGdataReader = new StreamReader(File.OpenRead(FilePath));

                while (!SGdataReader.EndOfStream)
                {
                    Data.Add(Convert.ToDouble(SGdataReader.ReadLine()));
                }

                return Data;
            }

            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(@"Data file not found.");
            }
        }
        public void Get_s_SignalType(string strWaveform, string strWaveformName, out s_SignalType value)
        {
            //Get the all value from 'struct s_SignalType'
            NoiseFloorWaveformMode ModulationType;
            ModulationType = (NoiseFloorWaveformMode)Enum.Parse(typeof(NoiseFloorWaveformMode), strWaveformName.ToUpper());
            int arrayNo = (int)Enum.Parse(ModulationType.GetType(), ModulationType.ToString());         //to get the int value from System.Enum

            value = NF_VSTDriver.SignalType[arrayNo];
        }

        #endregion

        #region Test Method

        public void BuildVSTTest(out ITest[] T, string testMode, string strWaveform, string strWaveformName, S_NoiseConfig[] noiseConfig)
        {
            /// A quick note about broadcasting a configuration..
            /// Broadcast will over-write pre-existing configuration data.
            /// That is why I broadcast first then configure triggers and broadcast the waveform.
            /// Iterate through the sites and change a configuration parameter to preserve existing settings.

            s_SignalType value = new s_SignalType();
            Get_s_SignalType(strWaveform, strWaveformName, out value);

            switch (testMode.ToUpper())
            {
                case "NOISE":
                    T = BuildNFTest(strWaveform, strWaveformName, noiseConfig);
                    break;

                case "CONTACT":
                    T = BuildContactTest(noiseConfig);
                    break;
                    
                case "POWERSERVO":
                    T = BuildPwrServoTest(strWaveform, strWaveformName, noiseConfig);
                    break;                    

                default: throw new Exception("Not such a test mode! >> " + testMode.ToUpper());
            }
        }

        #endregion
    }
}
