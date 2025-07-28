using System;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD100;

public partial class Ebd100
{
    #region 数值转换

    private byte SplitIf(string value)
    {
        byte i = 0;
        switch (value)
        {
            case "10.7":
                i = 0;
                break;
            case "21.4":
                i = 1;
                break;
            default:
                i = 0;
                break;
        }

        return i;
    }

    private byte SplitBandWidth(string value)
    {
        byte i = 0;
        switch (value)
        {
            case "1":
                i = 0;
                break;
            case "2.5":
                i = 1;
                break;
            case "8":
                i = 2;
                break;
            case "15":
                i = 3;
                break;
            case "100":
                i = 4;
                break;
            default:
                i = 3;
                break;
        }

        return i;
    }

    private byte SplitInterTime(string value)
    {
        byte i = 0;
        switch (value)
        {
            case "0.1":
                i = 0;
                break;
            case "0.2":
                i = 1;
                break;
            case "0.5":
                i = 2;
                break;
            case "1":
                i = 3;
                break;
            case "2":
                i = 4;
                break;
            case "5":
                i = 5;
                break;
            default:
                i = 1;
                break;
        }

        return i;
    }

    #endregion

    #region 线程

    /// <summary>
    ///     心跳检测线程
    /// </summary>
    private void KeepAlive()
    {
        while (!_thdHeartBeatTokenSource.IsCancellationRequested)
        {
            // 心跳检测
            // 连上以后会自动发送数据过来，不需要主动发送信息
            if (DateTime.Now.Subtract(_lastGetDataTime).TotalMilliseconds > 20000) //20s超时
            {
                SendMessage(new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                });
                break;
            }

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    ///     罗盘询问线程-5s询问一次
    /// </summary>
    private void ScanCompass()
    {
        while (!_thdScanCompassTokenSource.IsCancellationRequested)
        {
            SendCmd(Ebd100Command.QueryCompass);
            Thread.Sleep(5000);
        }
    }

    #endregion 线程
}