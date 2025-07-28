using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct LteEnbConfig
{
    /// MIB 参数
    public readonly short NCellID; //PCI  physical cell id

    public readonly short NDLRB; //number of RB, 物理资源块数 {6,15,25,50,75,100};
    public readonly short BandWidth; //信号带宽{1.4,3,5,10,15,20}
    public readonly short CP_TYPE; // NORMAL_CP EXTEND_CP
    public readonly short duplex; //双工类型TDD=1,FDD=2
    public readonly short CellRefP;
    public readonly short phich_config;
    public readonly short SFN; // 系统帧号 system frame number
    public readonly short TDD_uplink_downlink_config; //0,1,2...6
    public readonly int cellIdentity; // 28 bits  cellIdentity= [eNodeB CI]
    public readonly int eNodeB; //  eNodeB = cellIdentity>>8  
    public readonly int CI; //  CI = cellIdentity&0xff;
    public readonly ushort TAC; //tracking Area Code
    public readonly short MCC; //国家码

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly short[] MNC; //运营商代码

    public readonly short plmn_num; //运营商个数
    public readonly short freqband; //频段  
    public readonly short qRxLevMin;
    public readonly short pMax;
    public readonly short numberOfRAPreambles;
    public readonly short sizeOfRAPreamblesGroupA;
    public readonly short messageSizeGroupA;
    public readonly short messagePowerOffsetGroupB;
    public readonly short powerRampingStep;
    public readonly short preambleInitialReceivedTargetPower;
    public readonly short preambleTransMax;
    public readonly short raResponseWindowSize;
    public readonly short macContentionResolutionTimer;
    public readonly short maxHARQMsg3Tx;
    public readonly short modificationPeriodCoeff;
    public readonly short defaultPagingCycle;
    public readonly short nB;
    public readonly short rootSequenceIndex;
    public readonly short prachConfigIndex;
    public readonly short zeroCorrelationZoneConfig;
    public readonly short prachFreqOffset;
    public readonly short referenceSignalPower;
    public readonly short pb;
    public readonly short nSB;
    public readonly short hoppingMode;
    public readonly short puschHoppingOffset;
    public readonly short enable64QAM;
    public readonly short groupHoppingEnabled;
    public readonly short groupAssignmentPUSCH;
    public readonly short sequenceHoppingEnabled;
    public readonly short cyclicShift;
    public readonly short ulCarrierFreq;
    public readonly short ulBandwidth;
    public readonly short additionalSpectrumEmission;
    public readonly short timeAlignmentTimerCommon;
    public readonly short qHyst;
    public readonly short sNonIntraSearch;
    public readonly short threshServingLow;
    public readonly short cellReselectionPriority;
    public readonly short sIntraSearch;
    public readonly short presenceAntennaPort1;
    public readonly short neighCellConfig;
    public readonly short tReselectionEUTRA;
    public readonly short sIntraSearchPr9;
    public readonly short sIntraSearchQr9;
    public readonly short sNonIntraSearchPr9;
    public readonly short sNonIntraSearchQr9;
    public readonly short qQualMinr9;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly InterFreqCarrierFreqInfoStr[] InterFreqCarrierFreqInfo;

    public readonly uint freq;
    public readonly uint UARFCN;
    public readonly uint subFrame0Idx;
    public readonly uint paging_count;
    public readonly short RSRP;
    public readonly short RSSI;
    public readonly float RSRQ;
    public readonly float snr;
    public readonly short fake_bs_flag;
    public readonly uint sibx_gotten_mask; // 指示系统消息x是不是收到
    public readonly uint sibx_fprint_mask; // 指示系统消息x是不是输出到文件

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public readonly short[] reserved;
}

[StructLayout(LayoutKind.Sequential)]
public struct InterFreqCarrierFreqInfoStr
{
    public readonly ushort dlCarrierFreq;
    public readonly short qRxLevMin;
    public readonly short pMax;
    public readonly short tReselectionEUTRA;
    public readonly short threshXHigh;
    public readonly short threshXLow;
    public readonly short allowedMeasBandwidth;
    public readonly short presenceAntennaPort1;
    public readonly short cellReselectionPriority;
    public readonly short neighCellConfig;
    public readonly short qOffsetFreq;
}