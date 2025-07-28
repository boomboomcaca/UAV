using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CCC;

/// <summary>
///     由于某些情况下无法根据代码抓取到客户端掉线通知，因此自定义日志记录器，在日志中抓取客户端掉线信息
/// </summary>
public class DebugLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return Debugger.IsAttached && logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        if (formatter == null) throw new ArgumentNullException(nameof(formatter));
        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message)) return;
        _ = $"{logLevel}: {message}";
        if (exception != null) _ = Environment.NewLine + Environment.NewLine + exception;
        //Trace.WriteLine(message);
        // ID为2的事件是连接停止事件，检测到这个事件则将连接从缓存中清除
        if (eventId.Name != "ConnectionStop" || eventId.Id != 2) return;
        //
        if (state is IEnumerable dic)
            foreach (KeyValuePair<string, object> pair in dic)
                if (pair.Key == "ConnectionId")
                {
                    var id = pair.Value.ToString(); // 这个id是连接的SessionId
                    Server.ReleaseClient(Server.Port, id);
                }
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new DebugLogger();
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
        GC.SuppressFinalize(this);
    }
}