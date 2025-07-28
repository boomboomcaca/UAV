using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Core.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Newtonsoft.Json;

namespace Core.Storage;

public class ReportDataStorage
{
    private static readonly Lazy<ReportDataStorage> _lazy = new(() => new ReportDataStorage());
    private readonly ConcurrentDictionary<Guid, List<ReportDataSave>> _reportDataCache = new();
    private readonly string _rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSavedata);
    public static ReportDataStorage Instance => _lazy.Value;

    public void Initialized()
    {
    }

    public void StartSaveData(TaskInfo taskInfo)
    {
        if (string.IsNullOrEmpty(taskInfo.Remark)) return;
        var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskInfo.Remark);
        if (!dic.ContainsKey("needSaveData")) return;
        if (!dic.ContainsKey("interval")) return;
        if (!dic.ContainsKey("fileType")) return;
        if (!dic.ContainsKey("compress")) return;
        var needSaveData = dic["needSaveData"];
        if (needSaveData == null || !bool.TryParse(needSaveData.ToString(), out var need) || !need) return;
        var intervalValue = dic["interval"];
        if (intervalValue == null || !int.TryParse(intervalValue.ToString(), out var interval)) return;
        var typeValue = dic["fileType"];
        if (typeValue == null || !Enum.TryParse(typeValue.ToString(), false, out FileDataType type)) return;
        var compress = dic["compress"];
        if (compress == null || !bool.TryParse(compress.ToString(), out var isCompress)) return;
        if (!taskInfo.RunningParameters.ContainsKey(ParameterNames.ScanSegments))
        {
            Trace.WriteLine($"启动数据存储失败！任务 {taskInfo.Id} 参数中未找到 {ParameterNames.ScanSegments}");
            return;
        }

        var parameter = taskInfo.RunningParameters[ParameterNames.ScanSegments];
        if (parameter.Value is not Dictionary<string, object>[] segments) return;
        var list = new List<ReportDataSave>();
        taskInfo.IsReportNeeded = true;
        for (var i = 0; i < segments.Length; i++)
        {
            var startFreq = Convert.ToDouble(segments[i][ParameterNames.StartFrequency]);
            var stopFreq = Convert.ToDouble(segments[i][ParameterNames.StopFrequency]);
            var stepFreq = Convert.ToDouble(segments[i][ParameterNames.StepFrequency]);
            var save = new ReportDataSave(_rootPath, interval, type, isCompress, true);
            save.Initialized(i, startFreq, stopFreq, stepFreq, RunningInfo.EdgeId, taskInfo);
            save.Start();
            save.FileSaveChanged += ReportFileSaveChanged;
            list.Add(save);
        }

        _reportDataCache.TryAdd(taskInfo.Id, list);
    }

    public void Stop(Guid taskId)
    {
        if (_reportDataCache.TryRemove(taskId, out var list))
            list.ForEach(item =>
            {
                item.Stop();
                item.FileSaveChanged -= ReportFileSaveChanged;
                item.Dispose();
            });
    }

    public void SetData(Guid taskId, List<object> data)
    {
        if (!_reportDataCache.TryGetValue(taskId, out var list)) return;
        list.ForEach(item => item.SetData(data));
    }

    private void ReportFileSaveChanged(object sender, FileSavedNotification e)
    {
        MessageManager.Instance.SendMessage(e);
    }
}