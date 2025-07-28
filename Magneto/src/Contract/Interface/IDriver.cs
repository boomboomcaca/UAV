using System;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Interface;

/// <summary>
///     功能接口
/// </summary>
public interface IDriver
{
    /// <summary>
    ///     功能ID
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     功能初始化
    /// </summary>
    /// <param name="module">功能用到的设备信息</param>
    void Initialized(ModuleInfo module);

    /// <summary>
    ///     启动任务
    /// </summary>
    /// <param name="dataPort">数据传输的通道</param>
    /// <param name="mediaType"></param>
    bool Start(IDataPort dataPort, MediaType mediaType);

    bool Pause();

    /// <summary>
    ///     停止任务
    /// </summary>
    bool Stop();

    /// <summary>
    ///     设置参数
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="value">参数值</param>
    void SetParameter(string name, object value);

    /// <summary>
    ///     注册消息通道
    /// </summary>
    /// <param name="dataPort"></param>
    void Attach(IDataPort dataPort);
}