namespace Magneto.Device.GWGJ_PDU.Models;

public class PvcInfo
{
    /// <summary>
    ///     功率，单位为W
    /// </summary>
    public double Power { get; set; }

    /// <summary>
    ///     电压，单位为V
    /// </summary>
    public double Voltage { get; set; }

    /// <summary>
    ///     电流，单位为A
    /// </summary>
    public double Current { get; set; }
}