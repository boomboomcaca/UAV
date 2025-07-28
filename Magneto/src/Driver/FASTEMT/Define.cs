using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Protocol.Define;
using Newtonsoft.Json;

namespace Magneto.Driver.FASTEMT;

/// <summary>
///     本地射电天文电测任务
/// </summary>
public class LocalFastEmtTaskInfo
{
    /// <summary>
    ///     云端任务Id
    /// </summary>
    public string CloudTaskId { get; set; }

    /// <summary>
    ///     云端子任务Id
    /// </summary>
    public string CloudSubTaskId { get; set; }

    /// <summary>
    ///     强干扰-0，弱干扰-1
    /// </summary>
    public int Model { get; set; }

    /// <summary>
    ///     任务状态
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    ///     开始执行时间，弱干扰使用
    /// </summary>
    public TimeSpan? BeginTime { get; set; }

    /// <summary>
    ///     结束执行时间，弱干扰使用
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    ///     角度
    /// </summary>
    public float Angle { get; set; }

    /// <summary>
    ///     极化方式
    /// </summary>
    public Polarization Polarization { get; set; }

    /// <summary>
    ///     开始频率
    /// </summary>
    public double StartFrequency { get; set; }

    /// <summary>
    ///     结束频率
    /// </summary>
    public double StopFrequency { get; set; }

    /// <summary>
    ///     参考电平
    /// </summary>
    public float ReferenceLevel { get; set; }

    /// <summary>
    ///     衰减
    /// </summary>
    public float Attenuation { get; set; }

    /// <summary>
    ///     分辨率带宽
    /// </summary>
    public double ResolutionBandwidth { get; set; }

    /// <summary>
    ///     视频带宽
    /// </summary>
    public double VideoBandwidth { get; set; }

    /// <summary>
    ///     前置预放
    /// </summary>
    public bool PreAmpSwitch { get; set; }

    /// <summary>
    ///     积分时间
    /// </summary>
    public int IntegrationTime { get; set; }

    /// <summary>
    ///     重复次数
    /// </summary>
    public int RepeatTimes { get; set; }

    /// <summary>
    ///     扫描时间
    /// </summary>
    public int ScanTime { get; set; }
}

/// <summary>
///     射电天文电测任务信息
/// </summary>
public class FastEmtTaskInfo
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("edgeId")] public string EdgeId { get; set; }

    [JsonProperty("equipment")] public string Equipment { get; set; }

    /// <summary>
    ///     状态：未开始-0，进行中-1，已停止-2，已完成-3，意外中断-4
    /// </summary>
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("members")] public List<MembersItem> Members { get; set; }
}

/// <summary>
///     测试频段
/// </summary>
public class SegmentsItem
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("startFrequency")] public double StartFrequency { get; set; }

    [JsonProperty("stopFrequency")] public double StopFrequency { get; set; }

    [JsonProperty("referenceLevel")] public float ReferenceLevel { get; set; }

    [JsonProperty("attenuation")] public float Attenuation { get; set; }

    [JsonProperty("resolutionBandwidth")] public int ResolutionBandwidth { get; set; }

    [JsonProperty("videoBandwidth")] public int VideoBandwidth { get; set; }

    /// <summary>
    ///     前置预放：无-0，有-1
    /// </summary>
    [JsonProperty("preAmpSwitch")]
    public int PreAmpSwitch { get; set; }

    [JsonProperty("integrationTime")] public int IntegrationTime { get; set; }

    [JsonProperty("repeatTimes")] public int RepeatTimes { get; set; }

    [JsonProperty("scanTime")] public int ScanTime { get; set; }
}

/// <summary>
///     测试子项
/// </summary>
public class ItemsItem
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("testSegmentId")] public string TestSegmentId { get; set; }

    [JsonProperty("startFrequency")] public double StartFrequency { get; set; }

    [JsonProperty("stopFrequency")] public double StopFrequency { get; set; }

    [JsonProperty("angle")] public float? Angle { get; set; }

    /// <summary>
    ///     极化方式：垂直-0，水平-1
    /// </summary>
    [JsonProperty("polarization")]
    public int? Polarization { get; set; }

    /// <summary>
    ///     状态：未开始-0，进行中-1，已停止-2，已完成-3，意外中断-4
    /// </summary>
    [JsonProperty("status")]
    public int? Status { get; set; }
}

/// <summary>
///     测试项成员
/// </summary>
public class MembersItem
{
    [JsonProperty("id")] public string Id { get; set; }

    /// <summary>
    ///     强干扰-0，弱干扰-1
    /// </summary>
    [JsonProperty("model")]
    public int Model { get; set; }

    /// <summary>
    ///     状态：未开始-0，进行中-1，已停止-2，已完成-3
    /// </summary>
    [JsonProperty("status")]
    public int Status { get; set; }

    /// <summary>
    ///     开始时间
    /// </summary>
    [JsonProperty("beginTime")]
    public TimeSpan? BeginTime { get; set; }

    /// <summary>
    ///     结束时间
    /// </summary>
    [JsonProperty("endTime")]
    public TimeSpan? EndTime { get; set; }

    [JsonProperty("segments")] public List<SegmentsItem> Segments { get; set; }

    [JsonProperty("items")] public List<ItemsItem> Items { get; set; }
}

public class FastEmtTaskInfoResult
{
    [JsonProperty("result")] public FastEmtTaskInfo Result { get; set; }
}

/// <summary>
///     对 FAST 测量结果的封装
/// </summary>
public class FastData
{
    /// <summary>
    ///     频率（MHz，修约小数点后三位）
    /// </summary>
    public double Frequency { get; set; }

    /// <summary>
    ///     仪表读书（dBm，修约小数点后两位）
    /// </summary>
    public float OriginalData { get; set; }

    /// <summary>
    ///     测量值/计算之后（dBm，修约小数点后两位）
    /// </summary>
    public float SpectrumData { get; set; }

    /// <summary>
    ///     天线增益值（dBi，修约小数点后两位）
    /// </summary>
    public float AntennaGain { get; set; }

    /// <summary>
    ///     天线系数（dB/m，修约小数点后两位）
    /// </summary>
    public float AntennaFactor { get; set; }

    /// <summary>
    ///     系统增益值（dB，修约小数点后两位）
    /// </summary>
    public float CalibrationData { get; set; }

    /// <summary>
    ///     功率谱密度1(单位dBW/m²Hz²，修约小数点后两位)
    /// </summary>
    public float Psd1 { get; set; }

    /// <summary>
    ///     功率谱密度2(单位dBJy，修约小数点后两位)
    /// </summary>
    public float Psd2 { get; set; }

    /// <summary>
    ///     噪声温度，（单位K，修约小数点后两位）
    /// </summary>
    public float NoiseTemperature { get; set; }
}

/// <summary>
///     任务管理方案
/// </summary>
public class FastTaskManager
{
    private bool _isRunning;

    //停止测试的控制信号量
    private CancellationTokenSource _stopRequestCts;

    /// <summary>
    ///     任务计数
    /// </summary>
    private int _taskCount;

    /// <summary>
    ///     标识当前任务是否已经完成
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    ///     本次任务要测试的测试项
    /// </summary>
    public LocalFastEmtTaskInfo[] Items { get; set; }

    /// <summary>
    ///     框架提供的测试器
    /// </summary>
    public Fastemt Runner { get; set; }

    /// <summary>
    ///     由派生类调用，当业务逻辑执行完毕（整个工作完成，非暂停），必须调用这个方法，通知调度器可以安排下一个任务
    /// </summary>
    protected void OnCompleted()
    {
        IsCompleted = true;
        Completed?.Invoke(this, new EventArgs());
    }

    /// <summary>
    ///     任务已完成(通知调度器，该任务已经完结，可以安排下一个任务)
    /// </summary>
    public event EventHandler Completed;

    /// <summary>
    ///     任务项结束事件
    /// </summary>
    public event EventHandler<ItemStatusChangedEventArgs> ItemStopped;

    /// <summary>
    ///     任务项开始事件
    /// </summary>
    public event EventHandler<ItemStatusChangedEventArgs> ItemStarted;

    /// <summary>
    ///     测试项同步测试
    /// </summary>
    /// <param name="item"></param>
    private void ExecuteItemSync(LocalFastEmtTaskInfo item)
    {
        //设置测试项参数，并启动
        Runner.StartTask(item);
        if (item.Status == Status.Suspended) return;
        item.Status = Status.Running;
        //单项测试开始触发事件
        ItemStarted?.Invoke(this,
            new ItemStatusChangedEventArgs { Item = item, Total = Items.Length, Left = Items.Length - _taskCount });
        while (!Runner.IsCompleted) Thread.Sleep(100);
        if (_stopRequestCts.IsCancellationRequested) item.Status = Status.Suspended;
        ItemStopped?.Invoke(this,
            new ItemStatusChangedEventArgs { Item = item, Total = Items.Length, Left = Items.Length - _taskCount });
    }

    #region 方法定义

    /// <summary>
    ///     开始线程
    /// </summary>
    public void StartExecute()
    {
        if (Items is not { Length: > 0 })
        {
            OnCompleted();
            return;
        }

        _isRunning = true;
        _stopRequestCts = new CancellationTokenSource();
        foreach (var item in Items)
        {
            if (_stopRequestCts.IsCancellationRequested)
                return;
            if (item.Status != Status.Complete)
                try
                {
                    _taskCount++;
                    ExecuteItemSync(item);
                }
                catch
                {
                }
        }

        OnCompleted();
        _isRunning = false;
    }

    /// <summary>
    ///     取消线程
    /// </summary>
    public void StopExecute()
    {
        if (!_isRunning)
            return;
        _stopRequestCts.Cancel();
        try
        {
            Runner.StopTask();
        }
        catch
        {
        }
    }

    #endregion
}

/// <summary>
///     任务状态
/// </summary>
public enum Status
{
    /// <summary>
    ///     未开始
    /// </summary>
    None = 1,

    /// <summary>
    ///     进行中
    /// </summary>
    Running = 2,

    /// <summary>
    ///     挂起
    /// </summary>
    Suspended = 4,

    /// <summary>
    ///     已完成
    /// </summary>
    Complete = 8
}

/// <summary>
///     任务内部测试子项状态改变通知事件
/// </summary>
public class ItemStatusChangedEventArgs : EventArgs
{
    /// <summary>
    ///     状态改变的时间戳
    /// </summary>
    public DateTime TimeStamp { get; } = DateTime.Now;

    /// <summary>
    ///     状态改变的项
    /// </summary>
    public LocalFastEmtTaskInfo Item { get; set; }

    /// <summary>
    ///     任务总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    ///     剩余任务数
    /// </summary>
    public int Left { get; set; }
}