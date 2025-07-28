// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="PropertyCategoryNames.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Magneto.Protocol.Define;

/// <summary>
///     Class PropertyCategoryNames.
/// </summary>
public static class PropertyCategoryNames
{
    /// <summary>
    ///     The installation
    /// </summary>
    public const string Installation = "安装参数";

    /// <summary>
    ///     The configuration
    /// </summary>
    public const string Configuration = "配置参数";

    /// <summary>
    ///     The radio control
    /// </summary>
    public const string RadioControl = "射频控制";

    /// <summary>
    ///     The direction finding
    /// </summary>
    public const string DirectionFinding = "测向控制";

    /// <summary>
    ///     The scan
    /// </summary>
    public const string Scan = "扫描参数";

    /// <summary>
    ///     The demodulation
    /// </summary>
    public const string Demodulation = "解调解码";

    /// <summary>
    ///     The measurement
    /// </summary>
    public const string Measurement = "测量参数";

    /// <summary>
    ///     The data switch
    /// </summary>
    public const string DataSwitch = "数据开关";

    /// <summary>
    ///     The antenna control
    /// </summary>
    public const string AntennaControl = "天线控制";

    /// <summary>
    ///     The misc
    /// </summary>
    public const string Misc = "其他";

    /// <summary>
    ///     The driver specified
    /// </summary>
    public const string DriverSpecified = "功能参数";

    /// <summary>
    ///     TODO : 现在暂时这么做
    ///     这个分组内的参数都为命令，如果参数值相等的话一样需要下发（其他参数如果值相等则不会重复下发）
    ///     以后需要修改Parameter类，添加一个是否为命令的属性来标记当前参数是命令还是普通参数
    /// </summary>
    public const string Command = "命令参数";
}