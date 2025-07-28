using System;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.DC6510TLF;

[DeviceDescription(Name = "DC6510TLF",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Icb,
    MaxInstance = 1,
    Version = "1.0.0",
    Model = "DC6510TLF",
    FeatureType = FeatureType.FASTEMT,
    Description = "射电望远镜电磁环境测试系统控制板")]
public partial class Dc6510Tlf
{
    #region 设备参数

    private float _degree;

    [PropertyOrder(0)]
    [Name("degree")]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("水平转台角度")]
    [ValueRange(0f, 360f, 5)]
    [DefaultValue(0f)]
    [Description("设置水平转台角度")]
    public float Degree
    {
        get
        {
            var degree = GetDegree();
            if (degree != null) _degree = degree.Value;
            Console.WriteLine($"当前水平转台角度为 {_degree}");
            return _degree;
        }
        set
        {
            if (Math.Abs(_degree - value) > 1e-9)
                if (SetDegree(value))
                {
                    _degree = value;
                    Console.WriteLine($"当前水平转台角度设置为 {value}");
                }
        }
    }

    private Polarization _polarityType;

    [PropertyOrder(1)]
    [Parameter]
    [Name(ParameterNames.Polarization)]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("极化方式")]
    [Description("设置天线极化方向")]
    [DefaultValue(Polarization.Vertical)]
    [Browsable(false)]
    [StandardValues(
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化"
    )]
    [Style(DisplayStyle.Radio)]
    public virtual Polarization PolarityType
    {
        get
        {
            var polarity = GetPolarity();
            if (polarity != null) _polarityType = polarity.Value;
            Console.WriteLine($"当前天线极化方式为 {_polarityType}");
            return _polarityType;
        }
        set
        {
            if (_polarityType != value)
                if (SetPolarity(value))
                {
                    _polarityType = value;
                    Console.WriteLine($"当前天线极化方式设置为 {value}");
                }
        }
    }

    private int _antPath;

    [PropertyOrder(2)]
    [Name("antpath")]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("天线测试路径")]
    [Description("设置天线测试路径")]
    [DefaultValue(1)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|3",
        DisplayValues = "|50MHz~150MHz|150MHz~1GHz|1GHz~12GHz")]
    [Style(DisplayStyle.Dropdown)]
    public int AntPath
    {
        get => _antPath;
        set
        {
            if (_antPath != value)
                if (SetAntPath(value))
                {
                    _antPath = value;
                    Console.WriteLine($"当前天线测试路径设置为 {value}");
                }
        }
    }

    private int _noisPath;

    [PropertyOrder(3)]
    [Name("noisepath")]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("噪声源校准路径")]
    [Description("设置噪声源校准路径")]
    [DefaultValue(1)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|3",
        DisplayValues = "|50MHz~150MHz|150MHz~1GHz|1GHz~12GHz")]
    [Style(DisplayStyle.Dropdown)]
    public int NoisPath
    {
        get => _noisPath;
        set
        {
            if (_noisPath != value)
                if (SetNoisPath(value))
                {
                    _noisPath = value;
                    Console.WriteLine($"当前噪声源校准路径设置为 {value}");
                }
        }
    }

    private bool _noisStatus;

    [PropertyOrder(4)]
    [Name("noisstatus")]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("噪声源供电状态")]
    [Description("设置噪声源供电状态")]
    [DefaultValue(false)]
    public bool NoisStatus
    {
        get => _noisStatus;
        set
        {
            if (SetNoisStatus(value))
            {
                _noisStatus = value;
                Console.WriteLine($"当前噪声源供电设置为 {(value ? "ON" : "OFF")}");
            }
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(0)]
    [Name("com")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("串口号")]
    [Description("设置控制板使用的串口号")]
    [DefaultValue("COM1")]
    [ValueRange(double.NaN, double.NaN, 5)]
    [Style(DisplayStyle.Input)]
    public string Com { get; set; } = "COM1";

    [PropertyOrder(1)]
    [Name("baudRate")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [Description("设置与串口通信的波特率")]
    [DefaultValue(9600)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200",
        DisplayValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200")]
    [Style(DisplayStyle.Dropdown)]
    public int BaudRate { get; set; } = 9600;

    private int _timeout = 45000;

    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.FASTEMT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("超时时间")]
    [DefaultValue(45)]
    [Description("设置搜索超时时间，单位 秒。")]
    [ValueRange(1, 100, 0)]
    [Style(DisplayStyle.Input)]
    public int Timeout
    {
        get => _timeout;
        set
        {
            _timeout = value * 1000;
            if (_client != null)
                _client.Timeout = _timeout;
        }
    }

    [Parameter(IsInstallation = true)]
    [PropertyOrder(3)]
    [Name("isDemo")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("是否为虚拟设备")]
    [Description("是否为虚拟设备")]
    [DefaultValue(false)]
    public bool IsDemo { get; set; }

    #endregion
}