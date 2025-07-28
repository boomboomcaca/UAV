using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SPEVL;

public partial class Spevl : ScanBase
{
    /// <summary>
    ///     文件保存状态数据上报间隔默认3秒
    /// </summary>
    private const int ProcessInterval = 3;

    /// <summary>
    ///     天线挂高
    /// </summary>
    private readonly float _antAlti = 1.5f;

    private readonly ConcurrentQueue<float[]> _dataCache = new();
    private readonly int _deviceScanSpeed = 20;
    private readonly Dictionary<int, DateTime> _firstTimeDic = new();
    private readonly object _lockFirstTimeDic = new();

    private readonly Dictionary<string, DateTime> _preProcessTime = new();
    private readonly string _spevlFileSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2", "spevl");

    private CancellationTokenSource _cts;

    /// <summary>
    ///     手动-自动门限切换
    /// </summary>
    private bool _preAutoThresholdSign;

    private double[] _preThresholdValue = Array.Empty<double>();
    private Task _processTask;
    private Dictionary<string, Parameter> _runningParameters = new();
    private ScanFileWriter _scanWriter;
    private Guid _taskId = Guid.Empty;

    public Spevl(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        _runningParameters = module.Parameters.ToDictionary(item => item.Name, item => item);
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        Console.WriteLine("开始频谱评估采集");
        _taskId = dataPort.TaskId;
        StartMultiSegments();
        if (SaveStatData)
        {
            Console.WriteLine("开始保存频谱评估数据");
            StartProcess();
        }

        return true;
    }

    public override bool Stop()
    {
        SaveStatData = false;
        StopProcess();
        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency or ParameterNames.StepFrequency)
            // 过滤从前端直接设置的起始结束频率等参数
            // 如果不过滤，这三个参数会与频段参数冲突
            return;
        SetParameterInternal(name, value);
        if (name == ParameterNames.AutoThreshold)
        {
            if (_preAutoThresholdSign != AutoThreshold)
            {
                ResetOccupancy();
                _preAutoThresholdSign = AutoThreshold;
            }
        }
        else if (name == ParameterNames.ThresholdSwitch)
        {
            if (ThresholdSwitch) ResetOccupancy();
        }
        else if (name is ParameterNames.ScanSegments or ParameterNames.ScanMode)
        {
            lock (_lockFirstTimeDic)
            {
                _firstTimeDic.Clear();
            }

            ResetOccupancy();
        }
        else if (name == ParameterNames.ThresholdValue && !AutoThreshold)
        {
            lock (LockSegmentList)
            {
                ThresholdValue ??= Array.Empty<double>();
                if (_preThresholdValue.Length != ThresholdValue.Length ||
                    !_preThresholdValue.All(ThresholdValue.Contains))
                {
                    ResetOccupancy();
                    _preThresholdValue = ThresholdValue;
                }
            }
        }
        else if (name == "tolerance" && AutoThreshold)
        {
            ResetOccupancy();
        }
        else if (name == ParameterNames.UnitSelection && !AutoThreshold)
        {
            ResetOccupancy();
        }

        if (_runningParameters.TryGetValue(name, out var parameter)) parameter.Value = value;
    }

    public override void OnData(List<object> data)
    {
        SendDataWithSpan(data);
        lock (LockSegmentList)
        {
            if (SegmentList?.All(i => i.IsOver) == true && IsTaskRunning)
            {
                var len = SegmentList.Sum(x => x.Total);
                var allData = new float[len];
                var index = 0;
                foreach (var t in SegmentList)
                {
                    var realData = Array.ConvertAll(t.Data, p => p / 10f);
                    Buffer.BlockCopy(realData, 0, allData, index, t.Total);
                    index += realData.Length;
                }

                ProcessStaticsData(allData);
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

    private void StartProcess()
    {
        _dataCache.Clear();
        _preProcessTime.Clear();
        StartSaveData();
        _cts = new CancellationTokenSource();
        _processTask = new Task(p => ProcessAsync(p).ConfigureAwait(false), _cts.Token);
        _processTask.Start();
    }

    private void StopProcess()
    {
        StopSaveData();
        _preProcessTime.Clear();
        _dataCache.Clear();
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        catch
        {
        }
    }

    private void ResetOccupancy()
    {
        if (ThresholdSwitch && SegmentList.Count > 0)
        {
            var convertAll = SegmentList.ConvertAll(item =>
                new Tuple<double, double, double>(item.StartFrequency, item.StopFrequency, item.StepFrequency));
            for (var i = 0; i < SegmentList.Count; i++)
            {
                var thd = new double[SegmentList[i].Total];
                if (!AutoThreshold)
                {
                    double thv = 0;
                    if (ThresholdValue.Length > 0)
                        thv = ThresholdValue.Length > i ? ThresholdValue[i] : ThresholdValue[^1];

                    // Buffer.BlockCopy(_thresholdValue, offset * sizeof(double), thd, 0, thd.Length * sizeof(double));
                    var array = new short[SegmentList[i].Total];
                    if (UnitSelection == 1)
                    {
                        // 添加天线因子
                        var factor = Factors.Find(item =>
                            Utils.IsNumberEquals(item.StartFrequency, SegmentList[i].StartFrequency)
                            && Utils.IsNumberEquals(item.StopFrequency, SegmentList[i].StopFrequency)
                            && Utils.IsNumberEquals(item.StepFrequency, SegmentList[i].StepFrequency)
                            && item.SegmentOffset == SegmentList[i].Index
                            && item.Total == SegmentList[i].Total
                            && item.Data.Length >= SegmentList[i].Total);
                        Array.Copy(factor.Data, 0, array, 0, array.Length);
                    }

                    for (var j = 0; j < SegmentList[i].Total; j++)
                    {
                        // 对门限进行单位换算
                        switch (UnitSelection)
                        {
                            case 1:
                                thv -= array[j] / 10f;
                                break;
                            case 2:
                                thv += 107f;
                                break;
                        }

                        thd[j] = thv;
                    }

                    SegmentList[i].SetThreshold(thd.Select(item => (float)item).ToArray(), AutoThreshold, false);
                }
                else
                {
                    SegmentList[i].SetThreshold(null, AutoThreshold, false);
                }
            }
        }
    }

    private void ProcessStaticsData(float[] data)
    {
        if (!SaveStatData) return;
        _dataCache.Enqueue(data);
    }

    private async Task ProcessAsync(object obj)
    {
        if (!SaveStatData) return;
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            if (!_dataCache.TryDequeue(out var data)) continue;
            //生成评估数据包，并缓存  测试
            var evaData = new EvalDataPacket(Utils.GetNowTime(), data.ToArray(), RunningInfo.BufGpsData.Longitude,
                RunningInfo.BufGpsData.Latitude, _antAlti * 10, _deviceScanSpeed);
            //缓存一帧评估所需的数据
            //if (!_scanWriter.AddData(evaData)) continue;
        }
    }

    /// <summary>
    ///     启动原始数据保存
    /// </summary>
    private void StartSaveData()
    {
        var centerCode = "00000000";
        var stationCode = "0001";
        //如果监测站编号大于12位
        if (RunningInfo.StationId.Length >= 12)
        {
            centerCode = RunningInfo.StationId.Substring(0, 8);
            stationCode = RunningInfo.StationId.Substring(RunningInfo.StationId.Length - 4);
        }

        //判断监测站是否为移动站
        var isMoveStation = RunningInfo.StationType != StationType.Stationary;
        var parameters = _runningParameters.Values.Select(item => item.Clone()).ToList();
        var antPara = parameters.Find(item => item.Name == ParameterNames.ScanSegments);
        var i = 0;
        List<EvalueSegmentInfo> scanSegments = new();
        //将当前扫描的频段信息加入到全局的扫描频段中
        foreach (var segment in SegmentList)
        {
            //获取极化方式
            var polarization = "H";
            if (antPara?.Value is IList<Dictionary<string, object>> antdata)
                if (antdata[i].TryGetValue(ParameterNames.Polarization, out var data))
                    if (Utils.ConvertStringToEnum<Polarization>(data.ToString()) == Polarization.Vertical)
                        polarization = "V";
            var point = Utils.GetTotalCount(segment.StartFrequency, segment.StopFrequency, segment.StepFrequency);
            //生成公用频段信息
            EvalueSegmentInfo segmentItem = new(i, segment.StartFrequency, segment.StopFrequency,
                (float)segment.StepFrequency, polarization, point);
            scanSegments.Add(segmentItem);
            i++;
        }

        //数据文件保存路径
        var taskFilePath = Path.Combine(_spevlFileSavePath, Utils.GetNowTime().ToString("yyyyMMdd"));
        if (!Directory.Exists(taskFilePath)) Directory.CreateDirectory(taskFilePath);
        //实例化扫描数据写入
        _scanWriter = new ScanFileWriter(taskFilePath, scanSegments, centerCode, stationCode, isMoveStation);
        _scanWriter.FileSaved += _scanWriter_FileSaved;
        _scanWriter.FileModified += _scanWriter_FileModified;
        _scanWriter.Start();
    }

    private void _scanWriter_FileModified(object sender, FileSavedNotification e)
    {
        e.TaskId = _taskId.ToString();
        e.DriverId = Module.Id.ToString();
        e.RelativePath = Path.Combine("spevl", Utils.GetTimeByTicks(e.BeginRecordTime).ToString("yyyyMMdd"))
            .Replace('\\', '/');
        e.ComputerId = RunningInfo.ComputerId;
        e.DataType = FileDataType.Spevl;
        e.FileName = Path.GetFileName(e.FileName);
        SendMessageData(new List<object> { e });
    }

    private void _scanWriter_FileSaved(object sender, SDataSpevlFileSaveInfo e)
    {
        e.FileName = Path.GetFileName(e.FileName);
        if (e.Progress >= 100)
        {
            if (e.FileName != null && _preProcessTime.ContainsKey(e.FileName)) _preProcessTime.Remove(e.FileName);
            e.Progress = 100;
            SendMessageData(new List<object> { e });
        }
        else
        {
            if (!_preProcessTime.TryGetValue(e.FileName!, out var sendTime))
                //第一次没有值
                _preProcessTime.Add(e.FileName, DateTime.Now);
            if (DateTime.Now.Subtract(sendTime).TotalSeconds > ProcessInterval)
            {
                _preProcessTime[e.FileName] = DateTime.Now;
                SendMessageData(new List<object> { e });
            }
        }
    }

    /// <summary>
    ///     停止保存
    /// </summary>
    private void StopSaveData()
    {
        _scanWriter?.Stop();
    }
}