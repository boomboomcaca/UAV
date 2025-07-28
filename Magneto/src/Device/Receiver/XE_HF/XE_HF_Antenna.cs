using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_HF;

public partial class XeHf : IAntennaController
{
    //当前使用的天线，设备中对应的天线名（配置在天线码中）
    public string CurrAntenna { get; set; } = "ANT_DF";

    public bool SendControlCode(string code)
    {
        try
        {
            //注意_antennaTemplates是所有配置天线，Antennas是当前功能配置的天线
            if (_antennaTemplates.Length == 1)
            {
                //表示只有原厂天线,不需要再重设天线分段信息
                CurrAntenna = "ANT_DF";
            }
            else
            {
                CurrAntenna = code;
                //发送天线修改及通道参数信息,任务运行时才需要设置
                if (_cmdCollector != null)
                    try
                    {
                        _cmdCollector.SetAntenna(CurrAntenna);
                    }
                    catch
                    {
                    }
            }

            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"天线码值解析异常，天线码: {code}，异常信息：{ex}");
            return false;
        }
    }

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
    public AntennaSelectionMode AntennaSelectedType { get; set; } = AntennaSelectionMode.Auto;

    private AntennaInfo[] _antennaTemplates;

    [Parameter(IsInstallation = true, Template = typeof(AntennaInfo))]
    [Name("antennaSet")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装的天线集合")]
    [Description("设备配置的天线集合")]
    [PropertyOrder(4)]
    public Dictionary<string, object>[] AntennaSet
    {
        get => null;
        set
        {
            if (value == null) return;
            _antennaTemplates = Array.ConvertAll(value, item => (AntennaInfo)item);
        }
    }

    public List<AntennaInfo> Antennas { get; set; }

    [Parameter(IsInstallation = true, Template = typeof(AntennaInfo))]
    [Name("antennas")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("选择的天线集合")]
    [Description("设备选择的天线集合")]
    [PropertyOrder(4)]
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

    protected Polarization BufPolarityType;

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
    public virtual Polarization PolarityType
    {
        get => BufPolarityType;
        set
        {
            if (BufPolarityType != value)
            {
                BufPolarityType = value;
                Console.WriteLine($"当前天线极化方式设置为 {value}");
                this.OpenAntenna(_frequency, value, ref BufIsActive, ref AntennaIdGuid);
            }
        }
    }

    protected bool BufIsActive;

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
    public bool IsActive
    {
        get => BufIsActive;
        set
        {
            if (AntennaSelectedType == AntennaSelectionMode.Auto) return;
            if (BufIsActive != value)
            {
                var active = value;
                Console.WriteLine($"当前天线有源设置为 {value}");
                this.OpenAntenna(ref active, ref AntennaIdGuid, AntennaIdGuid);
                BufIsActive = active;
            }
        }
    }

    protected Guid AntennaIdGuid = Guid.Empty;

    [Parameter]
    [Name(ParameterNames.AntennaId)]
    [Category(PropertyCategoryNames.AntennaControl)]
    [DisplayName("天线名称")]
    [Description("当前选择的天线")]
    [Browsable(false)]
    [PropertyOrder(2)]
    public Guid AntennaId
    {
        get => AntennaIdGuid;
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
                Console.WriteLine("当前选择的天线打通方式更改为自动模式");
                AntennaSelectedType = AntennaSelectionMode.Auto;
                this.OpenAntenna(_frequency, BufPolarityType, ref BufIsActive, ref AntennaIdGuid);
            }
            else
            {
                Console.WriteLine($"当前选择的天线为 {info.Name}:{value}");
                AntennaSelectedType = AntennaSelectionMode.Manual;
                this.OpenAntenna(ref BufIsActive, ref AntennaIdGuid, AntennaIdGuid);
            }
        }
    }

    protected double AntennaFrequency;

    double IAntennaController.Frequency
    {
        get => AntennaFrequency;
        set
        {
            AntennaFrequency = value;
            if (AntennaSelectedType != AntennaSelectionMode.Manual)
                this.OpenAntenna(AntennaFrequency, BufPolarityType, ref BufIsActive, ref AntennaIdGuid);
        }
    }

    #endregion
}