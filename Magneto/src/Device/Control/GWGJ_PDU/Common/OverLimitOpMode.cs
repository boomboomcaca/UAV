namespace Magneto.Device.GWGJ_PDU.Common;

public enum OverLimitOpMode : byte
{
    /// <summary>
    ///     关闭
    /// </summary>
    Off = 0,

    /// <summary>
    ///     开启
    /// </summary>
    On,

    /// <summary>
    ///     重启
    /// </summary>
    Reboot,

    /// <summary>
    ///     延时关闭
    /// </summary>
    DelayOff,

    /// <summary>
    ///     延时开启
    /// </summary>
    DelayOn,

    /// <summary>
    ///     延时重启
    /// </summary>
    DelayReboot,

    /// <summary>
    ///     不操作
    /// </summary>
    None
}