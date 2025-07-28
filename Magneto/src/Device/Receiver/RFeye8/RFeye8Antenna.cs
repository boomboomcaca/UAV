using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye8;

public partial class RFeye8 : IAntennaController
{
    //当前使用的天线，设备中对应的天线名（配置在天线码中）
    public string CurrAntenna { get; set; } = "自动";

    /// <summary>
    ///     射频眼三个口对应码值分别是1，2，3
    ///     如果外接了天线控制器，则码值配置成EXT|A|B，EXT为外接标识，A为设备内部控制码，B为外部天线控制器控制码
    /// </summary>
    /// <param name="code"></param>
    public bool SendControlCode(string code)
    {
        try
        {
            Console.WriteLine($"当前天线码为 {code}");
            var tempCodes = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (!tempCodes[0].Contains("ext", StringComparison.OrdinalIgnoreCase))
            {
                _antennaCode = int.Parse(code);
            }
            else if (tempCodes.Length == 3)
            {
                _antennaCode = int.Parse(tempCodes[1]);
                //发送外部天线码
                lock (_switchSocketLock)
                {
                    if (_switchSocket?.Connected == true)
                    {
                        var tempCode = Encoding.ASCII.GetBytes(tempCodes[2] + "\r\n");
                        lock (_switchSocketLock)
                        {
                            if (_switchSocket?.Connected == true)
                                try
                                {
                                    _switchSocket.Send(tempCode);
                                }
                                catch
                                {
                                    //如果发送失败则尝试重连
                                    TryConnectToExtAntController();
                                }
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine(ex.ToString());
#endif
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

    private int _antennaCode = 1;
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
                this.OpenAntenna(Frequency, value, ref BufIsActive, ref AntennaIdGuid);
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
                this.OpenAntenna(Frequency, BufPolarityType, ref BufIsActive, ref AntennaIdGuid);
                Console.WriteLine($"天线ID：{AntennaIdGuid}");
            }
            else
            {
                Console.WriteLine($"当前选择的天线为 {info.Name}:{value}");
                AntennaSelectedType = AntennaSelectionMode.Manual;
                this.OpenAntenna(ref BufIsActive, ref AntennaIdGuid, value);
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