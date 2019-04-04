using System;

namespace Broadcom.Tests.NoiseFloor
{
    public class NF_VSTDriver
    {
        //***RFSG***   
        public const Boolean UseWaveformFile = true;
        public static s_SignalType[] SignalType;
        public const string SG_Path = @"C:\Avago.ATF.Common\Input\Waveform\";
    }

    public enum NoiseFloorWaveformMode
    {
        CW,
        CDMA2K,
        CDMA2KRC1,
        CDMA2KCUSTOM,
        GSM850,
        GSM900,
        GSM1800,
        GSM1900,
        GSM850A,
        GSM900A,
        GSM1800A,
        GSM1900A,
        IS95A,
        IS98,
        WCDMA,
        WCDMAUL,
        WCDMAGTC1,
        WCDMACUSTOM,
        LTETD5M8RB,
        LTETD10M12RB,
        LTETD10M50RB,
        LTE10M1RB,
        LTE10M12RB,
        LTE10M20RB,
        LTE10M50RB,
        LTE10M48RB,
        LTE10MCUSTOM,
        LTE15M75RB,
        LTE15MCUSTOM,
        LTE5M25RB,
        LTE5M8RB,
        LTE5MCUSTOM,
        LTE20M100RB,
        LTE20M18RB,
        LTE20M48RB,
        LTE20MCUSTOM
    }
    
    public struct s_SignalType
    {
        public bool status;
        public double SG_IQRate;
        public int signalLength;
        public string signalMode;
        public string SG_IPath;
        public string SG_QPath;
        public double SG_papr_dB;
    }

    public struct S_MutSignal_Setting
    {
        public bool enable;
        public double total_time_sec;
        public double mod_time_sec;
        public double mod_offset_sec;
        public double freq_offset_hz;
        public double f_off_delay_sec;
    }

    public struct S_MultiRBW_Data
    {
        public double RBW_Hz;
        public double[,] rsltTrace;             //double[sweepPts dbm, traceNo] rsltTrace;
        public double[] rsltMaxHoldTrace;       //double[sweepPts dbm] rsltMaxHoldTrace;
        public double[,] multiTraceData;        //double[sweepPts dbm, traceNo] multiTraceData;
    }
    public struct S_Multisite_TrData
    {
        public S_MultiRBW_Data[] multiSite_TrData;  //S_MultiRBW_Data[testsite_no]
    }

    public struct S_NoiseConfig
    {
        public int NumberOfRuns;
        public string Band;
        public string Modulation;
        public string WaveformName;
        public double TXFrequencyStart;
        public double TXFrequencyStop;
        public double TXFrequencyStep;
        public double DwellTime;
        public double RXFrequencyStart;
        public double RXFrequencyStop;
        public double RXFrequencyStep;
        public double SGPowerLevel;
        public double SAReferenceLevel;
        public double SoakTime;
        public double SoakFrequency;
        public double Rbw;
        public double Vbw;
        public bool preSoakSweep;
        public double multiplier_RXIQRate;
        public double[] Bandwidths;
    }

}
