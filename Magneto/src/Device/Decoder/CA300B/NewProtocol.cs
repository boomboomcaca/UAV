using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.CA300B;

public class NewProtocol
{
    public delegate void SearchFinishDelegate(double frequency, TvStandard standard, int count, string msg = "");

    private readonly ConcurrentQueue<byte[]> _cmdCache = new();
    private readonly ConcurrentQueue<byte[]> _recvDataCache = new();
    private readonly Socket _socket;
    private CancellationTokenSource _cts;
    private int _dvbtBandwidth = 8000;
    private Task _recvDataTask;
    private Task _sendCmdTask;
    private TvStandard _tvStandard = TvStandard.ANAFM;

    public NewProtocol(Socket socket)
    {
        _socket = socket;
    }

    public event SearchFinishDelegate SearchFinish;
    public event EventHandler<List<ChannelProgramInfo>> SearchResult;
    public event EventHandler<SDataPlayResult> PlayResult;

    public void Start(int dvbtBandwidth)
    {
        _mode = 0;
        _dvbtBandwidth = dvbtBandwidth;
        _cts = new CancellationTokenSource();
        _sendCmdTask = new Task(p => SendCmdAsync(p).ConfigureAwait(false), _cts.Token);
        _sendCmdTask.Start();
        _recvDataTask = new Task(p => RecvDataAsync(p).ConfigureAwait(false), _cts.Token);
        _recvDataTask.Start();
        // SendCmd($"{CA300BProtocol.DVBT8M}\r\n");
        SendCmd("*PATH?\r\n");
    }

    public void SendCmd(string cmd)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd);
        Trace.WriteLine($"==>{cmd}");
        _cmdCache.Enqueue(buffer);
    }

    public void SendCmd(byte[] cmd)
    {
        var buffer = BitConverter.ToString(cmd);
        Trace.WriteLine($"==>{buffer}");
        _cmdCache.Enqueue(cmd);
    }

    /// <summary>
    ///     搜台流程
    /// </summary>
    /// <param name="searchData"></param>
    /// <param name="token"></param>
    internal async Task SearchFlowAsync(List<SearchInfo> searchData, CancellationToken token)
    {
        _mode = 1;
        _isCancelled = false;
        // 1. 查询一次当前制式
        SendCmd("*PATH?\r\n");
        await Task.Delay(1000, token).ConfigureAwait(false);
        _programCache.Clear();
        foreach (var item in searchData)
        {
            ResetCache();
            if (_isCancelled) break;
            var res = await SearchAsync(item, item.Standard, CancellationToken.None);
            if (_isCancelled) break;
            SearchFinish?.Invoke(_frequency, item.Standard, res ? _programCount : 0);
        }

        SearchResult?.Invoke(null, _programCache);
        _isCancelled = false;
    }

    internal async Task PlayFlowAsync(string cmd, CancellationToken token)
    {
        _mode = 2;
        ResetCache();
        var playResult = new SDataPlayResult();
        var errMsg = string.Empty;
        var splits = cmd.Split("|");
        var sp = splits[2];
        var index = sp.IndexOf(splits[1], StringComparison.Ordinal);
        if (index >= 0) sp = sp.Remove(index, splits[1].Length);
        if (string.IsNullOrEmpty(sp)) sp = "0";
        var programNum = ushort.Parse(sp);
        var freq = double.Parse(splits[1]);
        var name = splits[3];
        playResult.OperateType = OperateType.RealPlayStart;
        playResult.ProgramNumber = programNum;
        playResult.ProgramName = name;
        playResult.Frequency = freq;
        if (!Enum.TryParse(splits[0], out TvStandard standard))
        {
            playResult.Standard = splits[0];
            playResult.Result = false;
            playResult.Uri = $"打通制式{splits[0]}失败，不支持的制式！";
            PlayResult?.Invoke(null, playResult);
            return;
        }

        playResult.Standard = Utils.GetNameByDescription(standard);
        var isPlaySuccess = false;
        var open = await OpenChannelAsync(standard, token);
        if (!open)
        {
            playResult.Result = false;
            playResult.Uri = $"打通制式{standard}失败!";
            PlayResult?.Invoke(null, playResult);
            return;
        }

        // 播放模拟电视
        if (standard == TvStandard.ANATV)
        {
            freq += 2;
            var nFreq = (int)(freq * 100);
            var tmp = new byte[11];
            var headLen = Ca300BProtocol.AnatvPlayHead.Length;
            Array.Copy(Ca300BProtocol.AnatvPlayHead, tmp, headLen);
            var bStart = BitConverter.GetBytes(nFreq);
            Array.Reverse(bStart);
            Array.Copy(bStart, 1, tmp, headLen, 3);
            var checkSum = CalcCrc(tmp, headLen + 3);
            checkSum[0] = 0x02;
            checkSum[1] = 0x77;
            Array.Copy(checkSum, 0, tmp, headLen + 3, 2);
            tmp[9] = 0x0D;
            tmp[10] = 0x0A;
            SendCmd(tmp);
        }
        else if (standard is TvStandard.DTMB or TvStandard.DVBT)
        {
            var bNum = BitConverter.GetBytes(programNum);
            //string num = Convert.ToString(bNum[1], 16) + Convert.ToString(bNum[0], 16);
            //num = num.PadLeft(4, '0');
            bNum = bNum.Reverse().ToArray();
            var num = BitConverter.ToString(bNum).Replace("-", "");
            var info = Ca300BProtocol.DtmbPlayHead + num.ToLower() + ";\r\n";
            SendCmd(info);
        }

        var waitSpan = 30000;
        _autoResetEvent.WaitOne(waitSpan);
        Console.WriteLine($"播放结果:{_isPlaySuccess},{_errorMessage}");
        if (_isPlaySuccess)
            _errorMessage = $"http://{RunningInfo.EdgeIp}:{RunningInfo.Port}/{PublicDefine.PathVideo}/real/index.m3u8";
        isPlaySuccess = _isPlaySuccess;
        errMsg = _errorMessage;
        playResult.Result = isPlaySuccess;
        playResult.Uri = errMsg;
        PlayResult?.Invoke(null, playResult);
    }

    internal void CancelSearch()
    {
        try
        {
            _isCancelled = true;
            Console.WriteLine("取消指令执行");
        }
        catch
        {
        }
    }

    internal void Close()
    {
        try
        {
            _cts?.Cancel();
            _isCancelled = true;
        }
        catch
        {
        }
    }

    private void ResetCache()
    {
        _codeRate = 0;
        _searchFinish = false;
        _programCount = 0;
        _frequency = 0;
        _waitRecvDataSign = false;
        _playFinish = false;
        _isPlaySuccess = false;
        _errorMessage = "";
        _autoResetEvent.Reset();
    }

    private async Task<bool> SearchAsync(SearchInfo info, TvStandard tvStandard, CancellationToken token)
    {
        // 1. 打通通道
        var open = await OpenChannelAsync(tvStandard, token);
        if (!open) return false;
        // 2. 搜索
        _frequency = info.Frequency;
        var res = SendSearchCmd(_frequency, tvStandard);
        if (!res) return false;
        var waitSpan = 30000;
        if (tvStandard == TvStandard.ANATV) waitSpan = 2000;
        if (tvStandard == TvStandard.DTMB) waitSpan = 30000;
        if (tvStandard == TvStandard.DVBT) waitSpan = 45000;
        // 取消搜台时需要等待上一次搜台结束才可以停止，否则会出现命令解析混乱的问题
        // if (token.IsCancellationRequested)
        // {
        //     return false;
        // }
        // token.WaitHandle.WaitOne(waitSpan);
        _autoResetEvent.WaitOne(waitSpan);
        return _searchFinish;
    }

    /// <summary>
    ///     打通通道
    /// </summary>
    /// <param name="tvStandard"></param>
    /// <param name="token"></param>
    private async Task<bool> OpenChannelAsync(TvStandard tvStandard, CancellationToken token)
    {
        if (_tvStandard != tvStandard)
        {
            string channelCmd;
            // 根据电视制式打通对应通道
            switch (tvStandard)
            {
                case TvStandard.ANATV:
                    channelCmd = string.Format(Ca300BProtocol.OpenChannelCmd, 3);
                    break;
                case TvStandard.DTMB:
                    channelCmd = string.Format(Ca300BProtocol.OpenChannelCmd, 1);
                    break;
                case TvStandard.DVBT:
                    channelCmd = string.Format(Ca300BProtocol.OpenChannelCmd, 2);
                    break;
                default:
                    // 不支持的制式直接跳过
                    return false;
            }

            SendCmd(channelCmd);
            // TODO : 这里为何要等待5秒？等待硬件启动？
            await Task.Delay(15000, token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return false;
            if (_dvbtBandwidth == 6000 && tvStandard == TvStandard.DVBT)
            {
                SendCmd($"{Ca300BProtocol.Dvbt6M}\r\n");
                await Task.Delay(5000, token).ConfigureAwait(false);
            }

            // else if (isSearch)
            // {
            //     // 这里只能在搜台时进行初始化，如果在播放时也初始化，会由于恢复了出厂设置而造成播放失败，提示0x02:"播放节目频道ID号错误"
            //     SendCmd($"{CA300BProtocol.DVBT8M}\r\n");
            //     await Task.Delay(5000, token).ConfigureAwait(false);
            // }
            if (token.IsCancellationRequested) return false;
        }

        if (_tvStandard != tvStandard)
        {
            // 防止下发了打通通道但是通道没有正常打通？
            Trace.WriteLine($"打开通道{tvStandard}失败");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     下发搜索指令
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="tVStandard"></param>
    private bool SendSearchCmd(double frequency, TvStandard tVStandard)
    {
        switch (tVStandard)
        {
            case TvStandard.ANATV:
            {
                frequency += 2;
                var nFreq = (int)(frequency * 100);
                var cmd = new byte[11];
                var headLen = Ca300BProtocol.AnatvPlayHead.Length;
                Array.Copy(Ca300BProtocol.AnatvPlayHead, cmd, headLen);
                var bStart = BitConverter.GetBytes(nFreq);
                Array.Reverse(bStart);
                Array.Copy(bStart, 1, cmd, headLen, 3);
                var checkSum = CalcCrc(cmd, headLen + 3);
                checkSum[0] = 0x02;
                checkSum[1] = 0x77;
                Array.Copy(checkSum, 0, cmd, headLen + 3, 2);
                cmd[9] = 0x0D;
                cmd[10] = 0x0A;
                SendCmd(cmd);
                return true;
            }
            case TvStandard.DTMB:
            case TvStandard.DVBT:
            {
                if (frequency is > 858 or < 473 && frequency != 0) return false;
                var cmd = frequency == 0
                    ? $"{Ca300BProtocol.Dtmbhead}{frequency.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')};\r\n"
                    : $"{Ca300BProtocol.Dtmbhead}{frequency};\r\n";
                SendCmd(cmd);
                return true;
            }
            default:
                return false;
        }
    }

    private async Task SendCmdAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                // await Task.Delay(1, token).ConfigureAwait(false);
                if (_cmdCache.IsEmpty)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                if (!_cmdCache.TryDequeue(out var cmd))
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                await _socket.SendAsync(cmd, SocketFlags.None, token);
            }
            catch
            {
            }
    }

    private async Task RecvDataAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var buffer = new byte[1024];
        List<byte> list = new();
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token).ConfigureAwait(false);
            try
            {
                var count = await _socket.ReceiveAsync(buffer, SocketFlags.None, token);
                if (count <= 0)
                {
                    // Console.WriteLine("收到数据个数为0，重连设备");
                    // _socket.Dispose();
                    // return;
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                for (var i = 0; i < count; i++)
                {
                    list.Add(buffer[i]);
                    if (list[^1] == 0x0a && list[^2] == 0x0d)
                    {
                        _recvDataCache.Enqueue(list.ToArray());
                        list.Clear();
                    }
                }

                ProcessData();
            }
            catch
            {
            }
        }
    }

    private void ProcessData()
    {
        if (_recvDataCache.IsEmpty) return;
        while (_recvDataCache.TryDequeue(out var buffer))
        {
            Trace.WriteLine($"<--{BitConverter.ToString(buffer)}");
            if (buffer.Length < 4) continue;
            if (buffer[0] == 0x2a)
            {
                ///// *PATH?查询返回
                // var str = Encoding.ASCII.GetString(buffer).TrimEnd('\r', '\n');
                _tvStandard = GetCurStandard(buffer);
                Trace.WriteLine($"当前制式为:{_tvStandard}");
            }
            else if (buffer[0] == 0xff)
            {
                // 数字电视
                switch (buffer[1])
                {
                    case 0x00:
                        // 数据类型为0x00，且数据为0x02标识等待数据接收，需要接收两次，0x00数据一起返回
                        if (buffer[4] == 0x02 || buffer[4] == 0x00)
                            // 等待接收
                            _waitRecvDataSign = true;
                        else if (buffer[4] == 0x01) _errorMessage = "指令错误";
                        break;
                    case 0x06: // 频率
                    case 0x07: // 信道功率
                    case 0x08: // 信道锁定状态
                    case 0x09: // 信号质量
                    case 0x0a: // 载波调制方式
                    case 0x0b: // 时域交织方式
                    case 0x0c: // 传输模式（FFT MODE）
                    case 0x0d: // 保护间隔
                    case 0x0e: // 误码率
                        // 以上信息暂时不做处理
                        break;
                    case 0x0f: // 码率
                        _codeRate = buffer[4];
                        break;
                    case 0x10: // 节目信息
                    case 0x12: // 所有节目信息
                        _searchFinish = true;
                        var list = AnalysisAvProgram(_tvStandard, _frequency, buffer, _codeRate);
                        _programCache.AddRange(list);
                        _programCount = list.Count;
                        break;
                    case 0x11: // 操作信息
                        if (_mode == 1 && _waitRecvDataSign && _frequency > 0)
                        {
                            if (buffer[4] == 0x03)
                            {
                                // 没有搜到台
                                _searchFinish = true;
                                _programCount = 0;
                            }

                            // ??????
                            _searchFinish = true;
                            _programCount = 0;
                        }
                        else if (_mode == 2)
                        {
                            object obj = new();
                            Ca300BProtocol.CopyBytes2Struct(buffer, 0, typeof(ProgramPlayRtnInfo), ref obj);
                            var programPlayRtnInfo = (ProgramPlayRtnInfo)obj;
                            var dTmbPlayErrorCode = (DtmbPlayErrorCode)programPlayRtnInfo.Data;
                            _isPlaySuccess = dTmbPlayErrorCode == DtmbPlayErrorCode.PlaySuccess;
                            _errorMessage = dTmbPlayErrorCode.GetDescriptionByName();
                            _playFinish = true;
                        }

                        break;
                    case 0x13: // 当前音量值
                    case 0x14: // 当前搜台制式 DVBT or DVBT2
                        break;
                }
            }
            else
            {
                // 模拟
                if (buffer[4] != 0xaa && _tvStandard == TvStandard.ANATV && _mode == 1 && _frequency > 0)
                {
                    ChannelProgramInfo chn = new()
                    {
                        Index = 0,
                        Ca = false,
                        ProgramName = "模拟电视",
                        FlowType = "ANA TV",
                        Standard = TvStandard.ANATV.ToString(),
                        ProgramNumber = $"{_frequency}0"
                    };
                    _programCache.Add(chn);
                    _programCount = 1;
                    _searchFinish = true;
                }
                else if (_mode == 2 && buffer[4] == 0xaa)
                {
                    _isPlaySuccess = true;
                    _playFinish = true;
                }
            }
        }

        if (_searchFinish && _mode == 1)
            _autoResetEvent.Set();
        else if (_mode == 2 && _playFinish) _autoResetEvent.Set();
    }

    /// <summary>
    ///     解析数字节目信息
    /// </summary>
    /// <param name="standard"></param>
    /// <param name="frequency">频率信息</param>
    /// <param name="info"></param>
    /// <param name="codeRate"></param>
    private static List<ChannelProgramInfo> AnalysisAvProgram(TvStandard standard, double frequency, byte[] info,
        byte codeRate)
    {
        var list = new List<ChannelProgramInfo>();
        if (codeRate == 0) return list;
        var obj = new object();
        Ca300BProtocol.CopyBytes2Struct(info, 0, typeof(ProgramInfoHead), ref obj);
        var programInfoHead = (ProgramInfoHead)obj;
        var headLen = Marshal.SizeOf(programInfoHead);
        var index = headLen;
        for (var i = 0; i < programInfoHead.ProgramCnt; i++)
        {
            var program = new ChannelProgramInfo
            {
                Frequency = frequency,
                Standard = standard.ToString()
            };
            Ca300BProtocol.CopyBytes2Struct(info, index, typeof(ProgramInfo), ref obj);
            var programInfo = (ProgramInfo)obj;
            index += Marshal.SizeOf(programInfo);
            Array.Reverse(programInfo.ProgramId, 0, 2);
            program.ProgramNumber = $"{program.Frequency}{BitConverter.ToUInt16(programInfo.ProgramId, 0)}";
            program.Resolution = codeRate.ToString();
            program.Ca = programInfo.ProgramEncrypt == 0x01;
            if (info[index] == 0x14)
            {
                var programName = new byte[programInfo.ProgramNameLen - 1];
                Array.Copy(info, index + 1, programName, 0, programInfo.ProgramNameLen - 1);
                program.ProgramName =
                    Utils.GetEncodeString(programName, "utf-16BE"); //Encoding.Default.GetString(programName);
                Console.WriteLine($"特殊标记:0x14,{program.ProgramName},{BitConverter.ToString(programName)}");
            }
            else
            {
                var programName = new byte[programInfo.ProgramNameLen];
                Array.Copy(info, index, programName, 0, programInfo.ProgramNameLen);
                program.ProgramName = Utils.GetGb2312String(programName); //Encoding.Default.GetString(programName);
            }

            index += programInfo.ProgramNameLen;
            program.Index = i;
            list.Add(program);
        }

        return DistinctSameTvName(list);
    }

    /// <summary>
    ///     去除名字相同的电视台
    /// </summary>
    /// <param name="sDataAvPrograms"></param>
    private static List<ChannelProgramInfo> DistinctSameTvName(List<ChannelProgramInfo> sDataAvPrograms)
    {
        var serNames = sDataAvPrograms.Select(item => item.ProgramName).Distinct().ToList();
        var result = new List<ChannelProgramInfo>();
        foreach (var name in serNames)
        {
            var program = sDataAvPrograms.Find(item => item.ProgramName == name);
            result.Add(program);
        }

        return result;
    }

    /// <summary>
    ///     获取当前制式
    /// </summary>
    /// <param name="data"></param>
    private static TvStandard GetCurStandard(byte[] data)
    {
        if (data.Length < 7) return TvStandard.ANAFM;
        var mode = Convert.ToInt32(data[6]) - 48;
        return mode switch
        {
            1 => TvStandard.DTMB,
            2 => TvStandard.DVBT,
            3 => TvStandard.ANATV,
            _ => TvStandard.ANAFM
        };
    }

    /// <summary>
    ///     计算CRC校验
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="len"></param>
    private static byte[] CalcCrc(byte[] cmd, int len)
    {
        short crc = 0;
        for (var i = 0; i < len; i++)
            if (i == 0)
                crc = cmd[0];
            else
                crc += cmd[i];
        var bCrc = BitConverter.GetBytes(crc);
        Array.Reverse(bCrc);
        return bCrc;
    }

    #region 流程定义

    private bool _isCancelled;

    /// <summary>
    ///     等待接收标记，必须有这个标记才可以，否则丢弃数据
    /// </summary>
    private bool _waitRecvDataSign;

    private readonly AutoResetEvent _autoResetEvent = new(false);
    private byte _codeRate;

    /// <summary>
    ///     搜台完毕
    /// </summary>
    private bool _searchFinish;

    private int _programCount;
    private readonly List<ChannelProgramInfo> _programCache = new();
    private double _frequency;
    private string _errorMessage = "";
    private bool _isPlaySuccess;
    private bool _playFinish;

    /// <summary>
    ///     设备模式
    ///     0- 默认
    ///     1- 搜台
    ///     2- 播放
    /// </summary>
    private int _mode;

    #endregion
}