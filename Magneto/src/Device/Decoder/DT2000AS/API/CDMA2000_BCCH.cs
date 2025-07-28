using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct Cdma2000Bcch
{
    //public byte PD;
    //public byte MSG_ID;
    //public byte P_REV;
    //public byte MIN_P_REV;
    //public ushort SID;
    //public ushort NID;
    //public ushort PILOT_PN;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    //public sbyte/*char*/[] LC_STATE/*[42]*/;
    ////public uint SYS_TIME_High32;
    ////public uint SYS_TIME_Low32;
    //public byte LP_SEC;
    //public byte LTM_OFF;
    //public byte DAYLT;
    //public byte PRAT;
    //public ushort CDMA_FREQ;
    //public ushort EXT_CDMA_FREQ;
    //public byte SR1_BCCH_SUPPORTED;
    ////// SPM 参数
    //public byte CONFIG_MSG_SEQ;
    //public ushort REG_ZONE;
    //public byte TOTAL_ZONES;
    //public byte ZONE_TIMER;
    //public byte MULT_SIDS;
    //public byte MULT_NIDS;
    //public ushort BASE_ID;
    //public byte BASE_CLASS;
    //public byte PAGE_CHAN;
    //public byte MAX_SLOT_CYCLE_INDEX;
    //public byte HOME_REG;
    //public byte FOR_SID_REG;
    //public byte FOR_NID_REG;
    //public byte POWER_UP_REG;
    //public byte POWER_DOWN_REG;
    //public byte PARAMETER_REG;
    //public byte REG_PRD;
    //public float BASE_LAT;
    //public float BASE_LONG;
    //public ushort REG_DIST;
    //public byte SRCH_WIN_A;
    //public byte SRCH_WIN_N;
    //public byte SRCH_WIN_R;
    //public byte NGHBR_MAX_AGE;
    //public byte PWR_REP_THRESH;
    //public byte PWR_REP_FRAMES;
    //public byte PWR_THRESH_ENABLE;
    //public byte PWR_PERIOD_ENABLE;
    //public byte PWR_REP_DELAY;
    //public byte RESCAN;
    //public byte T_ADD;
    //public byte T_DROP;
    //public byte T_COMP;
    //public byte T_TDROP;
    //public byte EXT_SYS_PARAMETER;
    //public byte EXT_NGHBR_LIST;
    //public byte GEN_NGHBR_LIST;
    //public byte GLOBAL_REDIRECT;
    //public byte PRI_NGHBR_LIST;
    //public byte USER_ZONE_ID;
    //public byte EXT_GLOBAL_REDIRECT;
    //public byte EXT_CHAN_LIST;
    ///////////Extended System Parameters//////////////////////////////////
    //public byte DELETE_FOR_TMSI;
    //public byte USE_TMSI;
    //public byte PREF_MSID_TYPE;
    //public ushort MCC;
    //public byte IMSI_11_12;
    //public byte TMSI_ZONE_LEN;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    //public byte[] TMSI_ZONE/*[8]*/;
    //public byte BCAST_INDEX;
    //public byte IMSI_T_SUPPORTED;
    //public byte SOFT_SLOPE;
    //public byte ADD_INTERCEPT;
    //public byte DROP_INTERCEPT;
    //public byte PACKET_ZONE_ID;
    //public byte MAX_NUM_ALT_SO;
    //public byte RESELECT_INCLUDED;
    //public byte EC_THRESH;
    //public byte EC_IO_THRESH;
    //public byte PILOT_REPORT;
    //public byte NGHBR_SET_ENTRY_INFO;
    //public byte NUM_FREQ;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    //public ushort[] ChannelList_CDMA_FREQ/*[8]*/;
    //public byte PILOT_INC;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    //public byte[] NGHBR_CONFIG/*[64]*/;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    //public ushort[] NGHBR_PN/*[64]*/;
    //public byte ACC_MSG_SEQ;
    //public byte ACC_CHAN;
    //public byte NOM_PWR;
    //public byte INIT_PWR;
    //public byte PWR_STEP;
    //public byte NUM_STEP;
    //public byte MAX_CAP_SZ;
    //public byte PAM_SZ;
    //public byte PSIST_0_9;
    //public byte PSIST_10;
    //public byte PSIST_11;
    //public byte PSIST_12;
    //public byte PSIST_13;
    //public byte PSIST_14;
    //public byte PSIST_15;
    //public byte MSG_PSIST;
    //public byte REG_PSIST;
    //public byte PROBE_PN_RAN;
    //public byte ACC_TMO;
    //public byte PROBE_BKOFF;
    //public byte BKOFF;
    //public byte MAX_REQ_SEQ;
    //public byte MAX_RSP_SEQ;
    //public byte AUTH;
    //public uint RAND;
    //public byte NOM_PWR_EXT;
    //public byte PSIST_EMG_INCL;
    //public byte PSIST_EMG;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    //public sbyte/*char*/ [] Page1_mask/*[42]*/;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    //public sbyte/*char*/[] Page2_mask/*[42]*/;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    //public sbyte/*char*/[] Page3_mask/*[42]*/;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    //public sbyte/*char*/[] Page4_mask/*[42]*/;
    //////////以下参数为非系统参数，为测量所得
    //public int rssi;
    //public uint centerfrequency;
    //public int fake_bs_flag;
    public readonly sbyte PD;
    public readonly sbyte MSG_ID;
    public readonly sbyte P_REV;
    public readonly sbyte MIN_P_REV;
    public readonly ushort SID; //System Identification Number  系统ID
    public readonly ushort NID; //network Identification Number 网络ID
    public readonly ushort PILOT_PN; // 导频码

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public readonly char[] LC_STATE;

    public readonly sbyte LP_SEC;
    public readonly sbyte LTM_OFF;
    public readonly sbyte DAYLT;
    public readonly sbyte PRAT;
    public readonly ushort CDMA_FREQ; // CDMA频率号
    public readonly ushort EXT_CDMA_FREQ;
    public readonly sbyte SR1_BCCH_SUPPORTED;
    public readonly sbyte CONFIG_MSG_SEQ;
    public readonly ushort REG_ZONE;
    public readonly sbyte TOTAL_ZONES;
    public readonly sbyte ZONE_TIMER;
    public readonly sbyte MULT_SIDS;
    public readonly sbyte MULT_NIDS;
    public readonly ushort BASE_ID; //basestation ID， 基站ID
    public readonly sbyte BASE_CLASS;
    public readonly sbyte PAGE_CHAN;
    public readonly sbyte MAX_SLOT_CYCLE_INDEX;
    public readonly sbyte HOME_REG;
    public readonly sbyte FOR_SID_REG;
    public readonly sbyte FOR_NID_REG;
    public readonly sbyte POWER_UP_REG;
    public readonly sbyte POWER_DOWN_REG;
    public readonly sbyte PARAMETER_REG;
    public readonly sbyte REG_PRD;
    public readonly float BASE_LAT; //纬度
    public readonly float BASE_LONG; //精度
    public readonly ushort REG_DIST;
    public readonly sbyte SRCH_WIN_A;
    public readonly sbyte SRCH_WIN_N;
    public readonly sbyte SRCH_WIN_R;
    public readonly sbyte NGHBR_MAX_AGE;
    public readonly sbyte PWR_REP_THRESH;
    public readonly sbyte PWR_REP_FRAMES;
    public readonly sbyte PWR_THRESH_ENABLE;
    public readonly sbyte PWR_PERIOD_ENABLE;
    public readonly sbyte PWR_REP_DELAY;
    public readonly sbyte RESCAN;
    public readonly sbyte T_ADD;
    public readonly sbyte T_DROP;
    public readonly sbyte T_COMP;
    public readonly sbyte T_TDROP;
    public readonly sbyte EXT_SYS_PARAMETER;
    public readonly sbyte EXT_NGHBR_LIST;
    public readonly sbyte GEN_NGHBR_LIST;
    public readonly sbyte GLOBAL_REDIRECT;
    public readonly sbyte PRI_NGHBR_LIST;
    public readonly sbyte USER_ZONE_ID;
    public readonly sbyte EXT_GLOBAL_REDIRECT;
    public readonly sbyte EXT_CHAN_LIST;
    public readonly sbyte DELETE_FOR_TMSI;
    public readonly sbyte USE_TMSI;
    public readonly sbyte PREF_MSID_TYPE;
    public readonly ushort MCC; //国家码
    public readonly sbyte IMSI_11_12;
    public readonly sbyte TMSI_ZONE_LEN;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly sbyte[] TMSI_ZONE;

    public readonly sbyte BCAST_INDEX;
    public readonly sbyte IMSI_T_SUPPORTED;
    public readonly sbyte SOFT_SLOPE;
    public readonly sbyte ADD_INTERCEPT;
    public readonly sbyte DROP_INTERCEPT;
    public readonly sbyte PACKET_ZONE_ID;
    public readonly sbyte MAX_NUM_ALT_SO;
    public readonly sbyte RESELECT_INCLUDED;
    public readonly sbyte EC_THRESH;
    public readonly sbyte EC_IO_THRESH;
    public readonly sbyte PILOT_REPORT;
    public readonly sbyte NGHBR_SET_ENTRY_INFO;
    public readonly sbyte NUM_FREQ;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly ushort[] ChannelList_CDMA_FREQ;

    public readonly sbyte PILOT_INC;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public readonly sbyte[] NGHBR_CONFIG;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public readonly ushort[] NGHBR_PN;

    public readonly sbyte ACC_MSG_SEQ;
    public readonly sbyte ACC_CHAN;
    public readonly sbyte NOM_PWR;
    public readonly sbyte INIT_PWR;
    public readonly sbyte PWR_STEP;
    public readonly sbyte NUM_STEP;
    public readonly sbyte MAX_CAP_SZ;
    public readonly sbyte PAM_SZ;
    public readonly sbyte PSIST_0_9;
    public readonly sbyte PSIST_10;
    public readonly sbyte PSIST_11;
    public readonly sbyte PSIST_12;
    public readonly sbyte PSIST_13;
    public readonly sbyte PSIST_14;
    public readonly sbyte PSIST_15;
    public readonly sbyte MSG_PSIST;
    public readonly sbyte REG_PSIST;
    public readonly sbyte PROBE_PN_RAN;
    public readonly sbyte ACC_TMO;
    public readonly sbyte PROBE_BKOFF;
    public readonly sbyte BKOFF;
    public readonly sbyte MAX_REQ_SEQ;
    public readonly sbyte MAX_RSP_SEQ;
    public readonly sbyte AUTH;
    public readonly uint RAND;
    public readonly sbyte NOM_PWR_EXT;
    public readonly sbyte PSIST_EMG_INCL;
    public readonly sbyte PSIST_EMG;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public readonly char[] Page1_mask;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public readonly char[] Page2_mask;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public readonly char[] Page3_mask;

    public readonly short Ec;
    public readonly short EcIo;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public readonly char[] Reserved;

    public readonly int paging_count;
    public readonly int packet_count;
    public readonly int rssi; //接收电平
    public readonly uint centerfrequency; //中心频率 (in Hz)
    public readonly int fake_bs_flag; //伪基站标志，=1时为伪基站
}