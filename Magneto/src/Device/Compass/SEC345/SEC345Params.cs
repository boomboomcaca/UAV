/*********************************************************************************************
 *
 * 文件名称:		..\Tracker800 V9\Server\Source\Device\Compass\SEC345\SEC345Params.cs
 *
 * 作    者:		陈鹏
 *
 * 创作日期:		2017-11-03
 *
 * 修    改:		无
 *
 * 备    注:		SEC345罗盘参数定义
 *
 *********************************************************************************************/

using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.SEC345;

[DeviceDescription(Name = "SEC345",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Compass,
    MaxInstance = 0,
    Version = "1.2.0",
    Model = "SEC345",
    FeatureType = FeatureType.None,
    Capacity = EdgeCapacity.Compass,
    Description = "适用于慧联科技SEC340系列电子罗盘模块")]
public partial class Sec345
{
    #region 安装参数

    [Name("connection")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通信方式")]
    [Description("设置罗盘数据输出串口或网口。"
                 + "\r\n"
                 + "【串口通信】格式：\"COMx\"或\"COMx,XXXX\"，其中\"x\"表示串口号，例如：\"COM1\"，\"XXXX\"表示波特率，例如：\"4800\"，未指定时，默认为\"115200\""
                 + "\r\n"
                 + "【网口通信】格式：\"[tcp://]x.x.x.x:y\"，其中\"[tcp://]\"为可选项，表示网络协议使用TCP，\"x\"表示IPv4点分十进制值,\"y\"表示网络服务端口号，例如：\"tcp://192.168.1.100:9553\"或\"192.168.1.100:9553\""
                 + "\r\n"
                 + "其它格式为非法输入值")]
    [PropertyOrder(0)]
    [DefaultValue("COM1,9600")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Connection { get; set; }

    [Name("reportingDirection")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("公布方位")]
    [Description("是否通过消息对外公布地理方位")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|是|否")]
    [PropertyOrder(1)]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool ReportingDirection { get; set; }

    [Name("extraAngle")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("补偿角度")]
    [Description("设置角度补偿值，有效值范围-180~180，主要用于工程安装时的角度纠偏")]
    [ValueRange(-180.0f, 180.0f)]
    [PropertyOrder(3)]
    [DefaultValue(0.0f)]
    [Style(DisplayStyle.Slider)]
    public float ExtraAngle { get; set; }

    #endregion
}