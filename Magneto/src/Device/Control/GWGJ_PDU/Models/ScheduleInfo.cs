using Magneto.Device.GWGJ_PDU.Common;

namespace Magneto.Device.GWGJ_PDU.Models;

public class ScheduleInfo
{
    /// <summary>
    ///     日程名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     频率
    /// </summary>
    public ScheduleFrequency Frequency { get; set; }

    /// <summary>
    ///     动作
    /// </summary>
    public SocketAction Action { get; set; }

    /// <summary>
    ///     延时，操作为延时**有效
    /// </summary>
    public short Delay { get; set; }

    /// <summary>
    ///     插座状态
    ///     二进制表示，1为true,0为false
    ///     高位在前，低位在后
    ///     eg: 4、2插座为true,二进制表示为1010，十进制为10
    /// </summary>
    public byte Socket { get; set; }

    /// <summary>
    ///     是否有效，1：有效，0：无效
    /// </summary>
    public byte State { get; set; }

    /// <summary>
    ///     ID
    /// </summary>
    public byte Id { get; set; }

    /// <summary>
    ///     时间，格式为yyyyMMddHHmm
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    ///     星期几，（1-7），频率为每周有效
    /// </summary>
    public byte Week { get; set; }
}