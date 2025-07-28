using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Contract.BaseClass;

public abstract class AntennaControllerBase(Guid deviceId) : DeviceBase(deviceId), IAntennaController
{
    /// <summary>
    ///     设置打通码
    /// </summary>
    /// <param name="code"></param>
    public abstract bool SendControlCode(string code);

    #region Installation

    [Parameter(IsInstallation = true)]
    [Name("antennaSelectionMode")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线选择模式")]
    [Description("设置通过天线选择器打通指定天线的方式")]
    [DefaultValue(AntennaSelectionMode.Auto)]
    [StandardValues(
        StandardValues = "|Auto|Manual|Polarization",
        DisplayValues = "|自动|手动|极化方式"
    )]
    [Browsable(false)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Radio)]
    public virtual AntennaSelectionMode AntennaSelectedType { get; set; } = AntennaSelectionMode.Auto;

    [Parameter(IsInstallation = true, Template = typeof(AntennaInfo))]
    [Name("antennaSet")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装的天线集合")]
    [Description("设备配置的天线集合")]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Default)]
    public virtual Dictionary<string, object>[] AntennaSet { get; set; }

    public virtual List<AntennaInfo> Antennas { get; set; }

    [Parameter(IsInstallation = true, Template = typeof(AntennaInfo))]
    [Name("antennas")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("选择的天线集合")]
    [Description("设备选择的天线集合")]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] AntennasSelect
    {
        get => null;
        set
        {
            if (value == null) return;
            Antennas = Array.ConvertAll(value, item => (AntennaInfo)item).ToList();
        }
    }

    #endregion

    #region 运行参数

    private Polarization _polarityType;

    [Parameter]
    [Name("polarization")]
    [Category(PropertyCategoryNames.AntennaControl)]
    [DisplayName("极化方式")]
    [Description("设置打通的极化方式")]
    [DefaultValue(Polarization.Vertical)]
    [Browsable(false)]
    [StandardValues(
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化"
    )]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Radio)]
    public virtual Polarization PolarityType
    {
        get => _polarityType;
        set
        {
            if (_polarityType != value)
            {
                _polarityType = value;
                Console.WriteLine($"当前天线极化方式设置为 {value}");
                this.OpenAntenna(_frequency, value, ref _isActive, ref _antennaId);
            }
        }
    }

    private bool _isActive;

    [Parameter]
    [Name("isActive")]
    [Category(PropertyCategoryNames.AntennaControl)]
    [DisplayName("有源天线")]
    [Description("当前天线为有源天线或无源天线")]
    [StandardValues(
        StandardValues = "|true|false",
        DisplayValues = "|有源|无源")]
    [PropertyOrder(5)]
    [Browsable(true)]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (AntennaSelectedType == AntennaSelectionMode.Auto) return;
            if (_isActive != value)
            {
                var active = value;
                Console.WriteLine($"当前天线有源设置为 {value}");
                this.OpenAntenna(ref active, ref _antennaId, _antennaId);
                _isActive = active;
            }
        }
    }

    private Guid _antennaId = Guid.Empty;

    [Parameter]
    [Name(ParameterNames.AntennaId)]
    [Category(PropertyCategoryNames.AntennaControl)]
    [DisplayName("天线名称")]
    [Description("当前选择的天线")]
    [Browsable(false)]
    [PropertyOrder(2)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public virtual Guid AntennaId
    {
        get => _antennaId;
        set
        {
            var info = Antennas.Find(item => item.Id == value);
            if (info == null)
            {
                Console.WriteLine($"天线集合中不存在ID为{value}的天线");
                return;
            }

            if (value == Guid.Empty)
            {
                // 自动
                // Console.WriteLine("当前选择的天线打通方式更改为自动模式");
                AntennaSelectedType = AntennaSelectionMode.Auto;
                this.OpenAntenna(_frequency, _polarityType, ref _isActive, ref _antennaId);
            }
            else
            {
                // Console.WriteLine($"当前选择的天线为 {info.Name}:{value}");
                AntennaSelectedType = AntennaSelectionMode.Manual;
                this.OpenAntenna(ref _isActive, ref _antennaId, value);
            }
        }
    }

    private double _frequency;

    public virtual double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            if (AntennaSelectedType != AntennaSelectionMode.Manual)
                this.OpenAntenna(_frequency, _polarityType, ref _isActive, ref _antennaId);
        }
    }

    #endregion
}