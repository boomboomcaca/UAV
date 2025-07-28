/*********************************************************************************************
 *
 * 文件名称:		..\Tracker800 V9\Server\Source\Device\AntennaController\EthernetAntennaController\EthernetAntennaControllerParams.cs
 *
 * 作    者:		陈鹏
 *
 * 创作日期:		2017-02-28
 *
 * 修    改:		无
 *
 * 备    注:		网口天线控制器参数定义
 *
 *********************************************************************************************/

using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Tracker800.Server.Device;

/// <summary>
///     网口天线控制器参数定义
/// </summary>
[DeviceDescription(Name = "网口串口天线控制器",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.AntennaControl,
    FeatureType = FeatureType.None,
    MaxInstance = 1,
    Model = "EthSerAntController",
    Version = "1.5.3",
    Description = "串口和网口天线控制器，适用于打通一个串口和一个网口同时进行控制两个天线"
)]
public partial class EthSerAntController
{
    #region 安装属性

    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接到设备的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [DefaultValue("127.0.0.1")]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; }

    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到设备的网络端口号。")]
    [ValueRange(1024, 65535, 0)]
    [PropertyOrder(1)]
    [DefaultValue(5555)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; }

    [Name("comPort")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(double.NaN, double.NaN, 5)]
    [DisplayName("串口号")]
    [Description("设置天线控制器占用的串口编号")]
    [PropertyOrder(0)]
    [DefaultValue("COM1")]
    [Style(DisplayStyle.Input)]
    public string Com { get; set; }

    [Name("baudRate")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [Description("设置与串口通信的波特率")]
    [PropertyOrder(1)]
    [DefaultValue(9600)]
    [ValueRange(240, 115200, 0)]
    [Style(DisplayStyle.Input)]
    public int BaudRate { get; set; }

    #endregion
}