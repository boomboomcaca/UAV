using System;
using System.Collections.Generic;
using Magneto.Contract;

namespace Magneto.Definition;

public class RunningLog
{
    private static readonly Lazy<RunningLog> _lazy = new(() => new RunningLog());
    private readonly object _lockLog = new();
    public static RunningLog Instance => _lazy.Value;
    public List<LogData> Logs { get; } = new();

    public void AddLog(string log)
    {
        lock (_lockLog)
        {
            Logs.Add(new LogData(log));
            if (Logs.Count > 100) Logs.RemoveAt(0);
        }
    }

    public void Clear()
    {
        lock (_lockLog)
        {
            Logs.Clear();
        }
    }
}

public class LogData(string log)
{
    public DateTime DateTime { get; init; } = Utils.GetNowTime();
    public string Log { get; init; } = log;
}