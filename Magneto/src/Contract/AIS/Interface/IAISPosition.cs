using System;

namespace Magneto.Contract.AIS.Interface;

public interface IAisPosition : IAisMessage
{
    /// <summary>
    ///     纬度
    /// </summary>
    double Latitude { get; }

    /// <summary>
    ///     经度
    /// </summary>
    double Longitude { get; }

    /// <summary>
    ///     船艏真航向
    /// </summary>
    int TrueHeading { get; }

    /// <summary>
    ///     对地航向
    /// </summary>
    double Cog { get; }

    /// <summary>
    ///     对地航速 speed 节
    /// </summary>
    double Sog { get; }
}

public interface IAisPositionA : IAisPosition
{
    /// <summary>
    ///     消息重复标志
    /// </summary>
    public int RepeatIndicator { get; }

    /// <summary>
    ///     消息时间
    /// </summary>
    public DateTime MsgTimestamp { get; }

    /// <summary>
    ///     航行状态
    /// </summary>
    public NavigationState NavigationState { get; }

    /// <summary>
    ///     转向率
    /// </summary>
    public double Rot { get; }
}

public interface IAisPositionB : IAisPosition
{
    /// <summary>
    ///     消息时间戳
    /// </summary>
    public DateTime MsgTimestamp { get; }
}