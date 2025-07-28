using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class AdsBUsb : DeviceBase
{
    /// <summary>
    ///     心跳线程
    /// </summary>
    private Task _heartBeatTask;

    private CancellationTokenSource _heartBeatTokenSource;

    /// <summary>
    ///     上次从串口读取到数据的时间，心跳检测使用
    /// </summary>
    private DateTime _lastGetDataTime = DateTime.Now;

    /// <summary>
    ///     通讯串口
    /// </summary>
    private SerialPort _serialPort;

    public AdsBUsb(Guid id) : base(id)
    {
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    /// <param name="moduleInfo">模块信息</param>
    /// <returns>true=成功；false=失败</returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        ReleaseResource();
        InitSerialPort();
        _lastGetDataTime = DateTime.Now;
        InitAllTask();
        return true;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        ReleaseResource();
        base.Dispose();
    }
}