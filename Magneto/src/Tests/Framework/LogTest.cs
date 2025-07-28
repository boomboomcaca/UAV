using System;
using System.IO;
using Magneto.Contract;
using Magneto.Core;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Core;

[TestFixture]
public class LogTest
{
    [SetUp]
    public void Setup()
    {
        _file = Path.Combine(Directory.GetCurrentDirectory(), PublicDefine.PathLog,
            $"Log-{DateTime.Now:yyyyMMdd}.log");
        if (File.Exists(_file))
            try
            {
                File.Delete(_file);
            }
            catch
            {
            }
    }

    private string _file = string.Empty;

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    // [TestCase(6)]
    public void LogMessage(int errorCode)
    {
        var msg = new SDataMessage
        {
            ErrorCode = errorCode,
            LogType = LogType.Warning,
            Description = "description",
            Detail = "detail"
        };
        MessageManager.Instance.Log(msg);
        FileAssert.Exists(_file);
    }

    [Test]
    [TestCase("none", LogType.None, "none message")]
    [TestCase("driver", LogType.Message, "message info")]
    [TestCase("login", LogType.Warning, "warning message")]
    [TestCase("device", LogType.Error, "error message")]
    public void LogInfo(string module, LogType type, string msg)
    {
        MessageManager.Instance.Log(module, type, msg);
        FileAssert.Exists(_file);
    }

    [Test]
    [TestCase("module1", "first message")]
    [TestCase("module2", "second message")]
    public void Error(string module, string msg)
    {
        try
        {
            File.Delete(null!);
        }
        catch (Exception e)
        {
            MessageManager.Instance.Error(module, msg, e);
            FileAssert.Exists(_file);
        }
    }
}