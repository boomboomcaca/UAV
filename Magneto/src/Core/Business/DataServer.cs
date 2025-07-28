using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CCC;
using Magneto.Contract;
using Magneto.Contract.Defines;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Magneto.Protocol.Extensions;
using StreamJsonRpc;

namespace Core.Business;

public class DataServer : IDisposable
{
    private readonly ClientInfo _client;
    private readonly CancellationTokenSource _cts;
    private readonly ConcurrentQueue<List<object>> _dataCache = new();

    /// <summary>
    ///     需要抽点的数据缓存
    /// </summary>
    private readonly ConcurrentDictionary<SDataType, DataCacheInfo> _dataFrameDic = new();

    ///// <summary>
    ///// 这个缓存保存以下类型的数据：
    ///// 数据每次发送N个，同时这N条数据不能丢，需要一次性发送
    ///// 不同时间发送的数据可以丢，只需要发送最新的N条数据
    ///// 例如：带宽很小的情况下，缓存了M组N条数据，则丢弃前面的M-1组数据，只发送最后一包的N条数据（建议这种数据直接打包为一个单独的数据结构）
    ///// TODO: 未来可能需要考虑一个更好的方法进行重构
    ///// </summary>
    //private readonly ConcurrentQueue<List<object>> _skipFrameDataCache = new();
    /// <summary>
    ///     需要缓存发送但是不抽点或发送最后一包的数据缓存
    /// </summary>
    private readonly ConcurrentDictionary<SDataType, List<object>> _frameDataCache = new();

    private readonly Guid _taskId;

    public DataServer(ClientInfo client)
    {
        _client = client;
        client.ActiveTime = DateTime.Now;
        Guid.TryParse(_client.TaskId, out var id);
        if (Guid.Empty.Equals(id) || !TaskManager.Instance.IsTaskIdExist(id))
            throw new NullReferenceException("任务ID不存在");
        var task = TaskManager.Instance.GetTask(id);
        if (task == null) return;
        task.AddClient(client);
        _taskId = id;
        _cts = new CancellationTokenSource();
        //Task sendDataTask = new(p => SendDataAsync(p).ConfigureAwait(false), _cts.Token);
        //sendDataTask.Start();
        //Task processDataTask = new(p => ProcessDataAsync(p).ConfigureAwait(false), _cts.Token);
        //processDataTask.Start();
        _ = Task.Run(() => SendDataAsync(_cts.Token));
        _ = Task.Run(() => ProcessDataAsync(_cts.Token));
        task.DataArrived += OnDataArrived;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        var task = TaskManager.Instance.GetTask(_taskId);
        if (task == null) return;
        if (task.TaskInfo.DataPort != null) task.DataArrived -= OnDataArrived;
        _dataCache.Clear();
        foreach (var pair in _dataFrameDic) pair.Value.Clear();
        _dataFrameDic.Clear();
        // 这里理论上应该只删除连接，不停止任务，停止任务由前端进行停止
        // 当任务30秒没有连接过来则由边缘端停止任务
        task.DeleteClient(_client);
        GC.SuppressFinalize(this);
    }

    [JsonRpcMethod("heartbeat")]
    public void HeartBeat()
    {
        _client.ActiveTime = DateTime.Now;
    }

    private void OnDataArrived(Guid taskId, List<object> obj)
    {
        if (_client.ClearDataSign)
        {
            _client.ClearDataSign = false;
            ClearData();
            return;
        }

        var task = TaskManager.Instance.GetTask(taskId);
        if (task == null || task.TaskInfo.State != TaskState.Start
                         || task.TaskInfo.RequestState != TaskState.Start)
            // Console.WriteLine("STOP!!!!!!!!!!!!!!!!!!!");
            return;
        if (obj == null) return;
        try
        {
            //    var audioList = new List<object>();
            //    foreach (var item in obj)
            //    {
            //        if (item is SDataAudio || (item is SDataDDC ddc && ddc.Data.Any(p => p is SDataAudio)))
            //        {
            //            audioList.Add(item);
            //        }
            //    }
            //    audioList.ForEach(p => obj.Remove(p));
            // 需要直接发送的数据（比如音频）
            var stack = new Stack<object>(obj.Count);
            for (var i = obj.Count - 1; i >= 0; i--)
            {
                var item = obj[i];
                if (item is not SDataRaw raw)
                {
                    obj.RemoveAt(i);
                    continue;
                }

                var (sendDirectly, _, _) = DataSamplingManager.Get(raw.Type);
                if (item is not SDataAudio && (item is not SDataDdc ddc || !ddc.Data.Any(p => p is SDataAudio)) &&
                    !sendDirectly) continue;
                stack.Push(item);
                obj.RemoveAt(i);
            }

            _dataCache.Enqueue(obj);
            if (stack.Count <= 0) return;
            var data = stack.ToList();
            if (data.Count <= 0) return;
            var sharedData = new SharedData
            {
                EdgeId = RunningInfo.EdgeId,
                TaskId = _taskId.ToString(),
                Timestamp = Magneto.Contract.Utils.GetNowTimestamp(),
                DataCollection = data
            };
            _ = _client.RpcServer
                .NotifyWithParameterObjectAsync(MethodDefine.DataHandlerNotify, sharedData.ToDictionary())
                .ConfigureAwait(false);
            // _dataCache.Enqueue(obj);
        }
        catch (Exception ex)
        {
            Console.WriteLine("发送异常:" + ex.Message);
            Dispose();
            // TaskManager.Instance.StopTask(taskID);
        }
    }

    private async Task SendDataAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            var list = GetData();
            if (list == null || list.Count == 0)
            {
                Thread.Sleep(1);
                // await Task.Delay(1, token).ConfigureAwait(false);
                continue;
            }

            var sharedData = new SharedData
            {
                EdgeId = RunningInfo.EdgeId,
                TaskId = _taskId.ToString(),
                Timestamp = Magneto.Contract.Utils.GetNowTimestamp(),
                DataCollection = list
            };
            if (_client.Socket.State == WebSocketState.Open && _client.RpcServer != null)
            {
                if (RunningInfo.FrameDynamic)
                    await _client.RpcServer
                        .NotifyWithParameterObjectAsync(MethodDefine.DataHandlerNotify, sharedData.ToDictionary())
                        .ConfigureAwait(false);
                else
                    await _client.RpcServer
                        .NotifyWithParameterObjectAsync(MethodDefine.DataHandlerNotify, sharedData.ToDictionary())
                        .ConfigureAwait(false);
            }
            else
            {
                Dispose();
                // TaskManager.Instance.StopTask(taskID);
            }
        }
    }

    private Task ProcessDataAsync(object obj)
    {
        if (obj is not CancellationToken token) return Task.CompletedTask;
        while (!token.IsCancellationRequested)
            try
            {
                if (!_dataCache.TryDequeue(out var data))
                {
                    Thread.Sleep(1);
                    // await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                if (data?.Any() != true)
                {
                    Thread.Sleep(1);
                    // await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                DataSampling(data);
            }
            catch
            {
                // ignored
            }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     数据抽点
    /// </summary>
    /// <param name="data"></param>
    private void DataSampling(List<object> data)
    {
        var dic = new Dictionary<SDataType, List<SDataRaw>>();
        data.ForEach(p =>
        {
            if (p is not SDataRaw raw) return;
            if (dic.TryGetValue(raw.Type, out var value))
                value.Add(raw);
            else
                dic.Add(raw.Type, [raw]);
        });
        foreach (var kv in dic)
        {
            var (_, needSampling, canDrop) = DataSamplingManager.Get(kv.Key);
            if (needSampling)
            {
                _dataFrameDic.AddOrUpdate(kv.Key,
                    _ =>
                    {
                        //t = kv.Key;
                        var info = new DataCacheInfo(kv.Key, canDrop);
                        info.AddData(kv.Value);
                        return info;
                    },
                    (_, i) =>
                    {
                        i.AddData(kv.Value);
                        return i;
                    });
            }
            else if (canDrop)
            {
                var list = kv.Value.ConvertAll(p => (object)p);
                _frameDataCache.AddOrUpdate(kv.Key, list, (_, _) => list);
            }
            else
            {
                var list = kv.Value.ConvertAll(p => (object)p);
                _frameDataCache.AddOrUpdate(kv.Key, list, (_, v) =>
                {
                    v.AddRange(list);
                    return v;
                });
            }
        }
    }

    private List<object> GetData()
    {
        try
        {
            List<object> list = new()
            {
                Capacity = 1048576
            };
            foreach (var pair in _dataFrameDic)
            {
                var data = pair.Value.GetData();
                if (data?.Any() != true) continue;
                list.AddRange(data);
            }

            foreach (var pair in _frameDataCache)
            {
                var data = pair.Value;
                if (data?.Any() != true) continue;
                list.AddRange(data);
            }

            _frameDataCache.Clear();
            //List<object> list = new();
            //foreach (var pair in _dataFrameDic)
            //{
            //    var data = pair.Value.GetData();
            //    if (data?.Count > 0)
            //    {
            //        list.AddRange(data);
            //    }
            //}
            //if (_skipFrameDataCahce.TryDequeue(out var skip))
            //{
            //    if (skip?.Count > 0)
            //    {
            //        list.AddRange(skip);
            //    }
            //}
            return list;
        }
        catch (Exception a)
        {
            Console.WriteLine(a);
            _frameDataCache.Clear();
            return null;
        }
    }

    /// <summary>
    ///     清理缓存数据
    /// </summary>
    private void ClearData()
    {
        Console.WriteLine("清理缓存");
        _dataCache.Clear();
        _frameDataCache.Clear();
        //_skipFrameDataCahce.Clear();
        foreach (var pair in _dataFrameDic) pair.Value.Clear();
        _dataFrameDic.Clear();
    }
}