using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Device.XE_VUHF.Protocols.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF;

public partial class XeVuhf : IAntennaController
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
                var infos = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                CurrAntenna = infos.Length == 2 ? infos[0] : code;
                //当前的天线信息
                var currAntInfo = Antennas.Find(item => item.PassiveCode == code);
                //查找是否存在水平测向天线
                var horizAntInfo = Array.Find(_antennaTemplates,
                    x => x.AntennaType == AntennaType.DirectionFinding && x.Polarization == Polarization.Horizontal);
                if (horizAntInfo != null && infos.Length == 2)
                {
                    //如果存在水平测向天线都需要强制设置天线分段信息,且垂直和水平分别对应有源和无源两个样本文件，此时将不对外公布放大器开关参数
                    _ampliConfig = PolarityType == Polarization.Vertical;
                    SendForceSubRangeCmd(currAntInfo, infos[1]);
                }

                //发送天线修改及通道参数信息,任务运行时才需要设置
                if (_cmdCollector != null)
                    try
                    {
                        _cmdCollector.SetAntenna(CurrAntenna, _ampliConfig);
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

    #region 辅助函数

    private void SendForceSubRangeCmd(AntennaInfo curr, string code)
    {
        uint band = 40000000, fmin = 20000000, fmax = 3000000000;
        ForcedSubrangeRequest request = new();
        List<SubRangeInfo> tempRanges = new();
        var xeSubRanges = _antennaSubRanges.Values.ToArray();
        switch (code)
        {
            case "J1,J2,J3":
                for (var i = 0; i < xeSubRanges.Length; ++i)
                    if (xeSubRanges[i].Jnumer == 1)
                        //因为J1包含了垂直和水平天线的范围
                        tempRanges.Add(GetSubRangeInfo(xeSubRanges[i].Jnumer, xeSubRanges[i].Fmin,
                            xeSubRanges[i + 1].Fmin + band));
                    else
                        tempRanges.Add(GetSubRangeInfo(xeSubRanges[i].Jnumer, xeSubRanges[i].Fmin,
                            xeSubRanges[i].Fmax));
                break;
            case "J1,J3":
                for (var i = 0; i < xeSubRanges.Length; ++i)
                    if (xeSubRanges[i].Jnumer == 2)
                        //J2为水平天线，不能被选中
                        tempRanges.Add(GetSubRangeInfo(xeSubRanges[i].Jnumer, xeSubRanges[0].Fmax,
                            xeSubRanges[0].Fmax));
                    else
                        tempRanges.Add(GetSubRangeInfo(xeSubRanges[i].Jnumer, xeSubRanges[i].Fmin,
                            xeSubRanges[i].Fmax));
                break;
            case "J1":
                tempRanges.Add(GetSubRangeInfo(1, (uint)(curr.StartFrequency * 1000000),
                    (uint)(curr.StopFrequency * 1000000)));
                tempRanges.Add(GetSubRangeInfo(2, fmin, fmin));
                tempRanges.Add(GetSubRangeInfo(3, fmax, fmax));
                break;
            case "J2":
                tempRanges.Add(GetSubRangeInfo(1, fmin, fmin));
                tempRanges.Add(GetSubRangeInfo(2, (uint)(curr.StartFrequency * 1000000),
                    (uint)(curr.StopFrequency * 1000000)));
                tempRanges.Add(GetSubRangeInfo(3, fmax, fmax));
                break;
        }

        request.NumOfSubranges.Value = (uint)tempRanges.Count;
        if (tempRanges.Count > 0) request.Subranges = tempRanges.ToArray();
        SendCmd(request.GetBytes());
    }

    private static SubRangeInfo GetSubRangeInfo(int jnumber, uint fmin, uint fmax)
    {
        var info = new SubRangeInfo();
        info.SubrangeNo.Value = (uint)jnumber;
        info.FMin.Value = fmin;
        info.FMax.Value = fmax;
        return info;
    }

    #endregion
}