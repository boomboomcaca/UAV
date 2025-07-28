using System;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Interface;

/// <summary>
///     开关矩阵接口
/// </summary>
public interface ISwitchCallback
{
    /// <summary>
    ///     按开关用途注册回调接口
    /// </summary>
    /// <param name="usage">开关用途</param>
    /// <param name="action">回调接口</param>
    /// <returns>返回开关阵列中可用的开关序号</returns>
    int Register(SwitchUsage usage, Action<SwitchInfo> action);

    /// <summary>
    ///     按开关序号注销回调接口
    /// </summary>
    /// <param name="index"></param>
    /// <param name="action"></param>
    void UnRegister(int index, Action<SwitchInfo> action);

    /// <summary>
    ///     开关阵复位操作
    /// </summary>
    void Reset();
}