/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\S7000.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/28
 *
 * 修    改:
 *
 * 备    注:	S7000电视分析仪模块功能与业务操作逻辑
 *
 *********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class S7000 : DeviceBase
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="mid">不可修改的本模块ID号，标示身份使用</param>
    public S7000(Guid mid)
        : base(mid)
    {
    }

    /// <summary>
    ///     分解要搜索的节目
    /// </summary>
    /// <param name="array"></param>
    private List<SearchInfo> GetSearchCount(SearchProgramTemplate[] array)
    {
        var list = new List<SearchInfo>();
        foreach (var item in array)
        {
            var tvStandard = item.Standard;
            var start = item.StartFrequency;
            var stop = item.StopFrequency;
            var step = _searchBandwidth;
            var freq = start;
            var freqs = new List<double>();
            while (freq <= stop)
            {
                freqs.Add(freq);
                freq += step / 1000;
            }

            foreach (TvStandard standard in Enum.GetValues(typeof(TvStandard)))
            {
                if ((tvStandard & standard) == 0) continue;
                foreach (var f in freqs)
                {
                    var s = new SearchInfo
                    {
                        Frequency = f,
                        Standard = standard
                    };
                    list.Add(s);
                }
            }
        }

        _totalSearchCount = list.Count;
        return list;
    }

    #region 字段|属性

    /// <summary>
    ///     消息队列
    /// </summary>
    private readonly ConcurrentQueue<List<object>> _dataQueue = new();

    private CancellationTokenSource _cts;

    /// <summary>
    ///     将接收到的数据同步的发送到客户端
    /// </summary>
    private Task _sendDataTask;

    /// <summary>
    ///     指令发送线程
    /// </summary>
    private Task _sendCmdTask;

    /// <summary>
    ///     设备控制客户端类
    /// </summary>
    private Client _client;

    /// <summary>
    ///     发送的字节流
    /// </summary>
    private byte[] _buffer;

    /// <summary>
    ///     模拟电视搜索
    /// </summary>
    private ProgramSearchAnatv _programSearchAnatv;

    /// <summary>
    ///     数字节目搜索（含DTMB/DVB-T/DVB-T2）
    /// </summary>
    private ProgramSearchAllInOne _programSearchAllInOne;

    /// <summary>
    ///     模拟节目播放
    /// </summary>
    private ProgramPlayAnatv _programPlayAnatv;

    /// <summary>
    ///     数字节目播放（含DTMB/DVB-T/DVB-T2）
    /// </summary>
    private ProgramPlayAllInOne _programPlayAllInOne;

    /// <summary>
    ///     心跳锁，用于重连设备
    /// </summary>
    public static readonly object MLock = new();

    /// <summary>
    ///     指令队列
    /// </summary>
    private readonly ConcurrentQueue<ProgramCmd> _cmdQueue = new();

    ///// <summary>
    /////     数据接收超时次数
    ///// </summary>
    //private uint _timeoutCount;

    /// <summary>
    ///     计时器，用于心跳检查：如果与设备有数据交互，则重置_seconds = 0；没有数据交互则_seconds++。
    /// </summary>
    private static uint _seconds;

    public static void ResetSeconds()
    {
        _seconds = 0;
    }

    #region 节目搜索

    /// <summary>
    ///     缓存搜到的节目信息
    /// </summary>
    private readonly List<ChannelProgramInfo> _programCache = new();

    private readonly object _lockProgramCache = new();
    private int _searchProgress;
    private int _totalSearchCount;

    /// <summary>
    ///     搜索节目的搜索带宽 kHz
    /// </summary>
    private readonly double _searchBandwidth = 8000;

    /// <summary>
    ///     发送标记，防止重复发送
    /// </summary>
    private bool _sendSearchResultOk;

    private readonly AutoResetEvent _searchResultEvent = new(false);

    #endregion

    #endregion

    #region DeviceBase

    /// <summary>
    ///     初始化S7000设备
    /// </summary>
    /// <param name="mi">模块ID号</param>
    /// <returns>true=成功；false=失败</returns>
    public override bool Initialized(ModuleInfo mi)
    {
        var result = base.Initialized(mi);
        if (!result) return false;
        Dispose();
        _client ??= new Client(Ip);
        for (var i = 0; i < 2; i++)
            // 建立网络连接
            result = _client.Connect();
        if (!result) return false;
        // 初始化设备
        InitS7000();
        SetHeartBeat(_client.Socket);
        return true;
    }

    /// <summary>
    ///     开始
    /// </summary>
    /// <param name="feature"></param>
    /// <param name="dataPort"></param>
    /// <returns>true=成功；false=失败</returns>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _cts = new CancellationTokenSource();
        _sendDataTask = new Task(p => MySendDataAsync(p).ConfigureAwait(false), _cts.Token);
        _sendDataTask.Start();
        _sendCmdTask = new Task(p => SendCmdAsync(p).ConfigureAwait(false), _cts.Token);
        _sendCmdTask.Start();
    }

    /// <summary>
    ///     停止
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    public override void Stop()
    {
        StopS7000();
        base.Stop();
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        StopS7000();
        if (_programSearchAnatv != null)
        {
            _programSearchAnatv.SearchComplete -= Program_SearchComplete;
            _programSearchAnatv = null;
        }

        if (_programSearchAllInOne != null)
        {
            _programSearchAllInOne.SearchComplete -= Program_SearchComplete;
            _programSearchAllInOne = null;
        }

        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }

        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Function

    /// <summary>
    ///     搜索播放线程函数
    /// </summary>
    /// <param name="obj"></param>
    private async Task SendCmdAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10).ConfigureAwait(false);
            try
            {
                if (_cmdQueue.IsEmpty) continue;
                if (!_cmdQueue.TryDequeue(out var programCmd)) continue;
                lock (MLock)
                {
                    switch (programCmd.Type)
                    {
                        // 搜索
                        case ProgramType.Search:
                            if (programCmd.Cmd is SearchProgramTemplate[] array) Search(array);
                            break;
                        // 播放
                        case ProgramType.Play:
                            Play(programCmd.Cmd.ToString());
                            break;
                    }
                }

                programCmd = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send Data To Devices Error:{ex.Message}");
            }
        }
    }

    /// <summary>
    ///     搜索节目
    /// </summary>
    /// <param name="cmd"></param>
    private void Search(SearchProgramTemplate[] cmd)
    {
        lock (_lockProgramCache)
        {
            _programCache.Clear();
        }

        _searchProgress = 0;
        _totalSearchCount = 0;
        var list = GetSearchCount(cmd);
        _sendSearchResultOk = false;
        _cancelSearch = false;
        foreach (var info in list)
        {
            Console.WriteLine($"开始搜索:{info.Frequency},{info.Standard}...");
            _searchProgress++;
            if (_cancelSearch) break;
            // 不支持的制式不进行搜台
            if ((info.Standard & (TvStandard.ANATV | TvStandard.DTMB | TvStandard.DVBT)) == 0)
            {
                SearchComplete(info.Frequency, info.Standard, 0);
                continue;
            }

            try
            {
                _programPlayAllInOne.Reset();
                _searchResultEvent.Reset();
                // 如果执行搜索，那么就要重置节目播放
                // 搜索节目
                GetProgramSearch(info.Standard).Search(info.Frequency, _searchBandwidth, info.Standard);
                _searchResultEvent.WaitOne();
                //_timeoutCount = 0;
            }
            catch (Exception ex)
            {
                // 强制Abort线程时不需要执行以下方法
                if (ex is not TaskCanceledException)
                    if (ex.Message.Contains("数组不能为空") || ex.Message.Contains("Array cannot be null"))
                        // 如果出现这样的异常，后面的频点都会出现异常
                        RebootDevice();
                if (_cancelSearch) break;
                SearchComplete(info.Frequency, info.Standard, 0, ex.Message);
            }
        }

        _cancelSearch = false;
        SendSearchResult();
    }

    //private int _changeFrequencyCount = 0;
    private double _currentFrequency;

    /// <summary>
    ///     播放节目
    /// </summary>
    /// <param name="cmd"></param>
    private void Play(string cmd)
    {
        var playResult = new SDataPlayResult();
        try
        {
            var splits = cmd.Split("|");
            var sp = splits[2];
            sp = sp.Replace(splits[1], "");
            var programNum = short.Parse(sp);
            var freq = double.Parse(splits[1]);
            var name = splits[3];
            // 节目切换结果
            playResult.OperateType = OperateType.RealPlayStart;
            playResult.ProgramNumber = programNum;
            playResult.ProgramName = name;
            playResult.Frequency = freq;
            if (!Enum.TryParse(splits[0], out TvStandard standard))
            {
                playResult.Standard = splits[0];
                playResult.Result = false;
                playResult.Uri = $"打通制式{splits[0]}失败，不支持的制式！";
                _dataQueue.Enqueue(new List<object> { playResult });
                return;
            }

            // if (_currentFrequency)
            // {
            //     Console.WriteLine("更改了三次频率，需要重启设备...");
            //     RebootDevice();
            //     _changeFrequencyCount = 0;
            // }
            if (_currentFrequency > 0 && IsSegmentChanged(_currentFrequency, freq, standard))
            {
                Console.WriteLine("跨频段，需要重启设备...");
                RebootDevice();
            }

            _currentFrequency = freq;
            var count = 0;
            while (count < 3)
            {
                count++;
                try
                {
                    var programPlay = default(ProgramPlay);
                    if (standard == TvStandard.ANATV)
                    {
                        // 模拟电视
                        programPlay = _programPlayAnatv;
                        _programPlayAllInOne.Reset();
                        // 播放
                        _programPlayAnatv.OpenDecoder(freq, 0, standard);
                    }
                    else
                    {
                        // 数字电视
                        programPlay = _programPlayAllInOne;
                        // 播放
                        _programPlayAllInOne.ProNum = programNum;
                        _programPlayAllInOne.OpenDecoder(freq, 8000, standard);
                    }

                    if (programPlay?.IsStable != true)
                    {
                        playResult.Result = false;
                        playResult.Uri = $"切换节目反馈:电视制式{standard},频率{freq}MHz,未返回稳定的节目信号,请确保天线接收正常!";
                        RebootDevice();
                        // reboot以后再次进行打通
                    }
                    else
                    {
                        // 如果正常，则计数归零
                        //_timeoutCount = 0;
                        playResult.Result = true;
                        playResult.Uri =
                            $"http://{RunningInfo.EdgeIp}:{RunningInfo.Port}/{PublicDefine.PathVideo}/real/index.m3u8";
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is not TaskCanceledException)
                    {
                        Console.WriteLine($"打通频道失败:{ex}");
                        playResult.Result = false;
                        playResult.Uri = $"切换节目反馈:电视制式{playResult.Standard},频率{playResult.Frequency}MHz,切换节目失败，请重试!";
                        _programPlayAllInOne.IsStable = false; // 如果播放异常则表示无信号
                        if (ex.Message.Contains("数组不能为空") || ex.Message.Contains("Array cannot be null"))
                            // 如果出现这样的异常，后面的频点都会出现异常
                            RebootDevice();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is not TaskCanceledException)
            {
                Console.WriteLine($"打通频道失败:{ex}");
                playResult.Result = false;
                playResult.Uri = $"切换节目反馈:电视制式{playResult.Standard},频率{playResult.Frequency}MHz,切换节目失败，请重试!";
                _programPlayAllInOne.IsStable = false; // 如果播放异常则表示无信号
            }
        }

        // 告诉客户端播放结果
        _dataQueue.Enqueue(new List<object> { playResult });
    }

    /// <summary>
    ///     判断频段修改了
    /// </summary>
    /// <param name="freq1"></param>
    /// <param name="freq2"></param>
    /// <param name="standard"></param>
    /// <returns></returns>
    private static bool IsSegmentChanged(double freq1, double freq2, TvStandard standard)
    {
        if (standard != TvStandard.DTMB) return false;
        if (freq1 <= 562 && freq2 >= 610)
            return true;
        if (freq1 >= 610 && freq2 <= 562) return true;
        return false;
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitS7000()
    {
        // 锁定键盘
        _buffer = S7000Protocol.LockKeyBoard.GetOrder("ON", 1);
        _ = _client.Send(_buffer, true) ?? throw new Exception("锁定键盘失败！", null);
        _programSearchAnatv = new ProgramSearchAnatv(_client);
        _programSearchAnatv.SearchComplete += Program_SearchComplete;
        _programSearchAllInOne = new ProgramSearchAllInOne(_client);
        _programSearchAllInOne.SearchComplete += Program_SearchComplete;
        _programPlayAnatv = new ProgramPlayAnatv(_client);
        _programPlayAllInOne = new ProgramPlayAllInOne(_client);
    }

    /// <summary>
    ///     停止S7000设备解码
    /// </summary>
    private void StopS7000()
    {
        _cancelSearch = true;
        if (_client?.Socket?.Connected == true)
        {
            // 停止模拟码流
            _buffer = S7000Protocol.SetAtvCloseDecoder.GetOrder();
            _client.Send(_buffer, true);
            // 停止数字码流分析
            _buffer = S7000Protocol.SetTsStop.GetOrder();
            _client.Send(_buffer);
            // 解锁键盘
            _buffer = S7000Protocol.LockKeyBoard.GetOrder("OFF", 0);
            _client.Send(_buffer);
        }

        // 重置播放对象
        _programPlayAllInOne?.Reset();
        // 清楚缓存的指令
        _cmdQueue?.Clear();
        try
        {
            _cts?.Cancel();
        }
        catch
        {
        }

        try
        {
            _sendCmdTask?.Dispose();
        }
        catch
        {
        }

        try
        {
            _sendDataTask?.Dispose();
        }
        catch
        {
        }
        finally
        {
            _cts?.Dispose();
        }
    }

    /// <summary>
    ///     根据字符串类型映射为节目搜索对象
    /// </summary>
    /// <param name="standard">类型</param>
    /// <returns>节目搜索对象</returns>
    private ProgramSearch GetProgramSearch(TvStandard standard)
    {
        return standard == TvStandard.ANATV ? _programSearchAnatv : _programSearchAllInOne;
    }

    /// <summary>
    ///     搜索节目完成
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="standard"></param>
    /// <param name="count"></param>
    /// <param name="msg"></param>
    private void SearchComplete(double frequency, TvStandard standard, int count, string msg = "")
    {
        var progress = new SDataSearchProgress();
        var db = 100 * _searchProgress / (double)_totalSearchCount;
        progress.Progress = Math.Round(db, 2);
        if (count == 0)
        {
            var str = "未检索到电视节目";
            if (!string.IsNullOrEmpty(msg)) str = msg;
            progress.Message = $"电视制式{standard},频率{frequency}MHz,{str}!";
        }
        else
        {
            progress.Message = $"电视制式{standard},频率{frequency}MHz,检索到{count}个电视节目!";
        }

        Trace.WriteLine(progress.Message);
        _dataQueue.Enqueue(new List<object> { progress });
        _searchResultEvent.Set();
    }

    private void Program_SearchComplete(double frequency, TvStandard standard, List<ChannelProgramInfo> programInfos,
        string msg)
    {
        if (programInfos == null || programInfos.Count == 0)
        {
            SearchComplete(frequency, standard, 0, msg);
            return;
        }

        lock (_lockProgramCache)
        {
            _programCache.AddRange(programInfos);
        }

        SearchComplete(frequency, standard, programInfos.Count);
    }

    private void SendSearchResult()
    {
        if (_sendSearchResultOk) return;
        _sendSearchResultOk = true;
        // 搜索完毕
        List<ChannelProgramInfo> list;
        lock (_lockProgramCache)
        {
            list = _programCache.ToList();
        }

        var data = new SDataVideoChannel
        {
            Programs = list
        };
        Console.WriteLine("搜索完毕，向前端发送结果信息:");
        foreach (var item in list)
            Console.WriteLine(
                $"    {item.Frequency},{item.Index},{item.ProgramNumber},{item.ProgramName},{item.FlowType}");
        _dataQueue.Enqueue(new List<object> { data });
    }

    /// <summary>
    ///     发送数据线程
    /// </summary>
    /// <param name="obj"></param>
    private async Task MySendDataAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            if (_dataQueue.IsEmpty) continue;
            if (!_dataQueue.TryDequeue(out var data)) continue;
            if (data != null) SendData(data);
        }
    }

    #endregion

    #region Device HeartBeat

    /// <summary>
    ///     心跳检查线程函数
    ///     实现 tcp 连接的心跳检查；子类可重载此方法，实现其它连接方式的心跳检查
    /// </summary>
    /// <param name="connObject"></param>
    protected override void KeepAlive(object connObject)
    {
        while (true)
        {
            lock (MLock)
            {
                // S7000有自己的心跳机制：20分钟内如果没有数据交换，则会断开连接
                // 10分钟发送一次无用数据，不用每秒发一次，防止设备可能意外出错。
                // 这里的10分钟，应该是空闲时的10分钟，如果与设备存在数据交互，则会重置_seconds
                // 2018/8/14 在板瓦口岸站 10 分钟会断一次线，怀疑设备不是20分钟检查连接有效性，故改成5分钟发一次有效数据
                if (_seconds > 5 * 60)
                {
                    _buffer = S7000Protocol.LockKeyBoard.GetOrder("ON", 0);
                    try
                    {
                        _client.Socket.Send(_buffer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DeviceInfo.DisplayName}({DeviceId})心跳线程检测到异常");
                        if (ex is SocketException) break;
                    }

                    _seconds = 0;
                }
                else
                {
                    if (!IsSocketConnected(_client.Socket)) break;
                    _seconds++;
                }
            }

            Thread.Sleep(1000);
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

    /// <summary>
    ///     在出现“数组为空的情况下”重连设备
    /// </summary>
    private void RebootDevice()
    {
        try
        {
            // 重启设备，并重新建立连接，在此期间不进行心跳检查
            //_timeoutCount++;
            if (_client == null) return;
            // 如果数据接收超时2次，则断开与S7000的连接并重新建立新的连接
            var pingOk = false;
            while (!pingOk)
            {
                _client.Send(S7000Protocol.Reset.GetOrder());
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
                _client.Connect();
                // 重启设备
                _client.Send(S7000Protocol.Reboot.GetOrder());
                Trace.WriteLine("设备收到控制回复或出现无TS输入的情况，重启设备然后继续。");
                // 很坑的一点，有时候发送重启命令竟然没有真的重启？？
                var ping = new Ping();
                var sign = false;
                var connected = false;
                var cnt = 0;
                while (cnt < 20)
                {
                    cnt++;
                    var reply = ping.Send(Ip);
                    if (reply != null)
                    {
                        Console.WriteLine($"连接状态:{reply.Status}");
                        connected = reply.Status == IPStatus.Success;
                        if (reply.Status != IPStatus.Success) sign = true;
                        if (reply.Status == IPStatus.Success && sign)
                        {
                            pingOk = true;
                            break;
                        }
                    }

                    // Thread.Sleep(1000);
                    Task.Delay(1000).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                if (!pingOk && connected) continue;
                // 能ping通后再等待15s （内部程序可能未启动）
                Task.Delay(10000).ConfigureAwait(false).GetAwaiter().GetResult();
                var res = false;
                while (!res)
                {
                    res = _client.Connect();
                    Trace.WriteLine($"设备重连结果:{res}。");
                    Task.Delay(1000).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                //_timeoutCount = 0;
            }
        }
        catch
        {
        }
    }

    #endregion
}