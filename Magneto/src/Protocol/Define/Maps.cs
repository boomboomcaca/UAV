// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="Maps.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Magneto.Protocol.Define;

/// <summary>
///     Class Maps.
/// </summary>
public static class Maps
{
    /// <summary>
    ///     系统通道，同主控通道，非云端使用
    /// </summary>
    public const string MapSystem = "/system";

    /// <summary>
    ///     The map configuration
    /// </summary>
    public const string MapConfiguration = "/configuration";

    /// <summary>
    ///     数据通道
    /// </summary>
    public const string MapTask = "/data";

    /// <summary>
    ///     主控通道，云端使用，其他端不可使用
    /// </summary>
    public const string MapControl = "/control";

    /// <summary>
    ///     环境监控通道，云端使用
    /// </summary>
    public const string MapAtomic = "/atomic";
}