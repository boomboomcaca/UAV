using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct LteIntraFreqNeighCellListStruct
{
    public readonly short phys_cell_id;
    public readonly short q_offset_range;
}

[StructLayout(LayoutKind.Sequential)]
public struct LteEnbConfig
{
    //public uint NCellID;
    //public uint NDLRB; //{6,15,25,50,75,100};
    //public uint BandWidth;//{1.4,3,5,10,15,20}
    //public uint CP_TYPE; // NORMAL_CP EXTEND_CP
    //public uint duplex; //TDD=1,FDD=2
    //public uint CellRefP;
    //public uint phich_config;
    //public uint SFN;
    //public uint TDD_uplink_downlink_config; //0,1,2...6
    //public uint CFI;   //jimbo  cell_resel_prio
    //public uint CI; // CELL ID
    //public uint TAC; //tracking area code, like LAC in GSM or WCDMA
    //public uint MCC;
    //public uint MNC;
    //public uint freqband;
    //public uint freq;
    //public uint UARFCN;
    //public uint subFrame0Idx;
    //public uint si_window_length;
    ////////SIB4/////
    //public uint intra_freq_neigh_cell_list_size;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    //public lte_intraFreqNeighCellList_struct[] intraFreqNeighCellList;
    ////// SIB5 parameters
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    //public byte[] sibx_gotten;
    //public LibLte.LIBLTE_RRC_SYS_INFO_BLOCK_TYPE_5_STRUCT SIB5_struct;
    //public int RSRP;
    //public int RSSI;
    //public float RSRQ;
    //public float snr;
    //public int referenceSignalPower;
    //public int fake_bs_flag;
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

public static class LibLte
{
    public enum LiblteRrcAllowedMeasBandwidthEnum
    {
        LiblteRrcAllowedMeasBandwidthMbw6 = 0,
        LiblteRrcAllowedMeasBandwidthMbw15,
        LiblteRrcAllowedMeasBandwidthMbw25,
        LiblteRrcAllowedMeasBandwidthMbw50,
        LiblteRrcAllowedMeasBandwidthMbw75,
        LiblteRrcAllowedMeasBandwidthMbw100,
        LiblteRrcAllowedMeasBandwidthNItems
    }

    public enum LiblteRrcPhysCellIdRangeEnum
    {
        LiblteRrcPhysCellIdRangeN4 = 0,
        LiblteRrcPhysCellIdRangeN8,
        LiblteRrcPhysCellIdRangeN12,
        LiblteRrcPhysCellIdRangeN16,
        LiblteRrcPhysCellIdRangeN24,
        LiblteRrcPhysCellIdRangeN32,
        LiblteRrcPhysCellIdRangeN48,
        LiblteRrcPhysCellIdRangeN64,
        LiblteRrcPhysCellIdRangeN84,
        LiblteRrcPhysCellIdRangeN96,
        LiblteRrcPhysCellIdRangeN128,
        LiblteRrcPhysCellIdRangeN168,
        LiblteRrcPhysCellIdRangeN252,
        LiblteRrcPhysCellIdRangeN504,
        LiblteRrcPhysCellIdRangeSpare2,
        LiblteRrcPhysCellIdRangeSpare1,
        LiblteRrcPhysCellIdRangeN1,
        LiblteRrcPhysCellIdRangeNItems
    }

    public enum LiblteRrcQOffsetRangeEnum
    {
        LiblteRrcQOffsetRangeDbN24 = 0,
        LiblteRrcQOffsetRangeDbN22,
        LiblteRrcQOffsetRangeDbN20,
        LiblteRrcQOffsetRangeDbN18,
        LiblteRrcQOffsetRangeDbN16,
        LiblteRrcQOffsetRangeDbN14,
        LiblteRrcQOffsetRangeDbN12,
        LiblteRrcQOffsetRangeDbN10,
        LiblteRrcQOffsetRangeDbN8,
        LiblteRrcQOffsetRangeDbN6,
        LiblteRrcQOffsetRangeDbN5,
        LiblteRrcQOffsetRangeDbN4,
        LiblteRrcQOffsetRangeDbN3,
        LiblteRrcQOffsetRangeDbN2,
        LiblteRrcQOffsetRangeDbN1,
        LiblteRrcQOffsetRangeDb0,
        LiblteRrcQOffsetRangeDb1,
        LiblteRrcQOffsetRangeDb2,
        LiblteRrcQOffsetRangeDb3,
        LiblteRrcQOffsetRangeDb4,
        LiblteRrcQOffsetRangeDb5,
        LiblteRrcQOffsetRangeDb6,
        LiblteRrcQOffsetRangeDb8,
        LiblteRrcQOffsetRangeDb10,
        LiblteRrcQOffsetRangeDb12,
        LiblteRrcQOffsetRangeDb14,
        LiblteRrcQOffsetRangeDb16,
        LiblteRrcQOffsetRangeDb18,
        LiblteRrcQOffsetRangeDb20,
        LiblteRrcQOffsetRangeDb22,
        LiblteRrcQOffsetRangeDb24,
        LiblteRrcQOffsetRangeNItems
    }

    public enum LiblteRrcSssfHighEnum
    {
        LiblteRrcSssfHigh0Dot25 = 0,
        LiblteRrcSssfHigh0Dot5,
        LiblteRrcSssfHigh0Dot75,
        LiblteRrcSssfHigh1Dot0,
        LiblteRrcSssfHighNItems
    }

    public enum LiblteRrcSssfMediumEnum
    {
        LiblteRrcSssfMedium0Dot25 = 0,
        LiblteRrcSssfMedium0Dot5,
        LiblteRrcSssfMedium0Dot75,
        LiblteRrcSssfMedium1Dot0,
        LiblteRrcSssfMediumNItems
    }

    public const int LiblteRrcMaxFreq = 8;
    public const int LiblteRrcMaxCellInter = 16;
    public const int LiblteRrcMaxCellBlack = 16;

    [StructLayout(LayoutKind.Sequential)]
    public struct LiblteRrcInterFreqNeighCellStruct
    {
        public readonly LiblteRrcQOffsetRangeEnum q_offset_cell;
        public readonly ushort phys_cell_id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LiblteRrcPhysCellIdRangeStruct
    {
        public readonly LiblteRrcPhysCellIdRangeEnum range;
        public readonly ushort start;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LiblteRrcSpeedStateScaleFactorsStruct
    {
        public readonly LiblteRrcSssfMediumEnum sf_medium;
        public readonly LiblteRrcSssfHighEnum sf_high;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LiblteRrcInterFreqCarrierFreqInfoStruct
    {
        public LiblteRrcSpeedStateScaleFactorsStruct t_resel_eutra_sf;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LiblteRrcMaxCellInter)]
        public readonly LiblteRrcInterFreqNeighCellStruct[] inter_freq_neigh_cell_list;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LiblteRrcMaxCellBlack)]
        public readonly LiblteRrcPhysCellIdRangeStruct[] inter_freq_black_cell_list;

        public readonly LiblteRrcAllowedMeasBandwidthEnum allowed_meas_bw;
        public readonly LiblteRrcQOffsetRangeEnum q_offset_freq;
        public readonly ushort dl_carrier_freq;
        public readonly short q_rx_lev_min;
        public readonly byte t_resel_eutra;
        public readonly byte threshx_high;
        public readonly byte threshx_low;
        public readonly byte cell_resel_prio;
        public readonly byte neigh_cell_cnfg;
        public readonly byte inter_freq_neigh_cell_list_size;
        public readonly byte inter_freq_black_cell_list_size;
        public readonly byte p_max;
        [MarshalAs(UnmanagedType.I1)] public readonly bool presence_ant_port_1;
        [MarshalAs(UnmanagedType.I1)] public readonly bool p_max_present;
        [MarshalAs(UnmanagedType.I1)] public readonly bool t_resel_eutra_sf_present;
        [MarshalAs(UnmanagedType.I1)] public readonly bool cell_resel_prio_present;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LiblteRrcSysInfoBlockType5Struct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LiblteRrcMaxFreq)]
        public readonly LiblteRrcInterFreqCarrierFreqInfoStruct[] inter_freq_carrier_freq_list;

        public readonly uint inter_freq_carrier_freq_list_size;
    }
}