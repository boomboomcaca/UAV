using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.E3238S;

public partial class E3238S
{
    #region 释放资源

    /// <summary>
    ///     清理所有非托管资源
    /// </summary>
    private void ReleaseResources()
    {
        Utils.CancelTask(_receiveDataTask, _receiveDataTokenSource);
        Utils.CancelTask(_paseDataTask, _paseDataTaskTokenSource);
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     发送命令
    /// </summary>
    /// <param name="command"></param>
    private void SendCommand(string command)
    {
        var socketTag = E3238SCommandTag.E3238SSocketCommand;
        var nLength = BitConverter.GetBytes(command.Length);
        Array.Reverse(nLength);
        var tagbytes = BitConverter.GetBytes(socketTag);
        Array.Reverse(tagbytes);
        var commandSendBytes = Encoding.ASCII.GetBytes(command.Trim());
        var data = new byte[tagbytes.Length + commandSendBytes.Length + nLength.Length];
        nLength.CopyTo(data, 0);
        tagbytes.CopyTo(data, nLength.Length);
        commandSendBytes.CopyTo(data, nLength.Length + tagbytes.Length);
        _socket.Send(data);
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化频段扫描参数
    /// </summary>
    private void InitScanParms()
    {
        SendCommand("searchType:GeneralSearch");
        //释放调谐器 
        SendCommand("tunerLock:Off");
        //关闭时域数据
        SendCommand("spewRawTimeData:Off");
        //获取频域数据
        SendCommand("spewRawData:On");
    }

    /// <summary>
    ///     初始化单频测量参数
    /// </summary>
    /// <returns></returns>
    private void InitFixFqParms()
    {
        //将扫描状态设置为Generalsearch
        SendCommand("searchType:GeneralSearch");
        //激活扫描该段状态
        SendCommand("bandStatus:Active");
        //锁定调谐器，以便获取声音数据
        SendCommand("tunerLock:On");
        //打开声音
        SendCommand("*audioParms:0 100 0 0 0 0 1750 0");
        //获取频域数据
        SendCommand("spewRawData:On");
        //由于获取的IQ 数据解析不正确，经讨论此处屏蔽IQ数据
        SendCommand("spewRawTimeData:Off");
    }

    /// <summary>
    ///     初始化E3238S连接
    /// </summary>
    private void InitSocket()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        _socket.Connect(Ip, TcpPort);
    }

    /// <summary>
    ///     初始化设备E3238S (备注:设置默认参数)
    /// </summary>
    private void InitE3238S()
    {
        //关闭所有声音
        SendCommand("*audioParms:1 100 0 0 0 0 1750 0");
        //禁止弹出错误消息框
        SendCommand("errorDialogBoxes:off");
        //设置波形系数为9
        SendCommand("shapeFactor:9");
    }

    /// <summary>
    ///     初始化所有线程
    /// </summary>
    private void InitAllTask()
    {
        _receiveDataTokenSource = new CancellationTokenSource();
        _receiveDataTask = new Task(ReceiveDataThread, _receiveDataTokenSource.Token);
        _receiveDataTask.Start();
        _paseDataTaskTokenSource = new CancellationTokenSource();
        _paseDataTask = new Task(PaseDataThread, _paseDataTaskTokenSource.Token);
        _paseDataTask.Start();
    }

    #endregion
}