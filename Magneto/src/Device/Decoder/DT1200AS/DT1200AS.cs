using System;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.DT1200AS.API;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT1200AS;

public partial class Dt1200As : DeviceBase
{
    private static bool _isInit; // just only onece

    //private bool _deviceError;
    private CancellationTokenSource _heartBeatCts;

    /// <summary>
    ///     心跳线程
    /// </summary>
    private Task _heartBeatTask;

    /// <summary>
    ///     设备上一个状态
    /// </summary>
    private int _preWorkState;

    private CancellationTokenSource _readDataCts;

    /// <summary>
    ///     读取数据线程
    /// </summary>
    private Task _readDataTask;

    /// <summary>
    ///     控制线程退出
    /// </summary>
    private bool _run;

    /// <summary>
    ///     设备当前状态 0=故障  1=待机   2=工作
    /// </summary>
    private int _workState;

    public Dt1200As(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var ret = base.Initialized(device);
        if (ret)
        {
            var count = Rx3GInterface.GetDeviceNum();
            ret = count != -1;
            if (ret == false) throw new Exception("板卡未连接");
            if (_isInit == false)
            {
                Rx3GInterface.RX3GInit();
                _isInit = true;
                Thread.Sleep(50);
            }

            Rx3GInterface.ResetScanList();
            Thread.Sleep(50);
            if (_segmentType == "中国频段")
                AddChinaFreqListAll();
            else
                AddBorderFreqListAll();
            _heartBeatCts = new CancellationTokenSource();
            _heartBeatTask = new Task(HeartBeat, _heartBeatCts.Token);
            _heartBeatTask.Start();
        }

        return ret;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        try
        {
            Rx3GInterface.start_scan();
        }
        catch (Exception)
        {
            throw new Exception("启动扫描失败");
        }

        _run = true;
        _readDataCts = new CancellationTokenSource();
        _readDataTask = new Task(ReadData, _readDataCts.Token);
        _readDataTask.Start();
    }

    public override void Stop()
    {
        base.Stop();
        _run = false;
        Utils.CancelTask(_readDataTask, _readDataCts);
        Thread.Sleep(200);
        Rx3GInterface.stop_scan();
        _isInit = false;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        _run = false;
        Thread.Sleep(200);
        Utils.CancelTask(_readDataTask, _readDataCts);
        Utils.CancelTask(_heartBeatTask, _heartBeatCts);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    // 心跳检测
    private void HeartBeat()
    {
        while (_heartBeatCts?.IsCancellationRequested == false)
        {
            try
            {
                _workState = Rx3GInterface.get_state();
                if (_workState == 0) // 设备故障
                {
                    Rx3GInterface.ResetScanList();
                    Thread.Sleep(100);
                    Rx3GInterface.stop_scan();
                    Thread.Sleep(100);
                    Rx3GInterface.close_device();
                    Thread.Sleep(100);
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                    return;
                }

                //if (_workState == 2)
                //{
                //    _deviceError = false;
                //}

                if (_preWorkState == 2)
                    if (_run && _workState != 2)
                        //_deviceError = true;
                        Rx3GInterface.stop_scan();
                //RX3GInterface.close_device();
                _preWorkState = _workState;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }

            Thread.Sleep(1000);
        }
    }
}