using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct Cdma2000Bcch
{
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