using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Configuration;
using Magneto.Contract;
using Magneto.Contract.Storage;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Tasks;

internal class TaskRawDataSave(TaskInfo taskInfo)
{
    private DateTime _preGetLenTime = DateTime.Now;

    /// <summary>
    ///     存储上一次的原始数据保存开关的值
    /// </summary>
    private int _preSaveSwitchSign;

    /// <summary>
    ///     本任务是否已经开启了数据存储
    /// </summary>
    public bool Running { get; private set; }

    /// <summary>
    ///     启动数据保存
    /// </summary>
    /// <param name="isStartTask"></param>
    public void CheckStartSaveData(bool isStartTask = true)
    {
        var rawParam = taskInfo.RunningParameters.FirstOrDefault(item => item.Key == ParameterNames.RawSwitch).Value;
        if (taskInfo.Feature == FeatureType.Nsic && rawParam != null)
        {
            // 新信号截获的比对模式默认打开原始数据保存,采集模板默认关闭原始数据保存
            var p = taskInfo.RunningParameters.FirstOrDefault(item => item.Key == "functionSwitch").Value;
            if (p != null && bool.TryParse(p.Value.ToString(), out var func)) rawParam.Value = func ? 1 : 0;
        }

        if (rawParam == null || !int.TryParse(rawParam.Value.ToString(), out var saveSign)) return;
        // 新存储周期标记
        var newCycle = _preSaveSwitchSign == 0 && saveSign is 1 or 3 or 4 or 5;
        _preSaveSwitchSign = saveSign;
        if (_preSaveSwitchSign == 2) _preSaveSwitchSign = 0;
        switch (saveSign)
        {
            case 1: // 启动存储
            case 3:
            case 4:
            case 5:
                StartSaveData(isStartTask, newCycle);
                break;
            case 0: // 停止存储
                StopSaveData(0);
                break;
            case 2: // 清除数据
                StopSaveData(2);
                break;
        }
    }

    /// <summary>
    ///     停止数据保存
    /// </summary>
    public void CheckStopSaveData()
    {
        var rawParam = taskInfo.RunningParameters.FirstOrDefault(item => item.Key == ParameterNames.RawSwitch).Value;
        if (taskInfo.Feature == FeatureType.Nsic && rawParam != null)
        {
            // 新信号截获的比对模式默认打开原始数据保存,采集模板默认关闭原始数据保存
            var p = taskInfo.RunningParameters.FirstOrDefault(item => item.Key == "functionSwitch").Value;
            if (p != null && bool.TryParse(p.Value.ToString(), out var func)) rawParam.Value = func ? 1 : 0;
        }

        StopSaveData();
    }

    /// <summary>
    ///     数据存储
    /// </summary>
    /// <param name="data"></param>
    public void SaveData(ref List<object> data)
    {
        // 保存数据
        // 去除统计数据
        var list = new List<object>();
        foreach (var obj in data)
        {
            if (obj is not SDataRaw raw) continue;
            if (obj is SDataIq) continue;
            if (taskInfo.MediaType != MediaType.None)
            {
                var dataList = ConvertDataType(taskInfo.MediaType);
                if (dataList.Exists(item => raw.Type == item)) list.Add(obj);
            }
            else if (raw.Type > SDataType.HeartBeat)
            {
                list.Add(obj);
            }

            if (raw.Type is SDataType.Dfind or SDataType.DfPan or SDataType.DfScan)
            {
                list.Add(RunningInfo.BufGpsData);
                list.Add(RunningInfo.BufCompassData);
            }

            if (taskInfo.Feature != FeatureType.Fdf && raw.Type == SDataType.Level) list.Add(RunningInfo.BufGpsData);
        }

        if (list.Count == 0) return;
        try
        {
            RawDataStorage.Instance.Write(taskInfo.Id, list);
            if (DateTime.Now.Subtract(_preGetLenTime).TotalSeconds > 5)
            {
                _preGetLenTime = DateTime.Now;
                var len = RawDataStorage.Instance.GetFileLength(taskInfo.Id);
                if (len > 0)
                {
                    SDataRawDataLen dl = new()
                    {
                        Data = len,
                        Unit = "B"
                    };
                    if (len < 1024 * 1024)
                    {
                        dl.Data = len / 1024d;
                        dl.Unit = "KB";
                    }
                    else if (len < 1024 * 1024 * 1024)
                    {
                        dl.Data = len / (1024d * 1024d);
                        dl.Unit = "MB";
                    }
                    else
                    {
                        dl.Data = len / (1024d * 1024d * 1024d);
                        dl.Unit = "GB";
                    }

                    var (freeHdd, unit, canSave) = RunningInfo.GetFreeHdd();
                    dl.DiskSpace = freeHdd;
                    dl.DiskUnit = unit;
                    dl.CanSave = canSave;
                    data.Add(dl);
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
            // 略
        }
    }

    /// <summary>
    ///     启动原始数据保存
    /// </summary>
    /// <param name="isStartTask">任务启动标记</param>
    /// <param name="newCycle">新存储周期标记</param>
    private void StartSaveData(bool isStartTask = true, bool newCycle = false)
    {
        if (taskInfo.IsDataSaveNeeded)
        {
            Console.WriteLine("停止保存并重新保存");
            StopSaveData(0);
            // return;
        }

        Console.WriteLine("启动数据保存");
        var summary = new FileDataSummary
        {
            EdgeId = StationConfig.Instance.Station.EdgeId,
            TaskId = taskInfo.Id.ToString(),
            PluginId = taskInfo.PluginId
        };
        var parameter =
            taskInfo.ModuleChain.ModuleInfo.Parameters.Find(item => item.IsPrimaryDevice && item.IsInstallation);
        if (parameter?.Value != null && Guid.TryParse(parameter.Value.ToString(), out var id))
        {
            var module = taskInfo.ModuleChain.Devices.Find(item => item.ModuleInfo.Id == id);
            if (module != null) summary.DeviceId = module.ModuleInfo.Id.ToString();
        }

        var feature = taskInfo.ModuleChain.ModuleInfo.Feature;
        summary.DriverId = taskInfo.ModuleChain.ModuleInfo.Id.ToString();
        var parameters = taskInfo.RunningParameters.Values.Select(item => item.Clone()).ToList();
        if (!isStartTask && Running)
        {
            RawDataStorage.Instance.ChangeParameters(taskInfo.Id, parameters, newCycle);
        }
        else
        {
            Running = true;
            RawDataStorage.Instance.Create(summary, feature, taskInfo.CreateTime, parameters);
        }

        taskInfo.IsDataSaveNeeded = true;
    }

    /// <summary>
    ///     停止原始数据保存
    /// </summary>
    /// <param name="stopTaskSign"></param>
    private void StopSaveData(int stopTaskSign = 1)
    {
        // Console.WriteLine("停止数据保存");
        switch (stopTaskSign)
        {
            case 0:
                RawDataStorage.Instance.StopSaveFile(taskInfo.Id);
                break;
            case 1:
                RawDataStorage.Instance.Complete(taskInfo.Id);
                break;
            case 2:
                RawDataStorage.Instance.ClearFile(taskInfo.Id, out var deleteSign);
                if (deleteSign) Running = false;
                break;
        }

        taskInfo.IsDataSaveNeeded = false;
    }

    /// <summary>
    ///     根据MediaType设置当前需要保存的数据
    /// </summary>
    /// <param name="media"></param>
    private static List<SDataType> ConvertDataType(MediaType media)
    {
        var arr = Enum.GetValues(typeof(MediaType));
        var list = new List<SDataType>();
        foreach (MediaType item in arr)
        {
            if ((media & item) == 0) continue;
            switch (item)
            {
                case MediaType.Level:
                    list.Add(SDataType.Level);
                    break;
                case MediaType.Spectrum:
                    list.Add(SDataType.Spectrum);
                    list.Add(SDataType.Scan);
                    break;
                case MediaType.Audio:
                    list.Add(SDataType.Audio);
                    break;
                case MediaType.Itu:
                    list.Add(SDataType.Itu);
                    break;
                case MediaType.Iq:
                case MediaType.Tdoa:
                    list.Add(SDataType.Iq);
                    break;
                case MediaType.Dfind:
                    list.Add(SDataType.Dfind);
                    break;
                case MediaType.Scan:
                    list.Add(SDataType.Scan);
                    break;
                default:
                    continue;
            }
        }

        return list;
    }
}