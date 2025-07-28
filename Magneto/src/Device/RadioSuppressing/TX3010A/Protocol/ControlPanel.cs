// #define DEBUG

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device.TX3010A.Protocol;

public class ControlPanel : RadioSuppressingBase
{
    #region 指令

    /// <summary>
    ///     功放模块使能
    ///     PA_On:pa1,pa2,pa3
    /// </summary>
    private const string CmdPaOn = "PA_ON:";

    #endregion

    private readonly ConcurrentQueue<string> _buffer = new();

    /// <summary>
    ///     缓存功放运行状态
    ///     键为控制板通道号，范围1~3
    ///     对外公布时需要转换为总系统的通道号
    /// </summary>
    private readonly ConcurrentDictionary<int, SDataRadioSuppressing> _statusCache = new();

    private CancellationTokenSource _cts;
    private Task _monitorTask;
    private volatile bool _sync;

    public ControlPanel(string ipAddress, int port, int index) : base(ipAddress, port, index)
    {
        SubSystemIndex = index;
        Name = $"控制板{index}({ipAddress}:{port})";
    }

    public int SubSystemIndex { get; }
    public override DeviceType DeviceType => DeviceType.ControlPanel;
    public event EventHandler<List<SDataRadioSuppressing>> StatusChanged;

    /// <summary>
    ///     设备掉线事件
    /// </summary>
    public event EventHandler<DeviceDisconnectedEventArgs> Disconnected;

    public override bool Initialized()
    {
        var success = base.Initialized();
        if (!success)
        {
            Init = true;
            return false;
        }

        _cts = new CancellationTokenSource();
        _monitorTask = new Task(Monitor, _cts.Token);
        _monitorTask.Start();
        Enabled = true;
        Init = true;
        return true;
    }

    public override bool Reinitialize()
    {
        if (!Init) return false;
        if (Enabled) return true;
        Utils.CancelTask(_monitorTask, _cts);
        Socket?.Dispose();
        Enabled = false;
        return Initialized();
    }

    /// <summary>
    ///     打开功放
    /// </summary>
    /// <param name="powerSwitch">开关使能状态</param>
    /// <param name="retry"></param>
    public bool EnabledPower(Dictionary<int, bool> powerSwitch, bool retry = true)
    {
        _statusCache.Clear();
        var list = new List<string>();
        for (var i = 1; i <= 3; i++)
        {
            var powerOn = powerSwitch.ContainsKey(i) && powerSwitch[i];
            list.Add(powerOn ? "1" : "0");
        }

        var cmd = $"{CmdPaOn}{string.Join(',', list)}";
        if (retry)
        {
            var attempt = 0;
            bool b;
            do
            {
                b = SendSyncCmd(cmd, @"PA_Onsta\s*\:\s*" + string.Join(@"\,\s*", list));
                if (b) break;
                Thread.Sleep(100);
                attempt++;
            } while (attempt < 5);

            return b;
        }

        SendCmd(cmd);
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (Disposed) return;
        Utils.CancelTask(_monitorTask, _cts);
        Socket?.Dispose();
        Enabled = false;
        Init = false;
        Disposed = true;
    }

    private bool SendSyncCmd(string cmd, string regex, int timeout = 500)
    {
        _sync = true;
        SendCmd(cmd);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var sb = new StringBuilder();
        var s1 = sb.ToString();
        try
        {
            while (stopwatch.ElapsedMilliseconds < timeout)
            {
                //
                var b = _buffer.TryDequeue(out var str);
                if (!b || string.IsNullOrEmpty(str))
                {
                    Thread.Sleep(100);
                    continue;
                }

                sb.Append(str);
                var s = s1;
                if (Regex.IsMatch(s, regex)) return true;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"同步发送命令异常，异常信息：{ex}");
        }
        finally
        {
            _sync = false;
            stopwatch.Stop();
        }

        return false;
    }

    private void Monitor()
    {
        var buffer = new byte[1024];
        while (_cts?.IsCancellationRequested == false)
            try
            {
                if (Socket is not { Connected: true })
                {
                    Enabled = false;
                    Disconnected?.Invoke(this, new DeviceDisconnectedEventArgs(SubSystemIndex));
                    break;
                }

                var count = Socket.Receive(buffer, SocketFlags.None);
                if (count == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var str = Encoding.ASCII.GetString(buffer, 0, count);
                if (string.IsNullOrEmpty(str))
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (_sync) _buffer.Enqueue(str);
                var array = str.Split('\n');
                foreach (var recv in array) ReceivDataProcess(recv);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode is SocketError.Shutdown or SocketError.NotConnected)
                {
                    Enabled = false;
                    Disconnected?.Invoke(this, new DeviceDisconnectedEventArgs(SubSystemIndex));
                }
                //Trace.WriteLine($"{_name}通信异常，异常代码：{e.SocketErrorCode}");
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
            }
    }

    private void ReceivDataProcess(string recvData)
    {
        if (string.IsNullOrWhiteSpace(recvData)) return;
        if (Regex.IsMatch(recvData, @"STA\s*\:\s*fault", RegexOptions.IgnoreCase)) //运行故障
            return;
        var pattern = @"(?<head>\w+)\s*\:\s*((?<value>\d+)\s*\,\s*){2,}(?<value>\d+)";
        var matches = Regex.Matches(recvData, pattern);
        if (matches.Any() != true) return;
        foreach (Match match in matches)
        {
            if (!match.Success) continue;
            var cmdHead = match.Groups["head"].Value;
            var valueCaps = match.Groups["value"].Captures;
            var values = new List<int>();
            var isOk = true;
            foreach (Capture capture in valueCaps)
            {
                if (!int.TryParse(capture.Value, out var value))
                {
                    isOk = false;
                    break;
                }

                values.Add(value);
            }

            if (!isOk) continue;
            for (var i = 0; i < values.Count; i++)
            {
                var deviceChannel = i + 1;
                UpdateStatus(deviceChannel, cmdHead, values[i]);
            }

            StatusChanged?.Invoke(this, _statusCache.Values.ToList());
        }
    }

    private void UpdateStatus(int deviceChannel, string cmdHead, int status)
    {
        if (string.IsNullOrWhiteSpace(cmdHead)) return;
        if (!_statusCache.ContainsKey(deviceChannel))
            _statusCache.TryAdd(deviceChannel,
                new SDataRadioSuppressing { ChannelNumber = deviceChannel, Warning = string.Empty });
        var exist = _statusCache.TryGetValue(deviceChannel, out var data);
        if (!exist || data == null) return;
        var warn = new List<string>();
        if (!string.IsNullOrWhiteSpace(data.Warning)) warn.AddRange(data.Warning.Split(','));
        if (cmdHead.Contains(RBist, StringComparison.OrdinalIgnoreCase))
        {
            if (status == 1)
                warn.Add("自检失败");
            else
                warn.Remove("自检失败");
        }
        else if (cmdHead.Contains(RPaOnsta, StringComparison.OrdinalIgnoreCase))
        {
            data.Power = status == 0 ? -1 : 1;
        }
        else if (cmdHead.Contains(RGwAlarm, StringComparison.OrdinalIgnoreCase))
        {
            data.OverHeating = status == 1;
        }
        else if (cmdHead.Contains(RVswrAlarm, StringComparison.OrdinalIgnoreCase))
        {
            data.Vsw = status == 1;
        }
        else if (cmdHead.Contains(RPaState, StringComparison.OrdinalIgnoreCase))
        {
            if (status == 1)
            {
                data.Power = 1;
                warn.Add("功放故障");
            }
            else
            {
                warn.Remove("功放故障");
            }
        }
        else
        {
            return;
        }

        data.Warning = warn.Count > 0 ? string.Join(",", warn) : string.Empty;
    }

    #region 反馈消息

    /// <summary>
    ///     开机自检上报
    /// </summary>
    private const string RBist = "BIST";

    /// <summary>
    ///     功放使能反馈
    /// </summary>
    private const string RPaOnsta = "PA_Onsta";

    /// <summary>
    ///     过温告警上报
    /// </summary>
    private const string RGwAlarm = "GW_Alarm";

    /// <summary>
    ///     驻波告警上报
    /// </summary>
    private const string RVswrAlarm = "VSWR_alarm";

    /// <summary>
    ///     功放状态上报
    /// </summary>
    private const string RPaState = "PA_State";

    /// <summary>
    ///     运行故障状态
    /// </summary>
    private const string RStaFault = "STA:fault";

    /// <summary>
    ///     温度上报
    /// </summary>
    private const string RTemp = "TEMP:";

    #endregion
}