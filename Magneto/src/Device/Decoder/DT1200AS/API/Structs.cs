using System.Runtime.InteropServices;

namespace Magneto.Device.DT1200AS.API;

#region //CDMA1x类

public struct Cdma2000Bcch
{
    public byte Pd;
    public byte MsgId;
    public byte PRev;
    public byte MinPRev;
    public ushort Sid;
    public ushort Nid;
    public ushort PilotPn;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public sbyte /*char*/[] LcState /*[42]*/;

    //public uint SYS_TIME_High32;
    //public uint SYS_TIME_Low32;
    public byte LpSec;
    public byte LtmOff;
    public byte Daylt;
    public byte Prat;
    public ushort CdmaFreq;
    public ushort ExtCdmaFreq;

    public byte Sr1BcchSupported;

    //// SPM 参数
    public byte ConfigMsgSeq;
    public ushort RegZone;
    public byte TotalZones;
    public byte ZoneTimer;
    public byte MultSids;
    public byte MultNids;
    public ushort BaseId;
    public byte BaseClass;
    public byte PageChan;
    public byte MaxSlotCycleIndex;
    public byte HomeReg;
    public byte ForSidReg;
    public byte ForNidReg;
    public byte PowerUpReg;
    public byte PowerDownReg;
    public byte ParameterReg;
    public byte RegPrd;
    public float BaseLat;
    public float BaseLong;
    public ushort RegDist;
    public byte SrchWinA;
    public byte SrchWinN;
    public byte SrchWinR;
    public byte NghbrMaxAge;
    public byte PwrRepThresh;
    public byte PwrRepFrames;
    public byte PwrThreshEnable;
    public byte PwrPeriodEnable;
    public byte PwrRepDelay;
    public byte Rescan;
    public byte Add;
    public byte Drop;
    public byte Comp;
    public byte Tdrop;
    public byte ExtSysParameter;
    public byte ExtNghbrList;
    public byte GenNghbrList;
    public byte GlobalRedirect;
    public byte PriNghbrList;
    public byte UserZoneId;
    public byte ExtGlobalRedirect;

    public byte ExtChanList;

    /////////Extended System Parameters//////////////////////////////////
    public byte DeleteForTmsi;
    public byte UseTmsi;
    public byte PrefMsidType;
    public ushort Mcc;
    public byte Imsi1112;
    public byte TmsiZoneLen;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] TmsiZone /*[8]*/;

    public byte BcastIndex;
    public byte ImsiTSupported;
    public byte SoftSlope;
    public byte AddIntercept;
    public byte DropIntercept;
    public byte PacketZoneId;
    public byte MaxNumAltSo;
    public byte ReselectIncluded;
    public byte EcThresh;
    public byte EcIoThresh;
    public byte PilotReport;
    public byte NghbrSetEntryInfo;
    public byte NumFreq;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public ushort[] ChannelListCdmaFreq /*[8]*/;

    public byte PilotInc;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] NghbrConfig /*[64]*/;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public ushort[] NghbrPn /*[64]*/;

    public byte AccMsgSeq;
    public byte AccChan;
    public byte NomPwr;
    public byte InitPwr;
    public byte PwrStep;
    public byte NumStep;
    public byte MaxCapSz;
    public byte PamSz;
    public byte Psist09;
    public byte Psist10;
    public byte Psist11;
    public byte Psist12;
    public byte Psist13;
    public byte Psist14;
    public byte Psist15;
    public byte MsgPsist;
    public byte RegPsist;
    public byte ProbePnRan;
    public byte AccTmo;
    public byte ProbeBkoff;
    public byte Bkoff;
    public byte MaxReqSeq;
    public byte MaxRspSeq;
    public byte Auth;
    public uint Rand;
    public byte NomPwrExt;
    public byte PsistEmgIncl;
    public byte PsistEmg;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public sbyte /*char*/[] Page1Mask /*[42]*/;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public sbyte /*char*/[] Page2Mask /*[42]*/;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public sbyte /*char*/[] Page3Mask /*[42]*/;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
    public sbyte /*char*/[] Page4Mask /*[42]*/;

    ////////以下参数为非系统参数，为测量所得
    public int Rssi;
    public uint Centerfrequency;
    public int FakeBsFlag;
}

#endregion

#region //EVDO类

public struct EvdoBcch
{
    public byte ColorCode;
    public uint SectorId24;
    public ushort SectorSignature;
    public ushort AccessSignature;
    public byte MsgId;
    public byte MinPRev;
    public byte MaximumRevision;
    public byte MinimumRevision;
    public ushort PilotPn;
    public ushort PacketZoneId;
    public ushort Nid;

    public ushort Sid;

    //public  uint SYS_TIME_High32;
    //public uint SYS_TIME_Low32;
    public ushort CdmaFreq;
    public float BaseLat;
    public float BaseLong;
    public ushort Mcc;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] SectorId; //[32]; //每个为4比特

    public byte SectorIdFlag;
    public byte SubnetMask;
    public ushort RouteUpdateRadius;
    public byte LeapSeconds;
    public ushort LocalTimeOffset;
    public byte ReverseLinkSilenceDuration;
    public byte ReverseLinkSilencePeriod;
    public ushort ChannelCount;
    public ushort NeighborCount;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public ushort[] Channel; //[32];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public ushort[] NeighborPilotPn; //[32];

    ////////以下参数为非系统参数，为测量所得
    public int Rssi;
    public int RxPacket;
    public uint Centerfrequency;
    public ushort BaseId;
}

#endregion

#region //GSM类

[StructLayout(LayoutKind.Sequential)]
public struct GsmBcch
{
    public readonly ushort CI; // Cell identity value
    public readonly ushort MCC; //Mobile country code 
    public readonly ushort MNC; //Mobile network code  
    public readonly ushort LAC; // Location area cod
    public readonly byte ATT; //ATT, Attach-detach allowed (octet 2)
    public readonly byte BS_AG_BLKS_RES;
    public readonly byte CCCH_CONF;
    public readonly byte BS_PA_MFRMS;
    public readonly byte T3212; //timeout value;
    public readonly byte PWRC; //10.5.2.3 Cell Options (BCCH)
    public readonly byte DTX; // 10.5.2.3 Cell Options (BCCH)
    public readonly byte RADIO_LINK_TIMEOUT; //10.5.2.3 Cell Options (BCCH)
    public readonly byte Max_retrans; //Maximum number of retransmissions 10.5.2.29
    public readonly byte Tx_integer; //Number of slots to spread transmission 10.5.2.29
    public readonly byte CELL_BAR_ACCESS; //1 The cell is barred 10.5.2.29
    public readonly byte RE; // 1 Call Reestablishment not allowed in the cell 10.5.2.29
    public readonly byte AC_CN; //Access Control Class N access is not barred if the AC CN bit is coded with a "0";
    public readonly byte CELL_RESELECT_HYSTERESIS; //RXLEV hysteresis for LA re-selection
    public readonly byte MS_TXPWR_MAX_CCH; //binary representation of the "power control level"
    public readonly byte ACS;
    public readonly byte NECI; // 1 New establishment causes are  supported
    public readonly short RXLEV_ACCESS_MIN; // binary representation of the minimum received signal level
    public readonly byte CBQ; //CELL_BAR_QUALIFY (1 bit field); 10.5.2.34 SI 3 Rest Octets
    public readonly byte CELL_RESELECT_OFFSET; //(6 bit field) 10.5.2.34 SI 3 Rest Octets
    public readonly byte TEMPORARY_OFFSET; // (3 bit field); 10.5.2.34 SI 3 Rest Octets
    public readonly byte PENALTY_TIME; // (5 bit field) 10.5.2.34 SI 3 Rest Octets
    public readonly byte Power_Offset; //10.5.2.34 SI 3 Rest Octets
    public readonly byte GPRS_Support; // 1 support GPRS
    public readonly byte RA_COLOUR;

    public readonly ushort C0; // main carrier 主要载体

    //     public ushort[] CA;// CA[65];  // Cell Channel Description
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public readonly ushort[] CA; //= new ushort[65];

    //  public ushort []BA;//[65];  //Neighbour Cell BCCH Frequency List
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public readonly ushort[] BA; // = new ushort[65];

    //  public ushort[] extBA;//[65];  //Extended BCCH Neighbour Cell Frequency List
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
    public readonly ushort[] extBA; // = new ushort[65];

    public readonly byte TSC; // training sequence
    public readonly byte BSIC; // TSC + PLMN = BSIC
    public readonly byte NCC;
    public readonly byte NCH_Position; //10.5.2.32 NCH Position on the CCCH
    public readonly short RSSI;
    public readonly byte SIB1;
    public readonly byte SIB2;
    public readonly byte SIB3;
    public readonly byte SIB4;
    public readonly short C1;
    public readonly short C2;
    public readonly short C2L;
    public readonly short fake_bs_flag;
    public readonly int ADCFrameCount;

    public readonly int paging_count;

    //
    public readonly int FN;
    public readonly int t;
    public readonly char updated_flag;
    public readonly uint centerfrequency;
    public readonly int C2I;
}

#endregion

#region //LTE类

public struct LteIntraFreqNeighCellListStruct
{
    public short PhysCellId;
    public short QOffsetRange;
}

public struct LteEnbConfig
{
    public uint NCellId;
    public uint Ndlrb; //{6,15,25,50,75,100};
    public uint BandWidth; //{1.4,3,5,10,15,20}
    public uint CpType; // NORMAL_CP EXTEND_CP
    public uint Duplex; //TDD=1,FDD=2
    public uint CellRefP;
    public uint PhichConfig;
    public uint Sfn;
    public uint TddUplinkDownlinkConfig; //0,1,2...6
    public uint Cfi;
    public uint Ci; // CELL ID
    public uint Tac; //tracking area code, like LAC in GSM or WCDMA
    public uint Mcc;
    public uint Mnc;
    public uint Freqband;
    public uint Freq;
    public uint Uarfcn;
    public uint SubFrame0Idx;

    public uint SiWindowLength;

    //////SIB4/////
    public uint IntraFreqNeighCellListSize;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public LteIntraFreqNeighCellListStruct[] IntraFreqNeighCellList;

    //// SIB5 parameters
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] SibxGotten;

    public LibLte.LiblteRrcSysInfoBlockType5Struct Sib5Struct;
    public int Rsrp;
    public int Rssi;
    public float Rsrq;
    public float Snr;
    public int ReferenceSignalPower;
    public int FakeBsFlag;
}

public class LibLte
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

    public struct LiblteRrcInterFreqNeighCellStruct
    {
        public LiblteRrcQOffsetRangeEnum QOffsetCell;
        public ushort PhysCellId;
    }

    public struct LiblteRrcPhysCellIdRangeStruct
    {
        public LiblteRrcPhysCellIdRangeEnum Range;
        public ushort Start;
    }

    public struct LiblteRrcSpeedStateScaleFactorsStruct
    {
        public LiblteRrcSssfMediumEnum SfMedium;
        public LiblteRrcSssfHighEnum SfHigh;
    }

    public struct LiblteRrcInterFreqCarrierFreqInfoStruct
    {
        public LiblteRrcSpeedStateScaleFactorsStruct ReselEutraSf;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LiblteRrcMaxCellInter)]
        public LiblteRrcInterFreqNeighCellStruct[] InterFreqNeighCellList;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LiblteRrcMaxCellBlack)]
        public LiblteRrcPhysCellIdRangeStruct[] InterFreqBlackCellList;

        public LiblteRrcAllowedMeasBandwidthEnum AllowedMeasBw;
        public LiblteRrcQOffsetRangeEnum QOffsetFreq;
        public ushort DlCarrierFreq;
        public short QRxLevMin;
        public byte ReselEutra;
        public byte ThreshxHigh;
        public byte ThreshxLow;
        public byte CellReselPrio;
        public byte NeighCellCnfg;
        public byte InterFreqNeighCellListSize;
        public byte InterFreqBlackCellListSize;
        public byte PMax;
        [MarshalAs(UnmanagedType.I1)] public bool PresenceAntPort1;
        [MarshalAs(UnmanagedType.I1)] public bool PMaxPresent;
        [MarshalAs(UnmanagedType.I1)] public bool ReselEutraSfPresent;
        [MarshalAs(UnmanagedType.I1)] public bool CellReselPrioPresent;
    }

    public struct LiblteRrcSysInfoBlockType5Struct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LiblteRrcMaxFreq)]
        public LiblteRrcInterFreqCarrierFreqInfoStruct[] InterFreqCarrierFreqList;

        public uint InterFreqCarrierFreqListSize;
    }
}

#endregion

#region //UMTS类

public struct UmtsBcch
{
    public ushort BaseId;
    public ushort Lac;
    public uint Ci;
    public ushort PrimaryScramblingCode;
    public ushort Uarfcn;
    public ushort Mnc;
    public ushort Mcc;

    ///10.3.3.44 UE Timers and Constants in idle mode
    public ushort T300;

    public ushort N300;
    public ushort T312;

    public ushort N312;

    ////10.3.3.43 UE Timers and Constants in connected mode
    public ushort T302;
    public ushort N302;
    public ushort T308;
    public ushort T309;
    public ushort T313;
    public ushort N313;

    public ushort T315;

    ////sib3 Cell selection and re-selection info for SIB3/4
    public ushort CellSelectQualityMeasure;
    public ushort SSearchHcs;
    public ushort RatIdentifier;
    public ushort SSearchRat;
    public ushort SHcsRat;
    public ushort SLimitSearchRat;
    public ushort QQualMin;
    public ushort QRxlevMin;
    public ushort QHystLs;
    public ushort ReselectionS;
    public ushort HcsServingCellInformation;
    public ushort CellBarred;
    public ushort CellReservedForOperatorUse;

    public ushort CellReservationExtension;

    ////////以下参数为非系统参数，为测量所得          
    [MarshalAs(UnmanagedType.I1)] public bool Mib;
    [MarshalAs(UnmanagedType.I1)] public bool Sib1;
    [MarshalAs(UnmanagedType.I1)] public bool Sib2;
    [MarshalAs(UnmanagedType.I1)] public bool Sib3;
    public int Rscp;
    public int Rssi;
    public uint Centerfrequency;
    public int FakeBsFlag;
}

#endregion