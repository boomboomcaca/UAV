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

internal class TaskIqDataSave(TaskInfo taskInfo)
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
        var rawParam = taskInfo.RunningParameters.FirstOrDefault(item => item.Key == "iqRawSwitch").Value;
        if (rawParam != null && int.TryParse(rawParam.Value.ToString(), out var saveSign))
        {
            // 新存储周期标记
            var newCycle = false;
            if (_preSaveSwitchSign == 0 && saveSign == 1) newCycle = true;
            _preSaveSwitchSign = saveSign;
            if (_preSaveSwitchSign == 2) _preSaveSwitchSign = 0;
            switch (saveSign)
            {
                case 1: // 启动存储
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
    }

    /// <summary>
    ///     停止数据保存
    /// </summary>
    public void CheckStopSaveData()
    {
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
            if (obj is not SDataIq) continue;
            list.Add(obj);
        }

        if (list.Count == 0) return;
        try
        {
            RawIqDataStorage.Instance.Write(taskInfo.Id, list);
            if (DateTime.Now.Subtract(_preGetLenTime).TotalSeconds > 5)
            {
                _preGetLenTime = DateTime.Now;
                var len = RawIqDataStorage.Instance.GetFileLength(taskInfo.Id);
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
        if (taskInfo.IsIqDataSaveNeeded)
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
            RawIqDataStorage.Instance.ChangeParameters(taskInfo.Id, parameters, newCycle);
        }
        else
        {
            Running = true;
            RawIqDataStorage.Instance.Create(summary, feature, taskInfo.CreateTime, parameters);
        }

        taskInfo.IsIqDataSaveNeeded = true;
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
                RawIqDataStorage.Instance.StopSaveFile(taskInfo.Id);
                break;
            case 1:
                RawIqDataStorage.Instance.Complete(taskInfo.Id);
                break;
            case 2:
                RawIqDataStorage.Instance.ClearFile(taskInfo.Id, out var deleteSign);
                if (deleteSign) Running = false;
                break;
        }

        taskInfo.IsIqDataSaveNeeded = false;
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