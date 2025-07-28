using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Define;

namespace Magneto.Device.AP510;

public partial class Ap510
{
    #region 音频任务调度

    /// <summary>
    ///     加载音频
    /// </summary>
    private void LoadAudioCapturer(bool onoff = true)
    {
        /* 只有引用计数不为零时才关心音频的开
         * 音频的关由音频数据采集线程根据当前音频引用计数自行判断，不在此方法中单独处理
         */
        try
        {
            SingleCall(CombineCmd(Constants.Audio, Constants.Space, Constants.AudioOff));
            if (onoff)
            {
                SingleCall(CombineCmd(Constants.Audio, Constants.AudioPort, Constants.Space, AudioPort.ToString()));
                // 从设备默认获取的声音为PCM，这是为了方便转换成客户需要的各种格式（PCM/MP3/GSM610）
                SingleCall(CombineCmd(Constants.Audio, Constants.AudioFormat, Constants.Space,
                    Constants.AudioFormatPcm));
                SingleCall(CombineCmd(Constants.Audio, Constants.Space, Constants.AudioOn));
            }
            /* 音频是设备全局的，音频数据采集线程是服务设备全局的
             * 音频线程一旦创建，使用同一服务设备的应用可通过引用计数共享音频，
             * 不同服务设备对同一设备音频的使用采用抢占的方式
             */
        }
        catch (Exception)
        {
        }
    }

    #endregion

/*
    private void ServiceCall(string command)
    {
        Trace.WriteLine($"ServiceCall => {command}");
        try
        {
            // 发送命令
            var sendBuffer = _encodingDefault.GetBytes(command);
            _dataSocket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
        }
        catch
        {
        }
    }
*/

    private bool ServiceCall(string command, out ScpiDataStruct ret, int timeout = 3000)
    {
        _dataCache.Clear();
        var sb = new StringBuilder();
        lock (_objLock)
        {
            try
            {
                _serviceCallEvent.Reset();
                Trace.WriteLine($"ServiceCall => {command}");
                // 发送命令
                var sendBuffer = _encodingDefault.GetBytes(command);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _dataSocket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
                do
                {
                    var b = _dataCache.TryDequeue(out var data);
                    if (!b || data == null) continue;
                    if (data.DataType == ScpiDataType.Default ||
                        (data is ScpiMapData mapData && mapData.Content.Count != 1)) continue;
                    ret = data;
                    Trace.WriteLine(data.ToString());
                    return true;
                } while (stopwatch.ElapsedMilliseconds < timeout);

                stopwatch.Stop();
            }
            catch
            {
            }
            finally
            {
                _serviceCallEvent?.Set();
            }
        }

        ret = null;
        Trace.WriteLine($"==>{sb}");
        return false;
    }

    public bool SingleCall(string command)
    {
        return SingleCall(command, out _);
    }

    public bool SingleCall(string command, out string ret, int timeout = 1000)
    {
        _cmdCache.Clear();
        var sb = new StringBuilder();
        try
        {
            _singleCallEvent.WaitOne();
            Trace.WriteLine($"SingleCall => {command}");
            // 发送命令
            var sendBuffer = _encodingDefault.GetBytes(command);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _cmdSocket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
            do
            {
                var b = _cmdCache.TryDequeue(out var s);
                if (!b || string.IsNullOrWhiteSpace(s)) continue;
                sb.Append(s);
                var match = Regex.Match(s, @"(?<code>\d+)\s*\:\s*(?<value>.+)");
                if (match.Success)
                {
                    var code = match.Groups["code"].Value;
                    var value = match.Groups["value"].Value;
                    ret = value;
                    return code == "0";
                }
            } while (stopwatch.ElapsedMilliseconds < timeout);

            stopwatch.Stop();
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            _singleCallEvent?.Set();
        }

        ret = sb.ToString();
        return false;
    }

    #region 初始化辅助方法

    /// <summary>
    ///     初始化套接字接口
    /// </summary>
    private void InitSocket()
    {
        _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            LingerState = new LingerOption(true, 2),
            SendBufferSize = Buffersize,
            ReceiveBufferSize = Buffersize
        };
        _cmdSocket.Connect(Ip, Port);
        SetHeartBeat(_cmdSocket);
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        _dataSocket.Connect(Ip, Port);
        _audioSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        var ipEndpoint = new IPEndPoint(IPAddress.Any, AudioPort);
        _audioSocket.Bind(ipEndpoint);
    }

    private void ClearMsgQueues()
    {
        _cmdQueue.Clear();
        _dataQueue.Clear();
        _cmdCache.Clear();
        _dataCache.Clear();
    }

    /// <summary>
    ///     初始化天线（对所有实例，只会被初始化一次），初始化天线过后，设备驱动将增加一个参数“antenna”，以便初始化获取功能/参数时，返回完整的参数列表。
    /// </summary>
    private void InitAntenna()
    {
        // 设置天线信息
        SingleCall(CombineCmd(Constants.Antenna, Constants.AntConfig, Constants.Clear));
        if (_antennaTemplates == null || _antennaTemplates.Length == 1)
        {
            // 只有一副天线，则使用自动
            SingleCall(CombineCmd(Constants.Antenna, Constants.AntConfig, Constants.AntAuto, Constants.Space,
                "true"));
        }
        else
        {
            foreach (var item in _antennaTemplates)
            {
                var iPolarityType = 0;
                if (item.Polarization == Polarization.Vertical)
                    iPolarityType = 1;
                else if (item.Polarization == Polarization.Horizontal) iPolarityType = 2;
                // 向设备添加天线码
                var code = CombineCmd(Constants.Antenna, Constants.AntConfig, Constants.AntAdd, Constants.Space);
                var antInfo = string.Format("{0},{1},{2},{3},{4}", item.PassiveCode, item.Name, iPolarityType,
                    item.StartFrequency, item.StopFrequency);
                code += antInfo;
                SingleCall(code);
            }

            SingleCall(
                CombineCmd(Constants.Antenna, Constants.AntConfig, Constants.AntAuto, Constants.Space, "false"));
        }
    }

    /// <summary>
    ///     初始化功能/参数
    /// </summary>
    private void InitFeatures()
    {
        try
        {
            // 暂停1秒，等待AP510接收机初始化完成
            Thread.Sleep(1000);
            // 只需要初始化一次
            var b = SingleCall(Constants.OptQuery, out var opt);
            if (!b || string.IsNullOrWhiteSpace(opt)) return;
            var opts = opt.Split(new[] { ',' });
            // 判断是否支持测量任务(单频测量，离散扫描，数字音频，全景扫描，频点扫描)
            if (opts.Any(item => item.Equals("RX")))
            {
                var rxFeatures = new[]
                {
                    Constants.RxSingle, Constants.RxMscan, Constants.RxFscan, Constants.RxPscan, Constants.RxCiq
                };
                foreach (var rxFeature in rxFeatures)
                {
                    var command = CombineCmd(rxFeature, Constants.Query, Constants.Space, Constants.Plist);
                    SingleCall(command, out var param);
                    _features.Add(rxFeature, param);
                }
            }

            // 判断是否支持测向任务（单频测向，频率表测向，宽带测向）
            if (opts.Any(item => item.Equals("DF")))
            {
                var dfFeatures = new[] { Constants.DfList, Constants.DfNarrow, Constants.DfWideband };
                foreach (var dfFeature in dfFeatures)
                {
                    var command = CombineCmd(dfFeature, Constants.Query, Constants.Space, Constants.Plist);
                    //_features[ID][dfFeature] = _requestWrapper.ReadString();
                    SingleCall(command, out var param);
                    _features.Add(dfFeature, param);
                }
            }
        }
        catch
        {
            throw new Exception("初始化设备功能失败！");
        }
    }

    private void InitTasks()
    {
        _cmdReceiveCts = new CancellationTokenSource();
        _cmdReceiveTask = new Task(ReceiveCmdResult, _cmdReceiveCts.Token);
        _cmdReceiveTask.Start();
        _dataReceiveCts = new CancellationTokenSource();
        _dataReceiveTask = new Task(ReceiveData, _dataReceiveCts.Token);
        _dataReceiveTask.Start();
        _cmdProcessCts = new CancellationTokenSource();
        _cmdProcessTask = new Task(ProcessCmdResult, _cmdProcessCts.Token);
        _cmdProcessTask.Start();
        _dataProcessCts = new CancellationTokenSource();
        _dataProcessTask = new Task(ProcessData, _dataProcessCts.Token);
        _dataProcessTask.Start();
        _dataCts = new CancellationTokenSource();
        _dataTask = new Task(DispatchData, _dataCts.Token);
        _dataTask.Start();
        _audioCts = new CancellationTokenSource();
        _audioTask = new Task(CaptureAudio, _audioCts.Token);
        _audioTask.Start();
    }

    #endregion
}