using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class Em200
{
    #region 指令发送

    private readonly object _lockObject = new();
    private readonly byte[] _tcpRecvBuffer = new byte[1024 * 1024]; //避免该方法被调用时频繁申请内存

    /// <summary>
    ///     发送查询命令
    /// </summary>
    /// <param name="cmd"></param>
    private void SendCmd(string cmd)
    {
        Console.WriteLine($"==> {cmd}");
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        _cmdSocket.Send(buffer);
    }

    /// <summary>
    ///     用于发送查询类指令并获取查询结果
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>查询结果</returns>
    private string SendSyncCmd(string cmd)
    {
        lock (_lockObject)
        {
            var sendBuffer = Encoding.ASCII.GetBytes(cmd + "\n");
            _cmdSocket.Send(sendBuffer);
            var result = string.Empty;
            var recvCount = _cmdSocket.Receive(_tcpRecvBuffer, SocketFlags.None);
            if (recvCount > 0)
            {
                if (_tcpRecvBuffer[recvCount - 1] == '\n')
                    result = Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount - 1);
                else
                    result = Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount);
            }

            return result;
        }
    }

    #endregion

    #region IFBandwidth/DemMode

    /// <summary>
    ///     单位 kHz
    /// </summary>
    /// <param name="dstIfBandwidth"></param>
    private void SetIfBandwidth(double dstIfBandwidth)
    {
        if (dstIfBandwidth > 9)
        {
            //当解调模式为CW,USB,LSB时,解调带宽只能<=9kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode is "CW" or "USB" or "LSB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }
        else if (dstIfBandwidth < 0.6)
        {
            //当解调带宽为ISB时，解调带宽只能 >= 0.6 kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode == "ISB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:BAND {dstIfBandwidth} kHz");
    }

    private void SetDemodulation(Modulation dstDemMode)
    {
        if (dstDemMode is Modulation.Cw or Modulation.Usb or Modulation.Lsb or Modulation.Isb)
        {
            //当解调带宽 > 9kHz时，解调带宽不能设置为CW,USB,LSB
            //当解调带宽 < 0.6 kHz时，解调带宽不能设置为ISB
            var strIfBandwidth = SendSyncCmd("SENS:BAND?");
            var ifBandwidth = double.Parse(strIfBandwidth) / 1000;
            if (dstDemMode == Modulation.Isb)
            {
                if (ifBandwidth < 0.6)
                {
                    SendCmd("SENS:BAND 600 hz");
                    Thread.Sleep(10);
                }
            }
            else if (ifBandwidth > 9)
            {
                SendCmd("SENS:BAND 9 kHz");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:DEM {dstDemMode}");
    }

    #endregion
}