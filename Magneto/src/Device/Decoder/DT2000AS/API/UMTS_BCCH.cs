using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct UmtsBcch
{
    //public ushort BASE_ID;
    //public ushort LAC;
    //public uint CI;
    //public ushort Primary_Scrambling_Code;
    //public ushort UARFCN;
    //public ushort MNC;
    //public ushort MCC;
    /////10.3.3.44 UE Timers and Constants in idle mode
    //public ushort T300;
    //public ushort N300;
    //public ushort T312;
    //public ushort N312;
    //////10.3.3.43 UE Timers and Constants in connected mode
    //public ushort T302;
    //public ushort N302;
    //public ushort T308;
    //public ushort T309;
    //public ushort T313;
    //public ushort N313;
    //public ushort T315;//jimbo  page_spread_factor
    //////sib3 Cell selection and re-selection info for SIB3/4
    //public ushort cellSelectQualityMeasure;
    //public ushort s_SearchHCS;
    //public ushort rat_Identifier;
    //public ushort s_SearchRAT;
    //public ushort s_HCS_RAT;
    //public ushort s_Limit_SearchRAT;
    //public ushort q_QualMin;
    //public ushort q_RxlevMin;
    //public ushort q_Hyst_l_S;
    //public ushort t_Reselection_S;
    //public ushort hcs_ServingCellInformation;
    //public ushort cellBarred;
    //public ushort cellReservedForOperatorUse;
    //public ushort cellReservationExtension;//jimbo   paging_count
    //////////以下参数为非系统参数，为测量所得          
    //[MarshalAs(UnmanagedType.I1)]
    //public bool MIB;
    //[MarshalAs(UnmanagedType.I1)]
    //public bool SIB1;
    //[MarshalAs(UnmanagedType.I1)]
    //public bool SIB2;
    //[MarshalAs(UnmanagedType.I1)]
    //public bool SIB3;
    //public int RSCP;
    //public int rssi;
    //public uint centerfrequency;
    //public int fake_bs_flag;
    public readonly ushort BASE_ID;
    public readonly ushort LAC; //位置区码  location area code
    public readonly uint CI; //小区ID,  cell id
    public readonly ushort Primary_Scrambling_Code; //主扰码
    public readonly ushort UARFCN; //频率号
    public readonly ushort MNC; //运营商代码
    public readonly ushort MCC; //国家码
    public readonly ushort T300;
    public readonly ushort N300;
    public readonly ushort T312;
    public readonly ushort N312;
    public readonly ushort T302;
    public readonly ushort N302;
    public readonly ushort T308;
    public readonly ushort T309;
    public readonly short ECIO;
    public readonly short SIR;
    public readonly ushort page_spread_factor; //T315;
    public readonly ushort cellSelectQualityMeasure;
    public readonly ushort s_SearchHCS;
    public readonly ushort rat_Identifier;
    public readonly ushort s_SearchRAT;
    public readonly ushort s_HCS_RAT;
    public readonly ushort s_Limit_SearchRAT;
    public readonly ushort q_QualMin;
    public readonly ushort q_RxlevMin;
    public readonly ushort q_Hyst_l_S;
    public readonly ushort t_Reselection_S;
    public readonly ushort hcs_ServingCellInformation;
    public readonly ushort cellBarred;
    public readonly ushort URA_Identity;
    public readonly ushort paging_count;
    public readonly bool MIB;
    public readonly bool SIB1;
    public readonly bool SIB2;
    public readonly bool SIB3;
    public readonly int RSCP; //码电平
    public readonly int RSSI; //接收电平
    public readonly uint centerfrequency; //中心频率
    public readonly int fake_bs_flag; //伪基站标志，=1时为伪基站
}