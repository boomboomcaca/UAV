using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.Jdq;

[DeviceDescription(Name = "Jdq压制机",
    DeviceCategory = ModuleCategory.RadioSuppressing,
    Manufacturer = "Aleph",
    Version = "1.10.1.0",
    Model = "Jdq",
    Description = "Jdq控制的压制机",
    MaxInstance = 1,
    FeatureType = FeatureType.UavDef)]
public partial class Jdq
{
    #region Installation MyRegion

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备Ip地址")]
    [DefaultValue("192.168.0.18")]
    [PropertyOrder(0)]
    public string IpAddress { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度")]
    [DefaultValue("")]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("isDemo")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("演示模式")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IsDemo { get; set; }

    #endregion

    #region RunTime Parameters

    private bool _isOpenAll;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("isOpenAll")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("真实设备模拟")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("是否模拟真实设备的网络状况 ")]
    [Style(DisplayStyle.Switch)]
    public bool IsOpenAll
    {
        get => _isOpenAll;
        set
        {
            _isOpenAll = value;
            if (_tcpClient.Connected)
                _tcpClient.Client.Send(_isOpenAll
                    ? [0xCC, 0xDD, 0xA1, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0x3C]
                    : [0xCC, 0xDD, 0xA1, 0x01, 0x00, 0x00, 0xFF, 0xFF, 0xA0, 0x40]);
            //_udpClient.Send(new byte[] { 0xCC, 0xDD, 0xA1, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x9E, 0x3C });
        }
    }

    #endregion
}