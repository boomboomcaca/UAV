using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.DC7530MOB.AIS;
using Magneto.Device.DC7530MOB.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7530MOB;

public partial class Dc7530Mob : DeviceBase
{
    private readonly ConcurrentQueue<string> _msgBuffer;

    /// <summary>
    ///     AIS 信息处理类
    /// </summary>
    private AisUtils _ais;

    private IClient _client;

    /// <summary>
    ///     指令发送线程
    /// </summary>
    private Task _cmdSendTask;

    private CancellationTokenSource _cmdSendTokenSource;

    /// <summary>
    ///     数据处理线程
    /// </summary>
    private Task _dataProcTask;

    private CancellationTokenSource _dataProcTokenSource;

    public Dc7530Mob(Guid deviceId) : base(deviceId)
    {
        _msgBuffer = new ConcurrentQueue<string>();
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (result)
        {
            ReleaseResource();
            _client = GetClient();
            _client.DataReceived += Client_DataReceived;
            result = _client.Init(new[] { UseChannel1, UseChannel2, UseChannel3 }, out var err);
            if (!result)
            {
                Trace.WriteLine($"初始化DC7530MOB设备失败，失败原因：{err}");
                return false;
            }

            _client.ConnectionChanged += Client_ConnectionChanged;
            _ais ??= new AisUtils();
            StartDataProcessTask(); // 启动数据解析线程
            _ais.Initilize();
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
        if (disposing) _msgBuffer.Clear();
        ReleaseResource();
        _ais?.Dispose();
    }

    private void Client_DataReceived(object sender, string e)
    {
        _msgBuffer.Enqueue(e);
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResource()
    {
        // 停止线程
        StopCmdSendTasks();
        // 停止数据解析线程
        StopDataProcessTask();
        if (_client != null)
        {
            _client.ConnectionChanged -= Client_ConnectionChanged;
            _client.DataReceived -= Client_DataReceived;
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