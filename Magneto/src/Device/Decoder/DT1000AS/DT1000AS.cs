using System;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.DT1000AS.Driver;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT1000AS;

public partial class Dt1000As : DeviceBase
{
    private Receiver _receiver;

    public Dt1000As(Guid deviceId) : base(deviceId)
    {
    }

    /// <summary>
    ///     初始化模块
    /// </summary>
    /// <param name="moduleInfo">模块信息</param>
    /// <returns>true-成功；false-失败</returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var ret = base.Initialized(moduleInfo);
        if (ret)
        {
            _receiver = new Receiver(Com, BaudRate);
            _receiver.OnDataReceived += ReceiverOnDataReceived;
            ret = _receiver.Init();
        }

        return ret;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _receiver?.StartScan(new CellSearchRequest
        {
            Cmcc1800Search = Cmcc1800Search,
            Cmcc900Search = Cmcc900Search,
            Cucc1800Search = Cucc1800Search,
            Cucc900Search = Cucc900Search,
            Gsm1800Search = Gsm1800Search,
            Gsm900Search = Gsm900Search,
            GsmrSearch = GsmrSearch,
            MinRssiScan = MinRssiScan
        });
    }

    /// <summary>
    ///     停止
    /// </summary>
    /// <returns>true-成功；false-失败</returns>
    public override void Stop()
    {
        _receiver?.StopScan();
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_receiver != null)
        {
            if (_receiver.OnDataReceived != null) _receiver.OnDataReceived -= ReceiverOnDataReceived;
            _receiver.Dispose();
        }
    }
}