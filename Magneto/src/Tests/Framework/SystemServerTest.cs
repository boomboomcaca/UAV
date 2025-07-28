using System;
using System.Threading.Tasks;
using Magneto.CCC;
using Magneto.Contract;
using Magneto.Contract.Http;
using Magneto.Core;
using Magneto.Core.Business;
using MessagePack;
using NUnit.Framework;
using StreamJsonRpc;

namespace Tests.Core;

/*
   进行这个测试需要在../../bin/Tests/Debug/configuration/文件夹下放置相关配置文件
*/
[TestFixture]
public class SystemServerTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        if (!HttpHelper.Instance.IsInitialized)
        {
            HttpHelper.Instance.Initialized("192.168.102.16", 12001);
            CloudClient.Instance.GetCloudTokenAsync("admin", "123456").ConfigureAwait(true).GetAwaiter().GetResult();
        }

        _server = new ControlServer(_client);
        Manager.Instance.Initialized();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _server?.Dispose();
        _server = null;
    }

    private readonly ClientInfo _client = new("127.0.0.1", 1);
    private ControlServer _server;

    [Test]
    [TestCase("myName", 18)]
    public async Task SayHelloAsync(string name, int age)
    {
        var result = await _server.SayHelloAsync(name, age).ConfigureAwait(false);
        Assert.AreEqual(result?.Message, $"您好， {name}！,{age}");
    }

    [Test]
    public async Task QueryAllInfoAsync()
    {
        var result = await _server.QueryAllInfoAsync().ConfigureAwait(false);
        if (result.DeviceInfo == null || result.DriverInfo == null || result.StationInfo == null) Assert.Fail("未查询到结果");
        Assert.Pass();
    }

    [Test]
    public async Task QueryDeviceAsync()
    {
        var result = await _server.QueryDeviceAsync().ConfigureAwait(false);
        if (result.Devices == null) Assert.Fail("未查询到结果");
    }

    [Test]
    public async Task QueryDriverAsync()
    {
        var result = await _server.QueryDriverAsync().ConfigureAwait(false);
        if (result.Drivers == null) Assert.Fail("未查询到结果");
    }

    [Test]
    public async Task QueryStationAsync()
    {
        var result = await _server.QueryStationAsync().ConfigureAwait(false);
        if (result.Station == null) Assert.Fail("未查询到结果");
    }

    [Test]
    [Ignore("重启APP将导致测试无法继续")]
    public async Task TestRestartAppAsync()
    {
        try
        {
            await _server.RestartAppAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex is LocalRpcException)
                Assert.Pass(ex.Message);
            else
                Assert.Fail();
        }

        Assert.Pass();
    }

    [Test]
    public async Task TestUpdateConfigAsync()
    {
        try
        {
            await _server.UpdateConfigAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex is LocalRpcException)
                Assert.Pass(ex.Message);
            else
                Assert.Fail();
        }

        Assert.Pass();
    }

    [Test]
    public async Task TestUpdateCrontabAsync()
    {
        try
        {
            await _server.UpdateCronTabAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex is LocalRpcException)
                Assert.Pass(ex.Message);
            else
                Assert.Fail();
        }

        Assert.Pass();
    }

    [Test]
    public async Task TestQueryFeaturesAsync()
    {
        try
        {
            var res = await _server.QueryFeaturesAsync().ConfigureAwait(false);
            MessagePackSerializer.SerializeToJson(res);
        }
        catch (Exception ex)
        {
            if (ex is LocalRpcException)
                Assert.Pass(ex.Message);
            else
                Assert.Fail();
        }

        Assert.Pass();
    }
    // [Test]
    // public void TestSysteControl()
    // {
    //     try
    //     {
    //         Magneto.Core.Utils.SystemControl.ConfigRsync(Magneto.Protocol.Define.RsyncConfigType.Start);
    //     }
    //     catch
    //     {
    //         Assert.Fail();
    //     }
    // }
    //[Test]
    //public async Task TestGetDir()
    //{
    //    var result = await _server.QueryDirectoryAsync("/20211201/001c49bb-4ea4-4a1e-9951-e76da9a543a8").ConfigureAwait(false);
    //    if (result.Directory == null || result.FileList == null)
    //    {
    //        Assert.Fail("未查询到结果");
    //    }
    //    Assert.Pass();
    //}
}