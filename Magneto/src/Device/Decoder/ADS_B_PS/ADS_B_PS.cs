using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.ADS_B_PS;

public partial class AdsBPs : DeviceBase
{
    private readonly ConcurrentQueue<string> _dataCache;
    private readonly Socket _dataSocket = null;
    private CancellationTokenSource _heartBeatCts;
    private Task _heartBeatTask;
    private CancellationTokenSource _parseDataCts;
    private Task _parseDataTask;
    private CancellationTokenSource _readDataCts;
    private Task _readDataTask;

    /// <summary>
    ///     设备TCP连接套接字
    /// </summary>
    private Socket _socket;

    ///// <summary>
    ///// 第一包设备状态数据
    ///// </summary>
    //private PingStationStatus _firstStatusData = null;
    public AdsBPs(Guid deviceId) : base(deviceId)
    {
        _dataCache = new ConcurrentQueue<string>();
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    /// <param name="moduleInfo">模块信息</param>
    /// <returns>true=成功；false=失败</returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            ReleaseResources();
            result = InitNetworks();
            _heartBeatCts = new CancellationTokenSource();
            _heartBeatTask = new Task(HeartBeat, _heartBeatCts.Token);
            _heartBeatTask.Start();
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _dataCache.Clear();
        InitTasks();
    }

    public override void Stop()
    {
        base.Stop();
        ReleaseTasks();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResources();
    }

    private void HeartBeat()
    {
        while (_heartBeatCts?.IsCancellationRequested == false)
            try
            {
                var b = IsSocketConnected();
                if (!b)
                {
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                    break;
                }

                Thread.Sleep(1000);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private bool IsSocketConnected(int maxRetry = 3)
    {
        if (TcpAlive) return IsSocketConnected(_socket, maxRetry);
        if (maxRetry == 0) return false;
        try
        {
            using var ping = new Ping();
            var reply = ping.Send(IPAddress.Parse(DeviceIp));
            if (reply is { Status: IPStatus.Success }) return true;
        }
        catch (PingException)
        {
            // 理论上此处不会抛出任何异常
            // 异常:
            // T:System.Net.NetworkInformation.NetworkInformationException: Win32 函数 GetTcpTable 失败。
        }

        Thread.Sleep(1000);
        return IsSocketConnected(--maxRetry);
    }
}