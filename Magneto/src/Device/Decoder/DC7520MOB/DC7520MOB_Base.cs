using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.DC7520MOB.IO;

namespace Magneto.Device.DC7520MOB;

public partial class Dc7520Mob
{
    #region 释放资源

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResource()
    {
        // 停止线程
        StopCmdSendTasks();
        if (_client != null)
        {
            _client.ConnectionChanged -= Client_ConnectionChanged;
            _client.Dispose();
            _client = null;
        }
    }

    #endregion

    #region 初始并启动化线程

    /// <summary>
    ///     创建客户端对象
    /// </summary>
    /// <returns>Client客户端对象</returns>
    private IClient GetClient()
    {
        IClient client = null;
        switch (NetType)
        {
            case "TCP":
                client = new TcpClient(Ip, Port);
                break;
            case "UDP":
                client = new UdpClient(Ip, Port);
                break;
            case "串口":
                client = new SerialPortClient(Com, Baudrate);
                break;
        }

        return client;
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void StartCmdSendTasks()
    {
        _cmdSendTokenSource = new CancellationTokenSource();
        _cmdSendTask = new Task(ExecuteSendCmd, _cmdSendTokenSource.Token);
        _cmdSendTask.Start();
    }

    private void StopCmdSendTasks()
    {
        Utils.CancelTask(_cmdSendTask, _cmdSendTokenSource);
    }

    #endregion

    #region 指令发送与数据读取

    private bool SendCmds(string[] cmds, out List<string> recv)
    {
        return _client.SendCommands(cmds, out recv);
    }

    private void SendCmd(string cmd)
    {
        _client.SendCommand(cmd);
    }

    private bool SendSyncCmd(string cmd, CancellationToken token, out string recv)
    {
        var (success, data) = _client.SendCommandAsync(cmd, token).ConfigureAwait(false).GetAwaiter().GetResult();
        recv = data;
        return success;
    }

    private bool SendCmds(string[] cmds, CancellationToken token, out List<string> recv)
    {
        var (success, datas) = _client.SendCommandsAsync(cmds, token).ConfigureAwait(false).GetAwaiter().GetResult();
        recv = datas;
        return success;
    }

    #endregion
}