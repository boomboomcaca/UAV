using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualCompass;

public partial class VirtualCompass : DeviceBase
{
    private bool _isRunning;

    public VirtualCompass(Guid deviceId) : base(deviceId)
    {
    }

    /// <summary>
    ///     初始化设备模块
    /// </summary>
    /// <param name="device">模块信息</param>
    /// <returns>初始化成功返回True，否则返回False</returns>
    public override bool Initialized(ModuleInfo device)
    {
        _isRunning = true;
        var result = base.Initialized(device);
        if (!result) return false;
        InitStatusAndMembers();
        InitCompassSender();
        return true;
    }

    public override void Dispose()
    {
        _isRunning = false;
        base.Dispose();
    }

    /// <summary>
    ///     初始化成员和状态
    /// </summary>
    private void InitStatusAndMembers()
    {
        _latestCompass = new SDataCompass
        {
            Heading = (Degree % 360 + 360) % 360
        };
    }

    /// <summary>
    ///     初始化罗盘数据发送线程
    /// </summary>
    private void InitCompassSender()
    {
        if (_compassSender?.IsAlive == true) return;
        _compassSender = new Thread(SendCompass)
        {
            Name = "virtual_compass_sender",
            IsBackground = true
        };
        _compassSender.Start();
    }

    #region 发送数据

    /// <summary>
    ///     罗盘数据发送线程
    /// </summary>
    private void SendCompass()
    {
        while (_isRunning)
        {
            if (IsMove)
            {
                _latestCompass.Heading += Step;
                if (_latestCompass.Heading >= 360) _latestCompass.Heading = 0f;
                _latestCompass.Heading -= (float)Math.Floor(_latestCompass.Heading / 360.0) * 360;
            }

            SendMessageData(new List<object> { _latestCompass });
            Thread.Sleep(1000);
        }
    }

    #endregion

    #region 成员变量

    private SDataCompass _latestCompass; // 缓存最新的罗盘数据
    private Thread _compassSender; //	电子罗盘数据发送线程

    #endregion
}