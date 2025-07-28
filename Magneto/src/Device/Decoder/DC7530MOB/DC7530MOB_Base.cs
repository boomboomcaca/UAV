using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.DC7530MOB.IO;

namespace Magneto.Device.DC7530MOB;

public partial class Dc7530Mob
{
    /// <summary>
    ///     提取AIS数据
    /// </summary>
    /// <param name="recv">接收到的数据</param>
    private string ExtractAisData(string recv)
    {
        var arr = recv.Split(new[] { "\r\n" }, StringSplitOptions.None);
        if (arr.Length <= 1) return recv;
        var sbStation = new StringBuilder();
        foreach (var s in arr)
        {
            var line = s;
            if (line.Contains("AIS:") || line.Contains("AIVDM") || line.Contains("AIVDO"))
            {
                if (line.Contains("!AIVDM")) // || aisItem.Contains("!AIVDO")),不解析本船信息
                {
                    if (line.Contains("AIS:")) line = line.Replace("AIS:", "");
                    _ais.Enqueue(line.Trim());
                }
            }
            else
            {
                line += "\r\n";
                sbStation.Append(line);
            }
        }

        return sbStation.ToString();
    }

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

    private void StartDataProcessTask()
    {
        /*
         * 吴德鹏修改于2020-7-31
         * 修改内容：
         *      针对“基站解码模块无法解调2G信号（示例无GSM模式），其他3G、4G信号可以正常解调”的问题
         * 硬件研发中心王凡提出的解决方案为：
         *      这是由于基站解码模块将模式锁定到了3G和4G，无法解调GSM信号，这是个别情况，目前不清楚原因。
         *      建议在设备驱动里增加下面的初始化指令用于设置模块网络搜索顺序为自动
         * 针对3个通道分别进行了设置
         */
        if (IsOldVersion)
        {
            if (UseChannel1) SendCmd("CH1:AT^SYSCFGEX=\"00\",3FFFFFFF,1,2,7FFFFFFFFFFFFFFF\r\n");
            if (UseChannel3) SendCmd("CH3:AT^SYSCFGEX=\"00\",3FFFFFFF,1,2,7FFFFFFFFFFFFFFF\r\n");
            if (UseChannel2) SendCmd("CH2:AT^SYSCFGEX=\"00\",3FFFFFFF,1,2,7FFFFFFFFFFFFFFF\r\n");
        }

        //基站信息数据接收线程
        _dataProcTokenSource = new CancellationTokenSource();
        _dataProcTask = new Task(DataProc, _dataProcTokenSource.Token);
        _dataProcTask.Start();
    }

    private void StopDataProcessTask()
    {
        Utils.CancelTask(_dataProcTask, _dataProcTokenSource);
    }

    private void StartCmdSendTasks()
    {
        if (UseChannel1 && IsOldVersion && (Gsm || Wcdma))
            SendCmd("CH1:AT^SYSCFGEX=\"00\",3FFFFFFF,1,2,7FFFFFFFFFFFFFFF\r\n");
        if (UseChannel3 && IsOldVersion && (Cdma1X || Evdo))
            SendCmd("CH3:AT^SYSCFGEX=\"00\",3FFFFFFF,1,2,7FFFFFFFFFFFFFFF\r\n");
        if (UseChannel2 && IsOldVersion && (Gsm || Wcdma || TdScdma || Lte))
            SendCmd("CH2:AT^SYSCFGEX=\"00\",3FFFFFFF,1,2,7FFFFFFFFFFFFFFF\r\n");
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