using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.N9917A;

public partial class N9917A
{
    #region 释放资源

    /// <summary>
    ///     释放连接与数据接收线程
    /// </summary>
    private void ClearResource()
    {
        Utils.CancelTask(_recvDataTask, _recvDataTokenSource);
        Utils.CloseSocket(_cmdSock);
    }

    #endregion

    #region 指令发送

    private void SendCmd(string cmd, int sleep = 50)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd + "\r\n");
        _cmdSock.Send(buffer);
        Thread.Sleep(sleep);
        buffer = null;
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitTasks()
    {
        _recvDataTokenSource = new CancellationTokenSource();
        _recvDataTask = new Task(SendMornitorData, _recvDataTokenSource.Token);
        _recvDataTask.Start();
    }

    private void InitFeatures()
    {
        //设置频谱仪为预设初始化状态
        SendCmd("SYST:PRES");
        //设置频谱仪工作模式为频谱分析模式，即SA模式
        SendCmd("INST:SEL \"SA\"");
        //设置连续扫描方式
        SendCmd("SWE:TYPE AUTO");
        //设置均值检波
        //SendCmd("DET:FUNC AVER");
        //将电平单位设置为dBuV
        SendCmd("AMPL:UNIT DBUV");
        //设置最快的扫描速度(1~5000)
        SendCmd("SWE:ACQ 1");
        //关闭频谱仪自动校验（避免测试过程中，与socket通讯中断）
        //SendCmd("CAL:AUTO OFF");//检查是否有该命令
        //连续扫描模式
        SendCmd("INIT:CONT 1");
    }

    private void InitSocket()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), TcpPort);
        _cmdSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cmdSock.NoDelay = false;
        _cmdSock.Connect(ep);
    }

    #endregion
}