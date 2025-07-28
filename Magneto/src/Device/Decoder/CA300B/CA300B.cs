using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.CA300B;

public partial class Ca300B : DeviceBase
{
    #region 公有函数

    public Ca300B(Guid guid) : base(guid)
    {
    }

    #endregion

    #region 成员变量定义

    /// <summary>
    ///     指令存储队列
    /// </summary>
    private readonly ConcurrentQueue<ProgramCmd> _cmdQueue = new();

    private int _searchProgress;
    private int _totalSearchCount;
    public const TvStandard SupportedTvStandard = TvStandard.ANATV | TvStandard.DTMB | TvStandard.DVBT;

    /// <summary>
    ///     搜索节目的搜索带宽 kHz
    /// </summary>
    private readonly double _searchBandwidth = 8000;

    /// <summary>
    ///     指令发送线程
    /// </summary>
    private Task _sendCmdTask;

    private CancellationTokenSource _cts;

    /// <summary>
    ///     tcp服务端监听端口
    /// </summary>
    private readonly int _port = 9999;

    private NewProtocol _newProtocol;
    private Socket _socket;

    #endregion

    #region 重写基类函数

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            // 释放资源
            ReleaseResource();
            Disposed = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(IPAddress.Parse(Ip), _port);
            _socket.ReceiveTimeout = 20000;
            _socket.NoDelay = true;
            _newProtocol = new NewProtocol(_socket);
            _newProtocol.SearchFinish += SearchFinish;
            _newProtocol.SearchResult += SearchResult;
            _newProtocol.PlayResult += PlayResult;
            _newProtocol.Start(DvbtBandwidth);
            // 初始化线程
            //InitThread();
            // _tcpManager.Send(Encoding.ASCII.GetBytes($"{CA300BProtocol.DVBT8M}\r\n"), true);
            // 设置心跳检测
            SetHeartBeat(_socket);
        }

        return result;
    }

    /// <summary>
    ///     开始
    /// </summary>
    /// <param name="feature"></param>
    /// <param name="dataPort"></param>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        StartThread();
    }

    public override void SetParameter(string name, object value)
    {
        if (TaskState != TaskState.Start)
        {
            // 功能启动前禁止安装参数以外的其他参数设置
            var para = DeviceInfo?.Parameters?.Find(item => item.Name == name);
            if (para?.IsInstallation == false) return;
        }

        base.SetParameter(name, value);
    }

    /// <summary>
    ///     停止
    /// </summary>
    public override void Stop()
    {
        base.Stop();
        try
        {
            _cancelSearch = true;
            _newProtocol.CancelSearch();
            StopThread();
            _cmdQueue.Clear();
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        try
        {
            ReleaseResource();
            base.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region 私有函数

    /// <summary>
    ///     停止线程
    /// </summary>
    private void StopThread()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }

        try
        {
            _sendCmdTask?.Dispose();
        }
        catch
        {
            // ignored
        }

        try
        {
            _sendCmdTask?.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    ///     启动线程线程
    /// </summary>
    private void StartThread()
    {
        _cts ??= new CancellationTokenSource();
        // 指令发送线程
        _sendCmdTask = new Task(p => SendCmdAsync(p).ConfigureAwait(false), _cts.Token);
        _sendCmdTask.Start();
    }

    /// <summary>
    ///     发送数据到设备
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
                if (programCmd == null)
                {
                    Thread.Sleep(10);
                    continue;
                }

                switch (programCmd.Type)
                {
                    case ProgramType.Search:
                        if (programCmd.Cmd is SearchProgramTemplate[] array)
                        {
                            _searchProgress = 0;
                            var list = GetSearchCount(array);
                            _totalSearchCount = list.Count;
                            await _newProtocol.SearchFlowAsync(list, token);
                            _cancelSearch = false;
                        }

                        break;
                    case ProgramType.Play:
                        if (programCmd.Cmd != null)
                            await _newProtocol.PlayFlowAsync(programCmd.Cmd.ToString(), token);
                        else
                            Trace.WriteLine("播放命令为空");
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send Data To Devices Error:{ex.Message}");
            }
        }
    }

    private void SearchResult(object sender, List<ChannelProgramInfo> e)
    {
        var data = new SDataVideoChannel
        {
            Programs = e
        };
        Console.WriteLine(
            $"搜台完毕，发送电视节目列表:{data.Programs.Count}:{string.Join(",", data.Programs.Select(item => item.ProgramNumber))}");
        SendData(new List<object> { data });
    }

    private void SearchFinish(double frequency, TvStandard standard, int count, string msg)
    {
        _searchProgress++;
        SearchComplete(frequency, standard, count, msg);
    }

    private void PlayResult(object sender, SDataPlayResult e)
    {
        SendData(new List<object> { e });
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
        SendData(new List<object> { progress });
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResource()
    {
        try
        {
            if (_socket?.Connected == true) _socket.Close();
            _socket?.Dispose();
            _socket = null;
        }
        catch
        {
            // ignored
        }

        _newProtocol?.Close();
        StopThread();
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
            var start = item.StartFrequency;
            var stop = item.StopFrequency;
            var stepMap = GetSearchStepMap(item);
            foreach (var kv in stepMap)
            {
                var step = kv.Value;
                var standard = kv.Key;
                if ((standard & SupportedTvStandard) == 0) //不支持的制式不加入搜索列表
                    continue;
                var freq = start;
                while (freq <= stop)
                {
                    var s = new SearchInfo
                    {
                        Frequency = freq,
                        Standard = standard
                    };
                    list.Add(s);
                    freq += step / 1000;
                }
            }
        }

        _totalSearchCount = list.Count;
        return list;
    }

    private Dictionary<TvStandard, double> GetSearchStepMap(SearchProgramTemplate item)
    {
        var dic = new Dictionary<TvStandard, double>();
        foreach (TvStandard standard in Enum.GetValues(typeof(TvStandard)))
        {
            if ((item.Standard & standard) == 0) continue;
            if (!item.StepFrequency.EqualTo(0d))
                dic.Add(standard, item.StepFrequency);
            else if (DvbtBandwidth == 8000 || (standard != TvStandard.DVBT && standard != TvStandard.DVBT2))
                dic.Add(standard, _searchBandwidth);
            else if (item.StartFrequency.CompareWith(473d) >= 0 && item.StopFrequency.CompareWith(857d) <= 0)
                dic.Add(standard, 6000d);
            else
                dic.Add(standard, _searchBandwidth);
        }

        return dic;
    }

    #endregion
}