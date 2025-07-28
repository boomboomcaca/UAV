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
        // 移除重复的消息管理器日志输出，避免重复显示
        // MessageManager.Instance.Log("Trace", LogType.Message, str);
    }

    public override void WriteLine(string str)
    {
        RunningLog.Instance.AddLog(str);
        Console.WriteLine(str);
        // 移除重复的消息管理器日志输出，避免重复显示
        // MessageManager.Instance.Log("Trace", LogType.Message, str);
    }
}