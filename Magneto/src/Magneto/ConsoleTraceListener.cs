using System;
using System.Diagnostics;
using Core;
using Magneto.Definition;
using Magneto.Protocol.Define;

namespace Magneto;

public class ConsoleTraceListener : TraceListener
{
    public override void Write(string str)
    {
        RunningLog.Instance.AddLog(str);
        Console.Write(str);
        MessageManager.Instance.Log("Trace", LogType.Message, str);
    }

    public override void WriteLine(string str)
    {
        RunningLog.Instance.AddLog(str);
        Console.WriteLine(str);
        MessageManager.Instance.Log("Trace", LogType.Message, str);
    }
}