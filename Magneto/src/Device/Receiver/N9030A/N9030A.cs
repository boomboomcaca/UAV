using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.N9030A;

public partial class N9030A : DeviceBase, IFastSpectrumScan
{
    #region 构造函数

    public N9030A(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     采集监测数据的线程。
    /// </summary>
    private Task _dataTask;

    /// <summary>
    /// </summary>
    private CancellationTokenSource _dataTokenSource;

    /// <summary>
    ///     客户端对象。
    /// </summary>
    protected Socket Socket;

    /// <summary>
    ///     频谱仪连接标志。
    /// </summary>
    protected bool IsConnect = false;

    /// <summary>
    ///     缓存上一次执行的功能
    /// </summary>
    private FeatureType _prevAbility = FeatureType.None;

    /// <summary>
    ///     是否发送频段扫描指令
    /// </summary>
    private bool _isSendScanCmd;

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        if (!base.Initialized(moduleInfo)) return false;
        InitNetFeatures();
        SetHeartBeat(Socket);
        SendCommand("SYST:PRES");
        return true;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        //初始化线程
        InitTasks();
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        SendCommand("SYST:PRES");
        SendCommand("TRAC:TYPE WRIT");
        SendCommand("TRACE:CLE TRACE1");
        SendCommand("*CLS");
        _isSendScanCmd = false;
        try
        {
            Utils.CancelTask(_dataTask, _dataTokenSource);
        }
        catch
        {
        }

        base.Stop();
    }

    public override void Dispose()
    {
        ClearResource();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 射电天文电测

    /// <summary>
    ///     获取校准数据
    /// </summary>
    /// <returns></returns>
    public FastGeneralScan GetCalibrationData()
    {
        //设置校准参数
        SetCalibrationArgs();
        SendCommand(":INIT:IMM");
        FastGeneralScan gs = new()
        {
            Start = StartFrequency,
            Stop = StopFrequency
        };
        //阻塞，直到扫描完成
        SendCommand("*OPC?", 1000000);
        var strData = SendCommand("TRAC:DATA? TRACE1", 30000); //字符串形式的数据
        gs.Data = ConvertStringDataToFloatArray(strData);
        var strFreq = SendCommand("TRAC:X? TRACE1", 30000); //字符串形式的频率
        gs.Freqs = HandleFreqData(strFreq);
        gs.TotalPoints = gs.Data.Length;
        return gs;
    }

    /// <summary>
    ///     读取频谱扫描数据
    /// </summary>
    private void ReadSpectrumScanData()
    {
        //设置扫频参数
        SetSpectrumScanArgs();
        var averDatas = Array.Empty<float>(); //存放平均值（当前频段最终需要保存的测试值）
        var freqList = Array.Empty<double>(); //存储频率列表
        //根据设置的重复次数多次循环
        for (var i = 0; i < RepeatTimes; i++)
        {
            var gs = new FastGeneralScan
            {
                Start = StartFrequency,
                Stop = StopFrequency
            };
            //如果还未存储当前测试项的频率列表，则从设备获取获取频率
            if (freqList.Length <= 0)
                try
                {
                    var strFreq = SendCommand("TRAC:X? TRACE1", 30000);
                    freqList = HandleFreqData(strFreq);
                }
                catch
                {
                }

            gs.Freqs = new double[freqList.Length];
            Array.Copy(freqList, gs.Freqs, freqList.Length);
            gs.TotalPoints = freqList.Length;
            //获取测量值
            float[] dataValues = null;
            try
            {
                var strData = SendCommand("TRAC:DATA? TRACE1", 30000);
                dataValues = ConvertStringDataToFloatArray(strData);
            }
            catch
            {
            }

            //判断本次循环扫描是否完成
            var scanStatus = SendCommand("STAT:OPER:COND?");
            if (scanStatus.Equals("0")) //仪表内部机制，当本次循环扫描完成时，返回0
            {
                //计算平均值
                if (averDatas.Length <= 0)
                    averDatas = dataValues;
                else
                    //做线性平均值计算（仅用仪表扫描完成后的一帧做平均）
                    for (var j = 0; j < averDatas.Length; j++)
                    {
                        var current = Math.Pow(10.0, dataValues![j] / 10.0);
                        var sum = current + i * Math.Pow(10.0, averDatas[j] / 10.0);
                        averDatas[j] = (float)(10 * Math.Log10(sum / (i + 1)));
                    }

                //填充数据
                gs.Data = new float[averDatas!.Length];
                Array.Copy(averDatas, gs.Data, averDatas.Length); //仪表完成一次扫描，显示本次扫描
                if (i == RepeatTimes - 1) //如果是最后一次循环扫描结束，则通知客户端保存数据并停止测试
                    SendFastData(gs, true);
                else //不是最后一次循环扫描，仅在客户端显示数据
                    SendFastData(gs, false, false);
                break; //跳出while循环，进行下一次循环扫描
            }

            //仪表内部还未完成当前一次的循环扫描，仅在客户端显示数据，并且显示刚从设备采集的数据（未做平均）
            gs.Data = new float[dataValues!.Length];
            Array.Copy(dataValues, gs.Data, dataValues.Length);
            SendFastData(gs, false, false);
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
        List<object> datasToClient = new();
        if (data == null)
            datasToClient.Add(new FastGeneralScan
                { Freqs = null, Data = null, Start = StartFrequency, Stop = StopFrequency, TotalPoints = 0 });
        else
            datasToClient.Add(data);
        datasToClient.Add(needToSave);
        datasToClient.Add(needToStop);
        SendData(datasToClient);
    }

    /// <summary>
    ///     设置扫频参数
    /// </summary>
    private void SetSpectrumScanArgs()
    {
        //设置扫描点数
        var sweepPoints = ComputeSweepPointCount();
        Points = sweepPoints;
    }

    /// <summary>
    ///     设置校准参数
    /// </summary>
    private void SetCalibrationArgs()
    {
        //设置扫描点数
        var sweepPoints = ComputeSweepPointCount();
        Points = sweepPoints;
    }

    /// <summary>
    ///     处理频谱数据
    /// </summary>
    /// <param name="sData">功率值，dBm</param>
    private float[] ConvertStringDataToFloatArray(string sData)
    {
        var sDataPoints = sData.Split(',');
        var fDataPoints = new float[sDataPoints.Length];
        for (var i = 0; i < fDataPoints.Length; i++) fDataPoints[i] = Convert.ToSingle(sDataPoints[i]);
        return fDataPoints;
    }

    /// <summary>
    ///     处理频点数据
    /// </summary>
    /// <param name="sFreq"></param>
    /// <returns>频点值，MHz</returns>
    private double[] HandleFreqData(string sFreq)
    {
        var sDataPoints = sFreq.Split(',');
        var fDataPoints = new double[sDataPoints.Length];
        for (var i = 0; i < fDataPoints.Length; i++)
        {
            fDataPoints[i] = Convert.ToDouble(sDataPoints[i]) / 1000000;
            fDataPoints[i] = Math.Round(fDataPoints[i], 6);
        }

        fDataPoints[0] = StartFrequency;
        fDataPoints[^1] = StopFrequency;
        return fDataPoints;
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