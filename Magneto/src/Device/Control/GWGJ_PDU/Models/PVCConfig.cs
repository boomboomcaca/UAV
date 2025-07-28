using Magneto.Device.GWGJ_PDU.Common;

namespace Magneto.Device.GWGJ_PDU.Models;

public class PvcConfig
{
    /// <summary>
    ///     上限
    /// </summary>
    public double Upper { get; set; }

    /// <summary>
    ///     下限
    /// </summary>
    public double Lower { get; set; }

    /// <summary>
    ///     接近警告值
    /// </summary>
    public double Warin { get; set; }

    /// <summary>
    ///     插座开关状态
    ///     二进制表示，1为true,0为false
    ///     高位在前，低位在后
    ///     eg: 4、2插座为true,二进制表示为1010，十进制为10
    /// </summary>
    public byte Port { get; set; }

    /// <summary>
    ///     超过上限操作类型
    /// </summary>
    public OverLimitOpMode Mode { get; set; }

    /// <summary>
    ///     延时，单位为秒
    /// </summary>
    public int Sec { get; set; }
}