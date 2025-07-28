using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Device.GWGJ_PDU.Common;
using Magneto.Device.GWGJ_PDU.Models;

namespace Magneto.Device.GWGJ_PDU;

public sealed class GwgjPdu : IDisposable
{
    private readonly string _ip;
    private readonly ConcurrentQueue<byte[]> _msgQueue;
    private readonly int _port;
    private readonly object _syncObj = new();
    private readonly int _timeout;
    private bool _disposed;
    private bool _isLogin;
    private Task _receiveTask;
    private CancellationTokenSource _receiveTokenSource;
    private AutoResetEvent _resetEvent;
    private Socket _socket;

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口</param>
    /// <param name="timeout">发送数据和接收数据超时时间，单位为毫秒</param>
    public GwgjPdu(string ip, int port = 4600, int timeout = 5000)
    {
        _ip = ip;
        _port = port;
        _timeout = timeout;
        _msgQueue = new ConcurrentQueue<byte[]>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public bool State => _socket?.Connected == true && _isLogin;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~GwgjPdu()
    {
        Dispose(false);
    }

    /// <summary>
    ///     连接
    /// </summary>
    /// <returns>是否连接成功</returns>
    public bool Connect()
    {
        _msgQueue.Clear();
        if (string.IsNullOrWhiteSpace(_ip) || _port < 0 || _port > 65535) return false;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var ipAddress = IPAddress.Parse(_ip);
        var endpoint = new IPEndPoint(ipAddress, _port);
        try
        {
            _socket.SendTimeout = _timeout;
            _socket.ReceiveTimeout = _timeout;
            _socket.Connect(endpoint);
            _receiveTokenSource = new CancellationTokenSource();
            _resetEvent = new AutoResetEvent(true);
            _receiveTask = new Task(() => ReceiveMessage(_receiveTokenSource.Token), _receiveTokenSource.Token);
            _receiveTask.Start();
        }
        catch (SocketException)
        {
            _socket.Dispose();
            return false;
        }

        return true;
    }

    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="user">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>登录状态<see cref="LoginState" /></returns>
    public LoginState Login(string user = "admin", string password = "admin")
    {
        var trimChar = new char[1];
        var cmd = $"S  user='{user.Trim(trimChar)}' password='{password.Trim(trimChar)}'  E";
        var recv = SendCmd(CmdFlags.Login, cmd);
        if (string.IsNullOrWhiteSpace(recv)) return LoginState.Fail;
        if (Regex.IsMatch(recv, @"Login\s+Successful", RegexOptions.IgnoreCase))
        {
            _isLogin = true;
            return LoginState.Success;
        }

        if (Regex.IsMatch(recv, @"User\s+Online", RegexOptions.IgnoreCase))
        {
            _isLogin = false;
            return LoginState.Repeat;
        }

        _isLogin = false;
        return LoginState.Fail;
    }

    /// <summary>
    ///     退出登录
    /// </summary>
    /// <returns>是否成功</returns>
    public bool Logout()
    {
        if (!State) return false;
        var recv = SendCmd(CmdFlags.Logout);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        var flag = recv.IndexOf("OK", StringComparison.OrdinalIgnoreCase) >= 0;
        if (flag) _isLogin = false;
        return flag;
    }

    /// <summary>
    ///     获取功率、电压和电流信息
    /// </summary>
    /// <param name="pvcInfo">功率、电压和电流信息</param>
    /// <returns>是否成功</returns>
    public bool GetPvcInfo(out PvcInfo pvcInfo)
    {
        pvcInfo = null;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetPvcInfo);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //power='0' voltage='23092' current='0'
        var b = GetIntValue("power", recv, out var power);
        b &= GetIntValue("voltage", recv, out var voltage);
        b &= GetIntValue("current", recv, out var current);
        if (!b) return false;
        pvcInfo = new PvcInfo
        {
            Power = power,
            Voltage = voltage / 100d,
            Current = current / 100d
        };
        return true;
    }

    /// <summary>
    ///     获取功率、电压和电流配置
    /// </summary>
    /// <param name="power">功率配置</param>
    /// <param name="voltage">电压配置</param>
    /// <param name="current">电流配置</param>
    /// <returns>是否成功</returns>
    public bool GetPvcConfig(out PvcConfig power, out PvcConfig voltage, out PvcConfig current)
    {
        power = null;
        voltage = null;
        current = null;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetPvcConfig);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //Pupper='3000' Plower='0' Pwarin='2200' Pport='0' Pmode='6' Psec='0' Vupper='24000' Vlower='20000' Vwarin='23000' Vport='0' Vmode='6' Vsec='0' Cupper='1200' Clower='0' Cwarin='1000' Cport='0' Cmode='6' Csec='0'
        var b = GetIntValue("Pupper", recv, out var pUpper);
        b &= GetIntValue("Plower", recv, out var pLower);
        b &= GetIntValue("Pwarin", recv, out var pWarin);
        b &= GetIntValue("Pport", recv, out var pPort);
        b &= GetIntValue("Pmode", recv, out var pMode);
        b &= GetIntValue("Psec", recv, out var pSec);
        b &= GetIntValue("Vupper", recv, out var vUpper);
        b &= GetIntValue("Vlower", recv, out var vLower);
        b &= GetIntValue("Vwarin", recv, out var vWarin);
        b &= GetIntValue("Vport", recv, out var vPort);
        b &= GetIntValue("Vmode", recv, out var vMode);
        b &= GetIntValue("Vsec", recv, out var vSec);
        b &= GetIntValue("Cupper", recv, out var cUpper);
        b &= GetIntValue("Clower", recv, out var cLower);
        b &= GetIntValue("Cwarin", recv, out var cWarin);
        b &= GetIntValue("Cport", recv, out var cPort);
        b &= GetIntValue("Cmode", recv, out var cMode);
        b &= GetIntValue("Csec", recv, out var cSec);
        if (!b) return false;
        power = new PvcConfig
        {
            Upper = pUpper,
            Lower = pLower,
            Warin = pWarin,
            Mode = (OverLimitOpMode)(byte)pMode,
            Port = (byte)pPort,
            Sec = pSec
        };
        voltage = new PvcConfig
        {
            Upper = vUpper / 100d,
            Lower = vLower / 100d,
            Warin = vWarin / 100d,
            Mode = (OverLimitOpMode)(byte)vMode,
            Port = (byte)vPort,
            Sec = vSec
        };
        current = new PvcConfig
        {
            Upper = cUpper / 100d,
            Lower = cLower / 100d,
            Warin = cWarin / 100d,
            Mode = (OverLimitOpMode)(byte)cMode,
            Port = (byte)cPort,
            Sec = cSec
        };
        return true;
    }

    /// <summary>
    ///     获取插座状态
    /// </summary>
    /// <param name="states">插座状态</param>
    /// <returns>是否成功</returns>
    public bool GetSocketState(out bool[] states)
    {
        states = null;
        if (!State) return false;
        var bytes = Encoding.Default.GetBytes("S   E");
        bytes[1] = (byte)CmdFlags.GetSocketState;
        var recv = SendCmdBuffer(bytes);
        if (recv == null || recv.Length < 8) return false;
        states = new bool[recv.Length];
        for (var i = 0; i < states.Length; i++) states[i] = recv[i] == 49;
        return true;
    }

    /// <summary>
    ///     获取插座配置
    /// </summary>
    /// <param name="socketInfos"></param>
    /// <returns>是否成功</returns>
    public bool GetSocketConfigs(out List<SocketConfig> socketInfos)
    {
        socketInfos = new List<SocketConfig>();
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetSocketConfigs);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //name1='苹果电脑' sdelay1='0' cdelay1='0' rdelay1='0' action1='1' icoid1='1'name2='台式电脑' sdelay2='0' cdelay2='0' rdelay2='0' action2='1' icoid2='2'name3='笔记本电脑' sdelay3='0' cdelay3='0' rdelay3='0' action3='1' icoid3='3'name4='服务器' sdelay4='0' cdelay4='0' rdelay4='0' action4='1' icoid4='4'name5='' sdelay5='0' cdelay5='0' rdelay5='0' action5='-48' icoid5='0'name6='' sdelay6='0' cdelay6='0' rdelay6='0' action6='-48' icoid6='0'name7='' sdelay7='0' cdelay7='0' rdelay7='0' action7='-48' icoid7='0'name8='' sdelay8='0' cdelay8='0' rdelay8='0' action8='-48' icoid8='0'
        var pattern = "name(?<nIdx>\\d+)\\s*\\=\\s*'\\s*(?<name>[^']*)\\s*'\\s*";
        pattern += "sdelay(?<sIdx>\\d+)\\s*\\=\\s*'\\s*(?<sdelay>\\d+)\\s*'\\s*";
        pattern += "cdelay(?<cIdx>\\d+)\\s*\\=\\s*'\\s*(?<cdelay>\\d+)\\s*'\\s*";
        pattern += "rdelay(?<rIdx>\\d+)\\s*\\=\\s*'\\s*(?<rdelay>\\d+)\\s*'\\s*";
        pattern += "action(?<aIdx>\\d+)\\s*\\=\\s*'\\s*(?<action>\\d+)\\s*'\\s*";
        pattern += "icoid(?<iIdx>\\d+)\\s*\\=\\s*'\\s*(?<icoid>\\d+)\\s*'\\s*";
        pattern = $"({pattern})+";
        var match = Regex.Match(recv, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        var nIdxCaps = match.Groups["nIdx"].Captures;
        var sIdxCaps = match.Groups["sIdx"].Captures;
        var cIdxCaps = match.Groups["cIdx"].Captures;
        var rIdxCaps = match.Groups["rIdx"].Captures;
        var aIdxCaps = match.Groups["aIdx"].Captures;
        var iIdxCaps = match.Groups["iIdx"].Captures;
        var nameCaps = match.Groups["name"].Captures;
        var sdelayCaps = match.Groups["sdelay"].Captures;
        var cdelayCaps = match.Groups["cdelay"].Captures;
        var rdelayCaps = match.Groups["rdelay"].Captures;
        var actionCaps = match.Groups["action"].Captures;
        var icoidCaps = match.Groups["icoid"].Captures;
        var b = nIdxCaps.Count == sIdxCaps.Count &&
                nIdxCaps.Count == cIdxCaps.Count &&
                nIdxCaps.Count == rIdxCaps.Count &&
                nIdxCaps.Count == aIdxCaps.Count &&
                nIdxCaps.Count == iIdxCaps.Count &&
                nIdxCaps.Count == nameCaps.Count &&
                nIdxCaps.Count == sdelayCaps.Count &&
                nIdxCaps.Count == cdelayCaps.Count &&
                nIdxCaps.Count == rdelayCaps.Count &&
                nIdxCaps.Count == actionCaps.Count &&
                nIdxCaps.Count == icoidCaps.Count;
        if (!b) return false;
        for (var i = 0; i < nIdxCaps.Count; i++)
        {
            var nIdx = int.Parse(nIdxCaps[i].Value);
            var sIdx = int.Parse(sIdxCaps[i].Value);
            var cIdx = int.Parse(cIdxCaps[i].Value);
            var rIdx = int.Parse(rIdxCaps[i].Value);
            var aIdx = int.Parse(aIdxCaps[i].Value);
            var iIdx = int.Parse(iIdxCaps[i].Value);
            var flag = nIdx == sIdx &&
                       nIdx == cIdx &&
                       nIdx == rIdx &&
                       nIdx == aIdx &&
                       nIdx == iIdx;
            if (!flag) return false;
            var name = nameCaps[i].Value;
            var sdelay = int.Parse(sdelayCaps[i].Value);
            var cdelay = int.Parse(cdelayCaps[i].Value);
            var rdelay = int.Parse(rdelayCaps[i].Value);
            var action = (ActionMode)int.Parse(actionCaps[i].Value);
            var icoid = (byte)int.Parse(icoidCaps[i].Value);
            socketInfos.Add(new SocketConfig
            {
                IcoId = icoid,
                Name = name,
                OnDelay = sdelay,
                OffDelay = cdelay,
                RebootDelay = rdelay,
                Action = action
            });
        }

        return true;
    }

    /// <summary>
    ///     获取温度信息
    /// </summary>
    /// <param name="info">温度信息</param>
    /// <returns>是否成功</returns>
    public bool GetTemperature(out TemperatureInfo info)
    {
        info = null;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetTemperature);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //flag='0' temperature='0' humidity='0' fahrenheit='0'
        var b = GetIntValue("flag", recv, out var flag);
        b &= GetIntValue("temperature", recv, out var temperature);
        b &= GetIntValue("humidity", recv, out var humidity);
        b &= GetIntValue("fahrenheit", recv, out var fahrenheit);
        if (!b) return false;
        info = new TemperatureInfo
        {
            Flag = flag,
            Temperature = temperature / 10d,
            Humidity = humidity / 10d,
            Fahrenheit = fahrenheit / 10d
        };
        return true;
    }

    /// <summary>
    ///     获取网络信息
    /// </summary>
    /// <param name="info">网络信息</param>
    /// <returns>是否成功</returns>
    public bool GetNetInfo(out NetInfo info)
    {
        info = null;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetNetInfo);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //name='PDU' mac='0.c0.43.15.84.14' ip='192.168.0.163' sub='255.255.255.0' gate='192.168.0.1' port_local='4600' port_web='80' dhcp='0' dns='192.168.0.1' type='XY-G10' version='1.2.8' date='2021年10月' hwver='AG-128' hwid='5dcff313038424843158414'
        var b = GetStringValue("name", recv, out var name);
        b &= GetStringValue("mac", recv, out var mac);
        b &= GetStringValue("ip", recv, out var ip);
        b &= GetStringValue("sub", recv, out var sub);
        b &= GetStringValue("gate", recv, out var gate);
        b &= GetIntValue("port_local", recv, out var portLocal);
        b &= GetIntValue("port_web", recv, out var portWeb);
        b &= GetIntValue("dhcp", recv, out var dhcp);
        b &= GetStringValue("dns", recv, out var dns);
        b &= GetStringValue("type", recv, out var type);
        b &= GetStringValue("version", recv, out var version);
        b &= GetStringValue("date", recv, out var date);
        b &= GetStringValue("hwver", recv, out var hwver);
        b &= GetStringValue("hwid", recv, out var hwid);
        if (!b) return false;
        info = new NetInfo
        {
            Name = name,
            Mac = mac,
            Version = version,
            HwVersion = hwver,
            Date = date,
            Dhcp = (byte)dhcp,
            Dns = dns,
            Gateway = gate,
            HwId = hwid,
            PortWeb = (short)portWeb,
            Ip = ip,
            PduType = type,
            PortLocal = (short)portLocal,
            SubMask = sub
        };
        return true;
    }

    /// <summary>
    ///     获取历史
    /// </summary>
    /// <param name="historyList">历史数据</param>
    /// <returns>是否成功</returns>
    public bool GetHistory(out List<ItemData<string>> historyList)
    {
        historyList = new List<ItemData<string>>();
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetHistory);
        //history1=''history2=''history3=''history4=''history5=''history6=''history7=''history8=''history9=''history10=''history11=''history12=''
        var b = GetStringValues("history", recv, out historyList);
        return b;
    }

    /// <summary>
    ///     获取日期时间
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <returns>是否成功</returns>
    public bool GetDate(out DateTime dateTime)
    {
        dateTime = default;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetDate);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //date='2021-12-09 14:55:22'
        var b = GetStringValue("date", recv, out var sDate);
        if (!b) return false;
        return DateTime.TryParse(sDate, out dateTime);
    }

    /// <summary>
    ///     获取操作记录页总数
    /// </summary>
    /// <param name="total"></param>
    /// <returns>是否成功</returns>
    public bool GetControlRecordPageTotal(out int total)
    {
        total = 0;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetControlRecordPageTotal);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //page='12'
        var b = GetIntValue("page", recv, out total);
        return b;
    }

    /// <summary>
    ///     根据页码获取操作记录
    /// </summary>
    /// <param name="pageIndex">页码，从0开始</param>
    /// <param name="items">操作记录</param>
    /// <returns>是否成功</returns>
    public bool GetControlRecordsByPage(int pageIndex, out List<ItemData<string>> items)
    {
        items = new List<ItemData<string>>();
        if (!State) return false;
        var cmd = $"S  page='{pageIndex}' E";
        var recv = SendCmd(CmdFlags.GetControlRecordsByPage, cmd);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        // item0='2021-12-09 15:50:43 admin 登录PDU成功' item1='2021-12-09 15:49:25 admin 登录PDU成功' item2='2021-12-09 15:12:29 admin 登录PDU成功' item3='2021-12-09 15:11:09 admin 登录PDU成功' item4='2021-12-09 15:08:32 admin 登录PDU成功' item5='2021-12-09 15:06:29 admin 登录PDU成功' item6='2021-12-09 14:55:18 admin 登录PDU成功' item7='2021-12-09 14:53:51 admin 登录PDU成功' item8='2021-12-09 14:48:51 admin 登录PDU成功' item9='2021-12-09 14:45:10 admin 登录PDU成功' item10='2021-12-09 14:37:23 admin 登录PDU成功' item11='2021-12-09 14:36:25 admin 登录PDU成功'
        var b = GetStringValues("item", recv, out items);
        return b;
    }

    /// <summary>
    ///     获取温度配置
    /// </summary>
    /// <param name="config">温度配置</param>
    /// <returns>是否成功</returns>
    public bool GetTemperatureConfig(out TemperatureConfig config)
    {
        config = null;
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetTemperatureConfig);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        var b = GetIntValue("mode", recv, out var mode);
        b &= GetIntValue("temperature", recv, out var temperature);
        b &= GetIntValue("temperature_setup_value", recv, out var temperatureSetupValue);
        b &= GetIntValue("huicha", recv, out var huicha);
        b &= GetIntValue("state", recv, out var state);
        if (!b) return false;
        config = new TemperatureConfig
        {
            Mode = (TemperatureMode)(byte)mode,
            Temperature = temperature / 10d,
            TemperatureSetupValue = temperatureSetupValue / 10d,
            Backlash = huicha / 10d,
            State = (byte)state
        };
        return true;
    }

    /// <summary>
    ///     获取日程安排
    /// </summary>
    /// <param name="schedules">日程安排</param>
    /// <returns>是否成功</returns>
    public bool GetScheduleInfos(out List<ScheduleInfo> schedules)
    {
        schedules = new List<ScheduleInfo>();
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetScheduleInfos);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        //name1='test' fre1='1'  action1='1' delay1='5' socket1='15' state1='1' proID1='0' calendar1='202101010000' week1='1'name2='' fre2='0'  action2='0' delay2='0' socket2='0' state2='0' proID2='0' calendar2='0' week2='0'name3='' fre3='0'  action3='0' delay3='0' socket3='0' state3='0' proID3='0' calendar3='0' week3='0'name4='' fre4='0'  action4='0' delay4='0' socket4='0' state4='0' proID4='0' calendar4='0' week4='0'name5='' fre5='0'  action5='0' delay5='0' socket5='0' state5='0' proID5='0' calendar5='0' week5='0'name6='' fre6='0'  action6='0' delay6='0' socket6='0' state6='0' proID6='0' calendar6='0' week6='0'name7='' fre7='0'  action7='0' delay7='0' socket7='0' state7='0' proID7='0' calendar7='0' week7='0'name8='' fre8='0'  action8='0' delay8='0' socket8='0' state8='0' proID8='0' calendar8='0' week8='0'name9='' fre9='0'  action9='0' delay9='0' socket9='0' state9='0' proID9='0' calendar9='0' week9='0'name10='' fre10='0'  action10='0' delay10='0' socket10='0' state10='0' proID10='0' calendar10='0' week10='0'
        var pattern = "name(?<nIdx>\\d+)\\s*\\=\\s*'\\s*(?<name>[^']+)\\s*'\\s*";
        pattern += "fre(?<fIdx>\\d+)\\s*\\=\\s*'\\s*(?<freq>\\d+)\\s*'\\s*";
        pattern += "action(?<aIdx>\\d+)\\s*\\=\\s*'\\s*(?<act>\\d+)\\s*'\\s*";
        pattern += "delay(?<dIdx>\\d+)\\s*\\=\\s*'\\s*(?<delay>\\d+)\\s*'\\s*";
        pattern += "socket(?<oIdx>\\d+)\\s*\\=\\s*'\\s*(?<socket>\\d+)\\s*'\\s*";
        pattern += "state(?<tIdx>\\d+)\\s*\\=\\s*'\\s*(?<state>\\d+)\\s*'\\s*";
        pattern += "proID(?<pIdx>\\d+)\\s*\\=\\s*'\\s*(?<id>\\d+)\\s*'\\s*";
        pattern += "calendar(?<cIdx>\\d+)\\s*\\=\\s*'\\s*(?<calendar>\\d+)\\s*'\\s*";
        pattern += "week(?<wIdx>\\d+)\\s*\\=\\s*'\\s*(?<week>\\d+)\\s*'\\s*";
        pattern = $"({pattern})+";
        var match = Regex.Match(recv, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        var nIdxCaps = match.Groups["nIdx"].Captures;
        var fIdxCaps = match.Groups["fIdx"].Captures;
        var aIdxCaps = match.Groups["aIdx"].Captures;
        var dIdxCaps = match.Groups["dIdx"].Captures;
        var oIdxCaps = match.Groups["oIdx"].Captures;
        var tIdxCaps = match.Groups["tIdx"].Captures;
        var pIdxCaps = match.Groups["pIdx"].Captures;
        var cIdxCaps = match.Groups["cIdx"].Captures;
        var wIdxCaps = match.Groups["wIdx"].Captures;
        var nameCaps = match.Groups["name"].Captures;
        var freqCaps = match.Groups["freq"].Captures;
        var actCaps = match.Groups["act"].Captures;
        var delayCaps = match.Groups["delay"].Captures;
        var socketCaps = match.Groups["socket"].Captures;
        var stateCaps = match.Groups["state"].Captures;
        var idCaps = match.Groups["id"].Captures;
        var calendarCaps = match.Groups["calendar"].Captures;
        var weekCaps = match.Groups["week"].Captures;
        var b = nIdxCaps.Count == fIdxCaps.Count &&
                nIdxCaps.Count == aIdxCaps.Count &&
                nIdxCaps.Count == dIdxCaps.Count &&
                nIdxCaps.Count == oIdxCaps.Count &&
                nIdxCaps.Count == tIdxCaps.Count &&
                nIdxCaps.Count == pIdxCaps.Count &&
                nIdxCaps.Count == cIdxCaps.Count &&
                nIdxCaps.Count == wIdxCaps.Count &&
                nIdxCaps.Count == nameCaps.Count &&
                nIdxCaps.Count == freqCaps.Count &&
                nIdxCaps.Count == actCaps.Count &&
                nIdxCaps.Count == delayCaps.Count &&
                nIdxCaps.Count == socketCaps.Count &&
                nIdxCaps.Count == stateCaps.Count &&
                nIdxCaps.Count == idCaps.Count &&
                nIdxCaps.Count == calendarCaps.Count &&
                nIdxCaps.Count == weekCaps.Count;
        if (!b) return false;
        for (var i = 0; i < nIdxCaps.Count; i++)
        {
            var nIdx = int.Parse(nIdxCaps[i].Value);
            var fIdx = int.Parse(fIdxCaps[i].Value);
            var aIdx = int.Parse(aIdxCaps[i].Value);
            var dIdx = int.Parse(dIdxCaps[i].Value);
            var oIdx = int.Parse(oIdxCaps[i].Value);
            var tIdx = int.Parse(tIdxCaps[i].Value);
            var pIdx = int.Parse(pIdxCaps[i].Value);
            var cIdx = int.Parse(cIdxCaps[i].Value);
            var wIdx = int.Parse(wIdxCaps[i].Value);
            var flag = nIdx == fIdx &&
                       nIdx == aIdx &&
                       nIdx == dIdx &&
                       nIdx == oIdx &&
                       nIdx == tIdx &&
                       nIdx == pIdx &&
                       nIdx == cIdx &&
                       nIdx == wIdx;
            if (!flag) return false;
            var name = nameCaps[i].Value;
            var freq = (byte)int.Parse(freqCaps[i].Value);
            var action = (byte)int.Parse(actCaps[i].Value);
            var delay = (short)int.Parse(delayCaps[i].Value);
            var socket = (byte)int.Parse(socketCaps[i].Value);
            var state = (byte)int.Parse(stateCaps[i].Value);
            var id = (byte)int.Parse(idCaps[i].Value);
            var calendar = calendarCaps[i].Value;
            var week = (byte)int.Parse(weekCaps[i].Value);
            schedules.Add(new ScheduleInfo
            {
                Name = name,
                Frequency = (ScheduleFrequency)freq,
                Action = (SocketAction)action,
                Delay = delay,
                Socket = socket,
                State = state,
                Id = id,
                Calendar = calendar,
                Week = week
            });
        }

        return true;
    }

    /// <summary>
    ///     获取普通用户列表
    /// </summary>
    /// <param name="users">普通用户列表</param>
    /// <returns>是否成功</returns>
    public bool GetGeneralUserList(out List<UserInfo> users)
    {
        users = new List<UserInfo>();
        if (!State) return false;
        var recv = SendCmd(CmdFlags.GetGeneralUserList);
        if (string.IsNullOrWhiteSpace(recv)) return false;
        var pattern = "name(?<nIdx>\\d+)\\s*\\=\\s*'\\s*(?<name>[^']+)\\s*'\\s*";
        pattern += "pass(?<pIdx>\\d+)\\s*\\=\\s*'\\s*(?<pass>\\d+)\\s*'\\s*";
        pattern += "able(?<aIdx>\\d+)\\s*\\=\\s*'\\s*(?<able>\\d+)\\s*'\\s*";
        pattern += "port(?<oIdx>\\d+)\\s*\\=\\s*'\\s*(?<port>\\d+)\\s*'\\s*";
        pattern = $"({pattern})+";
        //name1='user1' pass1='123456' able1='1' port1='15'name2='user2' pass2='123456' able2='0' port2='0'name3='user3' pass3='123456' able3='0' port3='0'name4='user4' pass4='123456' able4='0' port4='0'name5='user5' pass5='123456' able5='0' port5='0'name6='user6' pass6='123456' able6='0' port6='0'name7='user7' pass7='123456' able7='0' port7='0'name8='user8' pass8='123456' able8='0' port8='0'
        var match = Regex.Match(recv, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        var nIdxCaps = match.Groups["nIdx"].Captures;
        var pIdxCaps = match.Groups["pIdx"].Captures;
        var aIdxCaps = match.Groups["aIdx"].Captures;
        var oIdxCaps = match.Groups["oIdx"].Captures;
        var nameCaps = match.Groups["name"].Captures;
        var passCaps = match.Groups["pass"].Captures;
        var ableCaps = match.Groups["able"].Captures;
        var portCaps = match.Groups["port"].Captures;
        var b = nIdxCaps.Count == pIdxCaps.Count &&
                nIdxCaps.Count == aIdxCaps.Count &&
                nIdxCaps.Count == oIdxCaps.Count &&
                nIdxCaps.Count == nameCaps.Count &&
                nIdxCaps.Count == passCaps.Count &&
                nIdxCaps.Count == ableCaps.Count &&
                nIdxCaps.Count == portCaps.Count;
        if (!b) return false;
        for (var i = 0; i < nIdxCaps.Count; i++)
        {
            var nIdx = (byte)int.Parse(nIdxCaps[i].Value);
            var pIdx = int.Parse(pIdxCaps[i].Value);
            var aIdx = int.Parse(aIdxCaps[i].Value);
            var oIdx = int.Parse(oIdxCaps[i].Value);
            var flag = nIdx == pIdx &&
                       nIdx == aIdx &&
                       nIdx == oIdx;
            if (!flag) return false;
            var name = nameCaps[i].Value;
            var pass = passCaps[i].Value;
            var able = (byte)int.Parse(ableCaps[i].Value);
            var port = (byte)int.Parse(portCaps[i].Value);
            users.Add(new UserInfo
            {
                Name = name,
                Password = pass,
                Port = port,
                Available = able == 1,
                Id = nIdx
            });
        }

        return true;
    }

    /// <summary>
    ///     控制插座开关状态
    /// </summary>
    /// <param name="states">插座状态数组，数组长度根据插座数量而定，eg:[true,false,true,false]为1、3开启，其余关闭</param>
    /// <param name="action">是否开启</param>
    /// <returns>操作结果</returns>
    public OpStatus SocketControl(bool[] states, bool action)
    {
        return ExecuteCmd(() =>
        {
            if (states == null || states.Length == 0) return null;
            byte num = 0;
            var actionFlag = action ? 1 : 0;
            for (var i = 0; i < states.Length; i++)
                if (states[i])
                    num |= (byte)(1 << i);
            var cmd = $"S  socket8='{num}' action='{actionFlag}' E";
            return cmd;
        }, CmdFlags.SocketControl);
    }

    /// <summary>
    ///     更新功率、电压和电流配置
    /// </summary>
    /// <param name="power">功率配置</param>
    /// <param name="voltage">电压配置</param>
    /// <param name="current">电流配置</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdatePvcConfig(PvcConfig power, PvcConfig voltage, PvcConfig current)
    {
        return ExecuteCmd(() =>
        {
            var b = power != null && voltage != null && current != null;
            if (!b) return null;
            var cmd =
                $"S  Pupper='{(int)power.Upper}' Plower='{(int)power.Lower}' Pwarin='{(int)power.Warin}' Pport='{(int)power.Port}' Pmode='{(int)power.Mode}' Psec='{power.Sec}' Vupper='{(int)(voltage.Upper * 100)}' Vlower='{(int)(voltage.Lower * 100)}' Vwarin='{(int)(voltage.Warin * 100)}' Vport='{(int)voltage.Port}' Vmode='{(int)voltage.Mode}' Vsec='{voltage.Sec}' Cupper='{(int)(current.Upper * 100)}' Clower='{(int)(current.Lower * 100)}' Cwarin='{(int)(current.Warin * 100)}' Cport='{(int)current.Port}' Cmode='{(int)current.Mode}' Csec='{current.Sec}'  E";
            return cmd;
        }, CmdFlags.UpdatePvcConfig);
    }

    /// <summary>
    ///     更新超级账户
    /// </summary>
    /// <param name="oldUser">旧用户名</param>
    /// <param name="oldPwd">旧密码</param>
    /// <param name="user">新用户名</param>
    /// <param name="password">新密码</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdateSuperAccount(string oldUser, string oldPwd, string user, string password)
    {
        return ExecuteCmd(() =>
        {
            var b = CheckStringField(oldUser) && CheckStringField(oldPwd) && CheckStringField(user) &&
                    CheckStringField(password);
            if (!b) return null;
            return
                $"S  ouser='{oldUser.Trim()}'opassword='{oldPwd.Trim()}'nuser='{user.Trim()}'npassword='{password.Trim()}'  E";
        }, CmdFlags.UpdateSuperAccount);
    }

    /// <summary>
    ///     更新温度配置
    /// </summary>
    /// <param name="config">温度配置</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdateTemperatureConfig(TemperatureConfig config)
    {
        return ExecuteCmd(() =>
        {
            if (config.Mode != TemperatureMode.Off && (config.TemperatureSetupValue > 75 ||
                                                       config.TemperatureSetupValue < -45 ||
                                                       config.Backlash <= 0)) return null;
            var cmd =
                $"S  mode='{(byte)config.Mode}'temperature_setup_value='{(int)(config.TemperatureSetupValue * 10)}'huicha='{(int)(config.Backlash * 10)}'state='{config.State}'  E";
            return cmd;
        }, CmdFlags.UpdateTemperatureConfig);
    }

    /// <summary>
    ///     更新插座配置
    /// </summary>
    /// <param name="socketConfigs">插座配置，数组长度固定为8</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdateSocketConfig(SocketConfig[] socketConfigs)
    {
        return ExecuteCmd(() =>
        {
            if (socketConfigs == null || socketConfigs.Length == 0) return null;
            if (socketConfigs.Any(p => !CheckStringField(p.Name))) return null;
            var sb = new StringBuilder();
            sb.Append("S  ");
            for (var i = 0; i < 8; i++)
                if (i < socketConfigs.Length)
                {
                    var config = socketConfigs[i];
                    sb.Append("name").Append(i + 1).Append("='").Append(config.Name.Trim()).Append('\'');
                    sb.Append("sdelay").Append(i + 1).Append("='").Append(config.OnDelay).Append('\'');
                    sb.Append("cdelay").Append(i + 1).Append("='").Append(config.OffDelay).Append('\'');
                    sb.Append("rdelay").Append(i + 1).Append("='").Append(config.RebootDelay).Append('\'');
                    sb.Append("action").Append(i + 1).Append("='").Append((byte)config.Action).Append('\'');
                    sb.Append("icoid").Append(i + 1).Append("='").Append(i + 1).Append('\'');
                }
                else
                {
                    sb.Append("name").Append(i + 1).Append("='设备").Append(i + 1).Append('\'');
                    sb.Append("sdelay").Append(i + 1).Append("='0'");
                    sb.Append("cdelay").Append(i + 1).Append("='0'");
                    sb.Append("rdelay").Append(i + 1).Append("='0'");
                    sb.Append("action").Append(i + 1).Append("='").Append((byte)ActionMode.Off).Append('\'');
                    sb.Append("icoid").Append(i + 1).Append("='").Append(i + 1).Append('\'');
                }

            sb.Append("  E");
            var cmd = sb.ToString();
            return cmd;
        }, CmdFlags.UpdateSocketConfig);
    }

    /// <summary>
    ///     更新网络配置
    /// </summary>
    /// <param name="netInfo"></param>
    /// <returns>操作状态</returns>
    public OpStatus UpdateNetworkConfig(NetInfo netInfo)
    {
        return ExecuteCmd(() =>
        {
            if (netInfo == null) return null;
            if (!CheckStringField(netInfo.Name)) return null;
            var cmd =
                $"S  name='{netInfo.Name.Trim()}'ip='{netInfo.Ip.Trim()}'sub='{netInfo.SubMask.Trim()}'gate='{netInfo.Gateway.Trim()}'port_local='{netInfo.PortLocal}'port_web='{netInfo.PortWeb}'dhcp='{netInfo.Dhcp}'dns='{netInfo.Dns.Trim()}'  E";
            return cmd;
        }, CmdFlags.UpdateNetworkConfig);
    }

    /// <summary>
    ///     添加日程安排
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns>操作状态</returns>
    public OpStatus AddSchedule(ScheduleInfo schedule)
    {
        return ExecuteCmd(() =>
        {
            if (schedule == null) return null;
            if (!CheckStringField(schedule.Name)) return null;
            var cmd =
                $"S  name='{schedule.Name.Trim()}'fre='{(byte)schedule.Frequency}'action='{(byte)schedule.Action}'delay='{schedule.Delay}'socket='{schedule.Socket}'state='{schedule.State}'ProID='0'calendar='{schedule.Calendar}'week='{schedule.Week}'  E";
            return cmd;
        }, CmdFlags.AddSchedule);
    }

    /// <summary>
    ///     删除日程安排
    /// </summary>
    /// <param name="id"></param>
    /// <returns>操作状态</returns>
    public OpStatus DeleteSchedule(byte id)
    {
        return ExecuteCmd(() => $"S  programme_id='{id}'  E", CmdFlags.DeleteSchedule);
    }

    /// <summary>
    ///     更新日程安排
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns>操作状态</returns>
    public OpStatus UpdateSchedule(ScheduleInfo schedule)
    {
        return ExecuteCmd(() =>
        {
            if (schedule == null) return null;
            if (!CheckStringField(schedule.Name)) return null;
            var cmd =
                $"S  name='{schedule.Name.Trim()}'fre='{(byte)schedule.Frequency}'action='{(byte)schedule.Action}'delay='{schedule.Delay}'socket='{schedule.Socket}'state='{schedule.State}'ProID='{schedule.Id}'calendar='{schedule.Calendar.Trim()}'week='{schedule.Week}'  E";
            return cmd;
        }, CmdFlags.UpdateSchedule);
    }

    /// <summary>
    ///     重置
    /// </summary>
    /// <param name="socket">是否重置插座口配置</param>
    /// <param name="schedule">是否重置日程配置</param>
    /// <param name="pvc">是否重置功率、电流、电压和温度配置</param>
    /// <param name="net">是否重置网络配置</param>
    /// <param name="user">是否重置用户配置</param>
    /// <param name="history">是否重置警告信息</param>
    /// <param name="record">是否重置操作记录</param>
    /// <returns>操作结果</returns>
    public OpStatus ResetPdu(bool socket, bool schedule, bool pvc, bool net, bool user, bool history, bool record)
    {
        return ExecuteCmd(() =>
        {
            var cmd =
                $"S  socket='{GetStateFlag(socket)}'programme='{GetStateFlag(schedule)}'PVC='{GetStateFlag(pvc)}'net='{GetStateFlag(net)}'user='{GetStateFlag(user)}'history='{GetStateFlag(history)}'record='{GetStateFlag(record)}'  E";
            return cmd;
        }, CmdFlags.ResetPdu);
    }

    /// <summary>
    ///     更新时间
    /// </summary>
    /// <returns>操作结果</returns>
    public OpStatus UpdateDate()
    {
        return ExecuteCmd(() =>
        {
            var time = new DateTime(1970, 1, 1);
            var seconds = (int)(DateTime.Now - time).TotalSeconds;
            return $"S  sec='{seconds}'  E";
        }, CmdFlags.UpdateDate);
    }

    /// <summary>
    ///     手动控制（开启状态有效，其余无效）
    /// </summary>
    /// <param name="socket">插座口</param>
    /// <param name="flag">操作类型</param>
    /// <returns>操作结果</returns>
    public OpStatus HandControl(byte socket, SocketAction flag)
    {
        return ExecuteCmd(() => $"S  State='{socket}' Flag='{(byte)flag}'  E", CmdFlags.HandControl);
    }

    /// <summary>
    ///     更新普通用户的密码
    /// </summary>
    /// <param name="user">用户名</param>
    /// <param name="oldPwd">旧密码</param>
    /// <param name="password">新密码</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdateGeneralUserPassword(string user, string oldPwd, string password)
    {
        return ExecuteCmd(() =>
        {
            var b = CheckStringField(user) && CheckStringField(oldPwd) && CheckStringField(password);
            if (!b) return null;
            var cmd = $"S  ouser='{user.Trim()}'opassword='{oldPwd.Trim()}'npassword='{password.Trim()}'  E";
            return cmd;
        }, CmdFlags.UpdateGeneralUserPassword);
    }

    /// <summary>
    ///     更新普通用户的权限
    /// </summary>
    /// <param name="userInfo">用户信息</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdateGeneralUserPower(UserInfo userInfo)
    {
        return ExecuteCmd(() =>
        {
            var b = CheckStringField(userInfo.Name) && CheckStringField(userInfo.Password);
            if (!b) return null;
            var cmd =
                $"S  name='{userInfo.Name.Trim()}'pass='{userInfo.Password.Trim()}'id='{userInfo.Id}'able='{GetStateFlag(userInfo.Available)}'port='{userInfo.Port}'  E";
            return cmd;
        }, CmdFlags.UpdateGeneralUserPower);
    }

    /// <summary>
    ///     更新普通用户的可用状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="available">是否可用</param>
    /// <returns>操作结果</returns>
    public OpStatus UpdateGeneralUserAble(byte userId, bool available)
    {
        return ExecuteCmd(() =>
        {
            if (userId == 0) return null;
            var cmd = $"S  user='{userId}'able='{GetStateFlag(available)}'  E";
            return cmd;
        }, CmdFlags.UpdateGeneralUserAble);
    }

    /// <summary>
    ///     根据插座状态标识获取插座状态数组
    /// </summary>
    /// <param name="stateFlag">插座状态标识</param>
    /// <returns>插座状态数组</returns>
    public static bool[] GetSocketStateArray(byte stateFlag)
    {
        var s = Convert.ToString(stateFlag, 2);
        var len = s.Length;
        var array = new bool[len];
        for (var i = 0; i < len; i++) array[len - i - 1] = s[i] == '1';
        return array;
    }

    /// <summary>
    ///     根据插座状态数组获取插座状态标识
    /// </summary>
    /// <param name="states">插座状态数组</param>
    /// <returns>插座状态标识</returns>
    public static byte GetSocketStateFlag(bool[] states)
    {
        var sb = new StringBuilder();
        for (var i = 7; i >= 0; i--)
            if (i < states.Length && states[i])
                sb.Append("1");
            else
                sb.Append("0");
        return Convert.ToByte(sb.ToString(), 2);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            //释放托管资源
        }

        //释放非托管资源
        if (_receiveTokenSource != null)
        {
            if (_receiveTask?.IsCompleted == false) _receiveTokenSource.Cancel();
            _receiveTokenSource.Dispose();
        }

        _isLogin = false;
        _socket?.Dispose();
        _resetEvent?.Dispose();
        _disposed = true;
    }

    private void ReceiveMessage(CancellationToken token)
    {
        var length = 0;
        var data = new byte[2048];
        while (!token.IsCancellationRequested)
        {
            if (_socket?.Connected != true || _resetEvent == null) break;
            _resetEvent.WaitOne();
            try
            {
                length = _socket.Receive(data);
            }
            catch
            {
            }

            if (length > 0)
            {
                var isOk = CheckNum(data, out var real);
                if (!isOk || real == null) continue;
                _msgQueue.Enqueue(real);
            }
        }
    }

    private string SendCmd(byte[] bytes)
    {
        var recvBuffer = SendCmdBuffer(bytes);
        if (recvBuffer == null || recvBuffer.Length == 0) return null;
        var s = Encoding.GetEncoding("GB2312").GetString(recvBuffer);
        Console.WriteLine(s);
        return s;
    }

    private byte[] SendCmdBuffer(byte[] bytes)
    {
        CheckSet(bytes);
        if (_socket?.Connected != true) return null;
        byte[] recv = null;
        lock (_syncObj)
        {
            _resetEvent.Set();
            _msgQueue.Clear();
            _socket.Send(bytes, bytes.Length, SocketFlags.None);
            _resetEvent.Set();
            bool receiveFlag;
            var watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < _timeout * 2)
            {
                receiveFlag = _msgQueue.TryDequeue(out var msg);
                if (receiveFlag)
                {
                    recv = msg;
                    break;
                }

                Thread.Sleep(500);
            }

            watch.Stop();
        }

        return recv;
    }

    private void CheckSet(byte[] info)
    {
        short num1 = 1;
        byte num2 = 0;
        do
        {
            num2 += info[num1++];
        } while (num1 != info.Length - 2);

        info[^2] = num2;
    }

    private bool CheckNum(byte[] recvBytes, out byte[] data)
    {
        data = null;
        if (recvBytes is not { Length: > 3 }) return false;
        var firstIndex = 0;
        do
        {
            if (recvBytes[firstIndex] == 83) break;
            firstIndex++;
        } while (firstIndex < recvBytes.Length);

        if (firstIndex + 3 >= recvBytes.Length) return false;
        var lastIndex = firstIndex + 2;
        byte sum = 0;
        do
        {
            if (recvBytes[lastIndex] == 69 && sum == recvBytes[lastIndex - 1]) break;
            sum += recvBytes[lastIndex - 1];
            lastIndex++;
        } while (lastIndex < recvBytes.Length);

        if (lastIndex < firstIndex + 3) return false;
        var len = lastIndex - firstIndex - 2;
        data = new byte[len];
        Array.Copy(recvBytes, firstIndex + 1, data, 0, len);
        return true;
    }

    private bool CheckStringField(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 12) return false;
        if (value.Any(p => p is '=' or '\'')) return false;
        return true;
    }

    private bool GetIntValue(string key, string input, out int value)
    {
        value = 0;
        var pattern = $"{key}\\s*\\=\\s*'\\s*(?<value>(\\+|\\-)?\\d+)\\s*'";
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        var sValue = match.Groups["value"].Value;
        value = int.Parse(sValue);
        return true;
    }

    private bool GetStringValue(string key, string input, out string value)
    {
        value = default;
        var pattern = $"{key}\\s*\\=\\s*'\\s*(?<value>[^']*)\\s*'";
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        value = match.Groups["value"].Value;
        return true;
    }

    private bool GetStringValues(string key, string input, out List<ItemData<string>> items)
    {
        items = new List<ItemData<string>>();
        var pattern = $"({key}(?<idx>\\d+)\\s*\\=\\s*'(?<ctx>[^']*)'\\s*)+";
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        var idxCaps = match.Groups["idx"].Captures;
        var ctxCaps = match.Groups["ctx"].Captures;
        if (idxCaps.Count != ctxCaps.Count) return false;
        for (var i = 0; i < idxCaps.Count; i++)
        {
            var index = idxCaps[i].Value;
            var content = ctxCaps[i].Value;
            var item = new ItemData<string>
            {
                Index = int.Parse(index),
                Content = content
            };
            items.Add(item);
        }

        return true;
    }

    private OpStatus ExecuteCmd(Func<string> func, CmdFlags cmdFlag)
    {
        if (!State) return OpStatus.NetworkError;
        if (func == null) return OpStatus.ParametersError;
        var cmd = func();
        if (string.IsNullOrWhiteSpace(cmd)) return OpStatus.ParametersError;
        var recv = SendCmd(cmdFlag, cmd);
        return GetOpStatus(recv);
    }

    private string SendCmd(CmdFlags cmdFlag, string cmd = "S   E")
    {
        var bytes = Encoding.GetEncoding("GB2312").GetBytes(cmd);
        bytes[1] = (byte)cmdFlag;
        return SendCmd(bytes);
    }

    private static OpStatus GetOpStatus(string recv)
    {
        if (string.IsNullOrWhiteSpace(recv)) return OpStatus.ResultError;
        if (recv.IndexOf("OK", StringComparison.OrdinalIgnoreCase) >= 0)
            return OpStatus.Success;
        if (recv.IndexOf("FAIL", StringComparison.OrdinalIgnoreCase) >= 0)
            return OpStatus.Fail;
        if (recv.IndexOf("No Power", StringComparison.OrdinalIgnoreCase) >= 0) return OpStatus.NoPower;
        return OpStatus.ResultError;
    }

    private int GetStateFlag(bool state)
    {
        return state ? 1 : 0;
    }
}