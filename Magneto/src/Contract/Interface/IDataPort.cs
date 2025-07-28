using System;
using System.Collections.Generic;
using Magneto.Protocol.Data;

namespace Magneto.Contract.Interface;

/// <summary>
///     数据返回通知处理
/// </summary>
/// <param name="taskId">任务ID</param>
/// <param name="data"></param>
public delegate void DataArrivedHandler(Guid taskId, List<object> data);

public delegate void MessageArrivedHandler(SDataMessage message);

/// <summary>
///     消息数据返回通道
/// </summary>
public interface IDataPort
{
    Guid TaskId { get; }
    void OnData(List<object> data);
    void OnMessage(SDataMessage message);
}