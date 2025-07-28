using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.GSMR;

public partial class Gsmr : ScanBase
{
    private readonly List<SDataCellular> _datas;
    private readonly object _syncLocker = new();
    private readonly Timer _timer;

    public Gsmr(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
        _datas = new List<SDataCellular>();
        _timer = new Timer
        {
            Interval = DecodeDataSendInterval
        };
        _timer.Elapsed += Timer_Elapsed;
        _timer.Enabled = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        StartMultiSegments();
        _timer.Start();
        (Decoder as DeviceBase)?.Start(FeatureType.BsDecoding, this);
        return true;
    }

    public override bool Stop()
    {
        _timer.Stop();
        (Decoder as DeviceBase)?.Stop();
        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency or ParameterNames.StepFrequency)
            // 过滤从前端直接设置的起始结束频率等参数
            // 如果不过滤，这三个参数会与频段参数冲突
            return;
        SetParameterInternal(name, value);
        if (name == "bandList")
            if (_freqBands != null)
                lock (_syncLocker)
                {
                    _datas.RemoveAll(p => !_freqBands.Any(q =>
                        p.Frequency.CompareWith(q.StartFrequency) >= 0 &&
                        p.Frequency.CompareWith(q.StopFrequency) <= 0));
                }
    }

    public override void OnData(List<object> datas)
    {
        SendDataWithSpan(datas);
        if (datas?.Any() != true) return;
        foreach (var item in datas)
        {
            if (item is not SDataCellular data) continue;
            var legal = _freqBands?.Any(p =>
                data.Frequency.CompareWith(p.StartFrequency) >= 0 &&
                data.Frequency.CompareWith(p.StopFrequency) <= 0) != false;
            if (!legal)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"过滤数据=> 频率：{data.Frequency} 网络制式：{data.DuplexMode} 信道号:{data.Channel} 接收电平:{data.RxPower}");
                Console.ResetColor();
                continue;
            }

            lock (_syncLocker)
            {
                var index = _datas.FindIndex(p =>
                    p.DuplexMode == data.DuplexMode && p.Channel == data.Channel && p.Mnc == data.Mnc &&
                    p.Mcc == data.Mcc
                    && p.Lac == data.Lac && p.Ci == data.Ci);
                if (index < 0)
                    _datas.Add(data);
                else
                    _datas[index] = data;
            }
        }
    }

    protected override void StartDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Start(FeatureType.SCAN, this);
    }

    protected override void StopDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Stop();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        lock (_syncLocker)
        {
            if (_datas.Count == 0) return;
            var list = _datas.ConvertAll(p => (object)p);
            SendData(list);
        }
    }
}