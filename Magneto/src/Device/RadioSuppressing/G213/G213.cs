using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.G213;

public partial class G213 : DeviceBase
{
    private readonly ConcurrentQueue<CommandFrame> _sendDataCache = new();
    private DateTime _preGetStatusTime = DateTime.Now.AddSeconds(-5);
    private volatile bool _running;
    private CancellationTokenSource _sendDataAsyncCts;
    private Task _sendDataAsyncTask;
    private CancellationTokenSource _tcpDataProcessCts;
    private Task _tcpDataProcessTask;
    private UdpClient _udpServer;

    public G213(Guid deviceId) : base(deviceId)
    {
    }

    #region 重写基类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        // 首先调用基类方法
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            //检查非托管资源并释放
            ReleaseResource();
            //初始化用于和设备通信的TCP和UDP套接字
            InitSocket();
            //初始化线程
            InitTasks();
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _running = true;
        SendSuppressParameters();
    }

    public override void Stop()
    {
        _running = false;
        StopSuppress();
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResource();
    }

    #endregion
}