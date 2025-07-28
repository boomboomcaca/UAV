using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Newtonsoft.Json;

namespace Magneto.Driver.FASTEMT;

/// <summary>
///     射电天文电测
/// </summary>
public partial class Fastemt : DriverBase
{
    /// <summary>
    ///     本地任务缓存
    /// </summary>
    private readonly List<FastTaskManager> _localTaskQueue = new();

    /// <summary>
    ///     系统增益数据字典表
    /// </summary>
    private readonly Dictionary<double, float> _systemGainDic;

    /// <summary>
    ///     系统噪声数据字典表
    /// </summary>
    private readonly Dictionary<double, float> _systemTempratureDic;

    /// <summary>
    ///     天线增益字典表
    /// </summary>
    private Dictionary<double, float> _antennaGainDic;

    /// <summary>
    ///     控制箱内的天线损耗表
    /// </summary>
    private Dictionary<double, float> _antPathLossDic;

    //停止测试的控制信号量
    private CancellationTokenSource _cts;

    /// <summary>
    ///     当前执行任务
    /// </summary>
    private LocalFastEmtTaskInfo _currentTaskInfo;

    /// <summary>
    ///     频谱仪是否运行
    /// </summary>
    private bool _isRunning;

    /// <summary>
    ///     噪声源参照字典表
    /// </summary>
    private Dictionary<double, float> _noiseSourceEnrDic;

    /// <summary>
    ///     外部射频线缆损耗表
    /// </summary>
    private Dictionary<double, float> _rfCableLossDic;

    private Timer _timer;

    public Fastemt(Guid driverId) : base(driverId)
    {
        _systemGainDic = new Dictionary<double, float>();
        _systemTempratureDic = new Dictionary<double, float>();
    }

    /// <summary>
    ///     完成状态
    /// </summary>
    public bool IsCompleted => _currentTaskInfo.Status is Status.Complete or Status.Suspended;

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        // 获取噪声源 ENR 字典表
        _noiseSourceEnrDic = _icb.GetNoiseSourceEnr().OrderBy(a => a.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        _rfCableLossDic = _icb.GetRfCableLossTable().OrderBy(a => a.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        //设备复位
        _icb.Reset();
        _spectrumAnalyser.Reset();
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        return true;
    }

    public override bool Stop()
    {
        Trace.WriteLine("射电天文电测任务停止执行!");
        ClearLocalTask();
        (_spectrumAnalyser as DeviceBase)?.Stop();
        (_icb as DeviceBase)?.Stop();
        _cts.Cancel();
        _timer?.Dispose();
        _isRunning = false;
        return base.Stop();
    }

    /// <summary>
    ///     清理本地执行任务
    /// </summary>
    private void ClearLocalTask()
    {
        foreach (var item in _localTaskQueue)
            if (!item.IsCompleted)
                item.StopExecute();
        _localTaskQueue.Clear();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == "taskId")
        {
            ClearLocalTask();
            var taskId = $"{value}";
            if (string.IsNullOrEmpty(taskId)) return;
            _ = Task.Run(async () => { await GetFastEmtTasksAsync(taskId); }).ConfigureAwait(false);
        }
    }

    public override void OnData(List<object> data)
    {
        // 首先在接收的数据中找到匹配类型的数据
        var fastemtdata = (FastGeneralScan)data.Find(a => a is FastGeneralScan);
        if (fastemtdata == null) return;
        // 使用线程安全的集合类型
        var dataArray = new ConcurrentBag<FastData>();
        var rbwValue = _currentTaskInfo.ResolutionBandwidth;

        #region parallel handling

        // 并行计算每个频点相关的数据
        Parallel.For(0, fastemtdata.TotalPoints, () => new List<FastData>(),
            (index, _, partialList) =>
            {
                // 频率值
                var freq = fastemtdata.Freqs[index];
                // 天线增益值
                var antennaGain = CalculateInterpolationGain(freq, _antennaGainDic);
                // 天线增益因子，计算公式：F = 20 * lg⁡(f) - G - 29.79
                // F：天线系数，单位 dB/m；
                // f：频率，单位 MHz；
                // G：天线增益，单位 dBi
                var f = 20 * Math.Log10(freq) - antennaGain - 29.79;
                // 系统增益（校准值）
                var systemGain = CalculateInterpolationGain(freq, _systemGainDic);
                // 噪声温度
                var noiseTemperature = CalculateInterpolationGain(freq, _systemTempratureDic);
                // 频谱仪读数，单位 dBm
                var originalData = fastemtdata.Data[index];
                // Pm = Psa - Gsys - Ga
                // Pm：天线端口处测量功率值，单位 dBm；
                // Psa：频谱仪读书，单位 dBm；
                // Gsys：系统增益，单位 dB；
                // Ga：天线增益，单位 dBi
                var pm = originalData - systemGain - antennaGain;
                var fastData = new FastData
                {
                    Frequency = freq,
                    OriginalData = originalData,
                    SpectrumData = pm,
                    AntennaGain = antennaGain,
                    AntennaFactor = (float)f,
                    CalibrationData = systemGain,
                    NoiseTemperature = noiseTemperature,
                    // 功率谱密度，两种单位
                    Psd1 = (float)(pm - 10 * Math.Log10(rbwValue) - systemGain + f - 35.77) //单位：dBW/m2Hz
                };
                fastData.Psd2 = fastData.Psd1 + 260.7f; //单位：dBJy
                //fastData.PSD2 = (float)(pm - 10 * Math.Log10(rbwValue) - systemGain + F + 224.93);
                partialList.Add(fastData);
                return partialList;
            }, partialList => { partialList.ForEach(item => dataArray.Add(item)); });

        #endregion

        var data2 = dataArray.OrderBy(a => a.Frequency).ToList();
        if (!bool.TryParse($"{data.ElementAtOrDefault(2)}", out var isComplete)) return;
        //发送测试结果
        var testResult = new SDataFastTestData
        {
            Angle = _currentTaskInfo.Angle,
            Polarization = Utils.ConvertEnumToString(_currentTaskInfo.Polarization),
            StartFrequency = _currentTaskInfo.StartFrequency,
            StopFrequency = _currentTaskInfo.StopFrequency,
            TaskId = _currentTaskInfo.CloudSubTaskId,
            State = 1,
            Data = new FastTestResultInfo
            {
                AntennaGain = data2.Select(x => x.AntennaGain).ToArray(),
                AntennaMeasure = data2.Select(x => x.SpectrumData).ToArray(),
                Meter = data2.Select(x => x.OriginalData).ToArray(),
                NoiseTemperature = data2.Select(x => x.NoiseTemperature).ToArray(),
                Psd1 = data2.Select(x => x.Psd1).ToArray(),
                Psd2 = data2.Select(x => x.Psd2).ToArray(),
                SystemGain = data2.Select(x => x.CalibrationData).ToArray()
            }
        };
        SendData(new List<object> { testResult });
        if (isComplete)
        {
            SendMessageData(new List<object> { testResult });
            Console.WriteLine($"{Utils.GetNowTime()}  发送任务{_currentTaskInfo.CloudSubTaskId}结果");
            StopTask();
        }
    }

    /// <summary>
    ///     执行具体任务
    /// </summary>
    /// <param name="taskInfo"></param>
    /// <returns></returns>
    private bool ProcessTask(LocalFastEmtTaskInfo taskInfo)
    {
        //操作频谱仪
        _spectrumAnalyser.Reset();
        _spectrumAnalyser.StartFrequency = taskInfo.StartFrequency;
        _spectrumAnalyser.StopFrequency = taskInfo.StopFrequency;
        _spectrumAnalyser.ReferenceLevel = taskInfo.ReferenceLevel;
        _spectrumAnalyser.ResolutionBandwidth = taskInfo.ResolutionBandwidth;
        _spectrumAnalyser.VideoBandwidth = taskInfo.VideoBandwidth;
        _spectrumAnalyser.Attenuation = taskInfo.Attenuation;
        _spectrumAnalyser.IntegrationTime = taskInfo.IntegrationTime;
        _spectrumAnalyser.PreAmpSwitch = taskInfo.PreAmpSwitch;
        _spectrumAnalyser.ScanTime = taskInfo.ScanTime;
        _spectrumAnalyser.RepeatTimes = taskInfo.RepeatTimes;
        var path = GetPath(taskInfo.StartFrequency);
        if (path <= 0) return false;
        _icb.SwitchAntenna(path, taskInfo.Angle, taskInfo.Polarization);
        // 获取当前天线增益字典表
        _antennaGainDic = _icb.GetAntennaGainTable(path, taskInfo.Polarization).OrderBy(a => a.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        // 获取当前天线通道损耗字典表
        _antPathLossDic = _icb.GetAntPathLossTable(path, taskInfo.Polarization).OrderBy(a => a.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        try
        {
            SystemCalibration(path);
            var device = _spectrumAnalyser as DeviceBase;
            if (_isRunning) device?.Stop();
            if (!_cts.IsCancellationRequested)
            {
                device?.Start(FeatureType.FASTEMT, this);
                _isRunning = true;
            }
            else
            {
                _currentTaskInfo.Status = Status.Suspended;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     停止执行任务
    /// </summary>
    /// <returns></returns>
    public bool StopTask()
    {
        (_spectrumAnalyser as DeviceBase)?.Stop();
        _currentTaskInfo.Status = Status.Complete;
        _cts.Cancel();
        _timer?.Dispose();
        _isRunning = false;
        Console.WriteLine(
            $"{Utils.GetNowTime()}  {(_currentTaskInfo.Model == 0 ? "强" : "弱")}干扰任务{_currentTaskInfo.CloudSubTaskId}结束执行");
        return true;
    }

    public void StartTask(LocalFastEmtTaskInfo item)
    {
        _cts = new CancellationTokenSource();
        _currentTaskInfo = item;
        //发送测试状态
        SendMessageData(new List<object>
        {
            new SDataFastTestData
            {
                Angle = _currentTaskInfo.Angle,
                Polarization = Utils.ConvertEnumToString(_currentTaskInfo.Polarization),
                StartFrequency = _currentTaskInfo.StartFrequency,
                StopFrequency = _currentTaskInfo.StopFrequency,
                State = 0,
                TaskId = _currentTaskInfo.CloudSubTaskId
            }
        });
        //弱干扰判断执行时间
        if (_currentTaskInfo.Model == 1)
        {
            if (_currentTaskInfo.BeginTime != null)
            {
                var beginTime = Utils.GetNowTime().Date.Add(_currentTaskInfo.BeginTime.Value);
                if (_currentTaskInfo.EndTime != null)
                {
                    var endTime = Utils.GetNowTime().Date.Add(_currentTaskInfo.EndTime.Value);
                    if (endTime <= beginTime) endTime = endTime.AddDays(1);
                    var sendWaitMsg = false;
                    while (Utils.GetNowTime() < beginTime || Utils.GetNowTime() >= endTime)
                    {
                        if (_cts.IsCancellationRequested)
                        {
                            _currentTaskInfo.Status = Status.Suspended;
                            return;
                        }

                        if (!sendWaitMsg)
                        {
                            Console.WriteLine(
                                $"未在设定时间{_currentTaskInfo.BeginTime}-{_currentTaskInfo.EndTime}内，弱干扰任务{_currentTaskInfo.CloudSubTaskId}等待");
                            //发送测试等待状态
                            SendMessageData(new List<object>
                            {
                                new SDataFastTestData
                                {
                                    Angle = _currentTaskInfo.Angle,
                                    Polarization = Utils.ConvertEnumToString(_currentTaskInfo.Polarization),
                                    StartFrequency = _currentTaskInfo.StartFrequency,
                                    StopFrequency = _currentTaskInfo.StopFrequency,
                                    State = 4,
                                    TaskId = _currentTaskInfo.CloudSubTaskId
                                }
                            });
                            sendWaitMsg = true;
                        }

                        Thread.Sleep(1000);
                    }
                }
            }

            _timer = new Timer(Timer_Elapsed, this, 0, 1000);
        }

        //执行任务
        ProcessTask(_currentTaskInfo);
    }

    /// <summary>
    ///     获取云端参数
    /// </summary>
    /// <param name="taskId">任务详情</param>
    /// <returns></returns>
    private async Task GetFastEmtTasksAsync(string taskId)
    {
        Trace.WriteLine("云端发来射电天文电测任务任务更新通知，重新获取任务");
        var str = await CloudClient.Instance.GetFastEmtTasksAsync(taskId).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(str))
        {
            var fastEmtTask = JsonConvert.DeserializeObject<FastEmtTaskInfoResult>(str);
            if (fastEmtTask?.Result != null)
            {
                var taskInfo = fastEmtTask.Result;
                //判断主任务状态,除非已完成，都要执行
                if (taskInfo.Status == 3 || taskInfo.EdgeId != RunningInfo.EdgeId) return;
                List<LocalFastEmtTaskInfo> cloudTasks = new();
                foreach (var member in taskInfo.Members)
                {
                    if (member.Status == 3) continue;
                    if (member.Model == 1 && (member.BeginTime == null || member.EndTime == null))
                    {
                        member.BeginTime ??= new TimeSpan(0, 0, 0);
                        member.EndTime ??= new TimeSpan(0, 0, 0);
                    }

                    foreach (var item in member.Items)
                    {
                        //数据校验,移除无效的任务
                        if (item.Angle == null || item.Polarization == null || item.Status == null ||
                            item.Status == 3) continue;
                        var segmentInfo = member.Segments.First(x => x.Id == item.TestSegmentId);
                        var localTask = new LocalFastEmtTaskInfo
                        {
                            Angle = item.Angle.Value,
                            Attenuation = segmentInfo.Attenuation,
                            BeginTime = member.BeginTime,
                            EndTime = member.EndTime,
                            CloudSubTaskId = item.Id,
                            CloudTaskId = taskInfo.Id,
                            IntegrationTime = segmentInfo.IntegrationTime,
                            Model = member.Model,
                            Polarization = item.Polarization == 0 ? Polarization.Vertical : Polarization.Horizontal,
                            PreAmpSwitch = segmentInfo.PreAmpSwitch == 1,
                            ReferenceLevel = segmentInfo.ReferenceLevel,
                            RepeatTimes = segmentInfo.RepeatTimes,
                            ResolutionBandwidth = segmentInfo.ResolutionBandwidth,
                            ScanTime = segmentInfo.ScanTime,
                            StartFrequency = segmentInfo.StartFrequency,
                            StopFrequency = segmentInfo.StopFrequency,
                            VideoBandwidth = segmentInfo.VideoBandwidth,
                            Status = 0
                        };
                        cloudTasks.Add(localTask);
                    }
                }

                FastTaskManager fastTask = new()
                {
                    Runner = this,
                    Items = cloudTasks.ToArray()
                };
                fastTask.ItemStarted += FastTask_ItemStarted;
                fastTask.ItemStopped += FastTask_ItemStopped;
                fastTask.Completed += FastTask_Completed;
                _localTaskQueue.Add(fastTask);
                fastTask.StartExecute();
            }
        }
    }

    private void FastTask_Completed(object sender, EventArgs e)
    {
        Trace.WriteLine($"{Utils.GetNowTime()}  射电天文电测任务执行完毕！");
    }

    private void FastTask_ItemStopped(object sender, ItemStatusChangedEventArgs e)
    {
        Console.WriteLine($"{Utils.GetNowTime()}  结束执行{e.Item.CloudSubTaskId}");
    }

    private void FastTask_ItemStarted(object sender, ItemStatusChangedEventArgs e)
    {
        Console.WriteLine(
            $"{Utils.GetNowTime()}  开始执行{(e.Item.Model == 0 ? "强" : "弱")}干扰任务：{e.Item.CloudSubTaskId},共{e.Total}个，剩余{e.Left}个");
    }

    /// <summary>
    ///     获取路径
    /// </summary>
    /// <param name="frequency">单位MHz</param>
    /// <returns></returns>
    private static int GetPath(double frequency)
    {
        if (frequency is >= 50 and <= 150)
            return 1;
        if (frequency is > 150 and <= 1000)
            return 2;
        if (frequency is > 1000 and <= 12000)
            return 3;
        return 0;
    }

    /// <summary>
    ///     填充系统增益字典表方法
    /// </summary>
    /// <param name="path"></param>
    private void SystemCalibration(int path)
    {
        //打开当前通道上的噪声源
        _icb.SwitchNoiseSource(path, true);
        // 获取噪声源开启时的校准数据
        var calibrationDataWithNoiseSource = _spectrumAnalyser.GetCalibrationData();
        // 关闭当前通道上的噪声源
        _icb.SwitchNoiseSource(path, false);
        // 获取噪声源关闭时的校准数据
        var calibrationDataWithoutNoiseSource = _spectrumAnalyser.GetCalibrationData();
        // B 为带宽，由 Rbw 转化而来，单位为 Hz
        //double rbwValue = GetRbwValueFromString(_receiver.GetCurrentRealRbw());
        var rbwValue = _spectrumAnalyser.GetCurrentRealRbw() * 1000;
        _systemGainDic.Clear();
        _systemTempratureDic.Clear();
        for (var i = 0; i < calibrationDataWithNoiseSource.TotalPoints; i++)
        {
            var freq = calibrationDataWithNoiseSource.Freqs[i];
            // 线缆损耗值（dB）
            var cableLoss = CalculateInterpolationGain(freq, _antPathLossDic) +
                            CalculateInterpolationGain(freq, _rfCableLossDic);
            // 定义参数 γ，γ = 噪声源开启时的电平值 P(on) / 噪声源关闭时的电平值 P(off)，注意此处的电平值为线性值
            var pOn = 0.001 * Math.Pow(10, (calibrationDataWithNoiseSource.Data[i] + cableLoss) / 10f);
            var pOff = 0.001 * Math.Pow(10, (calibrationDataWithoutNoiseSource.Data[i] + cableLoss) / 10f);
            var gama = pOn / pOff;
            // ENR为噪声源的超噪比的真值，通过从字典表返回或插值得出,注意此处的ENR值为对数值
            var enr = CalculateInterpolationGain(freq, _noiseSourceEnrDic);
            // 系统噪声系数 NF（dB）= ENR（dB）-10log（γ-1）
            var nf = enr - 10 * Math.Log10(gama - 1);
            // 计算系统噪声温度：Tr=T0((ENR/(Gama-1))-1)，T0 = 290,注意此处的ENR值为线性值
            //var T_r = (float)(290 * ((ENR / (GAMA - 1)) - 1));
            var r = (float)(290 * (Math.Pow(10, enr / 10) / (gama - 1) - 1));
            // 计算系统增益 GR(dB)=P(on)(开启噪声源时的噪声电平)-NF-10log(ENR+1)-10logB+174
            //var GB = (float)(calibrationDataWithNoiseSource.data[i] - NF - (10 * Math.Log10(rbwValue)) + 174);
            var gb = (float)(calibrationDataWithNoiseSource.Data[i] - nf - 10 * Math.Log10(enr + 1) -
                10 * Math.Log10(rbwValue) + 174);
            _systemGainDic.Add(freq, gb);
            _systemTempratureDic.Add(freq, r);
        }

        //处理字典表中的因参数设置不合理而导致的NaN数据
        HandleNaNValuesInDictionary(_systemGainDic);
        HandleNaNValuesInDictionary(_systemTempratureDic);
    }

    /// <summary>
    ///     处理字典表中NaN
    /// </summary>
    /// <param name="dic"></param>
    private static void HandleNaNValuesInDictionary(Dictionary<double, float> dic)
    {
        var leftNoNaNIndex = -1;
        for (var i = 0; i < dic.Count; i++)
        {
            //右移一次右游标
            var rightNoNaNIndex = i;
            //右游标移到一个非NaN上
            if (!float.IsNaN(dic.ElementAt(i).Value))
            {
                //左右游标已经处于不同的位置，需要做插值处理
                if (leftNoNaNIndex < rightNoNaNIndex - 1)
                {
                    //计算一段连续的NaN应该插入的值
                    float fillValue;
                    if (leftNoNaNIndex <= -1) //左边无非NaN，右边有非NaN的一段连续NaN（左游标还未移动过）
                        fillValue = dic.ElementAt(rightNoNaNIndex).Value;
                    else //左右两边都有非NaN的一段的连续NaN(左游标已经移动过)
                        fillValue = (dic.ElementAt(leftNoNaNIndex).Value + dic.ElementAt(rightNoNaNIndex).Value) / 2f;
                    //填充一段连续的NaN
                    for (var k = leftNoNaNIndex + 1; k < rightNoNaNIndex; k++) dic[dic.ElementAt(k).Key] = fillValue;
                }

                //右移一次左游标
                leftNoNaNIndex = i;
            }
            //右游标移到一个NaN上
            else
            {
                //右游标已经达到右边缘
                if (rightNoNaNIndex >= dic.Count - 1)
                {
                    //左游标从未移动过（整个Dic都是NaN的情况）
                    if (leftNoNaNIndex <= -1) throw new Exception("字典表中所有值都为NaN,无法计算。");

                    var fillValue = dic.ElementAt(leftNoNaNIndex).Value;
                    for (var k = leftNoNaNIndex + 1; k < dic.Count; k++) dic[dic.ElementAt(k).Key] = fillValue;
                }
            }
        }
    }

    /// <summary>
    ///     计算插值增益的私有方法
    /// </summary>
    /// <param name="freq">指定频率值</param>
    /// <param name="dictionary">计算线性插值的字典参照表</param>
    /// <returns>增益值</returns>
    private static float CalculateInterpolationGain(double freq, Dictionary<double, float> dictionary)
    {
        // 如果列表为 null 或个数为 0，则抛出异常
        if (dictionary == null || dictionary.Count == 0)
            throw new ArgumentException("Argument invalid: dictionary does not exsit or no data.");
        // 列表个数为 1，则直接返回该频率的增益值
        if (dictionary.Count == 1) return dictionary.First().Value;
        // 如果增益列表包含目标频率，直接返回该频率值的值
        if (dictionary.TryGetValue(freq, out var gain)) return gain;
        // 如果天线增益列表数量大于2，而 目标频率 <= 第一项的频率值，则直接返回第一项
        if (freq <= dictionary.First().Key) return dictionary.First().Value;

        if (freq >= dictionary.Last().Key) return dictionary.Last().Value;
        // 排除前面的各种情况，即天线增益列表数量 >= 2，并且目标频率落在列表频率范围内
        // 遍历列表，找到目标频率的前后两点，求出其直线斜率，根据公式算出一个插值。
        var left = dictionary.LastOrDefault(pair => pair.Key < freq);
        var right = dictionary.FirstOrDefault(pair => pair.Key > freq);
        var k = (left.Value - right.Value) / (left.Key - right.Key);
        var b = left.Value - k * left.Key;
        return (float)(k * freq + b);
    }

    /// <summary>
    ///     超时检测
    /// </summary>
    /// <param name="sender"></param>
    private void Timer_Elapsed(object sender)
    {
        if (_currentTaskInfo is { Model: 1, BeginTime: not null })
        {
            var beginTime = Utils.GetNowTime().Date.Add(_currentTaskInfo.BeginTime.Value);
            if (_currentTaskInfo.EndTime != null)
            {
                var endTime = Utils.GetNowTime().Date.Add(_currentTaskInfo.EndTime.Value);
                if (endTime <= beginTime) endTime = endTime.AddDays(1);
                if (Utils.GetNowTime() > endTime)
                {
                    Console.WriteLine($"{Utils.GetNowTime()} 弱干扰{_currentTaskInfo.CloudSubTaskId}执行超时");
                    StopTask();
                }
            }
        }
    }
}