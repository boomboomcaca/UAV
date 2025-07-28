// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ErrorCode.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Magneto.Protocol.Define;

/// <summary>
///     Class ErrorCode.
/// </summary>
public static class ErrorCode
{
    /// <summary>
    ///     The error code common
    /// </summary>
    public const int ErrorCodeCommon = -32000;

    /// <summary>
    ///     The error code internal
    /// </summary>
    public const int ErrorCodeInternal = -32001;

    /// <summary>
    ///     接口已弃用
    /// </summary>
    public const int ErrorCodeInterfaceDeprecated = 10000;

    /// <summary>
    ///     未找到模块
    /// </summary>
    public const int ErrorCodeModuleNotFound = 10001;

    /// <summary>
    ///     任务创建失败
    /// </summary>
    public const int ErrorCodeTaskCreateFail = 10002;

    /// <summary>
    ///     任务启动失败
    /// </summary>
    public const int ErrorCodeTaskStartFail = 10003;

    /// <summary>
    ///     任务不存在
    /// </summary>
    public const int ErrorCodeTaskNotFound = 10004;

    /// <summary>
    ///     参数设置失败
    /// </summary>
    public const int ErrorCodeParameterSetFail = 10005;
}