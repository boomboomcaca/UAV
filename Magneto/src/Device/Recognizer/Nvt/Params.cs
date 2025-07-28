using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;
#if !DEMO
using System.Net;
#endif

namespace Magneto.Device.Nvt;

[DeviceDescription(
    Name = "图像识别设备",
    Manufacturer = "Aleph",
    DeviceCategory = ModuleCategory.Recognizer,
    Version = "1.10.1.0",
    Model = "Nvt",
    Description = "图象识别设备，包括电机，摄像头，识别、跟踪算法等一套完整设备",
    MaxInstance = 1,
    FeatureType = FeatureType.UavDef)]
public partial class Nvt
{
    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [Name("ipAddress")]
    [DisplayName("设备地址")]
    [Description("设置图像识别设备的摄像头的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [Name("port")]
    [DisplayName("设备端口号")]
    [Description("设置图像识别设备的摄像头的端口号")]
    [DefaultValue(6002)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度，海拔")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度,海拔")]
    [DefaultValue("104.06633519196313,30.639022659740476,500")]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [Name("userName")]
    [DisplayName("用户名")]
    [Description("图像识别设备的登录用户名。")]
    [DefaultValue("system")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string UserName { get; set; }

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Name("password")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("密码")]
    [Description("图像识别设备的登录密码。")]
    [DefaultValue("system")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Password { get; set; }

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Name("rtspUrl")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("Rtsp地址")]
    [Description("rtsp://{设置地址}:554/channel=0,stream=0")]
    [DefaultValue("rtsp://192.168.1.245:554/channel=0,stream=0")]
    public string RtspUrl { get; set; }

    #endregion

    #region 控制参数

    private readonly int[] _rect = new int[4];

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("rect")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("选择跟踪区域")]
    [Description("两个坐标，x1,x2,y1,y2表示一个区域")]
    public int[] Rect
    {
        get => _rect;
        set
        {
            if (value is null) return;
            if (!SetTraceArea(value)) return;
            _rect[0] = value[0];
            _rect[1] = value[1];
            _rect[2] = value[2];
            _rect[3] = value[3];
        }
    }

    private byte _direction;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("direction")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("云台控制按钮")]
    [Description("手工云台台控制按钮")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|11|12|13|14|15|16|21|22|23|24|25|26|27|28",
        DisplayValues = "|停止|截图|焦距变大|焦距变小|焦点前调|焦点后调|光圈扩大|光圈缩小|上|下|坐|右|右上|右下|左上|左下")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Ics)]
    public byte Direction
    {
        get => _direction;
        set
        {
            if (value is 1)
            {
                ScreenShot();
                return;
            }

            if (value is not 0) _direction = value;
            ControlPtz(_direction, value.Equals(0) ? 0 : (uint)1, (uint)Speed);
        }
    }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("speed")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("云台转动速度")]
    [Description("设置云台的转动速度")]
    [DefaultValue(40)]
    [Style(DisplayStyle.Default)]
    public int Speed { get; set; }

    private bool _isLaser;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("isLaser")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("激光")]
    [Description("激光开光")]
    public bool IsLaser
    {
        get => _isLaser;
        set
        {
            _isLaser = value;
            TurnLaser(value);
        }
    }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("isAutoTrack")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("自动引导")]
    [Description("切换自动引导开关")]
    [DefaultValue(true)]
    public bool IsAutoTrack
    {
        get;
        set;
        //if (_isAutoTrack) return;
        //SetTraceAuto(false);
        //TurnTrack();
    }

    private bool _stopTrack;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("stopTrack")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("停止跟踪")]
    [Description("停止跟踪")]
    public bool StopTrack
    {
        get => _stopTrack;
        set
        {
            _stopTrack = value;
            SetTraceAuto(false);
            TurnTrack();
        }
    }

    private bool _visibleLight;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("visibleLight")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("可见光")]
    [Description("可见光/热成像")]
    public bool VisibleLight
    {
        get => _visibleLight;
        set
        {
            _visibleLight = value;
            TurnVisibleLight(value);
        }
    }

    private bool _hideTrackRect;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("hideTrackRect")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("跟踪框")]
    [Description("跟踪框开关")]
    public bool HideTrackRect
    {
        get => _hideTrackRect;
        set
        {
            _hideTrackRect = value;
            TurnHideTrackRect(value);
        }
    }

    private bool _hideIdentifyRect;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("hideIdentifyRect")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("识别框")]
    [Description("识别框开关")]
    public bool HideIdentifyRect
    {
        get => _hideIdentifyRect;
        set
        {
            _hideIdentifyRect = value;
            TurnHideIdentifyRect(value);
        }
    }

    private bool _isRecording;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("isRecording")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("录像")]
    [Description("启动/停止录像")]
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            _isRecording = value;
            Record(value);
        }
    }

    /// <summary>
    ///     设置光电目址信息
    /// </summary>
    public class GuideTemplate
    {
        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("optoNumber")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("光电编号")]
        public uint OptoNumber { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("systemNumber")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("系统编号")]
        public uint SystemNumber { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("longitude")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("目标经度")]
        public double Longitude { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("latitude")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("目标纬度")]
        public double Latitude { get; set; }

        [Parameter(AbilitySupport = FeatureType.Uavd)]
        [Name("height")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("目标高度")]
        public double Height { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("distance")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("目标距离")]
        public double Distance { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("horAngle")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("目标方位")]
        public double HorAngle { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("verAngle")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("目标俯仰")]
        public double VerAngle { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("UserId")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("用户关联ID")]
        public ushort UserId { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("mode")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("引导模式")]
        [Description("0-方位俯仰距离，1-经纬高")]
        public byte Mode { get; set; }

        [Parameter(AbilitySupport = FeatureType.UavDef)]
        [Name("type")]
        [Category(PropertyCategoryNames.Misc)]
        [DisplayName("远离还是抵近")]
        [Description("0-抵近  1--远离")]
        public byte Type { get; set; }

        public static explicit operator GuideTemplate(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            var template = new GuideTemplate();
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

    private Dictionary<string, object>[] _targetPosition;

    [Parameter(AbilitySupport = FeatureType.UavDef, Template = typeof(GuideTemplate))]
    [Name("targetPosition")]
    [DisplayName("设置光电目址信息")]
    [Category(PropertyCategoryNames.Misc)]
    [Description("频段信息，存放频段扫描的频段信息")]
    [Browsable(false)]
    public Dictionary<string, object>[] TargetPosition
    {
        get => _targetPosition;
        set
        {
            _targetPosition = value;
            if (_targetPosition is null) return;
            if (!IsAutoTrack) return;
            _ = Array.ConvertAll(value, item => (GuideTemplate)item)[0];
            Address.Split(',');
            //Bearing.Complex(double.Parse(latAndLong[1]), double.Parse(latAndLong[0]), guideTemplate.Latitude,
            //    guideTemplate.Longitude);
            SetTraceAuto(false);
            TurnTrack();

#if !DEMO
            _udpClient.Send(cTargetDirectionProtocal.ToBytes(), new IPEndPoint(IPAddress.Parse("192.168.1.245"), 9966));
#endif
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                SetTraceAuto(true);
            });
        }
    }


    public static void GetAzimuthAndElevation(double sourceLatitude, double sourceLongitude, double sourceAltitude,
        double targetLatitude, double targetLongitude, double targetAltitude, out double azimuth, out double elevation)
    {
        azimuth = Bearing.Complex(sourceLatitude, sourceLongitude, targetLatitude, targetLongitude);
        var distance = Utils.GetDistance(sourceLongitude, sourceLatitude, targetLongitude, targetLatitude);
        elevation = Math.Tan((targetAltitude - sourceAltitude) / distance);
    }

    #endregion
}