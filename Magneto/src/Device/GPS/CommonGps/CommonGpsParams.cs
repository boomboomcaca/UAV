using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.CommonGps;

[DeviceDescription(
    Name = "通用GPS",
    DeviceCategory = ModuleCategory.Gps,
    Manufacturer = "Aleph",
    Version = "1.0.0",
    Model = "CommonGps",
    Description = "通用定位设备控制模块，适用于北斗与GPS标准协议",
    MaxInstance = 1,
    Capacity = EdgeCapacity.GPS,
    FeatureType = FeatureType.None)]
public partial class CommonGps
{
    #region 安装参数

    [Name("connection")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通信方式")]
    [Description("设置GPS或北斗定位设备数据输出串口或网口。"
                 + "\r\n"
                 + "【串口通信】格式：\"COMx\"或\"COMx,XXXX\"，其中\"x\"表示串口号，例如：\"COM1\"，\"XXXX\"表示波特率，例如：\"4800\"，未指定时，默认为\"9600\""
                 + "\r\n"
                 + "【网口通信】格式：\"[tcp://]x.x.x.x:y\"，其中\"[tcp://]\"为可选项，表示网络协议使用TCP，\"x\"表示IPv4点分十进制值,\"y\"表示网络服务端口号，例如：\"tcp://192.168.1.100:9553\"或\"192.168.1.100:9553\""
                 + "\r\n"
                 + "其它格式为非法输入值")]
    [PropertyOrder(0)]
    [DefaultValue("COM1")]
    [Style(DisplayStyle.Input)]
    public string Connection { get; set; }

    [Name("reportingPosition")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("发布坐标")]
    [Description("设置是否公布坐标数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|是|否")]
    [PropertyOrder(1)]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool ReportingPosition { get; set; }

    [Name("timing")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("同步时钟")]
    [Description("设置是否通过GPS为系统授时")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|是|否")]
    [PropertyOrder(2)]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool Timing { get; set; }

    #endregion
}