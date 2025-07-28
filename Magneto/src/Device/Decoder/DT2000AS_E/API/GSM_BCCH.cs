using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct GsmBcch
{
    public readonly ushort CI; // Cell identity value
    public readonly ushort MCC; //Mobile country code
    public readonly ushort MNC; //Mobile network code
    public readonly ushort LAC; // Location area cod
    public readonly sbyte ATT; //ATT, Attach-detach allowed (octet 2)
    public readonly sbyte BS_AG_BLKS_RES;
    public readonly sbyte CCCH_CONF;
    public readonly sbyte BS_PA_MFRMS;
    public readonly sbyte T3212; //timeout value;
    public readonly sbyte PWRC; //10.5.2.3 Cell Options (BCCH)
    public readonly sbyte DTX; // 10.5.2.3 Cell Options (BCCH)
    public readonly sbyte RADIO_LINK_TIMEOUT; //10.5.2.3 Cell Options (BCCH)
    public readonly sbyte Max_retrans; //Maximum number of retransmissions 10.5.2.29
    public readonly sbyte Tx_integer; //Number of slots to spread transmission 10.5.2.29
    public readonly sbyte CELL_BAR_ACCESS; //1 The cell is barred 10.5.2.29
    public readonly sbyte RE; // 1 Call Reestablishment not allowed in the cell 10.5.2.29
    public readonly sbyte AC_CN; //Access Control Class N access is not barred if the AC CN bit is coded with a "0";
    public readonly sbyte CELL_RESELECT_HYSTERESIS; //RXLEV hysteresis for LA re-selection
    public readonly sbyte MS_TXPWR_MAX_CCH; //binary representation of the "power control level"
    public readonly sbyte ACS;
    public readonly sbyte NECI; // 1 New establishment causes are  supported
    public readonly short RXLEV_ACCESS_MIN; // binary representation of the minimum received signal level
    public readonly sbyte CBQ; //CELL_BAR_QUALIFY (1 bit field); 10.5.2.34 SI 3 Rest Octets
    public readonly sbyte CELL_RESELECT_OFFSET; //(6 bit field) 10.5.2.34 SI 3 Rest Octets
    public readonly sbyte TEMPORARY_OFFSET; // (3 bit field); 10.5.2.34 SI 3 Rest Octets
    public readonly sbyte PENALTY_TIME; // (5 bit field) 10.5.2.34 SI 3 Rest Octets
    public readonly sbyte Power_Offset; //10.5.2.34 SI 3 Rest Octets
    public readonly sbyte GPRS_Support; // 1 support GPRS
    public readonly sbyte RA_COLOUR;
    public readonly ushort C0; // main carrier

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public readonly ushort[] CA; // Cell Channel Description

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public readonly ushort[] BA; //Neighbour Cell BCCH Frequency List

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public readonly ushort[] extBA; //Extended BCCH Neighbour Cell Frequency List

    public readonly sbyte TSC; // training sequence
    public readonly sbyte BSIC; // TSC + PLMN = BSIC
    public readonly sbyte NCC;
    public readonly sbyte NCH_Position; //10.5.2.32 NCH Position on the CCCH
    public readonly short RSSI;
    public readonly sbyte SIB1;
    public readonly sbyte SIB2;
    public readonly sbyte SIB3;
    public readonly sbyte SIB4;
    public readonly short C1;
    public readonly short C2;
    public readonly short C2L;
    public readonly short fake_bs_flag; //伪基站标志，=1时为伪基站
    public readonly int ADCFrameCount;
    public readonly int paging_count;
    public readonly int FN; //frame number  帧号
    public readonly int t;
    public readonly char updated_flag;
    public readonly uint centerfrequency;
    public readonly int C2I;
}