using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.N9030A;

public partial class N9030A
{
    #region 释放资源

    /// <summary>
    ///     释放连接与数据接收线程
    /// </summary>
    private void ClearResource()
    {
        IsConnect = false;
        Utils.CancelTask(_dataTask, _dataTokenSource);
        Utils.CloseSocket(Socket);
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化Socket
    /// </summary>
    /// <returns></returns>
    protected void InitNetFeatures()
    {
        //初始化网络连接
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Netport);
        if (Socket == null)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //_socket.NoDelay = true;
            // TODO 设置超时时间
            //_socket.ReceiveTimeout = 10000;
            //_socket.SendTimeout = 10000;
        }
        else
        {
            if (Socket.Connected)
            {
                Socket.Close();
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
        }

        Socket.Connect(ipEndPoint);
        IsConnect = true;
        if (IsConnect)
        {
            //设置频谱仪为频谱状态
            SendCommand("INST:SEL SA");
            //设置连续扫描方式
            SendCommand("INIT:CONT 1");
            //设置自动检波
            SendCommand("DET:FUNC AUTO");
            //关闭频谱仪自动校验（避免测试过程中，与socket通讯中断）
            SendCommand("CAL:AUTO OFF");
            // 设置平均类型
            SendCommand(":AVER:STAT 1");
            SendCommand(":AVER:TYPE:AUTO ON");
            SendCommand(":AVER:COUN 5");
        }
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitTasks()
    {
        _dataTokenSource = new CancellationTokenSource();
        _dataTask = new Task(CollectData, CurFeature, _dataTokenSource.Token);
        _dataTask.Start();
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     发送命令
    /// </summary>
    protected readonly object LockCmd = new();

    protected string SendCommand(string command, int receiveDataTimeOut = 0)
    {
        lock (LockCmd)
        {
            if (IsConnect)
            {
                var bytes = Encoding.ASCII.GetBytes(command + "\r\n");
                Socket.Send(bytes, 0, bytes.Length, SocketFlags.None);
            }

            if (command.IndexOf("?", StringComparison.Ordinal) > 0)
                return ReadData(receiveDataTimeOut);
            return null;
        }
    }

    #endregion
}