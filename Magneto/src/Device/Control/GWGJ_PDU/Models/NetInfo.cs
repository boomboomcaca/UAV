namespace Magneto.Device.GWGJ_PDU.Models;

public class NetInfo
{
    public string Name { get; set; }
    public string Mac { get; set; }
    public string Ip { get; set; }
    public string SubMask { get; set; }
    public string Gateway { get; set; }
    public short PortLocal { get; set; }
    public short PortWeb { get; set; }
    public byte Dhcp { get; set; }
    public string Dns { get; set; }
    public string PduType { get; set; }
    public string Version { get; set; }

    /// <summary>
    ///     机器版本
    /// </summary>
    public string HwVersion { get; set; }

    /// <summary>
    ///     机器号
    /// </summary>
    public string HwId { get; set; }

    public string Date { get; set; }
}