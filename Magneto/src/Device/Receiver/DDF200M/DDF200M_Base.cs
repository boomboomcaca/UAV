using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF200M;

public partial class Ddf200M
{
    #region 初始化

    /// <summary>
    ///     释放所有非托管资源
    /// </summary>
    private void ReleaseSource()
    {
        Utils.CancelTask(_tcpDataTask, _tcpDataCts);
        Utils.CancelTask(_udpDataProcessTask, _udpDataProcessCts);
        Utils.CancelTask(_udpDataCaptrueTask, _udpDataCaptureCts);
        Utils.CloseSocket(_dataSocket);
        Utils.CloseSocket(_cmdSocket);
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        //查询设备所有选件
        //初始化接收机恢复默认状态
        SendCmd("*RST");
        ////由于默认状态下音量不为0，此处将音量关闭
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        SendCmd("OUTP:TONE OFF");
        SendCmd("FORM ASC"); //定义二进制方式,制动高字节在高位;:FORM:BORD SWAP
        SendCmd("FORM:BORD SWAP"); //SWAP
        SendCmd("SYST:IF:REM:MODE SHORT");
        SendCmd("SYST:AUD:REM:MOD 1"); //PCM 32k,16bit,2ch 
        SendCmd("SENS:FREQ:AFC OFF"); //不使用自动频率控制
    }

    /// <summary>
    ///     初始化网络连接
    /// </summary>
    private void InitNetWork()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), TcpPort);
        _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cmdSocket.Connect(ep);
        _cmdSocket.NoDelay = true;
        _localaddr = (_cmdSocket.LocalEndPoint as IPEndPoint)?.Address.ToString();
        if (_localaddr != null) ep = new IPEndPoint(IPAddress.Parse(_localaddr), 0);
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataSocket.Bind(ep);
        _localudpport = ((IPEndPoint)_dataSocket.LocalEndPoint)!.Port;
    }

    /// <summary>
    ///     初始化所有线程(TCP采集数据线程、UDP采集数据线程、UDP数据解析线程)
    /// </summary>
    private void InitAllThread()
    {
        _udpDataQueue.Clear();
        _tcpDataCts = new CancellationTokenSource();
        _tcpDataTask = new Task(TcpDataProcess, _tcpDataCts.Token);
        _tcpDataTask.Start();
        _udpDataCaptureCts = new CancellationTokenSource();
        _udpDataCaptrueTask = new Task(UdpDataCaptrueProc, _udpDataCaptureCts.Token);
        _udpDataCaptrueTask.Start();
        _udpDataProcessCts = new CancellationTokenSource();
        _udpDataProcessTask = new Task(UdpDataProcessProc, _udpDataProcessCts.Token);
        _udpDataProcessTask.Start();
    }

    #endregion

    #region 线程方法

    /// <summary>
    ///     采集主通道电平
    /// </summary>
    private void TcpDataProcess()
    {
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送，否则界面参数列表刷新时也会出现卡顿
        //此处处理为2秒左右发送一次到客户端
        var startTime = DateTime.Now;
        var count = 10;
        while (true)
            try
            {
                //发送数据缓存列表
                var data = new List<object>();
                count++;
                if (count >= 10)
                {
                    // 获取电子罗盘信息
                    if (UseCompass)
                    {
                        var result = SendSyncCmd("SYSTem:COMPass:DATA? \"GH150@ADD197_V\"");
                        if (result.Length > 2)
                        {
                            var valueCompass = float.Parse(result.Split(',')[0]);
                            var dataCompass = new SDataCompass
                            {
                                Heading = valueCompass
                            };
                            SendMessageData(new List<object> { dataCompass });
                        }
                    }

                    // 获取GPS数据信息
                    if (UseGps)
                    {
                        var result = SendSyncCmd("SYSTem:GPS:DATA?");
                        var gpsInfos = result.Split(',');
                        var dataGps = new SDataGps
                        {
                            Latitude = double.Parse(gpsInfos[6]) + double.Parse(gpsInfos[7]) / 60 +
                                       double.Parse(gpsInfos[8]) / 3600,
                            Longitude = double.Parse(gpsInfos[10]) + double.Parse(gpsInfos[11]) / 60 +
                                        double.Parse(gpsInfos[12]) / 3600
                        };
                        SendMessageData(new List<object> { dataGps });
                    }

                    count = 0;
                }

                // 获取电平数据和ITU信息
                if ((_media & (MediaType.Level | MediaType.Itu)) > 0 && (_media & MediaType.Scan) == 0)
                {
                    var result = SendSyncCmd("SENS:DATA?");
                    var values = result.Split(',');
                    //解析本次电平值
                    var lev = float.Parse(values[0]);
                    if ((_media & MediaType.Level) > 0)
                        if (lev > -999f)
                        {
                            var dataLevel = new SDataLevel
                            {
                                Data = lev,
                                Frequency = _frequency,
                                Bandwidth = _ifbandwidth
                            };
                            data.Add(dataLevel);
                        }

                    //解析ITU数据,3秒发送一次
                    if (values.Length >= 9 && (_media & MediaType.Itu) > 0)
                    {
                        var ts = DateTime.Now - startTime;
                        if (ts.TotalMilliseconds >= 3000)
                        {
                            var am = double.Parse(values[1]);
                            var fm = double.Parse(values[4]) / 1000d;
                            var fmpos = double.Parse(values[5]) / 1000d;
                            var fmneg = double.Parse(values[6]) / 1000d;
                            var pm = double.Parse(values[7]);
                            var dAmDepth = am is < 0 or > 100 ? double.MinValue : am;
                            var dFmDev = fm < -1000000000f ? double.MinValue : fm;
                            var dFmDevPos = fmpos < -1000000000f ? double.MinValue : fmpos;
                            var dFmDevNeg = fmneg < -1000000000f ? double.MinValue : fmneg;
                            var dPmDepth = pm < -1000000000f ? double.MinValue : pm;
                            var dataItu = new SDataItu
                            {
                                Frequency = _frequency,
                                Misc = new Dictionary<string, object>
                                {
                                    { ParameterNames.ItuAmDepth, dAmDepth },
                                    { ParameterNames.ItuFmDev, dFmDev },
                                    { ParameterNames.ItuFmDevPos, dFmDevPos },
                                    { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                                    { ParameterNames.ItuPmDepth, dPmDepth }
                                },
                                Modulation = Modulation.Iq
                            };
                            data.Add(dataItu);
                            startTime = DateTime.Now; //保存最新的时间
                        }
                    }
                }

                if (data is { Count: > 0 } && TaskState == TaskState.Start) SendData(data);
                Thread.Sleep(30);
            }
            catch (Exception ex)
            {
                if (ex is SocketException) break;
            }
    }

    /// <summary>
    ///     UDP 采集业务数据
    /// </summary>
    private void UdpDataCaptrueProc()
    {
        var buffer = new byte[1024 * 1024];
        while (_udpDataCaptureCts?.IsCancellationRequested == false)
            try
            {
                var recvBytes = _dataSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                if (recvBytes == 0) break;
                var recvData = new byte[recvBytes];
                Buffer.BlockCopy(buffer, 0, recvData, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(recvData);
            }
            catch (SocketException)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    /// <summary>
    ///     解析UDP业务数据并发送到客户端
    /// </summary>
    private void UdpDataProcessProc(object isIfmch)
    {
        while (_udpDataProcessCts?.IsCancellationRequested == false)
            try
            {
                var b = _udpDataQueue.TryDequeue(out var mediaData);
                if (!b || mediaData == null) continue;
                var sendDatas = new List<object>();
                var offset = Marshal.SizeOf(typeof(Eb200DatagramFormat));
                while (offset < mediaData.Length)
                {
                    var ga = new GenericAttribute(mediaData, offset);
                    offset += Marshal.SizeOf(typeof(GenericAttribute));
                    object obj = null;
                    switch (ga.tag)
                    {
                        case (int)Tags.Audio:
                            obj = ToAudio(mediaData, offset);
                            break;
                        case (int)Tags.Ifpan:
                            obj = ToSpectrum(mediaData, offset);
                            break;
                        case (int)Tags.If:
                            obj = ToIq(mediaData, offset);
                            break;
                        case (int)Tags.Fscan:
                            obj = ToFScan(mediaData, offset);
                            break;
                        case (int)Tags.Pscan:
                            obj = ToPScan(mediaData, offset);
                            break;
                        //case (int)TAGS.MSCAN:
                        //    obj = ToMScan(mediaData, offset);
                        //    break;
                        case (int)Tags.DfPan:
                            obj = ToDfPan(mediaData, offset);
                            break;
                    }

                    if (obj != null)
                    {
                        if (obj is List<object> list)
                            sendDatas.AddRange(list);
                        else
                            sendDatas.Add(obj);
                    }

                    offset += ga.length;
                }

                sendDatas.RemoveAll(item => item == null);
                if (sendDatas.Count > 0 && TaskState == TaskState.Start) SendData(sendDatas);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    ///     发送命令
    /// </summary>
    /// <param name="cmd">命令</param>
    private void SendCmd(string cmd)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        _cmdSocket.Send(buffer);
    }

    /// <summary>
    ///     发送命令并检查设置命令结果
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private string SendSyncCmd(string cmd)
    {
        lock (this)
        {
            var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
            _cmdSocket.Send(buffer);
            var result = RecvResult('\n');
            return result;
        }
    }

    private string RecvResult(int endflag)
    {
        var total = 0;
        var buffer = new byte[1024 * 1024];
        while (_cmdSocket.Receive(buffer, total, 1, SocketFlags.None) > 0)
            if (buffer[total++] == endflag)
                break;
        return Encoding.ASCII.GetString(buffer, 0, total);
    }

    //设置单频测向测向带宽，该功能不再暴露频谱带宽，同步设置一个匹配的频谱带宽即可
    private readonly double[] _arrStep =
        { 1d, 1.25d, 2d, 2.5d, 3.125d, 5d, 6.25d, 8.333d, 10d, 12.5d, 20d, 25d, 50d, 100d, 200d, 500d, 1000d, 2000d };

    private readonly double[] _arrDefautSpan =
    {
        500d, 1000d, 1000d, 2000d, 2000d, 2000d, 5000d, 5000d, 5000d, 10000d, 10000d, 20000d, 20000d, 20000d, 20000d,
        20000d, 20000d, 20000d
    };

    private double _fixdfSpectrumSpan;

    private void SetDfBandwidth(double bw)
    {
        var index = Array.IndexOf(_arrStep, bw);
        var span = _arrDefautSpan[index];
        SendCmd("CALC:IFP:STEP:AUTO ON");
        Thread.Sleep(10);
        SendCmd($"FREQ:SPAN {span}KHz");
        //保存最新设置的频谱带宽
        _fixdfSpectrumSpan = span;
        Thread.Sleep(10);
        SendCmd("CALC:IFP:STEP:AUTO OFF");
        Thread.Sleep(10);
        SendCmd($"CALC:IFPAN:STEP {bw}KHz");
    }

    /// <summary>
    ///     宽带测向设置频谱带宽，约束脚本中以频谱带宽为主更新信道带宽列表
    /// </summary>
    /// <param name="span"></param>
    private void SetWbSpan(double span)
    {
        SendCmd("CALC:IFP:STEP:AUTO ON");
        Thread.Sleep(10);
        SendCmd($"FREQ:SPAN {span}KHz");
        Thread.Sleep(10);
        SendCmd("CALC:IFP:STEP:AUTO OFF");
        //TODO:如果宽带测向为运行时可修改参数，此处还需设置STEP
    }

    #endregion

    #region 数据校验

    /// <summary>
    ///     检查Fscan扫描参数是否已经设置成功
    /// </summary>
    /// <returns></returns>
    private bool CheckFScanParameters()
    {
        try
        {
            var start = SendSyncCmd("SENS:FREQ:STAR?");
            if (Math.Abs(double.Parse(start) / 1000000 - StartFrequency) > 1e-9) return false;
            var stop = SendSyncCmd("SENS:FREQ:STOP?");
            if (Math.Abs(double.Parse(stop) / 1000000 - StopFrequency) > 1e-9) return false;
            var step = SendSyncCmd("SENS:SWE:STEP?");
            if (Math.Abs(double.Parse(step) / 1000 - StepFrequency) > 1e-9) return false;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     检查PScan扫描参数是否已经设置成功
    /// </summary>
    /// <returns></returns>
    private bool CheckPScanParameters()
    {
        try
        {
            var start = SendSyncCmd("SENS:FREQ:PSC:START?");
            if (Math.Abs(double.Parse(start) / 1000000 - StartFrequency) > 1e-9) return false;
            var stop = SendSyncCmd("SENS:FREQ:PSC:STOP?");
            if (Math.Abs(double.Parse(stop) / 1000000 - StopFrequency) > 1e-9) return false;
            var band = SendSyncCmd("PSC:STEP?");
            if (Math.Abs(double.Parse(band) / 1000 - StepFrequency) > 1e-9) return false;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion
}