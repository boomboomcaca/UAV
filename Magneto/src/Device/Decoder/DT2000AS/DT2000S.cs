using System;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.DT2000AS.API;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT2000AS;

public partial class Dt2000As : DeviceBase
{
    private static bool _isInit; // just only onece
    private CancellationTokenSource _heartBeatCts;

    /// <summary>
    ///     心跳线程
    /// </summary>
    private Task _heartBeatTask;

    private CancellationTokenSource _readDataCts;
    private Task _readDataTask;

    /// <summary>
    ///     设备当前状态 0=故障  1=待机   2=工作
    /// </summary>
    private volatile int _workState;

    public Dt2000As(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var ret = base.Initialized(device);
        if (ret)
        {
            var count = Rx3GInterface.GetDeviceNum();
            ret = count != -1;
            if (!ret) throw new Exception("板卡未连接");
            if (!_isInit)
            {
                Rx3GInterface.RX3GInit();
                _isInit = true;
                Thread.Sleep(50);
            }

            Rx3GInterface.set_device_gain(50);
            Rx3GInterface.ResetScanList();
            Thread.Sleep(50);
            Rx3GInterface.ScanFreqConfig(0, GetChar(Gsm), GetChar(Cdma1X), GetChar(Evdo), GetChar(Wcdma),
                GetChar(TdScdma), GetChar(Lte), GetChar(Lte), GetChar(Nr, true), (char)0);
            if (Nr)
            {
                Add_5GScan_FreqBand((long)758e6, (long)798e6); // n28  广电700M 40M
                Add_5GScan_FreqBand((long)2110e6, (long)2170e6); // n1  电信联通
                Add_5GScan_FreqBand((long)2515e6, (long)2615e6); // n41 移动
                Add_5GScan_FreqBand((long)3400e6, (long)3600e6); // n78 电信联通
                Add_5GScan_FreqBand((long)4800e6, (long)4900e6); // n79   移动室内覆盖
            }

            _workState = Rx3GInterface.get_state();
            _heartBeatCts = new CancellationTokenSource();
            _heartBeatTask = new Task(HeartBeat, _heartBeatCts.Token);
            _heartBeatTask.Start();
        }

        return ret;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        var status = false;
        var deviceNum = Rx3GInterface.GetDeviceNum();
        if (deviceNum > 0)
        {
            var state = Rx3GInterface.get_state();
            if (state == 2)
            {
                Rx3GInterface.stop_scan();
                Thread.Sleep(1000);
            }

            status = state != 0;
        }

        if (!status)
        {
            Rx3GInterface.RX3GInit();
            Thread.Sleep(50);
            Rx3GInterface.set_device_gain(50);
        }

        Rx3GInterface.ResetScanList();
        Thread.Sleep(50);
        Rx3GInterface.ScanFreqConfig(0, GetChar(Gsm), GetChar(Cdma1X), GetChar(Evdo), GetChar(Wcdma),
            GetChar(TdScdma), GetChar(Lte), GetChar(Lte), GetChar(Nr, true), (char)0);
        if (Nr)
        {
            Add_5GScan_FreqBand((long)758e6, (long)798e6); // n28  广电700M 40M
            Add_5GScan_FreqBand((long)2110e6, (long)2170e6); // n1  电信联通
            Add_5GScan_FreqBand((long)2515e6, (long)2615e6); // n41 移动
            Add_5GScan_FreqBand((long)3400e6, (long)3600e6); // n78 电信联通
            Add_5GScan_FreqBand((long)4800e6, (long)4900e6); // n79   移动室内覆盖
        }

        Rx3GInterface.start_scan();
        _readDataCts = new CancellationTokenSource();
        _readDataTask = new Task(ProcessData, _readDataCts.Token);
        _readDataTask.Start();
    }

    public override void Stop()
    {
        base.Stop();
        Utils.CancelTask(_readDataTask, _readDataCts);
        Thread.Sleep(200);
        Rx3GInterface.close_device();
        _isInit = false;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        Utils.CancelTask(_readDataTask, _readDataCts);
        Utils.CancelTask(_heartBeatTask, _heartBeatCts);
        Thread.Sleep(200);
        try
        {
            Rx3GInterface.close_device();
        }
        catch (Exception)
        {
        }

        base.Dispose();
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

    private char GetChar(bool b, bool isNr = false)
    {
        if (isNr)
            return b ? (char)2 : (char)0;
        return b ? (char)1 : (char)0;
    }

    private void Add_5GScan_FreqBand(long lowf, long upperf)
    {
        Rx3GInterface.Add_5GScan_FreqBand(lowf, upperf);
    }
}