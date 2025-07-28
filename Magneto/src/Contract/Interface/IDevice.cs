using System;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Interface;

/// <summary>
///     设备接口
/// </summary>
public interface IDevice
{
    /// <summary>
    ///     设备ID
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     初始化设备
    /// </summary>
    /// <param name="device"></param>
    bool Initialized(ModuleInfo device);

    /// <summary>
    ///     设置参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    void SetParameter(string name, object value);

    /// <summary>
    ///     注册消息通道
    /// </summary>
    /// <param name="dataPort"></param>
    void Attach(IDataPort dataPort);
}