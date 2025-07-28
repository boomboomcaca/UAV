using Magneto.Device.GWGJ_PDU.Common;

namespace Magneto.Device.GWGJ_PDU.Models;

public class TemperatureConfig
{
    public TemperatureMode Mode { get; set; }

    /// <summary>
    ///     当前温度
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    ///     温度值
    /// </summary>
    public double TemperatureSetupValue { get; set; }

    /// <summary>
    ///     回差
    /// </summary>
    public double Backlash { get; set; }

    public byte State { get; set; }
}