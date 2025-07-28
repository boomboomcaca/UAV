using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualFASTEMT;

public partial class VirtualFastemt : DeviceBase, IFastSpectrumScan
{
    #region 成员变量

    /// <summary>
    ///     采集监测数据的线程。
    /// </summary>
    private Task _dataTask;

    /// <summary>
    /// </summary>
    private CancellationTokenSource _dataTokenSource;

    private long _demoCalData;

    #endregion

    #region 重载DeviceBase函数

    public VirtualFastemt(Guid id)
        : base(id)
    {
    }

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        if (!base.Initialized(moduleInfo)) return false;
        return true;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        //初始化线程
        _dataTokenSource = new CancellationTokenSource();
        _dataTask = new Task(CollectData, CurFeature, _dataTokenSource.Token);
        _dataTask.Start();
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        Utils.CancelTask(_dataTask, _dataTokenSource);
        base.Stop();
    }

    /// <summary>
    ///     获取监测数据的线程
    /// </summary>
    /// <param name="obj">启动标志</param>
    private void CollectData(object obj)
    {
        if (!_dataTokenSource.IsCancellationRequested) ReadSpectrumScanData();
    }

    #endregion

    #region 射电天文电测

    /// <summary>
    ///     获取校准数据
    /// </summary>
    /// <returns></returns>
    public FastGeneralScan GetCalibrationData()
    {
        _demoCalData++;
        FastGeneralScan ggs = new();
        var pointCont = ComputeSweepPointCount();
        ggs.Start = StartFrequency;
        ggs.Stop = StopFrequency;
        if (_demoCalData % 2 == 1)
            ggs.Data = GetRandomSpectrum(pointCont, 40);
        else
            ggs.Data = GetRandomSpectrum(pointCont);
        ggs.TotalPoints = pointCont;
        var step = (StopFrequency - StartFrequency) / (pointCont - 1);
        var freqs = new double[ggs.TotalPoints];
        for (var j = 0; j < freqs.Length; j++)
        {
            freqs[j] = StartFrequency + j * step;
            freqs[j] = Math.Round(freqs[j], 6);
        }

        ggs.Freqs = freqs;
        return ggs;
    }

    /// <summary>
    ///     获取随机频谱数组
    /// </summary>
    /// <param name="count"></param>
    /// <param name="wholeOffset"></param>
    /// <returns></returns>
    private float[] GetRandomSpectrum(int count, float wholeOffset = 0f)
    {
        var rd = new Random();
        var data = new float[count];
        for (var i = 0; i < count; i++)
        {
            data[i] = (float)(rd.NextDouble() * 20 - 40);
            if (i is > 1000 and < 2000) data[i] = data[i] + 30;
            data[i] = data[i] + wholeOffset;
        }

        return data;
    }

    /// <summary>
    ///     模拟频谱数据
    /// </summary>
    private void ReadSpectrumScanData()
    {
        var pointCont = ComputeSweepPointCount();
        for (var i = 0; i < RepeatTimes; i++)
        {
            if (_dataTokenSource.IsCancellationRequested) break;
            FastGeneralScan gs = new()
            {
                Start = StartFrequency,
                Stop = StopFrequency,
                Data = GetRandomSpectrum(pointCont),
                TotalPoints = pointCont
            };
            var step = (StopFrequency - StartFrequency) / (pointCont - 1);
            var freqs = new double[gs.TotalPoints];
            for (var j = 0; j < freqs.Length; j++)
            {
                freqs[j] = StartFrequency + j * step;
                freqs[j] = Math.Round(freqs[j], 6);
            }

            gs.Freqs = freqs;
            if (i == RepeatTimes - 1)
                //表示保存这一帧数据;这是最后一帧数据，设备端已经完成扫描，可以停止本次任务，从新开始下一次扫描任务
                SendFastData(gs, true);
            else
                SendFastData(gs, false, false);
            Thread.Sleep(50);
        }
    }

    /// <summary>
    ///     向客户端发送Fast测试数据
    /// </summary>
    /// <param name="data">要发送的数据</param>
    /// <param name="needToSave">标识这一帧数据是否需要保存</param>
    /// <param name="needToStop">标识这一帧数据是否是最后一帧，如果是，那么客户端应该停止当前测试项</param>
    private void SendFastData(FastGeneralScan data, bool needToSave = false, bool needToStop = true)
    {
        var datasToClient = new List<object>(); //提交到客户端的数据
        if (data == null)
            datasToClient.Add(new FastGeneralScan
                { Freqs = null, Data = null, Start = StartFrequency, Stop = StopFrequency, TotalPoints = 0 });
        else
            datasToClient.Add(data);
        datasToClient.Add(needToSave);
        datasToClient.Add(needToStop);
        SendData(datasToClient);
    }

    public double GetCurrentRealRbw()
    {
        return ResolutionBandwidth;
    }

    public double GetCurrentRealVbw()
    {
        return VideoBandwidth;
    }

    public float GetCurrentRealAtt()
    {
        return Attenuation;
    }

    public void Reset()
    {
    }

    /// <summary>
    ///     计算扫频频点数
    /// </summary>
    /// <returns></returns>
    private int ComputeSweepPointCount()
    {
        var hzStartFreq = StartFrequency * 1000000;
        var hzStopFreq = StopFrequency * 1000000;
        var hzRbw = ResolutionBandwidth * 1000;
        var count = (int)Math.Ceiling((hzStopFreq - hzStartFreq) / hzRbw);
        count = count < 101 ? 101 : count;
        count = count > 100001 ? 100001 : count;
        return count;
    }

    #endregion
}