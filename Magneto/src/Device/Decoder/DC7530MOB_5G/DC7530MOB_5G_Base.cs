using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.DC7530MOB_5G.IO;

namespace Magneto.Device.DC7530MOB_5G;

public partial class Dc7530Mob5G
{
    #region 初始并启动化线程

    /// <summary>
    ///     创建客户端对象
    /// </summary>
    /// <returns>Client客户端对象</returns>
    private IClient GetClient()
    {
        var client = new TcpClient(Ip, Port);
        return client;
    }

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

    private bool SendSyncCmd(string cmd, out string recv)
    {
        return _client.SendSyncCmd(cmd, out recv);
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