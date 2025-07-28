using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct UmtsBcch
{
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