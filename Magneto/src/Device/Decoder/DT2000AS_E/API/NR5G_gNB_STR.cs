using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct Nr5GgNbStr
{
    public readonly short PCI;
    public readonly long cellIdentity; // 36 bits 
    public readonly int TAC; //tracking Area Code
    public readonly int gNodeB; //  gNodeB = cellIdentity>>12
    public readonly short CI; //  CI = cellIdentity&0xfff;
    public readonly short MCC; //国家码

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly short[] MNC; //运营商代码

    public readonly short plmn_num; //运营商个数
    public readonly short freqband; //频段  n41 n78 等等
    public readonly short NSubcarriers;
    public readonly short SubcarrierSpacing; // 15k 30k 120k etc
    public readonly short RB_num; //273 etc
    public readonly short BandWidth; // 100M 40M  etc
    public readonly short CP_TYPE; // NORMAL_CP EXTEND_CP
    public readonly short duplex; //TDD=1,FDD=2
    public readonly short SFN;
    public readonly int absoluteFrequencyPointA; //pointA的频点号
    public readonly int absoluteFrequencySSB; //SSB的频点号
    public readonly short SSB_GSCN; //同步信道号
    public readonly short k_SSB;
    public readonly short DMRSTypeAPosition;
    public readonly short PDCCHConfigSIB1;
    public readonly short half_radio_frame;
    public readonly short CellBarred;
    public readonly short IntraFreqReselection;
    public readonly long freqSSB; //SSB的中心频率
    public readonly long centerFreq; //全部带宽(100M)的中心频率
    public readonly long pointA_Freq; //pointA的频率，全部带宽(100M)的最低端频率
    public readonly short num_decoded_ssb;
    public Nrsib1Str sib1info;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly Nr5GBeamStr[] Beam_result;

    public readonly int SSB_idx; // the SSB index of strongest signal
    public readonly float RSRP; //the strongest SS RSRP
    public readonly float RSRQ; //the strongest SS RSRQ
    public readonly float SINR; // //the strongest SS SINR
    public readonly float SSB_RSSI; // the maximum rssi of all SSB
    public readonly float demod_snr;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public readonly int[] reserved;
}

[StructLayout(LayoutKind.Sequential)]
public struct Nrsib1Str
{
    public readonly short ranac;
    public readonly short qRxLevMin;
    public readonly short qQualMin;
    public readonly short freqBandIndicatorNR; //频段号  78，41 etc
    public readonly short offsetToPointA;
    public readonly short subcarrierSpacing;
    public readonly short subcarrierSpacing_ul;
    public readonly short offsetToCarrier;
    public readonly short offsetToCarrier_ul;
    public readonly short carrierBandwidth;
    public readonly short carrierBandwidth_ul;
    public readonly int locationAndBandwidth;
    public readonly int locationAndBandwidth_ul;
    public readonly short prachConfigurationIndex;
    public readonly short msg1FDM;
    public readonly short msg1FrequencyStart;
    public readonly short zeroCorrelationZoneConfig;
    public readonly short preambleReceivedTargetPower;
    public readonly short preambleTransMax;
    public readonly short powerRampingStep;
    public readonly short raResponseWindow;
    public readonly short ssbperRACHOccasionAndCBPreamblesPerSSB;
    public readonly short raMsg3SizeGroupA;
    public readonly short messagePowerOffsetGroupB;
    public readonly short numberOfRAPreamblesGroupA;
    public readonly short raContentionResolutionTimer;
    public readonly short rsrpThresholdSSB;
    public readonly short prachRootSequenceIndex;
    public readonly short msg3DeltaPreamble;
    public readonly short p0NominalWithGrant;
    public readonly short pucchResourceCommon;
    public readonly short hoppingId;
    public readonly short p0nominal;
    public readonly short ssbPeriodicityServingCell;
    public readonly short pattern1_dlULTransmissionPeriodicity;
    public readonly short pattern1_nrofDownlinkSlots;
    public readonly short pattern1_nrofDownlinkSymbols;
    public readonly short pattern1_nrofUplinkSlots;
    public readonly short pattern1_nrofUplinkSymbols;
    public readonly short pattern2_dlULTransmissionPeriodicity;
    public readonly short pattern2_nrofDownlinkSlots;
    public readonly short pattern2_nrofDownlinkSymbols;
    public readonly short pattern2_nrofUplinkSlots;
    public readonly short pattern2_nrofUplinkSymbols;
    public readonly short ssPBCHBlockPower;
    public readonly short t300;
    public readonly short t301;
    public readonly short t310;
    public readonly short n310;
    public readonly short t311;
    public readonly short n311;
    public readonly short t319;
}

[StructLayout(LayoutKind.Sequential)]
public struct Nr5GBeamStr
{
    /// <summary>
    ///     =1 if the beam is decoed
    /// </summary>
    public readonly bool decoded_flag;

    /// <summary>
    ///     Received Power on the PSS (dBm/RE)
    /// </summary>
    public readonly float pss_rp;

    /// <summary>
    ///     Received Quality on the PSS (dB/RE)
    /// </summary>
    public readonly float pss_rq;

    /// <summary>
    ///     OPTIONAL: Received Signal to interference and noise ratio on the PSS (dB)
    /// </summary>
    public readonly float pss_cinr;

    /// <summary>
    ///     Received Power on the SSS (dBm/RE)
    /// </summary>
    public readonly float sss_rp;

    /// <summary>
    ///     Received Quality on the SSS (dB)
    /// </summary>
    public readonly float sss_rq;

    /// <summary>
    ///     OPTIONAL: Received Signal to interference and noise ratio on the SSS (dB)
    /// </summary>
    public readonly float sss_cinr;

    /// <summary>
    ///     Received Signal to interference and noise ratio on the PSS and SSS (dB)
    /// </summary>
    public readonly float ss_cinr;

    /// <summary>
    ///     Received Power on the DM-RS of the PBCH (dBm/RE)
    /// </summary>
    public readonly float rspbch_rp;

    /// <summary>
    ///     Received Quality on DM-RS of the PBCH (dB)
    /// </summary>
    public readonly float rspbch_rq;

    /// <summary>
    ///     Received Signal to interference and noise ratio on the DM-RS of the PBCH (dB)
    /// </summary>
    public readonly float rspbch_cinr;

    /// <summary>
    ///     Received Power on the SSB measured over the PSS, SSS, DM-RS PBCH (dBm/RE)
    /// </summary>
    public readonly float ssb_rp;

    /// <summary>
    ///     Received Quality on the SSB (dB)
    /// </summary>
    public readonly float ssb_rq;

    /// <summary>
    ///     Received Signal to interference and noise ratio on the SSB (dB)
    /// </summary>
    public readonly float ssb_cinr;

    /// <summary>
    ///     received rssi on the SSB (dB)
    /// </summary>
    public readonly float ssb_rssi;

    /// <summary>
    ///     PBCH demodulation SNR
    /// </summary>
    public readonly float demod_snr;

    /// <summary>
    ///     ssb_position offset to the  main_2345G(x, ....)
    /// </summary>
    public readonly int ssb_pos;

    /// <summary>
    ///     pbch星座图
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 432)]
    public readonly ComplexFloat[] pbch_qpsk;

    /// <summary>
    ///     pbch_dmrs星座图
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
    public readonly ComplexFloat[] pbch_dmrs_qpsk;

    public readonly float pbch_evm;
    public readonly float pbch_dmrs_evm;
}

public struct ComplexFloat
{
    public float Real;
    public float Imag;
}