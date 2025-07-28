using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class Em200 : DeviceBase
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

    private CancellationTokenSource _ctsTcpData;
    private CancellationTokenSource _ctsUdpData;
    private CancellationTokenSource _ctsDataConvert;

    /// <summary>
    ///     缓存接收到的数据
    /// </summary>
    private ConcurrentQueue<byte[]> _udpDataQueue;

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

    #endregion

    #region ReceiverBase

    public Em200(Guid id) : base(id)
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
            InitTasks();
            //心跳检测
            SetHeartBeat(_cmdSocket);
        }

        return result;
    }

    public override void Start(FeatureType featureType, IDataPort dataPort)
    {
        base.Start(featureType, dataPort);
        StartTask();
    }

    public override void Stop()
    {
        StopTask();
        Task.Delay(20).ConfigureAwait(false).GetAwaiter().GetResult();
        _udpDataQueue?.Clear();
        base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        _udpDataQueue?.Clear();
        if (TaskState == TaskState.Start
            && CurFeature == FeatureType.MScan
            && (name.Equals(ParameterNames.MscanPoints)
                || name.Equals(ParameterNames.DwellSwitch)
                || name.Equals(ParameterNames.SquelchThreshold)))
        {
            StopTask();
            StartTask();
        }
    }

    public override void Dispose()
    {
        ReleaseResource();
        base.Dispose();
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
        if (_localAddr != null) ep = new IPEndPoint(IPAddress.Parse(_localAddr), 19000);
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _dataSocket.Bind(ep);
        _dataSocket.Connect(Ip, 0);
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
        if (MonitorAntenna >= 0)
            SendCmd("ROUTe:VUHF (@" + MonitorAntenna + ")");
        else
            SendCmd("ROUTe:AUTO ON");
        CheckOption();
        _udpDataQueue = new ConcurrentQueue<byte[]>();
        //CheckOption();
    }

    /// <summary>
    ///     清理所有非托管资源
    /// </summary>
    private void ReleaseResource()
    {
        Utils.CancelTask(_tcpDataTask, _ctsTcpData);
        Utils.CancelTask(_udpDataTask, _ctsUdpData);
        Utils.CancelTask(_udpDataConvertTask, _ctsDataConvert);
        Utils.CloseSocket(_cmdSocket);
        Utils.CloseSocket(_dataSocket);
        if (_udpDataQueue != null)
            try
            {
                _udpDataQueue.Clear();
            }
            catch
            {
            }

        _udpDataQueue = null;
    }

    /// <summary>
    ///     初始化所有线程
    /// </summary>
    private void InitTasks()
    {
        _ctsTcpData = new CancellationTokenSource();
        _tcpDataTask = new Task(p => TcpDataProcessAsync(p).ConfigureAwait(false), _ctsTcpData.Token);
        _tcpDataTask.Start();
        _ctsUdpData = new CancellationTokenSource();
        _udpDataTask = new Task(UdpDataProcess, _ctsUdpData.Token);
        _udpDataTask.Start();
        _ctsDataConvert = new CancellationTokenSource();
        _udpDataConvertTask = new Task(p => UdpDataConvertProcessAsync(p).ConfigureAwait(false), _ctsDataConvert.Token);
        _udpDataConvertTask.Start();
    }

    #endregion

    #region 任务启动

    private void StartTask()
    {
        if (CurFeature.Equals(FeatureType.FFM))
        {
            _mediaType |= MediaType.Level;
        }
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Pscan && !_psOption)
                throw new Exception("当前设备没有全景扫描选件不支持全景扫描，请使用频点扫描！");
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

        SendMediaRequest();
    }

    private void StopTask()
    {
        SendCmd("ABORT");
        SendCmd("FREQ:SYNT:MODE LOWP");
        //PSCAN扫描停止后若直接删除UDP通道会导致设备死机
        //此处统一处理为先将设备置为单频测量模式再删除UDP通道
        SendCmd("SENS:FREQ:MODE FIX");
        _mediaType = MediaType.None;
        CloseUdpPath();
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
            SendCmd(
                $"TRAC:UDP:FLAG:ON \"{_localAddr}\",{_localUdpPort},\"SWAP\",\"PRIV\", \"IFP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\",\"FREQ:HIGH:RX\",\"FREQ:OFFS\",\"FSTR\"");
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

        //由于解调模式与滤波带宽有约束且EM200的PSCan模式下解调模式为有效参数，
        //为避免从单频测量切换到全景扫描时违背约束出错，全景扫描时将解调模式置为默认值
        SendCmd("SENS:DEM FM");
    }

    /// <summary>
    ///     启动离散扫描，并设置相关参数
    /// </summary>
    private void StartMScan()
    {
        if (Frequencys == null || Frequencys.Length == 0 || Frequencys.Length > 1000) return;
        SendCmd("FREQ:MODE MSC");
        SendCmd("MEMory:CLEar MEM0,MAXimum");
        SendCmd("OUTPUT:SQUELCH:CONTROL MEM"); //静噪控制方式
        if (CurFeature == FeatureType.MScan)
        {
            SendCmd("OUTP:SQU:STAT OFF");
            SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
            SendCmd("SENSE:MSCAN:DWELL 0");
            SendCmd("SENSE:MSCAN:HOLD:TIME 0");
        }
        else
        {
            SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENSE:MSCAN:DWELL {_dwellTime} s");
            SendCmd($"SENSE:MSCAN:HOLD:TIME {_holdTime} s");
        }

        SendCmd("SENSE:MSCAN:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        var att = _attenuation == -1 ? 0 : _attenuation;
        var attA = _attenuation == -1 ? "ON" : "OFF";
        _scanFreqs.Clear();
        for (var i = 0; i < Frequencys.Length; i++)
        {
            if (CurFeature == FeatureType.MScan)
            {
                SendCmd(
                    $"MEM:CONT MEM{i},{Frequencys[i][ParameterNames.Frequency]} MHz,0,{Frequencys[i][ParameterNames.DemMode]},{Frequencys[i][ParameterNames.FilterBandwidth]} kHz,(@1),{att},{attA},OFF,OFF,ON");
            }
            else
            {
                var squc = _squelchSwitch ? "ON" : "OFF";
                SendCmd(
                    $"MEM:CONT MEM{i},{Frequencys[i][ParameterNames.Frequency]} MHz,{_squelchThreshold},{Frequencys[i][ParameterNames.DemMode]},{Frequencys[i][ParameterNames.FilterBandwidth]} kHz,(@1),{att},{attA},{squc},OFF,ON");
            }

            _scanFreqs.Add(Convert.ToDouble(Frequencys[i][ParameterNames.Frequency]));
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
            if (Math.Abs(double.Parse(step) / 1000 - StepFrequency) > 1e-9) return false;
            return true;
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
            if (Math.Abs(double.Parse(band) / 1000 - StepFrequency) > 1e-9) return false;
            return true;
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
}