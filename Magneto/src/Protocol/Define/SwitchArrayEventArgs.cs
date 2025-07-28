// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="SwitchArrayEventArgs.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace Magneto.Protocol.Define;

/// <summary>
///     开关阵通道打开事件消息
/// </summary>
public class SwitchArrayEventArgs : EventArgs
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="switchNumber">开关序号</param>
    /// <param name="switchName">开关名称</param>
    public SwitchArrayEventArgs(int switchNumber, string switchName)
    {
        SwitchNumber = switchNumber;
        SwitchName = switchName;
    }

    /// <summary>
    ///     开关序号
    /// </summary>
    /// <value>The switch number.</value>
    public int SwitchNumber { get; private set; }

    /// <summary>
    ///     开关名称
    /// </summary>
    /// <value>The name of the switch.</value>
    public string SwitchName { get; private set; }
}