namespace Magneto.Device.ESMC;

/// <summary>
///     离散扫描信道结构
/// </summary>
public struct MemChanel
{
    public double Freq;
    public int Threth;
    public string Mode;
    public float SngBand;
    public int AntNo;
    public bool Att;
    public bool AutoAtt;
    public bool Squ;
    public bool Afc;
    public bool Act;
}

/// <summary>
///     工作模式
/// </summary>
public enum WorkMode
{
    Cw,
    Swe,
    Msc,
    Dsc,
    Fast,
    List
}