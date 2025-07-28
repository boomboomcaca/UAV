using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EthAntController;

public partial class EthAntController : AntennaControllerBase
{
    /// <summary>
    ///     控制流网络套接字
    /// </summary>
    private Socket _socket;

    public EthAntController(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (!result) return false;
        try
        {
            InitNetworks();
            // SetHeartBeat(_socket);
        }
        catch (SocketException)
        {
            return false;
        }

        return true;
    }

    public override void Dispose()
    {
        ReleaseSocket();
        base.Dispose();
    }

    public override bool SendControlCode(string code)
    {
        try
        {
            SendCmd(code);
            return true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine(ex.ToString());
#endif
            return false;
        }
    }

    private void SendCmd(string code)
    {
        if (_socket?.Connected != true)
        {
            ReleaseSocket();
            InitNetworks();
        }

        const string pattern = @"^((0[xX])?[0-9A-Fa-f]{1,2}\|)*(0[xX])?[0-9A-Fa-f]{1,2}$"; // 只匹配以“|”分隔的16进制数据，已考虑大小写情况
        var reg = new Regex(pattern, RegexOptions.CultureInvariant |
                                     RegexOptions.IgnorePatternWhitespace);
        if (!reg.IsMatch(code)) throw new FormatException("天线控制码格式有误");
        var codeArray = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        var commands = new byte[codeArray.Length];
        for (var index = 0; index < codeArray.Length; ++index)
            commands[index] = Convert.ToByte(codeArray[index], 16); // 无论字符串是否带有“0x”前缀，都可以转换为对应的16进制数据
        var sign = false;
        for (var i = 0; i < 3; i++)
            try
            {
                // using (var stream = new NetworkStream(_socket))
                // {
                //     using (var writer = new BinaryWriter(stream))
                //     {
                //         writer.Write(commands, 0, commands.Length);
                //         writer.Flush();
                //         sign = true;
                //         break;
                //     }
                // }
                _socket?.Send(commands);
                sign = true;
            }
            catch
            {
                ReleaseSocket();
                InitNetworks();
            }

        if (!sign)
        {
            var info = new SDataMessage
            {
                LogType = LogType.Warning,
                ErrorCode = (int)InternalMessageType.DeviceRestart,
                Description = DeviceId.ToString(),
                Detail = DeviceInfo.DisplayName
            };
            SendMessage(info);
        }
    }

    protected override void KeepAlive(object connObject)
    {
        var ping = new Ping();
        var sign = false;
        while (!Disposed)
        {
            for (var i = 0; i < 3; i++)
            {
                var reply = ping.Send(Ip, 1000);
                if (reply!.Status == IPStatus.Success)
                {
                    sign = true;
                    break;
                }

                Thread.Sleep(1000);
            }

            if (!sign) break;
        }

        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }
}