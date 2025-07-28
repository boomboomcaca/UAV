/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\EM550\EM550.cs
 *
 * 作    者:    苏 林 国(原著不详)
 *
 * 创作日期:    2018-04-03
 *
 * 备    注:	   1.将Tracker800 V8.0迁移到Tracker800 V9.0
 *             2.本驱动适用于EM550-20MHz接收机（若要支持其它选件则需要修改部分代码并新增配置文件）
 *
 *********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EM550;

public partial class Em550 : DeviceBase
{
    #region 成员变量

    /// <summary>
    ///     TCP Socket,主要用于发送指令
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     UDP Socket ,用于接收扫描数据
    /// </summary>
    private Socket _dataSocket;

    /// <summary>
    ///     本地用于连接设备的IP
    /// </summary>
    private string _localAddr;

    /// <summary>
    ///     本地用于接收业务数据的UDP数据端口
    /// </summary>
    private int _localUdpPort;

    /// <summary>
    ///     TCP 采集电平和ITU数据线程
    /// </summary>
    private Task _tcpDataTask;

    /// <summary>
    ///     UDP 采集业务数据线程
    /// </summary>
    private Task _udpDataTask;

    /// <summary>
    ///     UDP 解析发送业务数据线程
    /// </summary>
    private Task _udpDataConvertTask;

    private CancellationTokenSource _cts;

    /// <summary>
    ///     缓存接收到的数据
    /// </summary>
    private ConcurrentQueue<byte[]> _udpDataQueue = new();

    /// <summary>
    ///     电平数据缓存，由于电平数据和频谱数据分别为TCP查询方式和UDP推送模式获取，为保证两个数据量对等，
    ///     使界面显示效果相对流畅，缓存最新电平数据，在收到频谱数据时再同步增加电平数据
    /// </summary>
    private float _level = float.MinValue;

    /// <summary>
    ///     订阅数据种类
    /// </summary>
    private MediaType _mediaType = MediaType.None;

    /// <summary>
    ///     保存离散扫描频点列表用于判断本包数据索引
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    /// <summary>
    ///     缓存当前设备是否包含全景扫描选件
    /// </summary>
    private bool _psOption;

    /// <summary>
    ///     缓存当前设备是否包含ITU测量选件
    /// </summary>
    private bool _ituOption;

    // private bool _spectrumOption = false;

    #endregion

    #region ReceiverBase

    public Em550(Guid id) : base(id)
    {
    }

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            //释放所有非托资源
            ReleaseResource();
            //初始化网络连接
            InitNetWork();
            //初始化设备
            InitDevice();
            //初始化所有线程
            InitThread();
            //心跳检测
            SetHeartBeat(_cmdSocket);
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        StartTask();
    }

    public override void Stop()
    {
        SendCmd("ABORT");
        SendCmd("FREQ:SYNT:MODE LOWP");
        //PSCAN扫描停止后若直接删除UDP通道会导致设备死机
        //此处统一处理为先将设备置为单频测量模式再删除UDP通道
        SendCmd("SENS:FREQ:MODE FIX");
        _mediaType = MediaType.None;
        CloseUdpPath();
        Thread.Sleep(20);
        _udpDataQueue.Clear();
        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResource();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void SetParameter(string name, object value)
    {
        if (CurFeature == FeatureType.IFOUT &&
            name.Equals(ParameterNames.IfBandwidth, StringComparison.OrdinalIgnoreCase))
            value = Convert.ToDouble(value) > 500 ? 800 : 300;

        base.SetParameter(name, value);
        if (TaskState == TaskState.Start
            && CurFeature == FeatureType.MScan
            && name.Equals(ParameterNames.MscanPoints))
        {
            StopTask();
            StartTask();
        }

        if (TaskState == TaskState.Start
            && CurFeature == FeatureType.MScne
            && (name.Equals(ParameterNames.MscanPoints)
                || name.Equals(ParameterNames.DwellSwitch)
                || name.Equals(ParameterNames.SquelchThreshold)))
        {
            StopTask();
            StartTask();
        }
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化网络连接
    /// </summary>
    private void InitNetWork()
    {
        IPEndPoint ep = new(IPAddress.Parse(Ip), Port);
        _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cmdSocket.Connect(ep);
        _cmdSocket.NoDelay = true;
        _localAddr = (_cmdSocket.LocalEndPoint as IPEndPoint)?.Address.ToString();
        if (_localAddr != null) ep = new IPEndPoint(IPAddress.Parse(_localAddr), 0);
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataSocket.Bind(ep);
        _localUdpPort = (_dataSocket.LocalEndPoint as IPEndPoint)?.Port ?? 0;
    }

    /// <summary>
    ///     初始化设备参数
    /// </summary>
    private void InitDevice()
    {
        //初始化接收机恢复默认状态
        SendCmd("*RST");
        //由于默认状态下音量不为0，此处将音量关闭
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        SendCmd("OUTP:TONE OFF");
        SendCmd("FORM ASC"); //定义二进制方式,制动高字节在高位;:FORM:BORD SWAP
        SendCmd("FORM:BORD SWAP"); //SWAP
        SendCmd("SYST:AUD:REM:MOD 2"); //PCM 32k,16bit,1ch 
        SendCmd("SENS:FREQ:AFC OFF"); //不使用自动频率控制
        SendCmd("MEAS:TIME DEF"); //测量时间为默认
        SendCmd("FREQ:SYNT:MODE LOWP");
        SendCmd("SYSTem:IF:REMote:MODe SHORT");
        _udpDataQueue = new ConcurrentQueue<byte[]>();
        CheckOption();
    }

    /// <summary>
    ///     清理所有非托管资源
    /// </summary>
    private void ReleaseResource()
    {
        _cts?.Cancel();
        try
        {
            _tcpDataTask?.Dispose();
            _tcpDataTask = null;
        }
        catch
        {
        }

        try
        {
            _udpDataTask?.Dispose();
            _udpDataTask = null;
        }
        catch
        {
        }

        try
        {
            _udpDataConvertTask?.Dispose();
            _udpDataConvertTask = null;
        }
        catch
        {
        }

        if (_cmdSocket != null)
            try
            {
                _cmdSocket.Close();
                _cmdSocket.Dispose();
            }
            catch
            {
            }

        _cmdSocket = null;
        if (_dataSocket != null)
            try
            {
                _dataSocket.Close();
                _dataSocket.Dispose();
            }
            catch
            {
            }

        _dataSocket = null;
        try
        {
            _udpDataQueue?.Clear();
            _udpDataQueue = null;
        }
        catch
        {
        }
    }

    /// <summary>
    ///     初始化所有线程
    /// </summary>
    private void InitThread()
    {
        _cts = new CancellationTokenSource();
        _tcpDataTask = new Task(p => TcpDataProcessAsync(p).ConfigureAwait(false), _cts.Token);
        _tcpDataTask.Start();
        _udpDataTask = new Task(p => UdpDataProcessAsync(p).ConfigureAwait(false), _cts.Token);
        _udpDataTask.Start();
        _udpDataConvertTask = new Task(p => UdpDataConvertProcessAsync(p).ConfigureAwait(false), _cts.Token);
        _udpDataConvertTask.Start();
    }

    #endregion

    #region 任务启动

    private void StopTask()
    {
        SendCmd("ABORT");
        SendCmd("FREQ:SYNT:MODE LOWP");
        //PSCAN扫描停止后若直接删除UDP通道会导致设备死机
        //此处统一处理为先将设备置为单频测量模式再删除UDP通道
        SendCmd("SENS:FREQ:MODE FIX");
        _mediaType = MediaType.None;
        CloseUdpPath();
        _udpDataQueue.Clear();
    }

    private void StartTask()
    {
        if (CurFeature == FeatureType.FFM)
        {
            _mediaType |= MediaType.Level;
            if (_spectrumSwitch) _mediaType |= MediaType.Spectrum;
            if (_audioSwitch) _mediaType |= MediaType.Audio;
            if (_iqSwitch) _mediaType |= MediaType.Iq;
            if (_ituSwitch) _mediaType |= MediaType.Itu;
        }
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Pscan && !_psOption)
            {
                Trace.WriteLine("本设备不支持PScan扫描模式!");
                ScanMode = ScanMode.Fscan;
            }

            if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
            {
                ScanMode = ScanMode.MScan;
                if (CurFeature == FeatureType.MScne)
                {
                    if (_audioSwitch)
                        _mediaType |= MediaType.Audio;
                    else
                        _mediaType &= ~MediaType.Audio;
                    if (_spectrumSwitch)
                        _mediaType |= MediaType.Spectrum;
                    else
                        _mediaType &= ~MediaType.Spectrum;
                }
                else
                {
                    _mediaType &= ~MediaType.Audio;
                    _mediaType &= ~MediaType.Spectrum;
                }
            }
            else if (CurFeature == FeatureType.FScne)
            {
                ScanMode = ScanMode.Fscan;
            }

            _mediaType |= MediaType.Scan;
        }
        else if (CurFeature == FeatureType.IFOUT)
        {
            IfBandwidth = _ifBandwidth > 500 ? 800 : 300;
        }

        SendMediaRequest();
    }

    /// <summary>
    ///     发送数据请求
    /// </summary>
    private void SendMediaRequest()
    {
        if (_mediaType == MediaType.None) return;
        //由于单频测量可以在任务运行过程中更改参数，所以此处需要先删除之前的UDP通道
        if (CurFeature == FeatureType.FFM) CloseUdpPath();
        OpenUdpPath();
        StartMeasure();
    }

    private void StartMeasure()
    {
        if ((_mediaType & MediaType.Scan) == 0)
        {
            if ((_mediaType & MediaType.Itu) > 0)
            {
                SendCmd("FUNC:CONC ON");
                SendCmd(
                    "FUNC \"VOLT:AC\", \"AM\", \"AM:POS\", \"AM:NEG\", \"FM\", \"FM:POS\", \"FM:NEG\", \"PM\", \"BAND\"");
            }
            else
            {
                SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            }

            SendCmd("SENS:FREQ:MODE FIX");
        }
        else
        {
            SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            StartScan();
        }
    }

    /// <summary>
    ///     关闭UDP数据通道
    /// </summary>
    private void CloseUdpPath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Thread.Sleep(10);
    }

    /// <summary>
    ///     打开UDP数据通道
    /// </summary>
    private void OpenUdpPath()
    {
        string tag = null;
        if ((_mediaType & MediaType.Audio) > 0) tag += "AUD,";
        if ((_mediaType & MediaType.Scan) > 0) tag += "FSC,PSC,MSC,";
        if ((_mediaType & MediaType.Spectrum) > 0) tag += "IFP,";
        if ((_mediaType & MediaType.Iq) > 0) tag += "IF,";
        if (tag != null)
        {
            tag = tag.Remove(tag.Length - 1);
            SendCmd($"TRAC:UDP:TAG:ON \"{_localAddr}\",{_localUdpPort},{tag}");
            Thread.Sleep(tag.Split(',').ToList().Contains("IF") ? 50 : 10);
            SendCmd($"TRAC:UDP:FLAG:ON \"{_localAddr}\",{_localUdpPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\"");
            Thread.Sleep(10);
        }
    }

    private void StartScan()
    {
        SendCmd("TRAC SSTART,0;TRAC SSTOP,0"); //删除接收机中的忽略频点
        switch (ScanMode)
        {
            case ScanMode.Fscan:
                StartFScan();
                break;
            case ScanMode.Pscan:
                StartPScan();
                break;
            case ScanMode.MScan:
                StartMScan();
                break;
            default:
                return;
        }

        //开启快速扫描,扫描速度最快 LOWQ:低相噪，NORM:常规 速度介于低相噪和快速之间，FAST:快速
        SendCmd("FREQ:SYNT:MODE FAST");
        SendCmd("CALC:IFP:AVER:TYPE OFF"); //关闭FFT模式，保证快速扫描
        Thread.Sleep(10);
        SendCmd("INIT");
    }

    /// <summary>
    ///     启动FScan扫描模式，并设置相关参数
    /// </summary>
    private void StartFScan()
    {
        SendCmd("SENS:FREQ:MODE SWE");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:STAR {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:STOP {StopFrequency} MHz");
            SendCmd($"SENS:SWE:STEP {StepFrequency} kHz");
            if (CheckFscanParameters()) break;
        }

        SendCmd("SENS:SWE:COUN INF");
        SendCmd("SENS:SWE:DIR UP");
        SendCmd("SENS:GCON:MODE AGC");
        if (CurFeature == FeatureType.FScne)
        {
            //在驻留频段扫描的时候设置实际设置值
            SendCmd("SWEep:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENS:SWE:DWELL {_dwellTime} s");
            SendCmd($"SENS:SWE:HOLD:TIME {_holdTime} s");
        }
        else
        {
            //在频段扫描-频点扫描模式下设置以下默认值
            SendCmd("OUTP:SQU:STAT OFF");
            SendCmd("SENS:DEM FM");
            SendCmd("SENS:SWE:DWELL 0");
            SendCmd("SENS:SWE:HOLD:TIME 0");
        }
    }

    /// <summary>
    ///     启动PScan模式，并设置相关参数
    /// </summary>
    private void StartPScan()
    {
        SendCmd("SENS:FREQ:MODE PSC");
        SendCmd("SENS:PSC:COUN INF");
        SendCmd("SENS:GCON:MODE AGC");
        SendCmd("OUTP:SQU:STAT OFF");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:PSC:START {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:PSC:STOP {StopFrequency} MHz");
            SendCmd($"PSC:STEP {StepFrequency} kHz");
            if (CheckPscanParameters()) break;
        }

        //由于解调模式与滤波带宽有约束且EM550的PSCan模式下解调模式为有效参数，
        //为避免从单频测量切换到全景扫描时违背约束出错，全景扫描时将解调模式置为默认值
        SendCmd("SENS:DEM FM");
    }

    /// <summary>
    ///     启动离散扫描，并设置相关参数
    /// </summary>
    private void StartMScan()
    {
        if (MScanPoints == null || MScanPoints.Length == 0 || MScanPoints.Length > 1000) return;
        SendCmd("FREQ:MODE MSC");
        SendCmd("MEMory:CLEar MEM0,MAXimum");
        SendCmd("OUTPUT:SQUELCH:CONTROL MEM"); //静噪控制方式
        if (CurFeature == FeatureType.MScan)
        {
            SendCmd("OUTP:SQU:STAT OFF");
            SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
            SendCmd("SENSE:MScan:DWELL 0");
            SendCmd("SENSE:MScan:HOLD:TIME 0");
        }
        else
        {
            SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENSE:MScan:DWELL {_dwellTime} s");
            SendCmd($"SENSE:MScan:HOLD:TIME {_holdTime} s");
        }

        SendCmd("SENSE:MScan:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        var att = _attenuation == -1 ? 0 : _attenuation;
        var attA = _attenuation == -1 ? "ON" : "OFF";
        _scanFreqs.Clear();
        for (var i = 0; i < MScanPoints.Length; i++)
        {
            if (CurFeature == FeatureType.MScan)
            {
                SendCmd(
                    $"MEM:CONT MEM{i},{MScanPoints[i][ParameterNames.Frequency]} MHz,0,{MScanPoints[i][ParameterNames.DemMode]},{MScanPoints[i][ParameterNames.FilterBandwidth]} kHz,(@1),{att},{attA},OFF,OFF,ON");
            }
            else
            {
                var squc = _squelchSwitch ? "ON" : "OFF";
                SendCmd(
                    $"MEM:CONT MEM{i},{MScanPoints[i][ParameterNames.Frequency]} MHz,{_squelchThreshold},{MScanPoints[i][ParameterNames.DemMode]},{MScanPoints[i][ParameterNames.FilterBandwidth]} kHz,(@1),{att},{attA},{squc},OFF,ON");
            }

            _scanFreqs.Add(Convert.ToDouble(MScanPoints[i][ParameterNames.Frequency]));
        }
    }

    /// <summary>
    ///     检查Fscan扫描参数是否已经设置成功
    /// </summary>
    private bool CheckFscanParameters()
    {
        try
        {
            var start = SendSyncCmd("SENS:FREQ:STAR?");
            if (Math.Abs(double.Parse(start) / 1000000 - StartFrequency) > 1e-9) return false;
            var stop = SendSyncCmd("SENS:FREQ:STOP?");
            if (Math.Abs(double.Parse(stop) / 1000000 - StopFrequency) > 1e-9) return false;
            var step = SendSyncCmd("SENS:SWE:STEP?");
            return Math.Abs(double.Parse(step) / 1000 - StepFrequency) < 1e-9;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     检查PScan扫描参数是否已经设置成功
    /// </summary>
    private bool CheckPscanParameters()
    {
        try
        {
            var start = SendSyncCmd("SENS:FREQ:PSC:START?");
            if (Math.Abs(double.Parse(start) / 1000000 - StartFrequency) > 1e-9) return false;
            var stop = SendSyncCmd("SENS:FREQ:PSC:STOP?");
            if (Math.Abs(double.Parse(stop) / 1000000 - StopFrequency) > 1e-9) return false;
            var band = SendSyncCmd("PSC:STEP?");
            return Math.Abs(double.Parse(band) / 1000 - StepFrequency) < 1e-9;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     检查当前设备是否包含ITU选件
    /// </summary>
    private void CheckOption()
    {
        var result = SendSyncCmd("*OPT?");
        var options = result.Split(',');
        _ituOption = options.Contains("IM");
        _psOption = options.Contains("PS");
    }

    #endregion

    #region 线程实现方法

    /// <summary>
    ///     TCP获取电平和ITU信息的线程函数
    /// </summary>
    /// <param name="obj"></param>
    public async Task TcpDataProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        //"VOLT:AC", "AM", "AM:POS", "AM:NEG", "FM", "FM:POS", "FM:NEG", "PM", "BAND"
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送
        //否则界面参数列表刷新时也会出现卡顿,此处处理为3秒左右发送一次到客户端
        var sendTime = DateTime.MinValue;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(10, token).ConfigureAwait(false);
                //当前未订阅电平或者ITU数据则直接返回
                if ((_mediaType & (MediaType.Level | MediaType.Itu)) == 0)
                {
                    //释放时间片，促进线程切换
                    await Task.Delay(1).ConfigureAwait(false);
                    continue;
                }

                //发送数据缓存列表
                List<object> datas = new();
                //获取电平和ITU数据
                var result = SendSyncCmd("SENS:DATA?");
                var values = result.Split(',');
                if ((_mediaType & MediaType.Level) > 0)
                {
                    _level = float.Parse(values[0]);
                    if ((_mediaType & MediaType.Spectrum) == 0)
                    {
                        //频谱开关关闭时在此发送否则和频谱一起发送
                        SDataLevel dataLevel = new()
                        {
                            Frequency = _frequency,
                            Bandwidth = _filterBandwidth,
                            Data = _level
                        };
                        datas.Add(dataLevel);
                    }
                }

                if ((_mediaType & MediaType.Itu) > 0 && values.Length >= 9)
                {
                    var ts = DateTime.Now - sendTime;
                    if (ts.TotalMilliseconds >= 3000)
                    {
                        var am = double.Parse(values[1]);
                        var fm = double.Parse(values[4]) / 1000d;
                        var fmpos = double.Parse(values[5]) / 1000d;
                        var fmneg = double.Parse(values[6]) / 1000d;
                        var pm = double.Parse(values[7]);
                        var bw = double.Parse(values[8]) / 1000d;
                        //TODO:无效为-9E37,此处用-1000000000f判断即可
                        SDataItu dataItu = new()
                        {
                            Frequency = _frequency,
                            Bandwidth = _filterBandwidth,
                            Modulation = Modulation.Iq
                        };
                        var dAmDepth = am is < 0 or > 100 ? double.MinValue : am;
                        var dFmDev = fm < -1000000000f ? double.MinValue : fm;
                        var dFmDevPos = fmpos < -1000000000f ? double.MinValue : fmpos;
                        var dFmDevNeg = fmneg < -1000000000f ? double.MinValue : fmneg;
                        var dPmDepth = pm < -1000000000f ? double.MinValue : pm;
                        dataItu.Misc = new Dictionary<string, object>
                        {
                            { ParameterNames.ItuAmDepth, dAmDepth },
                            { ParameterNames.ItuFmDev, dFmDev },
                            { ParameterNames.ItuFmDevPos, dFmDevPos },
                            { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                            { ParameterNames.ItuPmDepth, dPmDepth }
                        };
                        if (string.Equals(_bandMeasureMode, "XDB", StringComparison.OrdinalIgnoreCase))
                        {
                            var value = bw < -1000000000f ? double.MinValue : bw;
                            dataItu.Misc.Add(ParameterNames.ItuXdb, value);
                        }
                        else
                        {
                            var value = bw < -1000000000f ? double.MinValue : bw;
                            dataItu.Misc.Add(ParameterNames.ItuBeta, value);
                        }

                        datas.Add(dataItu);
                        sendTime = DateTime.Now;
                    }
                }

                if (datas.Count > 0 && TaskState == TaskState.Start) SendData(datas);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    /// <summary>
    ///     UDP读取数据线程函数
    /// </summary>
    /// <param name="obj"></param>
    public async Task UdpDataProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var buffer = new byte[1024 * 1024];
        _dataSocket.ReceiveBufferSize = buffer.Length;
        EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        while (!token.IsCancellationRequested)
            try
            {
                var recvBytes = _dataSocket.ReceiveFrom(buffer, SocketFlags.None, ref ep);
                if (recvBytes <= 0) break;
                var recvData = new byte[recvBytes];
                Buffer.BlockCopy(buffer, 0, recvData, 0, recvBytes);
                if (TaskState == TaskState.Start)
                    //Console.WriteLine($"收到数据，长度{recvBytes}");
                    _udpDataQueue.Enqueue(recvData);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
                await Task.Delay(1).ConfigureAwait(false);
            }
    }

    /// <summary>
    ///     UDP数据处理线程函数
    /// </summary>
    /// <param name="obj"></param>
    private async Task UdpDataConvertProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            if (_udpDataQueue.IsEmpty || !_udpDataQueue.TryDequeue(out var buffer))
            {
                await Task.Delay(1).ConfigureAwait(false);
                continue;
            }

            try
            {
                var offset = Marshal.SizeOf(typeof(Eb200DatagramFormat));
                List<object> sendDatas = new();
                while (offset < buffer.Length)
                {
                    GenericAttribute ga = new(buffer, offset);
                    offset += Marshal.SizeOf(typeof(GenericAttribute));
                    object data = null;
                    switch (ga.tag)
                    {
                        case (int)Tags.Audio:
                            data = ToAudio(buffer, offset);
                            break;
                        case (int)Tags.Ifpan:
                            data = ToSpectrum(buffer, offset);
                            break;
                        case (int)Tags.Fscan:
                            data = ToFScan(buffer, offset);
                            break;
                        case (int)Tags.Pscan:
                            data = ToPScan(buffer, offset);
                            break;
                        case (int)Tags.Mscan:
                            data = ToMScan(buffer, offset);
                            break;
                        case (int)Tags.If:
                            data = ToIq(buffer, offset);
                            break;
                    }

                    if (data != null)
                    {
                        if (data is List<object> list)
                            sendDatas.AddRange(list);
                        else
                            sendDatas.Add(data);
                    }

                    offset += ga.length;
                }

                if (sendDatas.Count > 0 && TaskState == TaskState.Start)
                {
                    sendDatas.RemoveAll(x => x == null);
                    SendData(sendDatas);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch
            {
            }
        }
    }

    #endregion

    #region 解析业务数据

    /// <summary>
    ///     解析音频数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>音频类数据</returns>
    private static object ToAudio(byte[] buffer, int offset)
    {
        try
        {
            CommonHeader pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeader));
            //new(buffer, offset);
            offset += pCommon.optional_header_length;
            var pAudioData = new byte[pCommon.number_of_trace_items * 2];
            Buffer.BlockCopy(buffer, offset, pAudioData, 0, pAudioData.Length);
            SDataAudio dataAudio = new()
            {
                Format = AudioFormat.Pcm,
                SamplingRate = 32000,
                BytesPerSecond = 32000 * 2,
                BitsPerSample = 16,
                BlockAlign = 2,
                Channels = 1,
                Data = pAudioData
            };
            return dataAudio;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>IQ类数据</returns>
    private static object ToIq(byte[] buffer, int offset)
    {
        try
        {
            CommonHeader pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeader));
            OptionalHeaderIf header = new(buffer, offset);
            offset += pCommon.optional_header_length;
            if (pCommon.number_of_trace_items == 0) return null;
            var iq = new short[pCommon.number_of_trace_items * 2];
            Buffer.BlockCopy(buffer, offset, iq, 0, pCommon.number_of_trace_items * 4);
            SDataIq dataIq = new()
            {
                Frequency = header.Freq / 1000000d,
                Bandwidth = header.Bw / 1000d,
                SamplingRate = header.Samplerate / 1000d,
                Attenuation = header.RxAtt,
                Data16 = iq
            };
            return dataIq;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析频谱数据(单频测量使用)
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>音频类数据</returns>
    private object ToSpectrum(byte[] buffer, int offset)
    {
        try
        {
            CommonHeader pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeader));
            OptionalHeaderIfPan header = new(buffer, offset);
            offset += pCommon.optional_header_length;
            var spectrum = new short[pCommon.number_of_trace_items];
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = BitConverter.ToInt16(buffer, offset);
                if (spectrum[i] > 1200 || spectrum[i] < -9990) return null;
                offset += 2;
            }

            List<object> datas = new();
            SDataSpectrum dataSpectrum = new()
            {
                Frequency = header.frequency / 1000000d,
                Span = header.spanFrequency / 1000d,
                Data = spectrum
            };
            datas.Add(dataSpectrum);
            if ((_mediaType & MediaType.Level) > 0 && Math.Abs(_level - float.MinValue) > 1e-9)
            {
                SDataLevel dataLevel = new()
                {
                    Frequency = _frequency,
                    Bandwidth = _filterBandwidth,
                    Data = _level
                };
                datas.Add(dataLevel);
            }

            return datas;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析频段扫描数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>扫描类数据</returns>
    private object ToFScan(byte[] buffer, int offset)
    {
        try
        {
            CommonHeader pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeader));
            //new(buffer, offset);
            offset += pCommon.optional_header_length;
            var levels = new short[pCommon.number_of_trace_items];
            var freqs = new double[pCommon.number_of_trace_items];
            var flag = Flags.Level;
            while (flag != Flags.Siggtsqu)
            {
                if ((pCommon.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < levels.Length; i++)
                                levels[i] = BitConverter.ToInt16(buffer, i * 2 + offset);
                            offset += 2 * pCommon.number_of_trace_items;
                            break;
                        case Flags.Frequency:
                            for (var i = 0; i < freqs.Length; i++)
                                freqs[i] = BitConverter.ToUInt32(buffer, i * 4 + offset) / 1000000d;
                            offset += 4 * pCommon.number_of_trace_items;
                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            //有可能包含多帧数据（当测量时间极小，频段个数极少时）
            List<short> tempLevels = new();
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            var j = 0;
            for (; j < pCommon.number_of_trace_items; ++j)
                if (levels[j] != 2000)
                {
                    tempLevels.Add(levels[j]);
                }
                else
                {
                    //完成一次完整扫描
                    var currIndex = Utils.GetCurrIndex(freqs[j - tempLevels.Count], StartFrequency, StepFrequency);
                    if (currIndex >= 0)
                    {
                        for (var k = currIndex + tempLevels.Count; k < total; ++k) tempLevels.Add(0);
                        SDataScan scan = new()
                        {
                            StartFrequency = StartFrequency,
                            StopFrequency = StopFrequency,
                            StepFrequency = StepFrequency,
                            Offset = currIndex,
                            Total = total,
                            Data = tempLevels.ToArray()
                        };
                        if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                    }

                    tempLevels.Clear();
                }

            if (tempLevels.Count > 0)
            {
                var currIndex = Utils.GetCurrIndex(freqs[j - tempLevels.Count], StartFrequency, StepFrequency);
                if (currIndex >= 0)
                {
                    SDataScan scan = new()
                    {
                        StartFrequency = StartFrequency,
                        StopFrequency = StopFrequency,
                        StepFrequency = StepFrequency,
                        Offset = currIndex,
                        Total = total,
                        Data = tempLevels.ToArray()
                    };
                    return scan;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析全景扫描数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>扫描类数据</returns>
    private object ToPScan(byte[] buffer, int offset)
    {
        try
        {
            CommonHeader pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeader));
            //new(buffer, offset);
            offset += pCommon.optional_header_length;
            List<short> levels = new()
            {
                Capacity = pCommon.number_of_trace_items
            };
            //本包数据第一个频点的索引
            var currIndex = -1;
            var flag = Flags.Level;
            while (flag != Flags.Siggtsqu)
            {
                if ((pCommon.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < pCommon.number_of_trace_items; i++)
                                levels.Add(BitConverter.ToInt16(buffer, i * 2 + offset));
                            offset += 2 * pCommon.number_of_trace_items;
                            break;
                        case Flags.Frequency:
                            double freq = BitConverter.ToUInt32(buffer, offset);
                            currIndex = Utils.GetCurrIndex(freq / 1000000d, StartFrequency, StepFrequency);
                            offset += 4 * pCommon.number_of_trace_items;
                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            if (currIndex < 0) return null;
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            //表示本次扫描结束,此处仅补齐缺少的点
            if (levels.Last() == 2000)
            {
                levels.RemoveAt(levels.Count - 1);
                for (var i = currIndex + levels.Count; i < total; ++i) levels.Add(0);
            }

            SDataScan scan = new()
            {
                StartFrequency = StartFrequency,
                StopFrequency = StopFrequency,
                StepFrequency = StepFrequency,
                Offset = currIndex,
                Total = total,
                Data = levels.ToArray()
            };
            return scan;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析离散扫描数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>离散扫描类数据</returns>
    private object ToMScan(byte[] buffer, int offset)
    {
        try
        {
            CommonHeader pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeader));
            //new(buffer, offset);
            offset += pCommon.optional_header_length;
            var levels = new short[pCommon.number_of_trace_items];
            var freqs = new double[pCommon.number_of_trace_items];
            var flag = Flags.Level;
            while (flag != Flags.Siggtsqu)
            {
                if ((pCommon.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < levels.Length; i++)
                                levels[i] = BitConverter.ToInt16(buffer, i * 2 + offset);
                            offset += 2 * pCommon.number_of_trace_items;
                            break;
                        case Flags.Frequency:
                            for (var i = 0; i < freqs.Length; i++)
                                freqs[i] = BitConverter.ToUInt32(buffer, i * 4 + offset) / 1000000d;
                            offset += 4 * pCommon.number_of_trace_items;
                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            List<short> tempLevels = new();
            var j = 0;
            for (; j < pCommon.number_of_trace_items; ++j)
                if (levels[j] != 2000)
                {
                    tempLevels.Add(levels[j]);
                }
                else
                {
                    //完成一次完整扫描
                    var currIndex = _scanFreqs.IndexOf(freqs[j - tempLevels.Count]);
                    if (currIndex >= 0)
                    {
                        SDataScan scan = new()
                        {
                            Offset = currIndex,
                            Total = _scanFreqs.Count,
                            Data = tempLevels.ToArray()
                        };
                        if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                    }

                    tempLevels.Clear();
                }

            if (tempLevels.Count > 0)
            {
                var currIndex = _scanFreqs.IndexOf(freqs[j - tempLevels.Count]);
                if (currIndex >= 0)
                {
                    SDataScan scan = new()
                    {
                        Offset = currIndex,
                        Total = _scanFreqs.Count,
                        Data = tempLevels.ToArray()
                    };
                    return scan;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     发送查询命令
    /// </summary>
    /// <param name="cmd"></param>
    private void SendCmd(string cmd)
    {
        Console.WriteLine($"==> {cmd}");
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        _cmdSocket.Send(buffer);
    }

    private readonly byte[] _tcpRecvBuffer = new byte[1024 * 1024]; //避免该方法被调用时频繁申请内存
    private readonly object _lockObject = new();

    /// <summary>
    ///     用于发送查询类指令并获取查询结果
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>查询结果</returns>
    private string SendSyncCmd(string cmd)
    {
        lock (_lockObject)
        {
            var sendBuffer = Encoding.ASCII.GetBytes(cmd + "\n");
            _cmdSocket.Send(sendBuffer);
            var result = string.Empty;
            var recvCount = _cmdSocket.Receive(_tcpRecvBuffer, SocketFlags.None);
            if (recvCount > 0)
                result = _tcpRecvBuffer[recvCount - 1] == '\n'
                    ? Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount - 1)
                    : Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount);

            return result;
        }
    }

    #endregion

    #region IFBandwidth/DemMode

    /// <summary>
    ///     单位 kHz
    /// </summary>
    /// <param name="dstIfBandwidth"></param>
    private void SetIfBandwidth(double dstIfBandwidth)
    {
        if (dstIfBandwidth > 9)
        {
            //当解调模式为CW,USB,LSB时,解调带宽只能<=9kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode is "CW" or "USB" or "LSB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }
        else if (dstIfBandwidth < 0.6)
        {
            //当解调带宽为ISB时，解调带宽只能 >= 0.6 kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode == "ISB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:BAND {dstIfBandwidth} kHz");
    }

    private void SetDemodulation(Modulation dstDemMode)
    {
        if (dstDemMode is Modulation.Cw or Modulation.Usb or Modulation.Lsb or Modulation.Isb)
        {
            //当解调带宽 > 9kHz时，解调带宽不能设置为CW,USB,LSB
            //当解调带宽 < 0.6 kHz时，解调带宽不能设置为ISB
            var strIfBandwidth = SendSyncCmd("SENS:BAND?");
            var ifBandwidth = double.Parse(strIfBandwidth) / 1000;
            if (dstDemMode == Modulation.Isb)
            {
                if (ifBandwidth < 0.6)
                {
                    SendCmd("SENS:BAND 600 hz");
                    Thread.Sleep(10);
                }
            }
            else if (ifBandwidth > 9)
            {
                SendCmd("SENS:BAND 9 kHz");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:DEM {dstDemMode}");
    }

    #endregion
}