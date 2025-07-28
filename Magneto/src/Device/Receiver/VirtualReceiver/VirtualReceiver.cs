using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualReceiver;

public partial class VirtualReceiver : DeviceBase
{
    public VirtualReceiver(Guid deviceId) : base(deviceId)
    {
    }

    #region 初始化

    private void InitSocket()
    {
        var ip = IPAddress.Parse(IpAddress);
        _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cmdSocket.Connect(ip, CmdPort);
        _cmdSocket.NoDelay = true;
        var localPoint = _cmdSocket.LocalEndPoint as IPEndPoint;
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(localPoint?.Address!, DataPort);
        _dataSocket.Bind(endPoint);
        _dataSocket.Listen(1);
        _dataSocket.NoDelay = true;
        SendServerConnection(localPoint?.Address.ToString(), DataPort);
        _dataRecevieThread = new Thread(DataReceive)
        {
            IsBackground = true
        };
        _dataRecevieThread.Start();
        _dataProcessThread = new Thread(DataProcess)
        {
            IsBackground = true
        };
        _dataProcessThread.Start();
    }

    #endregion

    private void SendCommand(string cmd)
    {
        var sendBuffer = Encoding.UTF8.GetBytes(cmd + "|");
        var bytesToSend = sendBuffer.Length;
        var offset = 0;
        var total = 0;
        try
        {
            // 流式套接字，循环发送，直到所有数据全部发送完毕
            while (total < bytesToSend)
            {
                var sentBytes = _cmdSocket.Send(sendBuffer, offset, bytesToSend - total, SocketFlags.None);
                offset += sentBytes;
                total += sentBytes;
            }

            Console.WriteLine("{0:HH:mm:ss} <-- {1}", DateTime.Now, cmd.ToLower());
            Thread.Sleep(100);
        }
        catch
        {
            // 此处的出现的异常（套机字异常），不在向上抛，会通过别的地方（比如心跳线程）对该异常进行处理
        }
    }

    #region 释放非托管资源

    private void ReleaseResource()
    {
        base.Stop();
        if (_cmdSocket != null)
        {
            try
            {
                _cmdSocket.Close();
            }
            catch
            {
                // 容错代码
            }

            _cmdSocket = null;
        }

        if (_dataProcessThread?.IsAlive == true)
        {
            _dataProcessThread.Join();
            _dataProcessThread = null;
        }

        if (_dataRecevieThread?.IsAlive == true)
        {
            _dataRecevieThread.Join();
            _dataRecevieThread = null;
        }

        try
        {
            _dataSocket?.Close();
        }
        catch
        {
            // ignored
        }

        _dataSocket = null;
    }

    #endregion

    #region 变量

    /// <summary>
    ///     命令通道
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     数据通道
    /// </summary>
    private Socket _dataSocket;

    /// <summary>
    ///     数据处理线程
    /// </summary>
    private Thread _dataProcessThread;

    private Thread _dataRecevieThread;

    /// <summary>
    ///     当前订阅的数据类型
    ///     ///
    /// </summary>
    private MediaType _media = MediaType.None;

    private readonly object _lockChannel = new();
    private readonly AutoResetEvent _mchEvent = new(false);
    private readonly MQueue<byte[]> _dataQueue = new();
    private bool _isDispose;

    #endregion

    #region 重写基类

    public override bool Initialized(ModuleInfo device)
    {
        if (!base.Initialized(device)) return false;
        _isDispose = false;
        ReleaseResource();
        InitSocket();
        SetHeartBeat(_cmdSocket); //设置心跳包
        return true;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        TaskControl(feature, true);
        SetAllParameter();
    }

    public override void Stop()
    {
        base.Stop();
        TaskControl(CurFeature, false);
    }

    public override void Dispose()
    {
        _isDispose = true;
        ReleaseResource();
        base.Dispose();
    }

    #endregion

    #region 任务控制

    /// <summary>
    ///     任务控制
    /// </summary>
    /// <param name="feature">功能</param>
    /// <param name="isTaskStart">启动任务/停止任务标记</param>
    private void TaskControl(FeatureType feature, bool isTaskStart)
    {
        var feat = Utils.ConvertEnumToString(feature);
        var taskStart = isTaskStart ? "on" : "off";
        var dic = new Dictionary<string, object>
        {
            { "method", "task" }
        };
        var para = new Dictionary<string, object>
        {
            { "operation", taskStart },
            { "feature", feat }
        };
        dic.Add("params", para);
        var cmd = Utils.ConvertToJson(dic);
        SendCommand(cmd);
        if (feature == FeatureType.FFDF)
            _media |= MediaType.Dfind;
        else if (feature == FeatureType.FFM)
            _media |= MediaType.Level;
        else if (feature == FeatureType.SCAN) _media = MediaType.Scan;
    }

    private void SetAllParameter()
    {
        var dic = new Dictionary<string, object>
        {
            { "method", "setParameter" }
        };
        var para = new Dictionary<string, object>
        {
            { "frequency", (int)(_frequency * 1000000) },
            { "bandwidth", (int)(_ifBandwidth * 1000) },
            { "start_freq", (int)(_startFrequency * 1000000) },
            { "stop_freq", (int)(_stopFrequency * 1000000) },
            { "step", (int)(_stepFrequency * 1000) },
            { "level_threshold", _levelThreshold },
            { "quality_threshold", _qualityThreshold }
        };
        dic.Add("params", para);
        var cmd = Utils.ConvertToJson(dic);
        SendCommand(cmd);
    }

    private void SetSingleParameter()
    {
        SetAllParameter();
        // var dic = new Dictionary<string, object>()
        // {
        //     {"method","setParameter"}
        // };
        // var para = new Dictionary<string, object>
        // {
        //     { name, value },
        // };
        // dic.Add("params", para);
        // var cmd = Utils.ConvertToJson(dic);
        // SendCommand(cmd);
    }

    private void SendServerConnection(string ip, int port)
    {
        var dic = new Dictionary<string, object>
        {
            { "method", "server" }
        };
        var para = new Dictionary<string, object>
        {
            { "ip", ip },
            { "port", port }
        };
        dic.Add("params", para);
        var cmd = Utils.ConvertToJson(dic);
        SendCommand(cmd);
    }

    #endregion

    #region 数据处理

    private void DataReceive()
    {
        var buffer = new byte[1024 * 1024];
        var socket = _dataSocket.Accept();
        while (true)
            try
            {
                if (_isDispose) return;
                var recvBytes = socket.Receive(buffer, 0, 1, SocketFlags.None);
                if (recvBytes > 0)
                {
                    while (buffer[0] != 0x23) recvBytes = socket.Receive(buffer, 0, 1, SocketFlags.None);
                    recvBytes = socket.Receive(buffer, 0, 52, SocketFlags.None);
                    var recvData = new byte[recvBytes];
                    Buffer.BlockCopy(buffer, 0, recvData, 0, recvBytes);
                    var len = BitConverter.ToInt32(recvData, 40);
                    var list = new List<byte>();
                    list.AddRange(recvData);
                    recvBytes = 0;
                    var totalRecvLen = 0;
                    while (totalRecvLen < len)
                    {
                        var recvLen = socket.Receive(buffer, totalRecvLen, len - totalRecvLen, SocketFlags.None);
                        if (recvLen <= 0) throw new SocketException(10054);
                        totalRecvLen += recvLen;
                    }

                    recvData = new byte[len];
                    Buffer.BlockCopy(buffer, 0, recvData, 0, len);
                    list.AddRange(recvData);
                    if (TaskState == TaskState.Start) _dataQueue.EnQueue(list.ToArray());
                }
                else
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException) break;
            }
    }

    private void DataProcess()
    {
        while (true)
            try
            {
                if (_isDispose) return;
                if (_dataQueue.Count == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var data = _dataQueue.DeQueue(100);
                if (data == null) continue;
                var offset = 0;
                var freq = BitConverter.ToInt64(data, offset);
                offset += 8;
                var bandwidth = BitConverter.ToInt64(data, offset);
                offset += 8;
                BitConverter.ToInt64(data, offset);
                offset += 8;
                BitConverter.ToInt64(data, offset);
                offset += 8;
                BitConverter.ToInt64(data, offset);
                offset += 8;
                var dataLen = BitConverter.ToInt32(data, offset);
                offset += 4;
                var timestamp = BitConverter.ToDouble(data, offset);
                offset += 8;
                var str = Encoding.UTF8.GetString(data, offset, dataLen);
                var dic = Utils.ConvertFromJson<Dictionary<string, object>>(str);
                var list = new List<object>();
                var dataTime = new Dictionary<string, object>
                {
                    { "type", "timestamp" },
                    { "data", timestamp }
                };
                list.Add(dataTime);
                if ((_media & MediaType.Level) > 0
                    && dic.TryGetValue("level", out var obj))
                    if (float.TryParse(obj.ToString(), out var fl))
                    {
                        var level = new SDataLevel
                        {
                            Frequency = freq / 1000000d,
                            Bandwidth = bandwidth / 1000d,
                            Data = (float)Math.Round(fl, 2)
                        };
                        list.Add(level);
                    }

                if ((_media & MediaType.Dfind) > 0
                    && dic.ContainsKey("quality")
                    && dic.ContainsKey("directivity"))
                {
                    var dfind = ToDfind(freq, bandwidth, dic);
                    if (dfind != null) list.Add(dfind);
                }

                if ((_media & MediaType.Spectrum) > 0)
                {
                    var sDataSpec = ToSpectrum(freq, bandwidth, dic);
                    if (sDataSpec != null) list.Add(sDataSpec);
                }

                if ((_media & MediaType.Scan) > 0)
                {
                    var scan = ToScan(dic);
                    if (scan != null) list.Add(scan);
                }

                if (list.Count > 0) SendData(list);
            }
            catch
            {
                // ignored
            }
    }

    private SDataDfind ToDfind(long freq, long bandwidth, Dictionary<string, object> dic)
    {
        var str1 = dic["quality"].ToString();
        var str2 = dic["directivity"].ToString();
        if (!float.TryParse(str1, out var qu)
            || !float.TryParse(str2, out var azi))
            return null;
        return new SDataDfind
        {
            Frequency = freq / 1000000d,
            BandWidth = bandwidth / 1000d,
            Azimuth = (float)Math.Round(azi, 2),
            Quality = (float)Math.Round(qu, 2)
        };
    }

    private SDataSpectrum ToSpectrum(long freq, long bandwidth, Dictionary<string, object> dic)
    {
        var str1 = dic["spectrum"].ToString();
        var str2 = str1?.Replace("[", "").Replace("]", "");
        var arrStr = str2?.Split(',');
        var arr = arrStr!.Select(float.Parse).ToArray();
        var spec = new short[arr.Length / 2];
        for (var i = 0; i < arr.Length / 2; i++) spec[i] = (short)(Math.Round(arr[i * 2 + 1], 2) * 10);
        var sDataSpec = new SDataSpectrum
        {
            Frequency = freq / 1000000d,
            Span = bandwidth / 1000d,
            Data = spec
        };
        return sDataSpec;
    }

    private SDataScan ToScan(Dictionary<string, object> dic)
    {
        var str1 = dic["spectrum"].ToString();
        var str2 = str1?.Replace("[", "").Replace("]", "");
        var arrStr = str2?.Split(',');
        var arr = arrStr!.Select(float.Parse).ToArray();
        var spec = new short[arr.Length / 2];
        for (var i = 0; i < arr.Length / 2; i++) spec[i] = (short)(10 * Math.Round(arr[i * 2 + 1], 2));
        var scan = new SDataScan
        {
            StartFrequency = _startFrequency,
            StopFrequency = _stopFrequency,
            StepFrequency = _stepFrequency,
            Offset = 0,
            Total = spec.Length,
            Data = spec
        };
        return scan;
    }

    #endregion
}