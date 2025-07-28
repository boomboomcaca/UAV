using System;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Http;
using Magneto.Core.Define;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.Contract;

public class CloudClientTest
{
    private readonly AutoResetEvent _autoResetEvent = new(false);

    [OneTimeSetUp]
    public void Setup()
    {
        HttpHelper.Instance.Initialized("192.168.102.191", 10000);
        CloudClient.Instance.GetCloudTokenAsync("admin", "123456").ConfigureAwait(true).GetAwaiter().GetResult();
        // Thread.Sleep(1000);
    }

    [Test]
    [TestCase("40002")]
    public void GetCrondTaskTest(string edgeId)
    {
        var sign = false;
        var message = string.Empty;
        _ = Task.Run(async () =>
        {
            var str = await CloudClient.Instance.GetCrondTasksAsync(edgeId, new DateTime(1970, 1, 1));
            if (string.IsNullOrEmpty(str))
            {
                message = "未查询到计划";
                sign = false;
            }
            else
            {
                try
                {
                    JsonConvert.DeserializeObject<BatchedCrondInfo[]>(str);
                    sign = true;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    sign = false;
                }
            }

            _autoResetEvent.Set();
        });
        _autoResetEvent.WaitOne(10000);
        if (!sign) Assert.Fail($"Get {edgeId} Plan Failed:{message}");
    }

    [Test]
    [TestCase(88, 108)]
    // [TestCase(89)]
    public void GetStationSignalsInfoTest(double startFreq, double? stopFreq)
    {
        var sign = false;
        var message = string.Empty;
        _ = Task.Run(async () =>
        {
            try
            {
                var arr = await CloudClient.Instance.GetStationSignalsInfoAsync(startFreq, stopFreq);
                sign = true;
                if (arr == null)
                {
                    message = "未查询到计划";
                    sign = false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                sign = false;
            }

            _autoResetEvent.Set();
        });
        _autoResetEvent.WaitOne(10000);
        if (!sign) Assert.Fail($"Get {startFreq}{stopFreq} StationSignalsInfo Failed:{message}");
    }
}