using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.DC7530MOB_5G.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7530MOB_5G;

public partial class Dc7530Mob5G : DeviceBase
{
    private IClient _client;
    private Task _cmdSendTask;
    private CancellationTokenSource _cmdSendTokenSource;

    public Dc7530Mob5G(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (result)
        {
            ReleaseResource();
            _client = GetClient();
            result = _client.Init(new[] { UseChannel1, UseChannel2, UseChannel3 }, out var err);
            if (!result)
            {
                Trace.WriteLine($"初始化DC7530MOB_5G设备失败，失败原因：{err}");
                return false;
            }

            _client.ConnectionChanged += Client_ConnectionChanged;
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        StartCmdSendTasks();
    }

    public override void Stop()
    {
        StopCmdSendTasks();
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResource();
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResource()
    {
        // 停止线程
        StopCmdSendTasks();
        if (_client != null)
        {
            _client.ConnectionChanged -= Client_ConnectionChanged;
            _client.Dispose();
        }
    }

    private void Client_ConnectionChanged(object sender, bool e)
    {
        if (e) return;
        // 连接中断
        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }
}