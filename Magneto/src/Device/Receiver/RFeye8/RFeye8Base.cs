using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.RFeye8;

public partial class RFeye8
{
    private void InitResources()
    {
        //检查非托管资源并释放
        ReleaseResources();
        //初始化用于与设备通信的套接字
        InitNetworks();
        //初始化线程
        InitTasks();
        //初始化ITU计算类
        _itu = new Itu();
        _itu.Initialize();
        //初始化外接天线控制器，其故障状态不反馈为设备故障
        TryConnectToExtAntController();
        //初始成功后再启动心跳检查线程
        SetHeartBeat(_tcpSocket);
    }

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseTasks();
        ReleaseNetworks();
        ReleaseQueues();
        _itu?.Dispose();
    }

    private void ReleaseNetworks()
    {
        Utils.CloseSocket(_tcpSocket);
        Utils.CloseSocket(_gpsSocket);
        Utils.CloseSocket(_switchSocket);
    }

    private void ReleaseQueues()
    {
        _dataQueue?.Clear();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_dataCaptureTask, _dataCaptureTokenSource);
        Utils.CancelTask(_dataProcessTask, _dataProcessTokenSource);
        Utils.CancelTask(_gpsProcessTask, _gpsProcessTokenSource);
        Utils.CancelTask(_extAntControllerConnectTask, _extAntControllerConnectTokenSource);
        Utils.CancelTask(_freqsSwitchTask, _freqsSwitchTokenSource);
    }

    #endregion

    #region 外部天线切换单元

    private void TryConnectToExtAntController()
    {
        //若重连线程已存在则不再启动
        if (ExtAntControllerPort == -1 || _extAntControllerConnectTask?.IsCompleted == false) return;
        _extAntControllerConnectTokenSource = new CancellationTokenSource();
        _extAntControllerConnectTask = new Task(ExtAntControllerConnectPoc, _extAntControllerConnectTokenSource.Token);
        _extAntControllerConnectTask.Start();
    }

    private void ExtAntControllerConnectPoc()
    {
        while (_extAntControllerConnectTokenSource?.IsCancellationRequested == false)
        {
            try
            {
                lock (_switchSocketLock)
                {
                    _switchSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _switchSocket.Connect(ExtAntControllerIp, ExtAntControllerPort);
                }

                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _switchSocket = null;
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }

            Thread.Sleep(2000);
        }
    }

    #endregion

    #region 初始化

    private void InitNetworks()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _tcpSocket.Connect(ep);
        _tcpSocket.NoDelay = true;
        CreateAuthentication(_tcpSocket);
        if (!GpsSwitch) return;
        _gpsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _gpsSocket.Connect(ep);
        _gpsSocket.NoDelay = true;
        CreateAuthentication(_gpsSocket);
    }

    private void InitTasks()
    {
        _dataCaptureTokenSource = new CancellationTokenSource();
        _dataCaptureTask = new Task(DataCaptrueProc, _dataCaptureTokenSource.Token);
        _dataCaptureTask.Start();
        _dataProcessTokenSource = new CancellationTokenSource();
        _dataProcessTask = new Task(DataProcessProc, _dataProcessTokenSource.Token);
        _dataProcessTask.Start();
        if (!GpsSwitch) return;
        _gpsProcessTokenSource = new CancellationTokenSource();
        _gpsProcessTask = new Task(GpsProcessProc, _gpsProcessTokenSource.Token);
        _gpsProcessTask.Start();
    }

    #endregion
}