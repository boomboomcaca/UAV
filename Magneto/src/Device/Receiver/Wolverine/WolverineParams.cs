using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.Wolverine;

[DeviceDescription(
    Name = "Wolverine",
    DeviceCategory = ModuleCategory.DirectionFinding,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Version = "1.10.1.0",
    Model = "Wolverine",
    FeatureType = FeatureType.UavDef | FeatureType.ScanDf,
    MaxInstance = 1,
    Description = "应用于自研测向机")]
public partial class Wolverine
{
    #region 天线控制

    private Polarization _dfPolarization = Polarization.Vertical;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name("dfPolarization")]
    [DisplayName("极化方式")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|垂直极化|水平极化",
        StandardValues = "|Vertical|Horizontal")]
    [Description("设置测向天线极化方式")]
    [DefaultValue(Polarization.Vertical)]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Radio)]
    public Polarization DfPolarization
    {
        get => _dfPolarization;
        set
        {
            _dfPolarization = value;
            SendCommand(_dfPolarization is Polarization.Vertical ? "ANT:SEL:POL VERT" : "ANT:SEL:POL HOR");
        }
    }

    #endregion

    #region 射频控制

    private RfMode _rfMode;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.RfMode)]
    [DisplayName("射频模式")]
    [Description("设置接收机射频工作模式")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowNoise|LowDistort",
        DisplayValues = "|常规|低噪声|低失真")]
    [DefaultValue(RfMode.Normal)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Radio)]
    public RfMode RfMode
    {
        get => _rfMode;
        set
        {
            _rfMode = value;
            switch (_rfMode)
            {
                case RfMode.Normal:
                    SendCommand("ATT:RF:MOD NORM");
                    break;
                case RfMode.LowNoise:
                    SendCommand("ATT:RF:MOD LOWN");
                    break;
                case RfMode.LowDistort:
                    SendCommand("ATT:RF:MOD LOWD");
                    break;
                default:
                    SendCommand("ATT:RF:MOD NORM");
                    break;
            }
        }
    }

    private bool _autoAttenuation;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.AttCtrlType)]
    [DisplayName("自动衰减控制")]
    [Description("设置衰减控制的方式")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [DefaultValue(true)]
    [PropertyOrder(2)]
    [Children($"|{ParameterNames.RfAttenuation}|{ParameterNames.IfAttenuation}", false)]
    [Style(DisplayStyle.Switch)]
    public bool AutoAttenuation
    {
        get => _autoAttenuation;
        set
        {
            _autoAttenuation = value;
            if (_autoAttenuation)
            {
                SendCommand("ATT:AUT ON");
            }
            else
            {
                SendCommand("ATT:AUT OFF");
                SendCommand($"ATT {_rfAttenuation}");
            }
        }
    }

    private int _rfAttenuation;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.RfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("衰减控制")]
    [Description("设置衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|30|24|18|12|6|0",
        DisplayValues = "|30|24|18|12|6|0")]
    [ValueRange(0, 30)]
    [DefaultValue(0)]
    [Unit(UnitNames.Db)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Slider)]
    public int RfAttenuation
    {
        get => _rfAttenuation;
        set
        {
            _rfAttenuation = value;
            if (value % 2 != 0) // 射频衰减步进为2
                _rfAttenuation--;
            SendCommand($"ATT {_rfAttenuation}");
        }
    }

    #endregion

    #region 测向控制

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name("avgTimes")]
    [DisplayName("积分次数")]
    [Description("设置测向积分次数")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|200|150|100|50|10|5|0",
        DisplayValues = "|200|150|100|50|10|5|0")] //695333
    [DefaultValue(5)]
    [ValueRange(0, 200)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Slider)]
    public int AvgTimes { get; set; }

    private int _integrationTime = 500;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.IntegrationTime)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间，单位：ms")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000000|1000000|500000|200000|100000|50000|20000|10000|5000|2000|1000|0",
        DisplayValues = "|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|5ms|2ms|1ms|0ms")]
    [ValueRange(0, 5000)]
    [DefaultValue(500)]
    [Unit(UnitNames.Us)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Slider)]
    public int IntegrationTime
    {
        get => _integrationTime;
        set
        {
            _integrationTime = value;
            SendCommand($"MEAS:DFIN:TIM {_integrationTime / 1000}");
        }
    }

    private int _levelThreshold;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.LevelThreshold)]
    [DisplayName("电平门限")]
    [Description("设置测向电平门限，当信号电平超过门限时返回测向结果")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [ValueRange(-40, 120)]
    [DefaultValue(30)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Slider)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            SendCommand($"MEAS:DFIN:THR {value}");
        }
    }

    private int _qualityThreshold = 40;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.QualityThreshold)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，当测向质量超过门限时返回测向结果")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [ValueRange(0, 100)]
    [DefaultValue(40)]
    [Unit(UnitNames.Pct)]
    [PropertyOrder(7)]
    [Style(DisplayStyle.Slider)]
    public int QualityThreshold
    {
        get => _qualityThreshold;
        set
        {
            _qualityThreshold = value;
            SendCommand($"MEAS:DFIN:QUAL {value}");
        }
    }

    #endregion

    #region 扫描参数

    private double _startFrequency;

    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.StartFrequency)]
    [DisplayName("起始频率")]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Category(PropertyCategoryNames.Scan)]
    [ValueRange(300.0d, 8000.0d, 0.000001d)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            if (TaskState != TaskState.Start) return; // 适用于运行时修改参数
            _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            SendCommand($"FREQ:STAR {value * 1e6}");
        }
    }

    private double _stopFrequency;

    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.StopFrequency)]
    [DisplayName("结束频率")]
    [Description("设置扫描终止频率，单位MHz")]
    [Category(PropertyCategoryNames.Scan)]
    [ValueRange(300.0d, 8000.0d, 0.000001d)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(9)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            if (TaskState != TaskState.Start) return; // 适用于运行时修改参数
            _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            SendCommand($"FREQ:STOP {value * 1e6}");
        }
    }

    private double _stepFrequency = 25.0d;

    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef)]
    [Name(ParameterNames.StepFrequency)]
    [DisplayName("扫描步进")]
    [Description("设置频段扫描步进")]
    [Category(PropertyCategoryNames.Scan)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|800|400|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|800kHz|400kHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            if (TaskState != TaskState.Start) return; // 适用于运行时修改参数
            _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            SendCommand($"FREQ:STEP {value * 1e3}");
        }
    }

    private SegmentTemplate[] _segments;

    [PropertyOrder(11)]
    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef,
        Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    [Style(DisplayStyle.Default)]
    [Browsable(false)]
    public Dictionary<string, object>[] ScanSegments
    {
        get => null;
        set
        {
            _curSegmentIndex = 0;
            lock (_bufSignalProcesses)
            {
                _bufSignalProcesses.ForEach(f => f.Dispose());
                _bufSignalProcesses.Clear();
            }

            lock (_signalInners)
                _signalInners.Clear();
            if (value == null) return;
            _segments = Array.ConvertAll(value, item => (SegmentTemplate)item);
        }
    }

    public class SegmentTemplate
    {
        [PropertyOrder(0)]
        [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef)]
        [Name("startFrequency")]
        [Category(PropertyCategoryNames.Scan)]
        [Resident]
        [DisplayName("起始频率")]
        [ValueRange(20.0d, 8000.0d)]
        [ValueRange(8000.0d, 18000.0d, 6, "DF3000A(1)")]
        [DefaultValue(87.0d)]
        [Unit(UnitNames.MHz)]
        [Browsable(false)]
        [Description("设置频段扫描起始频点，单位为MHz")]
        public double StartFrequency { get; set; } = 87.0d;

        [PropertyOrder(1)]
        [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef)]
        [Name("stopFrequency")]
        [Category(PropertyCategoryNames.Scan)]
        [Resident]
        [DisplayName("终止频率")]
        [ValueRange(20.0d, 8000.0d)]
        [ValueRange(8000.0d, 18000.0d, 6, "DF3000A(1)")]
        [DefaultValue(108.0d)]
        [Unit(UnitNames.MHz)]
        [Browsable(false)]
        [Description("设置扫描终止频率，单位MHz")]
        public double StopFrequency { get; set; } = 108.0d;

        [PropertyOrder(2)]
        [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf | FeatureType.UavDef)]
        [Name("stepFrequency")]
        [Category(PropertyCategoryNames.Scan)]
        [Resident]
        [DisplayName("扫描步进")]
        [Browsable(false)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|500|200|100|50|25|12.5|6.25|3.125",
            DisplayValues = "|500kHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
        [ValueRange(0.1d, 500.0d)]
        [DefaultValue(25.0d)]
        [Unit(UnitNames.KHz)]
        [Description("设置频段扫描步进，单位kHz")]
        public double StepFrequency { get; set; } = 25.0d;

        public static explicit operator SegmentTemplate(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            var template = new SegmentTemplate();
            var type = template.GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var name =
                        Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                            ? property.Name
                            : nameAttribute.Name;
                    if (dict.TryGetValue(name, out var value)) property.SetValue(template, value, null);
                }
            }
            catch
            {
                // 容错代码
            }

            return template;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dic = new Dictionary<string, object>();
            var type = GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                        continue;
                    var name =
                        Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                            ? property.Name
                            : nameAttribute.Name;
                    var value = property.GetValue(this);
                    dic.Add(name, value);
                }
            }
            catch
            {
                // 容错代码
            }

            return dic;
        }
    }

    #endregion

    #region 测向门限控制

    private bool _switchSignal = true;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("信号提取")]
    [Description("设置信号提取开关")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [DefaultValue(true)]
    [PropertyOrder(12)]
    [Style(DisplayStyle.Switch)]
    public bool SwitchSignal
    {
        get => _switchSignal;
        set
        {
            _switchSignal = value;
            SendCommand(_switchSignal ? "MEAS:SIGN ON" : "MEAS:SIGN OFF");
        }
    }

    private bool _switchSignalAutoThreshold = true;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("自动门限")]
    [Description("设置信号提取自动门限开关")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [DefaultValue(true)]
    [PropertyOrder(13)]
    [Children("|signalThreshold", false)]
    [Style(DisplayStyle.Switch)]
    public bool SwitchSignalAutoThreshold
    {
        get => _switchSignalAutoThreshold;
        set
        {
            _switchSignalAutoThreshold = value;
            SendCommand(_switchSignalAutoThreshold ? "MEAS:SIGN:THR:AUT ON" : "MEAS:SIGN:THR:AUT OFF");
            if (!_switchSignalAutoThreshold) SignalThreshold = _signalThreshold;
        }
    }

    private int _signalThreshold = 20;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("直线门限值")]
    [Name("signalThreshold")]
    [Description("设置信号提取直线门限")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(20)]
    [PropertyOrder(14)]
    [Style(DisplayStyle.Input)]
    public int SignalThreshold
    {
        get => _signalThreshold;
        set
        {
            _signalThreshold = value;
            SendCommand($"MEAS:SIGN:THR {value}");
        }
    }

    private int _signalBandConc = 500000;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("信号合并带宽")]
    [Description("设置信号合并带宽")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(500000)]
    [PropertyOrder(15)]
    [Style(DisplayStyle.Input)]
    public int SignalBandConc
    {
        get => _signalBandConc;
        set
        {
            _signalBandConc = value;
            SendCommand($"MEAS:SIGN:BAND:CONC {value}");
        }
    }

    private int _signalBandMin = 9000000;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("最小信号带宽")]
    [Description("设置最小信号带宽")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(9000000)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Input)]
    public int SignalBandMin
    {
        get => _signalBandMin;
        set
        {
            _signalBandMin = value;
            SendCommand($"MEAS:SIGN:BAND:MIN {value}");
        }
    }

    private int _signalBandMax = 21000000;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("最大信号带宽")]
    [Description("设置最大信号带宽")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(21000000)]
    [PropertyOrder(17)]
    [Style(DisplayStyle.Input)]
    public int SignalBandMax
    {
        get => _signalBandMax;
        set
        {
            _signalBandMax = value;
            SendCommand($"MEAS:SIGN:BAND:MAX {value}");
        }
    }

    private int _signalThresholdTol = 5;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("信号阈值容差")]
    [Description("设置信号阈值容差")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(5)]
    [PropertyOrder(18)]
    [Style(DisplayStyle.Input)]
    public int SignalThresholdTol
    {
        get => _signalThresholdTol;
        set
        {
            _signalThresholdTol = value;
            SendCommand($"MEAS:SIGN:THR:TOL {value}");
        }
    }

    private int _timeLength = 1;

    [Parameter(AbilitySupport = FeatureType.ScanDf | FeatureType.UavDef)]
    [DisplayName("测向积分时间")]
    [Description("设置宽带测向数据归一化后的优化时间窗")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(1)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Input)]
    public int TimeLength
    {
        get => _timeLength;
        set
        {
            _timeLength = value;
            lock (_bufSignalProcesses)
            {
                _bufSignalProcesses.ForEach(f => f.TimeLength = value);
            }
        }
    }

    #endregion

    #region 安装属性

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [DisplayName("地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [Category(PropertyCategoryNames.Installation)]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [DisplayName("端口")]
    [Description("设置连接到设置的网络控制端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535)]
    [DefaultValue(5025)]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Slider)]
    public int Port { get; set; } = 5025;

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度，海拔")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度,海拔")]
    [DefaultValue("104.063,30.64902,500")]
    [PropertyOrder(21)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("enableGPS")]
    [DisplayName("启用GPS")]
    [Description("设置接收机是否返回GPS数据")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [DefaultValue(false)]
    [PropertyOrder(22)]
    [Style(DisplayStyle.Switch)]
    public bool EnableGps { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("enableCompass")]
    [DisplayName("启用罗盘")]
    [Description("设置接收机是否返回电子罗盘数据")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [DefaultValue(false)]
    [PropertyOrder(23)]
    [Style(DisplayStyle.Switch)]
    public bool EnableCompass { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("compasssIntallingAngle")]
    [DisplayName("罗盘安装夹角")]
    [Description("设置电子罗盘安装夹角,单位：度")]
    [Category(PropertyCategoryNames.Configuration)]
    [ValueRange(0.0f, 360.0f)]
    [DefaultValue(0.0f)]
    [PropertyOrder(24)]
    [Style(DisplayStyle.Slider)]
    public float CompassInstallingAngle { get; set; }

    #endregion
}