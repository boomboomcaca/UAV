using System;
using System.Collections.Generic;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF255;

public partial class Ddf255 : DeviceBase
{
    #region 全局变量

    /// <summary>
    ///     业务数据类型
    /// </summary>
    private MediaType _media = MediaType.None;

    /// <summary>
    ///     保留离散扫描应包含的频点，用于判断本包数据的索引
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    private readonly object _lockFreqs = new();

    /// <summary>
    ///     缓存子通道数量
    /// </summary>
    private List<IfmcaTemplate> _preChannels;

    /// <summary>
    ///     用于浮点数比较大小
    /// </summary>
    private const double Epsilon = 1.0E-7d;

    private List<AngleCompensationInfo> _angleCompensationList = new();
    private int _offsetPscan;

    #endregion

    #region 框架实现

    public Ddf255(Guid id) : base(id)
    {
    }

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        // 首先调用基类方法
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            _media = MediaType.None;
            InitResources();
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        _offsetPscan = 0;
        base.Start(feature, dataPort);
        StartTask();
    }

    public override void Stop()
    {
        StopTask();
        _offsetPscan = 0;
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResources();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (TaskState == TaskState.Start
            && CurFeature == FeatureType.MScan
            && (name.Equals(ParameterNames.MscanPoints) || name.Equals(ParameterNames.DwellSwitch) ||
                name.Equals(ParameterNames.SquelchThreshold)))
        {
            StopTask();
            StartTask();
        }
    }

    #endregion
}