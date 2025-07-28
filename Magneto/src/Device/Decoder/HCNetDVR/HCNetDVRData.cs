/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\HiNetDVR\HikNetDVRData.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/07/20
 *
 * 修    改:
 *
 * 备    注:	录播盒模块数据定义
 *
 *********************************************************************************************/

namespace Magneto.Device.HCNetDVR;

/// <summary>
///     播放模式
/// </summary>
internal enum PlayTypeEnum
{
    /// <summary>
    ///     实时预览
    /// </summary>
    RealTime,

    /// <summary>
    ///     回放
    /// </summary>
    Playback,

    /// <summary>
    ///     无播放数据
    /// </summary>
    None
}