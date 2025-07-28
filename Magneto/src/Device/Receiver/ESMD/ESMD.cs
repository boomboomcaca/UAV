using System;
using System.Collections.Generic;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMD;

public partial class Esmd : DeviceBase
{
    /// <summary>
    ///     用于浮点数比较大小
    /// </summary>
    private readonly double _epsilon = 1.0E-7d;

    #region 构造函数

    public Esmd(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 成员变量

    private MediaType _media = MediaType.None;

    /// <summary>
    ///     保存离散扫描频点列表用于判断本包数据索引
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    private readonly object _lockFreqs = new();

    /// <summary>
    ///     电平数据缓存，由于电平数据和频谱数据分别为TCP查询方式和UDP推送模式获取，为保证两个数据量对等，
    ///     使界面显示效果相对流畅，缓存最新电平数据，在收到频谱数据时再同步增加电平数据
    /// </summary>
    private float _level = float.MinValue;

    /// <summary>
    ///     用于过滤扫描功能中在收到扫描数据前收到的无效的频谱数据
    /// </summary>
    private bool _bReceivedScan;

    /// <summary>
    ///     开始获取TDOA数据标记
    /// </summary>
    private bool _isReadTdoaStart;

    /// <summary>
    ///     避免被调用时频繁申请内存
    /// </summary>
    private readonly byte[] _tcpRecvBuffer = new byte[1024 * 1024];

    #region DDC

    /// <summary>
    ///     缓存子通道数量
    /// </summary>
    private List<IfmcaTemplate> _preChannels;

    #endregion

    /// <summary>
    ///     修正值
    /// </summary>
    private IDictionary<long, long> _frequencyOffsetDic; // 频率修正表

    /// <summary>
    ///     频率逆向修正表
    /// </summary>
    private IDictionary<long, long> _reverseFrequencyOffsetDic;

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        // 首先调用基类方法
        var result = base.Initialized(moduleInfo);
        if (result) InitResources();
        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _level = float.MinValue;
        _bReceivedScan = false;
        StartTask();
    }

    public override void Stop()
    {
        _isReadTdoaStart = false;
        StopTask();
        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
    }

    public override void SetParameter(string name, object value)
    {
        if ("frequency".Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            var temp = (long)(Convert.ToDouble(value) * 1000000);
            if (_frequencyOffsetDic.TryGetValue(temp, out var value1)) value = value1 / 1000000.0d;
        }

        if (CurFeature == FeatureType.IFOUT && "IFBandwidth".Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            IfBandwidth = Convert.ToDouble(value) > 500 ? 800 : 300;
            return;
        }

        base.SetParameter(name, value);
        if (name != null
            && TaskState == TaskState.Start
            && CurFeature == FeatureType.MScne
            && (name.Equals(ParameterNames.MscanPoints)
                || name.Equals(ParameterNames.DwellSwitch)
                || name.Equals(ParameterNames.SquelchThreshold)))
        {
            StopTask();
            StartTask();
        }

        if (name != null
            && TaskState == TaskState.Start
            && CurFeature == FeatureType.MScan
            && name.Equals(ParameterNames.MscanPoints))
        {
            StopTask();
            StartTask();
        }
    }

    #endregion
}