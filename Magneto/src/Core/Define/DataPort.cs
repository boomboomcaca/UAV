using System;
using System.Collections.Generic;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;

namespace Core.Define;

public class DataPort(Guid taskId) : IDataPort
{
    public Guid TaskId { get; } = taskId;

    public void OnData(List<object> data)
    {
        DataArrived?.Invoke(TaskId, data);
    }

    public void OnMessage(SDataMessage message)
    {
        MessageArrived?.Invoke(message);
    }

    public event DataArrivedHandler DataArrived;
    public event MessageArrivedHandler MessageArrived;
}