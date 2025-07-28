using System;
using System.Collections.Generic;
using System.Diagnostics;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.RTV;

public partial class Rtv : DriverBase
{
    /// <summary>
    ///     当前播放的频率
    /// </summary>
    private double _currentFrequency;

    /// <summary>
    ///     当前播放的节目号
    /// </summary>
    private int _currentNumber;

    private string _currentProgramName;

    /// <summary>
    ///     当前播放的制式
    /// </summary>
    private TvStandard _currentStandard = TvStandard.ANAFM;

    private string _playProgram;
    private bool _startRealPlay;

    public Rtv(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        (AvReceiver as DeviceBase)?.Start(FeatureType.AVProcess, this);
        (TvAnalysis as DeviceBase)?.Start(FeatureType.RTV, this);
        return true;
    }

    public override bool Stop()
    {
        base.Stop();
        (AvReceiver as DeviceBase)?.Stop();
        (TvAnalysis as DeviceBase)?.Stop();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == "playProgram" && value != null)
        {
            _playProgram = value.ToString();
            // 制式|频率|节目编号|节目名称
            var split = value.ToString()?.Split('|');
            if (split is { Length: < 4 })
                // Trace.WriteLine("播放的节目格式错误！");
                return;
            Enum.TryParse(split?[0], out _currentStandard);
            double.TryParse(split?[1], out _currentFrequency);
            int.TryParse(split?[2], out _currentNumber);
            _currentProgramName = split?[3];
            _startRealPlay = true;
        }
    }

    public override void OnData(List<object> data)
    {
        base.OnData(data);
        if (data?.Find(item => item is SDataVideoChannel) is SDataVideoChannel)
            SetParameter("playProgram", _playProgram);
        if (_startRealPlay && data?.Find(item => item is SDataPlayResult) is SDataPlayResult playResult)
        {
            if (playResult.OperateType == OperateType.RealPlayStart)
            {
                if (!playResult.Result)
                {
                    playResult.OperateType = OperateType.RealPlay;
                    _startRealPlay = false;
                }
                else
                {
                    Trace.WriteLine("电视分析仪开启频道成功，准备播放视频...");
                    data.Remove(playResult);
                    AvReceiver.SetParameter("startRealPlay", true);
                }
            }
            else if (playResult.OperateType == OperateType.RealPlay)
            {
                Trace.WriteLine($"播放结果:{playResult.Result}");
                _startRealPlay = false;
            }
        }

        if (data?.Count > 0) SendData(data);
    }
}