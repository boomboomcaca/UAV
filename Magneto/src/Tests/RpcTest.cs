using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.CCC;
using Magneto.Core.Business;
using Magneto.Protocol.Define;
using MessagePack;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests;

/*
    进行这个测试需要在../../bin/Tests/Debug/configuration/文件夹下放置相关配置文件
*/
[TestFixture]
public class RpcTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        var webHost = Server.CreateHostBuilder(null).Build();
        Server.CreateJsonRpcServer(webHost, "0.0.0.0", _port, FormatterType.CustomMessagePackFormatter);
        Server.MapServer<ControlServer>(_port, Maps.MapControl);
        Server.MapServer<DataServer>(_port, Maps.MapTask);
        _ = Server.RunServerAsync(_port).ConfigureAwait(false);
        Thread.Sleep(1000);
        _jsonRpcRequest.Add("jsonrpc", "2.0");
        _jsonRpcRequest.Add("id", Guid.NewGuid().ToString());
        _jsonRpcRequest.Add("method", "");
        _jsonRpcRequest.Add("params", null);
        _client = new ClientWebSocket();
        _client.ConnectAsync(new Uri($"ws://127.0.0.1:{_port}{Maps.MapControl}"), CancellationToken.None)
            .ConfigureAwait(false).GetAwaiter().GetResult();
        _isInitialized = true;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _ = Server.CloseServerAsync(_port);
        _client.Dispose();
    }

    private readonly int _port = 5001;
    private readonly Dictionary<string, object> _jsonRpcRequest = new();
    private ClientWebSocket _client;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private string _msg = "";
    private bool _sign;
    private bool _isInitialized;

    [Test]
    [Order(1)]
    public void SayHelloTest()
    {
        if (!_isInitialized) Setup();
        _jsonRpcRequest["method"] = "sayHello";
        _jsonRpcRequest["params"] = new Dictionary<string, object>
        {
            { "name", "test" },
            { "age", 33 }
        };
        var id = Guid.NewGuid().ToString();
        _jsonRpcRequest["id"] = id;
        var arr = new byte[1024 * 1024];
        SendData();
        _sign = false;
        _msg = "等待超时";
        _ = Task.Run(async () =>
        {
            var res = await _client.ReceiveAsync(arr, CancellationToken.None);
            var data = new byte[res.Count];
            Array.Copy(arr, 0, data, 0, res.Count);
            CheckResult(data, id);
        });
        _autoResetEvent.WaitOne(5000);
        Console.WriteLine(_msg);
        Assert.AreEqual(_sign, true);
    }

    [Test]
    [Order(2)]
    [TestCase("queryStation")]
    [TestCase("queryDevice")]
    [TestCase("queryDriver")]
    [TestCase("queryAllinfo")]
    [TestCase("restartApp")]
    [TestCase("updateConfig")]
    [TestCase("updateCrontab")]
    [TestCase("updateRsync", "{\"configType\":\"start\"}")]
    [TestCase("updateRsync", "{\"configType\":\"stop\"}")]
    [TestCase("updateRsync", "{\"configType\":\"update\"}")]
    [TestCase("beat")]
    [TestCase("beat2")]
    public void SystemTest(string method, string param = null)
    {
        if (!_isInitialized) Setup();
        _jsonRpcRequest["method"] = method;
        _jsonRpcRequest["params"] = null;
        if (param != null)
        {
            var paramDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(param);
            _jsonRpcRequest["params"] = paramDic;
        }

        var id = Guid.NewGuid().ToString();
        _jsonRpcRequest["id"] = id;
        _autoResetEvent.Reset();
        var arr = new byte[1024 * 1024];
        SendData();
        _sign = false;
        _msg = "等待超时";
        var recvComplete = false;
        var data = new List<byte>();
        var waitRecv = new AutoResetEvent(false);
        var reCount = 0;
        do
        {
            var recvOk = false;
            _ = Task.Run(async () =>
            {
                var res = await _client.ReceiveAsync(arr, CancellationToken.None);
                var tmp = new byte[res.Count];
                Array.Copy(arr, 0, tmp, 0, res.Count);
                data.AddRange(tmp);
                if (res.EndOfMessage)
                {
                    recvComplete = true;
                    CheckResult(data.ToArray(), id);
                }

                recvOk = true;
                waitRecv.Set();
            });
            waitRecv.WaitOne(2000);
            if (!recvOk)
            {
                reCount++;
                if (reCount > 3) break;
            }
        } while (!recvComplete);

        _autoResetEvent.WaitOne(8000);
        Console.WriteLine(_msg);
        Assert.AreEqual(_sign, true);
    }

    [Test]
    [TestCase("presetTask")]
    [TestCase("startTask")]
    [TestCase("stopTask")]
    [TestCase("setTaskParameters")]
    public void TaskTest(string method)
    {
        if (!_isInitialized) Setup();
        _jsonRpcRequest["method"] = method;
        _jsonRpcRequest["params"] = null;
        if (method == "presetTask")
            _jsonRpcRequest["params"] = new Dictionary<string, object>
            {
                { "moduleId", Guid.NewGuid().ToString() },
                { "pluginId", "" },
                { "pluginName", "" }
            };
        else if (method is "startTask" or "stopTask")
            _jsonRpcRequest["params"] = new Dictionary<string, object>
            {
                { "id", Guid.NewGuid().ToString() }
            };
        else if (method == "setTaskParameters")
            _jsonRpcRequest["params"] = new Dictionary<string, object>
            {
                { "id", Guid.NewGuid().ToString() },
                { "parameters", new List<Parameter>() }
            };
        var id = Guid.NewGuid().ToString();
        _jsonRpcRequest["id"] = id;
        _autoResetEvent.Reset();
        var arr = new byte[1024 * 1024];
        SendData();
        _sign = false;
        _msg = "等待超时";
        var recvComplete = false;
        var data = new List<byte>();
        var waitRecv = new AutoResetEvent(false);
        var reCount = 0;
        do
        {
            var recvOk = false;
            _ = Task.Run(async () =>
            {
                var res = await _client.ReceiveAsync(arr, CancellationToken.None);
                var tmp = new byte[res.Count];
                Array.Copy(arr, 0, tmp, 0, res.Count);
                data.AddRange(tmp);
                if (res.EndOfMessage)
                {
                    recvComplete = true;
                    CheckResult(data.ToArray(), id);
                }

                recvOk = true;
                waitRecv.Set();
            });
            waitRecv.WaitOne(2000);
            if (!recvOk)
            {
                reCount++;
                if (reCount > 3) break;
            }
        } while (!recvComplete);

        _autoResetEvent.WaitOne(8000);
        Console.WriteLine(_msg);
        Assert.AreEqual(_sign, true);
    }

    [Test]
    public void TestQueryFeatures()
    {
        if (!_isInitialized) Setup();
        _jsonRpcRequest["method"] = "edge.queryAvailableFeatures";
        _jsonRpcRequest["params"] = null;
        var id = Guid.NewGuid().ToString();
        _jsonRpcRequest["id"] = id;
        _autoResetEvent.Reset();
        var arr = new byte[1024 * 1024];
        SendData();
        _sign = false;
        _msg = "等待超时";
        var recvComplete = false;
        var data = new List<byte>();
        var waitRecv = new AutoResetEvent(false);
        var reCount = 0;
        do
        {
            var recvOk = false;
            _ = Task.Run(async () =>
            {
                var res = await _client.ReceiveAsync(arr, CancellationToken.None);
                var tmp = new byte[res.Count];
                Array.Copy(arr, 0, tmp, 0, res.Count);
                data.AddRange(tmp);
                if (res.EndOfMessage)
                {
                    recvComplete = true;
                    CheckResult(data.ToArray(), id);
                }

                recvOk = true;
                waitRecv.Set();
            });
            waitRecv.WaitOne(2000);
            if (!recvOk)
            {
                reCount++;
                if (reCount > 3) break;
            }
        } while (!recvComplete);

        _autoResetEvent.WaitOne(8000);
        Console.WriteLine(_msg);
        Assert.AreEqual(_sign, true);
    }

    [Test]
    public void TestGetEdgeCapacity()
    {
        if (!_isInitialized) Setup();
        _jsonRpcRequest["method"] = "edge.getCapacity";
        _jsonRpcRequest["params"] = null;
        var id = Guid.NewGuid().ToString();
        _jsonRpcRequest["id"] = id;
        _autoResetEvent.Reset();
        var arr = new byte[1024 * 1024];
        SendData();
        _sign = false;
        _msg = "等待超时";
        var recvComplete = false;
        var data = new List<byte>();
        var waitRecv = new AutoResetEvent(false);
        var reCount = 0;
        do
        {
            var recvOk = false;
            _ = Task.Run(async () =>
            {
                var res = await _client.ReceiveAsync(arr, CancellationToken.None);
                var tmp = new byte[res.Count];
                Array.Copy(arr, 0, tmp, 0, res.Count);
                data.AddRange(tmp);
                if (res.EndOfMessage)
                {
                    recvComplete = true;
                    CheckResult(data.ToArray(), id);
                }

                recvOk = true;
                waitRecv.Set();
            });
            waitRecv.WaitOne(2000);
            if (!recvOk)
            {
                reCount++;
                if (reCount > 3) break;
            }
        } while (!recvComplete);

        _autoResetEvent.WaitOne(8000);
        Console.WriteLine(_msg);
        Assert.AreEqual(_sign, true);
    }

    private void SendData()
    {
        var buffer = MessagePackSerializer.Serialize(_jsonRpcRequest);
        _ = Task.Run(() => _client.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None)
            .ConfigureAwait(false));
    }

    private void CheckResult(byte[] data, string id)
    {
        var dic = MessagePackSerializer.Deserialize<Dictionary<string, object>>(data);
        var str = MessagePackSerializer.SerializeToJson(dic);
        if (dic.TryGetValue("error", out var value))
        {
            var error = MessagePackSerializer.SerializeToJson(value);
            var e = JsonConvert.DeserializeObject<Dictionary<string, object>>(error);
            if (!e.ContainsKey("code") || !e.ContainsKey("message"))
            {
                _msg = $"返回数据的格式不正确:{str}";
                _autoResetEvent.Set();
                return;
            }

            _sign = true;
            _msg = e["message"].ToString();
            _autoResetEvent.Set();
            return;
        }

        if (!dic.ContainsKey("jsonrpc")
            || !dic.ContainsKey("result")
            || !dic.ContainsKey("id")
            || dic["jsonrpc"].ToString() != "2.0"
            || dic["id"].ToString() != id)
        {
            _msg = $"返回数据的格式不正确:{str}";
            _autoResetEvent.Set();
            return;
        }

        _sign = true;
        _msg = $"测试成功！{str}";
        _autoResetEvent.Set();
    }
}