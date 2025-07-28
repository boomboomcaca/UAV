using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Contract.Storage;

/// <summary>
///     原始IQ数据保存
///     数据文件结构：数据帧*N
///     索引文件结构：总帧数N（4个字节）+ 文件头长度（4个字节）+ 文件头 + 索引帧*N
///     参数文件结构：MessagePack序列化<see cref="ParametersInfo">参数信息ParametersInfo</see> 的二进制数据
/// </summary>
public sealed class RawIqDataStorage
{
    private const string DataSuffix = ".dat";
    private const string IndexSuffix = ".idx";
    private const string ParamSuffix = ".params";
    private const string TempSuffix = ".temp";
    private const int IndexSize = sizeof(long) + sizeof(ushort) + sizeof(int) + sizeof(ulong);
    private const long SplitSize = 200 * 1024 * 1024;
    private static readonly Lazy<RawIqDataStorage> _lazy = new(() => new RawIqDataStorage());

    private readonly string _rootPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSaveiqdata);

    private readonly ConcurrentDictionary<Guid, TaskFileManagerInfo> _taskFileManagerInfoList = new();

    private RawIqDataStorage()
    {
    }

    // private bool _disposed = false;
    public static RawIqDataStorage Instance => _lazy.Value;

    /// <summary>
    ///     文件更新事件
    ///     包括数据文件和报表文件的通知内容
    /// </summary>
    public event EventHandler<FileSavedNotification> FileModified;

    /// <summary>
    ///     新建文件，调用此方法应捕获异常。
    /// </summary>
    /// <param name="header">文件概要</param>
    /// <param name="feature"></param>
    /// <param name="createdTime"></param>
    /// <param name="parameters"></param>
    public void Create(FileDataSummary header, FeatureType feature, DateTime createdTime,
        List<Parameter> parameters)
    {
        //初始化数据根目录
        if (!Directory.Exists(_rootPath)) Directory.CreateDirectory(_rootPath);
        //初始化时间目录
        var timeFolderName = createdTime.ToString("yyyyMMdd");
        var timeFolderPath = Path.Combine(_rootPath, timeFolderName);
        if (!Directory.Exists(timeFolderPath)) Directory.CreateDirectory(timeFolderPath);
        //初始化任务目录
        var taskFolderName = $"{header.TaskId}";
        var taskPath = Path.Combine(timeFolderPath, taskFolderName);
        if (Directory.Exists(taskPath)) Directory.Delete(taskPath);
        Directory.CreateDirectory(taskPath);
        //生成文件名
        var featureName = Utils.ConvertEnumToString(feature);
        const ushort fileIndex = 1;
        var name = $"{header.EdgeId}_{featureName}_IQ_{createdTime:yyyyMMddHHmmss}";
        var fileName = $"{name}.{fileIndex}";
        var dataFilePath = Path.Combine(taskPath, fileName);
        var indexFilePath = Path.Combine(taskPath, name);
        //读入文件
        var indexStream = new List<byte>();
        //写入文件头
        var summary = MessagePackSerializer.Serialize(header);
        indexStream.AddRange(new byte[4]); //总帧数
        indexStream.AddRange(BitConverter.GetBytes((uint)summary.Length)); //文件头长度
        indexStream.AddRange(summary); //文件头
        //预处理文件
        var tempDataFileName = PreprocessFile(dataFilePath + DataSuffix);
        var tempIndexFileName = PreprocessFile(indexFilePath + IndexSuffix);
        using (var fs = new FileStream(tempIndexFileName, FileMode.OpenOrCreate))
        {
            fs.Write(indexStream.ToArray(), 0, indexStream.Count);
            fs.Flush();
        }

        //初始化通知信息
        var fileNotificationInfo = new FileSavedNotification
        {
            NotificationType = FileNotificationType.Created,
            TaskId = header.TaskId,
            DriverId = header.DriverId,
            PluginId = header.PluginId,
            RootPath = _rootPath.Replace('\\', '/'),
            RelativePath = Path.Combine(timeFolderName, taskFolderName).Replace('\\', '/'),
            ComputerId = RunningInfo.ComputerId,
            FileName = name,
            DataType = FileDataType.Iq,
            Parameters = GetMainParaString(parameters),
            BeginRecordTime = Utils.GetNowTimestamp(),
            LastModifiedTime = Utils.GetNowTimestamp()
        };
        FileModified?.Invoke(this, fileNotificationInfo.Clone());
        var info = new TaskFileManagerInfo
        {
            TaskId = Guid.Parse(header.TaskId),
            Feature = feature,
            EdgeId = header.EdgeId,
            PluginId = header.PluginId,
            DriverId = header.DriverId,
            DeviceId = header.DeviceId,
            TaskCreatedTime = createdTime,
            TaskFolderPath = taskPath,
            TempDataFileName = tempDataFileName,
            TempIndexFileName = tempIndexFileName,
            Summary = summary,
            FileNotificationInfo = fileNotificationInfo,
            CurrentDataIndex = 0,
            CurrentFileIndex = fileIndex,
            CurrentCycleFileIndex = fileIndex,
            CurrentCycleIndexPosition = indexStream.Count,
            CurrentIndexPosition = indexStream.Count,
            CurrentCycleDataIndex = 0,
            CurrentCycleRunning = true,
            PublicFileName = name,
            ParametersInfo = new ParametersInfo
            {
                Parameters = new Dictionary<string, List<Parameter>>
                {
                    { "1", parameters }
                },
                ParameterMap = new Dictionary<string, List<int>>
                {
                    { "1", [fileIndex] }
                },
                StorageTimeSegments =
                [
                    new()
                    {
                        StartTime = Utils.GetNowTimestamp(),
                        StopTime = 0
                    }
                ]
            },
            Parameters = parameters,
            IsRunning = true,
            IsFirstFile = true
        };
        _taskFileManagerInfoList.TryAdd(info.TaskId, info);
    }

    public void ChangeParameters(Guid taskId, List<Parameter> parameters, bool newCycle = false)
    {
        Console.WriteLine($"修改参数，重新存储，新周期:{newCycle}");
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return;
        if (!managerInfo.IsRunning) return;
        //更新参数信息
        lock (managerInfo.DataSyncLock)
        {
            var oldCount = managerInfo.ParametersInfo.Parameters.Count;
            var key = (oldCount + 1).ToString();
            (managerInfo.ParametersInfo.Parameters ??= new Dictionary<string, List<Parameter>>()).AddOrUpdate(key,
                parameters);
            managerInfo.FileNotificationInfo.Parameters = GetMainParaString(parameters);
            managerInfo.Parameters = parameters;
            SaveNewDataFile(managerInfo, newCycle);
            managerInfo.CurrentCycleRunning = true;
        }
    }

    /// <summary>
    ///     更新天线因子
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="factor"></param>
    public void UpdateFactor(Guid taskId, List<SDataFactor> factor)
    {
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return;
        if (!managerInfo.IsRunning) return;
        //更新天线因子
        lock (managerInfo.DataSyncLock)
        {
            if (managerInfo.Parameters == null) return;
            managerInfo.ParametersInfo.Factors ??= new Dictionary<string, List<SDataFactor>>();
            var key = managerInfo.ParametersInfo.Parameters.LastOrDefault().Key;
            managerInfo.ParametersInfo.Factors[key] = factor;
        }
    }

    /// <summary>
    ///     写入数据，调用此方法应捕获异常。
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <param name="data">数据</param>
    public void Write(Guid taskId, List<object> data)
    {
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return;
        if (!managerInfo.IsRunning) return;
        lock (managerInfo.DataSyncLock)
        {
            if (data == null) return;
            if (!managerInfo.CurrentCycleRunning) return;
            data.ForEach(item =>
            {
                if (item is SDataRaw raw)
                {
                    var type = Utils.ConvertEnumToString(raw.Type);
                    if (!managerInfo.DataTypeList.Contains(type)) managerInfo.DataTypeList.Add(type);
                }
            });
            var stamp = Utils.GetNowTimestamp();
            managerInfo.CurrentDataIndex++;
            var dat = MessagePackSerializer.Serialize(data);
            if (dat.Length + managerInfo.DataStreamPosition > SplitSize) SaveNewDataFile(managerInfo);
            //写数据
            var position = (int)managerInfo.DataStreamPosition;
            var idx = CreateIndexBytes(managerInfo.CurrentDataIndex, managerInfo.CurrentFileIndex, position, stamp);
            using (var indexStream = new FileStream(managerInfo.TempIndexFileName, FileMode.OpenOrCreate))
            {
                indexStream.Position = indexStream.Length;
                indexStream.Write(idx);
                // 更新索引文件中当前帧的位置
                managerInfo.CurrentIndexPosition = indexStream.Length;
            }

            using (var dataStream = new FileStream(managerInfo.TempDataFileName, FileMode.OpenOrCreate))
            {
                dataStream.Position = managerInfo.DataStreamPosition;
                dataStream.Write(dat);
                managerInfo.DataStreamPosition += dat.Length;
                managerInfo.AllDataLength += dat.Length;
            }

            //更新通知信息
            if (managerInfo.CurrentDataIndex == 1) managerInfo.FileNotificationInfo.BeginRecordTime = stamp;
            managerInfo.FileNotificationInfo.EndRecordTime = stamp;
            managerInfo.FileNotificationInfo.RecordCount = managerInfo.CurrentDataIndex;
        }
    }

    /// <summary>
    ///     任务停止，完成数据写入，调用此方法应捕获异常。
    /// </summary>
    /// <param name="taskId">任务ID</param>
    public void Complete(Guid taskId)
    {
        Console.WriteLine("任务停止，结束存储");
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return;
        if (!managerInfo.IsRunning) return;
        lock (managerInfo.DataSyncLock)
        {
            managerInfo.IsRunning = false;
            managerInfo.CurrentCycleRunning = false;
            using (var fs = new FileStream(managerInfo.TempIndexFileName, FileMode.OpenOrCreate))
            {
                var bytes = BitConverter.GetBytes((uint)managerInfo.CurrentDataIndex);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
            }

            RecoveryFileProcessed(managerInfo.TempIndexFileName);
            if (string.IsNullOrEmpty(managerInfo.TempDataFileName)) return;
            var segment = managerInfo.ParametersInfo.StorageTimeSegments.LastOrDefault();
            if (segment?.StopTime == 0) segment.StopTime = Utils.GetNowTimestamp();
            var paramsFilePath = Path.Combine(managerInfo.TaskFolderPath, managerInfo.PublicFileName + ParamSuffix);
            using (var paramsStream = new FileStream(paramsFilePath, FileMode.OpenOrCreate))
            {
                var bytes = MessagePackSerializer.Serialize(managerInfo.ParametersInfo);
                paramsStream.Write(bytes);
                paramsStream.Flush();
            }

            var fileName = RecoveryFileProcessed(managerInfo.TempDataFileName);
            if (string.IsNullOrWhiteSpace(fileName)) return;
            //更新通知信息
            var fileInfo = new FileInfo(fileName);
            managerInfo.FileNotificationInfo.Size = fileInfo.Length;
            if (managerInfo.Parameters != null)
            {
                // 更新当前存储包含的所有数据
                Parameter parameter = new()
                {
                    Name = "saveDataTypeList",
                    Value = managerInfo.DataTypeList
                };
                managerInfo.Parameters.Add(parameter);
                managerInfo.FileNotificationInfo.Parameters = GetMainParaString(managerInfo.Parameters);
            }

            managerInfo.FileNotificationInfo.NotificationType = FileNotificationType.Modified;
            var time = Utils.ConvertTimeToCloud(fileInfo.LastWriteTimeUtc);
            managerInfo.FileNotificationInfo.LastModifiedTime = Utils.GetTimestamp(time);
            FileModified?.Invoke(this, managerInfo.FileNotificationInfo);
        }

        _taskFileManagerInfoList.Remove(managerInfo.TaskId, out _);
    }

    /// <summary>
    ///     完成单个数据的写入
    /// </summary>
    /// <param name="taskId"></param>
    public void StopSaveFile(Guid taskId)
    {
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return;
        if (!managerInfo.IsRunning) return;
        Console.WriteLine("存储停止，结束单个文件存储");
        lock (managerInfo.DataSyncLock)
        {
            var seg = managerInfo.ParametersInfo.StorageTimeSegments.LastOrDefault();
            if (seg != null) seg.StopTime = Utils.GetNowTimestamp();
            RecoveryFileProcessed(managerInfo.TempDataFileName);
            managerInfo.CurrentCycleRunning = false;
        }
    }

    /// <summary>
    ///     清理某任务最后一次保存的数据
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="isDeleteTask"></param>
    public void ClearFile(Guid taskId, out bool isDeleteTask)
    {
        Console.WriteLine("点击清除，清理最近一次存储");
        isDeleteTask = false;
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return;
        if (!managerInfo.IsRunning) return;
        lock (managerInfo.DataSyncLock)
        {
            // 删除数据文件
            for (int i = managerInfo.CurrentCycleFileIndex; i <= managerInfo.CurrentFileIndex; i++)
            {
                var feature = Utils.ConvertEnumToString(managerInfo.Feature);
                var name = $"{managerInfo.EdgeId}_{feature}_{managerInfo.TaskCreatedTime:yyyyMMddHHmmss}";
                var fileName = $"{name}.{i}";
                var file1 = fileName + DataSuffix;
                var file = Path.Combine(managerInfo.TaskFolderPath, file1);
                File.Delete(file);
                var file2 = fileName + DataSuffix + TempSuffix;
                file = Path.Combine(managerInfo.TaskFolderPath, file2);
                File.Delete(file);
            }

            if (managerInfo.ParametersInfo?.StorageTimeSegments?.Any() == true)
            {
                var last = managerInfo.ParametersInfo.StorageTimeSegments.Count - 1;
                managerInfo.ParametersInfo.StorageTimeSegments.RemoveAt(last);
            }

            // 清理索引文件
            using var indexStream = new FileStream(managerInfo.TempIndexFileName, FileMode.OpenOrCreate);
            indexStream.SetLength(managerInfo.CurrentCycleIndexPosition);
            managerInfo.CurrentIndexPosition = managerInfo.CurrentCycleIndexPosition;
            managerInfo.CurrentDataIndex = managerInfo.CurrentCycleDataIndex;
            managerInfo.CurrentCycleRunning = false;
            managerInfo.AllDataLength -= managerInfo.DataStreamPosition;
            if (managerInfo.AllDataLength < 0) managerInfo.AllDataLength = 0;
            managerInfo.DataStreamPosition = 0;
            indexStream.Close();
            if (managerInfo.IsFirstFile)
            {
                // 首包文件，需要清理
                managerInfo.FileNotificationInfo.NotificationType = FileNotificationType.Delete;
                FileModified?.Invoke(this, managerInfo.FileNotificationInfo);
                _taskFileManagerInfoList.Remove(managerInfo.TaskId, out _);
                isDeleteTask = true;
                if (Directory.Exists(managerInfo.TaskFolderPath)) Directory.Delete(managerInfo.TaskFolderPath, true);
            }
        }
    }

    /// <summary>
    ///     查询存储数据文件的大小
    /// </summary>
    /// <param name="taskId"></param>
    public long GetFileLength(Guid taskId)
    {
        if (!_taskFileManagerInfoList.TryGetValue(taskId, out var managerInfo) || managerInfo == null) return 0;
        if (!managerInfo.IsRunning) return 0;
        return managerInfo.AllDataLength;
    }

    /// <summary>
    ///     读数据（仅作测试使用）
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileName">不带后缀的文件名（包含完整路径）</param>
    /// <param name="summary"></param>
    /// <param name="data"></param>
    /// <param name="parametersInfo"></param>
    public void ReadFile(string folderPath, string fileName, out FileDataSummary summary,
        out List<object> data, out ParametersInfo parametersInfo)
    {
        parametersInfo = null;
        summary = null;
        data = null;
        var idxFile = Path.Combine(folderPath, fileName + IndexSuffix);
        var paramsFile = Path.Combine(folderPath, fileName + ParamSuffix);
        if (!File.Exists(idxFile) || !File.Exists(paramsFile)) return;
        var dataFiles = Directory.GetFiles(folderPath, "*" + DataSuffix);
        if (dataFiles.Any() != true) return;
        data = [];
        //读参数文件
        using var paramsStream = File.OpenRead(paramsFile);
        parametersInfo = MessagePackSerializer.Deserialize<ParametersInfo>(paramsStream);
        //读索引文件
        uint index = 0;
        using var idxStream = File.OpenRead(idxFile);
        idxStream.Seek(index, SeekOrigin.Begin);
        var frameTotalArray = new byte[4];
        _ = idxStream.Read(frameTotalArray, 0, frameTotalArray.Length);
        var frameTotal = BitConverter.ToUInt32(frameTotalArray);
        index += (uint)frameTotalArray.Length;
        idxStream.Seek(index, SeekOrigin.Begin);
        var headerLenArray = new byte[4];
        _ = idxStream.Read(headerLenArray, 0, headerLenArray.Length);
        var headerLen = BitConverter.ToUInt32(headerLenArray);
        index += (uint)headerLenArray.Length;
        idxStream.Seek(index, SeekOrigin.Begin);
        var headerArray = new byte[headerLen];
        _ = idxStream.Read(headerArray, 0, headerArray.Length);
        summary = MessagePackSerializer.Deserialize<FileDataSummary>(headerArray);
        index += headerLen;
        var frameList = new List<(long dataIndex, ushort fileIndex, int position, ulong timestamp)>();
        for (var i = 0; i < frameTotal; i++)
        {
            idxStream.Seek(index, SeekOrigin.Begin);
            var tempIdxArray = new byte[IndexSize];
            _ = idxStream.Read(tempIdxArray, 0, tempIdxArray.Length);
            GetIndexValue(tempIdxArray, out var dataIndex, out var fileIndex, out var position, out var timestamp);
            frameList.Add((dataIndex, fileIndex, position, timestamp));
            index += IndexSize;
        }

        var maxIndex = frameList.Max(p => p.dataIndex);
        if (maxIndex != frameTotal) return;
        //读取数据文件
        var pattern = $"^{fileName}\\.(?<fileIndex>\\d+)\\{DataSuffix}$";
        var list = new List<(long dataIndex, object data)>();
        foreach (var file in dataFiles)
        {
            var dataFileName = Path.GetFileName(file);
            var match = Regex.Match(dataFileName, pattern);
            if (!match.Success) continue;
            var fileIndex = Convert.ToInt32(match.Groups["fileIndex"].Value);
            var selectedFrames = frameList.Where(p => p.fileIndex == fileIndex)
                .OrderBy(p => p.position)
                .ToList();
            if (selectedFrames.Any() != true) continue;
            using var dataStream = File.OpenRead(file);
            for (var i = 0; i < selectedFrames.Count; i++)
            {
                int len;
                var frame = selectedFrames[i];
                if (i == selectedFrames.Count - 1)
                    len = (int)(dataStream.Length - frame.position);
                else
                    len = selectedFrames[i + 1].position - frame.position;
                if (len <= 0) continue;
                var dataArray = new byte[len];
                dataStream.Seek(frame.position, SeekOrigin.Begin);
                _ = dataStream.Read(dataArray, 0, dataArray.Length);
                var obj = MessagePackSerializer.Deserialize<object>(dataArray);
                list.Add((frame.dataIndex, obj));
            }
        }

        data = list.OrderBy(p => p.dataIndex).Select(p => p.data).ToList();
    }

    private void SaveNewDataFile(TaskFileManagerInfo managerInfo, bool newCycle = false)
    {
        //保存旧文件
        RecoveryFileProcessed(managerInfo.TempDataFileName);
        //更新参数信息
        managerInfo.CurrentFileIndex++;
        managerInfo.DataStreamPosition = 0;
        managerInfo.IsFirstFile = false;
        if (newCycle)
        {
            managerInfo.CurrentCycleFileIndex = managerInfo.CurrentFileIndex;
            managerInfo.CurrentCycleIndexPosition = managerInfo.CurrentIndexPosition;
            managerInfo.CurrentCycleDataIndex = managerInfo.CurrentDataIndex;
            managerInfo.AllDataLength = 0;
            var seg = managerInfo.ParametersInfo.StorageTimeSegments.LastOrDefault();
            if (seg?.StopTime == 0) seg.StopTime = Utils.GetNowTimestamp();
            var ns = new StorageTimeSegment
            {
                StartTime = Utils.GetNowTimestamp()
            };
            managerInfo.ParametersInfo.StorageTimeSegments.Add(ns);
        }

        var key = managerInfo.ParametersInfo.Parameters.LastOrDefault().Key;
        (managerInfo.ParametersInfo.ParameterMap ??= new Dictionary<string, List<int>>())
            .TryGetValue(key, out var list);
        (list ??= []).Add(managerInfo.CurrentFileIndex);
        managerInfo.ParametersInfo.ParameterMap.AddOrUpdate(key, list);
        //新文件
        var featureName = Utils.ConvertEnumToString(managerInfo.Feature);
        var name =
            $"{managerInfo.EdgeId}_{featureName}_{managerInfo.TaskCreatedTime:yyyyMMddHHmmss}.{managerInfo.CurrentFileIndex}";
        var nameWithPath = Path.Combine(managerInfo.TaskFolderPath, name);
        var tempDataFileName = PreprocessFile(nameWithPath + DataSuffix);
        managerInfo.TempDataFileName = tempDataFileName;
    }

    private string GetMainParaString(List<Parameter> parameters)
    {
        var para = parameters.ConvertAll(p => p.ToSimple());
        //临时版本
        return JsonConvert.SerializeObject(para);
        // return MessagePack.MessagePackSerializer.SerializeToJson(parameters);
        // if (parameters == null || parameters.Count == 0)
        // {
        //     return string.Empty;
        // }
        // //频段扫描
        // if ((parameters.FirstOrDefault()?.Feature & FeatureType.SCAN) > 0)
        // {
        //     var start = parameters.Find(p => p.Name.Equals("startFrequency"));
        //     var stop = parameters.Find(p => p.Name.Equals("stopFrequency"));
        //     var step = parameters.Find(p => p.Name.Equals("stepFrequency"));
        //     return $"{ParseFrequencyPara(start)}-{ParseFrequencyPara(stop)}|{ParseFrequencyPara(step)}";
        // }
        // //离散扫描
        // if ((parameters.FirstOrDefault()?.Feature & FeatureType.MScan) > 0)
        // {
        //     if (!(parameters.Find(p => p.Name.Equals("mscanPoints"))?.Value is Dictionary<string, object>[] points))
        //     {
        //         return string.Empty;
        //     }
        //     if (points.Length == 0)
        //     {
        //         return string.Empty;
        //     }
        //     var sb = new StringBuilder();
        //     foreach (var val in points)
        //     {
        //         sb.Append("|").Append((double)val["frequency"]).Append("MHz");
        //     }
        //     return sb.ToString().Remove(0, 1);
        // }
        // //其他
        // var freq = parameters.Find(p => p.Name.Equals("frequency"));
        // var bandwidth = parameters.Find(p => p.Name.Equals("ifBandwidth")) ??
        //             parameters.Find(p => p.Name.Equals("span")) ??
        //             parameters.Find(p => p.Name.Equals("dfBandwidth")) ??
        //             parameters.Find(p => p.Name.Equals("filterBandwidth"));
        // return $"{ParseFrequencyPara(freq)}|{ParseFrequencyPara(bandwidth)}";
    }

    private byte[] CreateIndexBytes(long dataIndex, ushort fileIndex, int position, ulong timestamp)
    {
        var array = new byte[IndexSize];
        var dataIdx = BitConverter.GetBytes(dataIndex);
        var fileIdx = BitConverter.GetBytes(fileIndex);
        var pos = BitConverter.GetBytes(position);
        var ts = BitConverter.GetBytes(timestamp);
        var index = 0;
        Array.Copy(dataIdx, 0, array, index, dataIdx.Length);
        index += dataIdx.Length;
        Array.Copy(fileIdx, 0, array, index, fileIdx.Length);
        index += fileIdx.Length;
        Array.Copy(pos, 0, array, index, pos.Length);
        index += pos.Length;
        Array.Copy(ts, 0, array, index, ts.Length);
        return array;
    }

    private void GetIndexValue(byte[] array, out long dataIndex, out ushort fileIndex,
        out int position, out ulong timestamp)
    {
        dataIndex = 0;
        fileIndex = 0;
        position = 0;
        timestamp = 0;
        if (array.Length != IndexSize) return;
        var index = 0;
        var temp = new byte[sizeof(long)];
        Array.Copy(array, index, temp, 0, temp.Length);
        dataIndex = BitConverter.ToInt64(temp);
        index += sizeof(long);
        temp = new byte[sizeof(ushort)];
        Array.Copy(array, index, temp, 0, temp.Length);
        fileIndex = BitConverter.ToUInt16(temp);
        index += sizeof(ushort);
        temp = new byte[sizeof(int)];
        Array.Copy(array, index, temp, 0, temp.Length);
        position = BitConverter.ToInt32(temp);
        index += sizeof(int);
        temp = new byte[sizeof(ulong)];
        Array.Copy(array, index, temp, 0, temp.Length);
        timestamp = BitConverter.ToUInt64(temp);
    }

    private string ParseFrequencyPara(Parameter parameter)
    {
        if (parameter == null) return string.Empty;
        if (!parameter.Name.Contains("frequency", StringComparison.CurrentCultureIgnoreCase)) return string.Empty;
        var value = parameter.Value;
        if (parameter.Values != null && parameter.DisplayValues != null)
            if (parameter.Values.Contains(value) && parameter.Values.Count == parameter.DisplayValues.Count)
            {
                var index = parameter.Values.IndexOf(value);
                return parameter.DisplayValues[index];
            }

        return $"{parameter.Value}{parameter.Unit}";
    }

    /// <summary>
    ///     预处理文件
    ///     创建附加.temp后缀的文件或在已有文件的名称附加.temp后缀
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>处理后的文件名（.temp）</returns>
    private string PreprocessFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        if (!File.Exists(fileName)) File.Create(fileName).Dispose();
        File.Copy(fileName, fileName + TempSuffix);
        File.Delete(fileName);
        return fileName + TempSuffix;
    }

    /// <summary>
    ///     恢复预处理文件
    ///     删除预处理文件的.temp后缀
    /// </summary>
    /// <param name="fileName"></param>
    private string RecoveryFileProcessed(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        var name = fileName.Remove(fileName.Length - TempSuffix.Length, TempSuffix.Length);
        if (File.Exists(fileName))
        {
            if (!fileName.EndsWith(TempSuffix)) return null;
            File.Move(fileName, name, true);
            return name;
        }

        if (File.Exists(name)) return name;
        return null;
    }
}