namespace Magneto.Device.TX3010A;

/// <summary>
///     压制设备类型
/// </summary>
public enum DeviceType
{
    /// <summary>
    ///     控制板
    /// </summary>
    ControlPanel,

    /// <summary>
    ///     信号源
    /// </summary>
    SignalSource
}

#region 信号源枚举

/// <summary>
///     信号源开关
/// </summary>
public enum SignalSwitch
{
    On,
    Off
}

/// <summary>
///     信号源模式
/// </summary>
public enum SignalMode
{
    /// <summary>
    ///     单音
    /// </summary>
    SingleTones = 0,

    /// <summary>
    ///     多音
    /// </summary>
    MultiTones = 1,

    /// <summary>
    ///     扫频
    /// </summary>
    Scan = 2,

    /// <summary>
    ///     梳状谱
    /// </summary>
    Comb = 3
}

#endregion