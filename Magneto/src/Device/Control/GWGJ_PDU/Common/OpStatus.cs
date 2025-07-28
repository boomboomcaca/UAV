namespace Magneto.Device.GWGJ_PDU.Common;

public enum OpStatus
{
    /// <summary>
    ///     成功
    /// </summary>
    Success,

    /// <summary>
    ///     失败
    /// </summary>
    Fail,

    /// <summary>
    ///     无权限
    /// </summary>
    NoPower,

    /// <summary>
    ///     参数错误
    /// </summary>
    ParametersError,

    /// <summary>
    ///     网络错误
    /// </summary>
    NetworkError,

    /// <summary>
    ///     结果错误
    /// </summary>
    ResultError
}