namespace Magneto.Contract;

public enum InternalMessageType
{
    /// <summary>
    ///     空消息，用于检测、维护网络状态
    /// </summary>
    None,

    /// <summary>
    ///     未知的，尚未定义的
    /// </summary>
    Other,

    /// <summary>
    ///     普通消息，消息内容类型为string
    /// </summary>
    Information,

    /// <summary>
    ///     警告消息，消息内容类型为string
    /// </summary>
    Warning,

    /// <summary>
    ///     错误消息，消息内容类型为string
    /// </summary>
    Error,

    /// <summary>
    ///     设备要重启才能恢复已有故障，消息中的ModuleID表明是哪一个设备模块要重启，消息类型为string
    /// </summary>
    DeviceRestart,

    /// <summary>
    ///     设备状态改变，消息内容类型为ModuleState
    /// </summary>
    DeviceStateChange,

    /// <summary>
    ///     设备故障，消息中的ModuleID表明是哪一个设备模块出错，消息内容类型为string
    /// </summary>
    DeviceFault,

    /// <summary>
    ///     功能模块状态改变，消息内容类型为ModuleState
    /// </summary>
    DriverStateChange,

    /// <summary>
    ///     功能模块故障，消息内容类型为故障原因string
    /// </summary>
    DriverFault,

    /// <summary>
    ///     新节点入监测网
    /// </summary>
    NewMonNode,

    /// <summary>
    ///     监测节点位置改变，消息内容类型为GPS数据
    /// </summary>
    MonNodeGpsChange,

    /// <summary>
    ///     监测节点正北位置改变，消息内容类型为COMPASS数据
    /// </summary>
    MonNodeCompassChange,

    /// <summary>
    ///     任务被强行上，消息内容类型为string，包含被中止的原因，被谁中止
    /// </summary>
    AbortTask,

    /// <summary>
    ///     客户端通道关闭 可能是客户端程序关闭、客户端与服务端断网等 消息内容类型为客户端 IDataPort 对象
    /// </summary>
    ClientChannelClosed,

    /// <summary>
    ///     航空态势消息
    /// </summary>
    AdsB,

    /// <summary>
    ///     船舶消息
    /// </summary>
    Ais
}