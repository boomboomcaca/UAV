// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="SwitchInfo.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Magneto.Protocol.Define;

/// <summary>
///     开关信息
/// </summary>
public class SwitchInfo
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="index">开关序号</param>
    /// <param name="usage">开关的用途</param>
    /// <param name="name">开关名称</param>
    /// <param name="code">开关码</param>
    public SwitchInfo(int index, SwitchUsage usage, string name, string code)
    {
        Index = index;
        Usage = usage;
        Name = name;
        Code = code;
    }

    /// <summary>
    ///     开关序号
    /// </summary>
    /// <value>The index.</value>
    public int Index { get; private set; }

    /// <summary>
    ///     开关用途
    /// </summary>
    /// <value>The usage.</value>
    public SwitchUsage Usage { get; private set; }

    /// <summary>
    ///     开关名称
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; private set; }

    /// <summary>
    ///     开关码
    /// </summary>
    /// <value>The code.</value>
    public string Code { get; private set; }
}