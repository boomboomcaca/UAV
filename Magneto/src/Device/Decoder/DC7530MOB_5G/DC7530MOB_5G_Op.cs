using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Magneto.Protocol.Data;

namespace Magneto.Device.DC7530MOB_5G;

public partial class Dc7530Mob5G
{
    /// <summary>
    ///     执行命令发送
    /// </summary>
    private void ExecuteSendCmd()
    {
        var commandInfos = new List<DuplexCommandInfo>
        {
            new()
            {
                Channel = 1,
                NetworkStandard = NetworkStandard.Nr,
                Expression = p => p % 8 == 2,
                Enable = Nr && UseChannel1,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ($"CH1:AT^NETSCAN={Count},-125,4\r\n", CommandType.Nr)
                }
            },
            new()
            {
                Channel = 2,
                NetworkStandard = NetworkStandard.Gsm,
                Expression = p => p % 4 == 0,
                Enable = Gsm && UseChannel2,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ($"CH2:AT^NETSCAN={Count},-110,0\r\n", CommandType.Gsm)
                }
            },
            new()
            {
                Channel = 2,
                NetworkStandard = NetworkStandard.Lte,
                Expression = p => p % 4 == 1,
                Enable = Lte && UseChannel2,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ($"CH2:AT^NETSCAN={Count},-110,3\r\n", CommandType.Lte)
                }
            },
            new()
            {
                Channel = 2,
                NetworkStandard = NetworkStandard.Wcdma,
                Expression = p => p % 16 == 7,
                Enable = Wcdma && UseChannel2,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ("CH2:AT^NETSCAN=2,-110,1\r\n", CommandType.Wcdma)
                }
            },
            new()
            {
                Channel = 2,
                NetworkStandard = NetworkStandard.Tdscdma,
                Expression = p => p % 16 == 15,
                Enable = TdScdma && UseChannel2,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ("CH2:AT^NETSCAN=2,-110,2\r\n", CommandType.TdScdma)
                }
            },
            new()
            {
                Channel = 3,
                NetworkStandard = NetworkStandard.Cdma2000,
                Expression = p => p % 8 == 3,
                Enable = Cdma1X && UseChannel3,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ("CH3:AT^PREFMODE=2\r\n", CommandType.Cdma20001XMode),
                    ("CH3:AT^SIQ\r\n", CommandType.Cdma20001XSiq),
                    ("CH3:AT^BSIN?\r\n", CommandType.Cdma20001XBsin)
                }
            },
            new()
            {
                Channel = 3,
                NetworkStandard = NetworkStandard.Evdo,
                Expression = p => p % 8 == 6,
                Enable = Evdo && UseChannel3,
                Commands = new List<(string cmd, CommandType commandType)>
                {
                    ("CH3:AT^PREFMODE=4\r\n", CommandType.Cdma2000EvdoMode),
                    ("CH3:AT^SIQ\r\n", CommandType.Cdma2000EvdoSiq),
                    ("CH3:AT^BSIN?\r\n", CommandType.Cdma2000EvdoBsin),
                    ("CH3:AT^CURRSID?\r\n", CommandType.Cdma2000EvdoSid)
                }
            }
        };
        while (_cmdSendTokenSource?.IsCancellationRequested == false && _client != null)
            try
            {
                for (var i = 0; i < 16; i++)
                {
                    if (_cmdSendTokenSource?.IsCancellationRequested != false || _client == null) break;
                    var commandInfo = commandInfos.Find(p => p.Expression?.Invoke(i) == true);
                    if (commandInfo?.Enable != true) continue;
                    var cmds = commandInfo.Commands.Select(p => p.cmd).ToArray();
                    var success = SendCmds(cmds, _cmdSendTokenSource.Token, out var recv);
                    if (success)
                    {
                        var commandTypes = commandInfo.Commands.Select(p => p.commandType).ToArray();
                        ParseRecv(commandInfo.Channel, recv, commandTypes);
                    }

                    Thread.Sleep(10);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                Thread.Sleep(50);
            }
    }

    private void ParseRecv(int channel, List<string> recvList, CommandType[] commandTypes)
    {
        var datas = new List<object>();
        if (channel == 1)
        {
            for (var i = 0; i < recvList.Count; i++)
            {
                var commandType = commandTypes[i];
                var list = ParseCh1Data(recvList[i], commandType);
                list?.ForEach(p => datas.Add(p));
            }
        }
        else if (channel == 2)
        {
            for (var i = 0; i < recvList.Count; i++)
            {
                var commandType = commandTypes[i];
                var list = ParseCh2Data(recvList[i], commandType);
                list?.ForEach(p => datas.Add(p));
            }
        }
        else if (channel == 3)
        {
            var data = new SDataCellular();
            for (var i = 0; i < recvList.Count; i++)
            {
                var commandType = commandTypes[i];
                ParseCh3Data(recvList[i], commandType, ref data);
            }

            if (data != null) datas.Add(data);
        }

        if (datas.Any()) SendData(datas);
    }
}

internal class DuplexCommandInfo
{
    public NetworkStandard NetworkStandard { get; set; }
    public int Channel { get; set; }
    public bool Enable { get; set; }
    public Func<int, bool> Expression { get; set; }
    public List<(string cmd, CommandType commandType)> Commands { get; set; }
}

internal enum NetworkStandard
{
    Gsm,
    Cdma2000,
    Evdo,
    Wcdma,
    Tdscdma,
    Lte,
    Nr
}