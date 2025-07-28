using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.DT1000AS.Driver.Base;

[StructLayout(LayoutKind.Sequential)]
public struct BcchDataStruct : ICloneable
{
    /// <summary>
    ///     Cell identity value
    /// </summary>
    public ushort CI;

    /// <summary>
    ///     Mobile country code
    /// </summary>
    public ushort MCC;

    /// <summary>
    ///     Mobile network code
    /// </summary>
    public ushort MNC;

    /// <summary>
    ///     Location area cod
    /// </summary>
    public ushort LAC;

    /// <summary>
    ///     ATT, Attach-detach allowed (octet 2)
    /// </summary>
    public byte ATT;

    public byte BS_AG_BLKS_RES;
    public byte CCCH_CONF;
    public byte BS_PA_MFRMS;

    /// <summary>
    ///     timeout value;
    /// </summary>
    public byte T3212;

    /// <summary>
    ///     10.5.2.3 Cell Options (BCCH)
    /// </summary>
    public byte PWRC;

    /// <summary>
    ///     10.5.2.3 Cell Options (BCCH)
    /// </summary>
    public byte DTX;

    /// <summary>
    ///     10.5.2.3 Cell Options (BCCH)
    /// </summary>
    public byte RADIO_LINK_TIMEOUT;

    /// <summary>
    ///     Maximum number of retransmissions 10.5.2.29
    /// </summary>
    public byte Max_retrans;

    /// <summary>
    ///     Number of slots to spread transmission 10.5.2.29
    /// </summary>
    public byte Tx_integer;

    /// <summary>
    ///     1 The cell is barred 10.5.2.29
    /// </summary>
    public byte CELL_BAR_ACCESS;

    /// <summary>
    ///     1 Call Reestablishment not allowed in the cell 10.5.2.29
    /// </summary>
    public byte RE;

    /// <summary>
    ///     Access Control Class N access is not barred if the AC CN bit is coded with a "0";
    /// </summary>
    public byte AC_CN;

    /// <summary>
    ///     RXLEV hysteresis for LA re-selection
    /// </summary>
    public byte CELL_RESELECT_HYSTERESIS;

    /// <summary>
    ///     binary representation of the "power control level"
    /// </summary>
    public byte MS_TXPWR_MAX_CCH;

    public byte ACS;

    /// <summary>
    ///     1 New establishment causes are  supported
    /// </summary>
    public byte NECI;

    /// <summary>
    ///     binary representation of the minimum received signal level
    /// </summary>
    public byte RXLEV_ACCESS_MIN;

    /// <summary>
    ///     CELL_BAR_QUALIFY (1 bit field); 10.5.2.34 SI 3 Rest Octets
    /// </summary>
    public byte CBQ;

    /// <summary>
    ///     (6 bit field) 10.5.2.34 SI 3 Rest Octets
    /// </summary>
    public byte CELL_RESELECT_OFFSET;

    /// <summary>
    ///     (3 bit field); 10.5.2.34 SI 3 Rest Octets
    /// </summary>
    public byte TEMPORARY_OFFSET;

    /// <summary>
    ///     (5 bit field) 10.5.2.34 SI 3 Rest Octets
    /// </summary>
    public byte PENALTY_TIME;

    /// <summary>
    ///     10.5.2.34 SI 3 Rest Octets
    /// </summary>
    public byte Power_Offset;

    /// <summary>
    ///     1 support GPRS
    /// </summary>
    public byte GPRS_Support;

    public byte RA_COLOUR;

    /// <summary>
    ///     main carrier
    /// </summary>
    public ushort C0;

    /// <summary>
    ///     Cell Channel Description
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public ushort[] CA;

    /// <summary>
    ///     Neighbour Cell BCCH Frequency List
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public ushort[] BA;

    /// <summary>
    ///     Extended BCCH Neighbour Cell Frequency List
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public ushort[] extBA;

    /// <summary>
    ///     training sequence
    /// </summary>
    public byte TSC;

    /// <summary>
    ///     TSC + PLMN = BSIC
    /// </summary>
    public byte BSIC;

    public byte NCC;

    /// <summary>
    ///     10.5.2.32 NCH Position on the CCCH
    /// </summary>
    public byte NCH_Position;

    public short RSSI;
    public byte SIB1;
    public byte SIB2;
    public byte SIB3;
    public byte SIB4;
    public short C2L;
    public short fake_bs_flag;
    public int ADCFrameCount;
    public int paging_count;

    public object Clone()
    {
        var tempCa = new ushort[65];
        var tempBa = new ushort[65];
        var tempExtBa = new ushort[65];
        if (CA != null) Array.Copy(CA, tempCa, 65);
        if (BA != null) Array.Copy(BA, tempBa, 65);
        if (extBA != null) Array.Copy(extBA, tempExtBa, 65);
        return new BcchDataStruct
        {
            ACS = ACS,
            AC_CN = AC_CN,
            ADCFrameCount = ADCFrameCount,
            ATT = ATT,
            BSIC = BSIC,
            BS_AG_BLKS_RES = BS_AG_BLKS_RES,
            BS_PA_MFRMS = BS_PA_MFRMS,
            CBQ = CBQ,
            C0 = C0,
            C2L = C2L,
            CCCH_CONF = CCCH_CONF,
            CELL_BAR_ACCESS = CELL_BAR_ACCESS,
            CELL_RESELECT_HYSTERESIS = CELL_RESELECT_HYSTERESIS,
            CELL_RESELECT_OFFSET = CELL_RESELECT_OFFSET,
            CI = CI,
            LAC = LAC,
            MCC = MCC,
            Max_retrans = Max_retrans,
            MNC = MNC,
            MS_TXPWR_MAX_CCH = MS_TXPWR_MAX_CCH,
            PENALTY_TIME = PENALTY_TIME,
            Power_Offset = Power_Offset,
            PWRC = PWRC,
            paging_count = paging_count,
            GPRS_Support = GPRS_Support,
            DTX = DTX,
            fake_bs_flag = fake_bs_flag,
            NCC = NCC,
            NCH_Position = NCH_Position,
            NECI = NECI,
            RADIO_LINK_TIMEOUT = RADIO_LINK_TIMEOUT,
            TEMPORARY_OFFSET = TEMPORARY_OFFSET,
            RA_COLOUR = RA_COLOUR,
            RE = RE,
            RSSI = RSSI,
            RXLEV_ACCESS_MIN = RXLEV_ACCESS_MIN,
            SIB1 = SIB1,
            SIB2 = SIB2,
            SIB3 = SIB3,
            SIB4 = SIB4,
            T3212 = T3212,
            TSC = TSC,
            Tx_integer = Tx_integer,
            CA = tempCa,
            BA = tempBa,
            extBA = tempExtBa
        };
    }
}