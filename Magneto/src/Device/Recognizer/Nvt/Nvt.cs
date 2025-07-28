#if !DEMO
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.UavDef;
using Magneto.Protocol.Define;
using Nvt.SDK;
using static Nvt.SDK.GlobalMembers;

namespace Nvt;

public struct ObjRect
{
    public int X1;
    public int Y1;
    public int X2;
    public int Y2;
}

public partial class Nvt : DeviceBase
{
    private static bool _firstInitializedFailed = true;
    private readonly IntPtr _contextIntPtr = GCHandle.ToIntPtr(GCHandle.Alloc(new object()));
    private readonly WebSocketServer _webSocketServer = new();
    private StreamCallback _streamDataCallback;
    private TmccTrackerDataCallback _trackerDataCallback;
    private readonly UdpClient _udpClient = new();

    public Nvt(Guid deviceId) : base(deviceId)
    {
    }

    #region Override Methods

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (!result) return false;
        DisConnect();
        Release();
        Init();
        //实时视频流连接
        var ret = 0;
        ret |= TMCC_SetAutoReConnect(_preView, true);
        // .NET6.0 返回值一直不为0.但解码有效果。
        _ = TMCC_SetDisplayShow(_preView, true);
        ret |= TMCC_SetStreamBufferTime(_preView, 0);
        var stream = new GlobalMembers.TmPlayRealStreamCfgT();
        stream.dwSize = (uint)Marshal.SizeOf(stream);
        stream.szAddress = Get(32, IpAddress.ToCharArray());
        stream.szTurnAddress = Get(32, IpAddress.ToCharArray());
        stream.szUser = Get(32, UserName.ToCharArray());
        stream.szPass = Get(32, Password.ToCharArray());
        stream.iPort = Port;
        stream.byChannel = 0;
        stream.byStream = 0;
        stream.byForceDecode = 1;
        ret |= TMCC_ConnectStream(_preView, ref stream, IntPtr.Zero);
        if (ret != TmccErrSuccess)
        {
            if (_firstInitializedFailed) Console.WriteLine("Nvt模块，图像识别设备初始化失败！");
            _firstInitializedFailed = false;
            return false;
        }

        /*控制连接，如配置参数，转台控制等*/
        var info = new GlobalMembers.TmConnectInfoT();
        info.dwSize = (uint)Marshal.SizeOf(info);
        info.pIp = Get(32, IpAddress.ToCharArray());
        info.szPass = Get(32, "system".ToCharArray());
        info.szUser = Get(32, "system".ToCharArray());
        info.iPort = 6002;
        ret |= TMCC_SetAutoReConnect(_login, true);
        ret |= TMCC_Connect(_login, ref info, false);

        _trackerDataCallback = DataCallBack;
        _ = TMCC_Tracker_RegisterDataCallBack(_tracker, _trackerDataCallback, _tracker);

        _streamDataCallback = StreamDataCallBack;
        _ = TMCC_RegisterStreamCallBack(_preView, _streamDataCallback, _contextIntPtr);
        if (ret is TmccErrSuccess)
        {
            _ = _webSocketServer.StartAsync();
            return true;
        }

        if (_firstInitializedFailed) Console.WriteLine("Nvt模块，图像识别设备登录失败！");
        _firstInitializedFailed = false;
        return false;
    }

    public void StreamDataCallBack(IntPtr hTmCc, ref GlobalMembers.TmRealStreamInfoT pStreamInfo, IntPtr context)
    {
        if (pStreamInfo.byFrameType != 0) return;
        var data = new byte[pStreamInfo.iBufferSize];
        Marshal.Copy(pStreamInfo.pBuffer, data, 0, pStreamInfo.iBufferSize);
        _webSocketServer.SendData(data);
    }

    private static void DataCallBack(uint icmd, IntPtr pdata, int idatalen, IntPtr context)
    {
        if (pdata.Equals(IntPtr.Zero)) return;
        switch (icmd)
        {
            case (uint)OptoCfgE.OptoCmdStatus:
                TagOptoDeviceStateT status = new();
                Marshal.PtrToStructure(pdata, status);
                break;
            case (uint)OptoCfgE.OptoCmdHorver:
                TagOptoHvInfoT hv = new();
                Marshal.PtrToStructure(pdata, hv);
                break;
            case (uint)OptoCfgE.OptoCmdStatusExtend:
                TagOptoDeviceStateExT statusEx = new();
                Marshal.PtrToStructure(pdata, statusEx);
                break;
            case (uint)OptoCfgE.OptoCmdNotifyTarget:
                TagSearchDataExT search = new();
                Marshal.PtrToStructure(pdata, search);
                break;
            case (uint)OptoCfgE.OptoCmdLensExtend:
                TagLensInfoT lens = new();
                Marshal.PtrToStructure(pdata, lens);
                break;
            case (uint)OptoCfgE.OptoCmdTrackStatus:
                TagTrackStatusT track = new();
                Marshal.PtrToStructure(pdata, track);
                break;
        }
    }

    #endregion

    #region Global Members

    /// <summary>
    ///     是否初始化标志
    /// </summary>
    private bool _init;

    /// <summary>
    ///     录制文件的唯一标志；会记录到数据库和文件名上。
    /// </summary>
    private Guid _recordFileIdentity;

    private DateTime _startTime;

    /// <summary>
    ///     否连接标志
    /// </summary>
    private bool _connect;

    private readonly object _pdzLocker = new();
    private IntPtr _preView = IntPtr.Zero;

    private IntPtr _login = IntPtr.Zero;

    //private GlobalMembers.TmVideoInCfgT _videoIn = new();
    //private GlobalMembers.TmAlarmInfoT _alarmInfo = new();
    private GlobalMembers.TmAiSelObjectT _stSelectObj;
    private GlobalMembers.TmAiVideoInCfgT _stAiVin;

    private TmTraceParamT _stTracerParam;

    //private ObjRect _mStObjectRect = new();
    /*回放相关*/
    private IntPtr _play = IntPtr.Zero;
    private IntPtr _tracker = IntPtr.Zero;

    #endregion

    #region Private Methods

    /// <summary>
    ///     初始化
    /// </summary>
    private void Init()
    {
        if (_init) return;
        /*句柄的初始化只需要一个实例一次，通常在进程退出时释放*/
        _preView = TMCC_Init(5); //实时流句柄
        _login = TMCC_Init(0); //控制连接句柄
        _play = TMCC_Init(TmccInitTypeStream); //回放句柄
        _tracker = TMCC_Tracker_Init(IpAddress, 9966);
        _init = true;
    }

    /// <summary>
    ///     释放资源，通常是在进程退出时调用
    /// </summary>
    private void Release()
    {
        if (!_init) return;
        _ = TMCC_Done(_preView);
        _ = TMCC_Done(_play);
        _ = TMCC_Done(_login);
        TMCC_Tracker_Done(_tracker);
        _preView = IntPtr.Zero;
        _play = IntPtr.Zero;
        _login = IntPtr.Zero;
        _tracker = IntPtr.Zero;
        _init = false;
    }

    /// <summary>
    ///     断开连接
    /// </summary>
    private void DisConnect()
    {
        if (!_connect) return;
        /*断开视频流连接*/
        _ = TMCC_CloseStream(_preView);
        /*断开控制连接*/
        _ = TMCC_DisConnect(_login);
        _connect = false;
    }

    /// <summary>
    ///     控制云台转动
    /// </summary>
    /// <param name="dwPtzCommand">命令代号</param>
    /// <param name="dwPtzControl">启动（命令>0）/停止（0）</param>
    /// <param name="dwSpeed">执行速度</param>
    private void ControlPtz(uint dwPtzCommand, uint dwPtzControl, uint dwSpeed)
    {
        lock (_pdzLocker)
        {
            _ = TMCC_PtzOpen(_login, 0, true);
            _ = TMCC_PtzLock(_login, 0);
            _ = TMCC_PtzControl(_login, dwPtzCommand, dwPtzControl, dwSpeed);
            _ = TMCC_PtzClose(_login);
        }
    }

    private void TurnLaser(bool turnOn)
    {
        const int iChannel = 0;
        const uint dwPtzCmd = PtzLightPwrOn;
        var dwStart = turnOn ? 1u : 0u; /*1--开启 0--关闭*/
        const uint dwPtzSpeed = 40u;
        _ = TMCC_PtzOpen(_login, iChannel, true);
        _ = TMCC_PtzLock(_login, iChannel);
        _ = TMCC_PtzControl(_login, dwPtzCmd, dwStart, dwPtzSpeed);
        _ = TMCC_PtzClose(_login);
    }

    private void TurnTrack()
    {
        /*停止跟踪*/
        var cmdInfo = new GlobalMembers.TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.iChannel = 0; /*如果是热像，=1*/
        cmdInfo.iStream = 0;
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdStopTrace;
        cmdInfo.iCommandBufferLen = 0;
        cmdInfo.iCommandDataLen = 0;
        cmdInfo.dwResult = 0;
        //设置参数
        _ = TMCC_SetConfig(_login, ref cmdInfo) is TmccErrSuccess;
        _ = TMCC_SaveConfig(_login);
    }

    private void TurnVisibleLight(bool isVisibleLight)
    {
        _stAiVin.dwSize = (uint)Marshal.SizeOf(_stAiVin);
        /*修改*/
        _stAiVin.byVideoId = isVisibleLight ? (byte)1 : (byte)2;
        _stAiVin.byEnable = 1;
        var spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stAiVin));
        Marshal.StructureToPtr(_stAiVin, spObj, false);
        var cmdInfo = new GlobalMembers.TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdAiVideoCfg;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stAiVin);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stAiVin);
        cmdInfo.dwResult = 0;
        _ = TMCC_SetConfig(_login, ref cmdInfo);
        _ = TMCC_SaveConfig(_login);
    }

    private void TurnHideTrackRect(bool hideTrackingRect)
    {
        /*先获取原来的配置参数*/
        _stTracerParam.dwSize = (UInt32)Marshal.SizeOf(_stTracerParam);
        IntPtr spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stTracerParam));
        Marshal.StructureToPtr(_stTracerParam, spObj, false);

        var cmdInfo = new GlobalMembers.TmCommandInfoT();
        cmdInfo.dwSize = (UInt32)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        TMCC_GetConfig(_login, ref cmdInfo);

        var pByte = new byte[Marshal.SizeOf(_stTracerParam)];
        Marshal.Copy(cmdInfo.pCommandBuffer, pByte, 0, Marshal.SizeOf(_stTracerParam));
        _stTracerParam = (TmTraceParamT)BytesToStruct(pByte, _stTracerParam.GetType());
        /*修改需要设置的参数，然后设置进去*/
        _stTracerParam.byHideTrackRect = (byte)(hideTrackingRect ? 1 : 0); //隐藏跟踪框
        Marshal.StructureToPtr(_stTracerParam, spObj, false);
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        _ = TMCC_SetConfig(_login, ref cmdInfo);
        /*保存配置，保证设备断电重启还生效*/
        _ = TMCC_SaveConfig(_login);
    }

    private void TurnHideIdentifyRect(bool hideIdentifyRect)
    {
        /*先获取原来的配置参数*/
        _stTracerParam.dwSize = (UInt32)Marshal.SizeOf(_stTracerParam);
        IntPtr spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stTracerParam));
        Marshal.StructureToPtr(_stTracerParam, spObj, false);

        var cmdInfo = new GlobalMembers.TmCommandInfoT();
        cmdInfo.dwSize = (UInt32)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        TMCC_GetConfig(_login, ref cmdInfo);

        var pByte = new byte[Marshal.SizeOf(_stTracerParam)];
        Marshal.Copy(cmdInfo.pCommandBuffer, pByte, 0, Marshal.SizeOf(_stTracerParam));
        _stTracerParam = (TmTraceParamT)BytesToStruct(pByte, _stTracerParam.GetType());
        /*修改需要设置的参数，然后设置进去*/
        _stTracerParam.byHideIdentifyRect = (byte)(hideIdentifyRect ? 1 : 0); //隐藏跟踪框
        Marshal.StructureToPtr(_stTracerParam, spObj, false);
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        _ = TMCC_SetConfig(_login, ref cmdInfo);
        /*保存配置，保证设备断电重启还生效*/
        _ = TMCC_SaveConfig(_login);
    }

    private bool SetTraceArea(IReadOnlyList<int> area)
    {
        _stSelectObj.dwSize = (uint)Marshal.SizeOf(_stSelectObj);
        _stSelectObj.bySelMode = 0; /*设置为手动选择目标*/
        _stSelectObj.dwLeft = area[0];
        _stSelectObj.dwRight = area[1];
        _stSelectObj.dwTop = area[2];
        _stSelectObj.dwBottom = area[3];
        var spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stSelectObj));
        Marshal.StructureToPtr(_stSelectObj, spObj, false);
        var cmdInfo = new GlobalMembers.TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.iChannel = 0; /*如果是热像，=1*/
        cmdInfo.iStream = 0;
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdSelectObject;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stSelectObj);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stSelectObj);
        cmdInfo.dwResult = 0;
        //设置参数
        var ret = TMCC_SetConfig(_login, ref cmdInfo);
        _ = TMCC_SaveConfig(_login);
        return ret is TmccErrSuccess;
    }

    private bool SetTraceAuto(bool isAuto)
    {
        /*先获取原来的配置参数*/
        _stTracerParam.dwSize = (UInt32)Marshal.SizeOf(_stTracerParam);
        IntPtr spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stTracerParam));
        Marshal.StructureToPtr(_stTracerParam, spObj, false);

        var cmdInfo = new GlobalMembers.TmCommandInfoT();
        cmdInfo.dwSize = (UInt32)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        TMCC_GetConfig(_login, ref cmdInfo);

        var pByte = new byte[Marshal.SizeOf(_stTracerParam)];
        Marshal.Copy(cmdInfo.pCommandBuffer, pByte, 0, Marshal.SizeOf(_stTracerParam));
        _stTracerParam = (TmTraceParamT)BytesToStruct(pByte, _stTracerParam.GetType());

        /*修改需要设置的参数，然后设置进去*/
        _stTracerParam.byEnableAutoTrace = (byte)(isAuto ? 1 : 0); //自动跟踪
        Marshal.StructureToPtr(_stTracerParam, spObj, false);

        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        var ret = TMCC_SetConfig(_login, ref cmdInfo);
        _ = TMCC_SaveConfig(_login);
        return ret is TmccErrSuccess;
    }

    private void ScreenShot()
    {
        var guid = Guid.NewGuid();
        var path = "/videos/" + guid + ".jpeg";
        if (TMCC_CapturePictureToFile(_preView, path, "JPEG").Equals(0))
            SendData(new List<object>
            {
                new PlaybackFile
                {
                    FilePath = "/videos/",
                    CreatedAt = DateTime.Now,
                    FileType = FileType.Image,
                    FileName = guid + ".jpeg",
                    UpDatedAt = DateTime.Now
                }
            });
    }

    private void Record(bool isRunning)
    {
        if (isRunning)
        {
            _recordFileIdentity = Guid.NewGuid();
            TMCC_StartRecord(_preView,
                AppDomain.CurrentDomain.BaseDirectory + "/videos/" + _recordFileIdentity + ".avi",
                "mp4");
            _startTime = DateTime.Now;
        }
        else
        {
            TMCC_StopRecord(_preView);
            var path = "/videos/" + _recordFileIdentity;
            _ = Task.Run(() => ConvertAviToMp4(AppDomain.CurrentDomain.BaseDirectory + path + ".avi",
                AppDomain.CurrentDomain.BaseDirectory + path + ".mp4"));
            SendData(new List<object>
            {
                new PlaybackFile
                {
                    FilePath = "/videos/",
                    CreatedAt = _startTime,
                    FileType = FileType.Video,
                    FileName = _recordFileIdentity + ".mp4",
                    UpDatedAt = DateTime.Now,
                    Duration = DateTime.Now - _startTime
                }
            });
        }
    }

    public static void ConvertAviToMp4(string aviFilePath, string mp4FilePath)
    {
        //设置 FFmpeg 可执行文件的路径
        if (!Utils.GetFFmpegPath(out var ffmpegPath))
            return;
        //设置命令行参数
        var arguments =
            $"-i \"{aviFilePath}\" -codec:v libx264 -preset medium -crf 23 -codec:a aac -b:a 128k -movflags +faststart \"{mp4FilePath}\"";
        //启动 FFmpeg 进程
        var ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = ffmpegPath;
        ffmpegProcess.StartInfo.Arguments = arguments;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = true;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.Start();
        //等待进程结束
        ffmpegProcess.WaitForExit();
        File.Delete(aviFilePath);
    }

    private static char[] Get(int size, char[] arr)
    {
        var total = new char[size];
        Array.Copy(arr, total, arr.Length);
        return total;
    }

    public static object BytesToStruct(byte[] bytes, Type strcutType)
    {
        int size = Marshal.SizeOf(strcutType);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, buffer, size);
            return Marshal.PtrToStructure(buffer, strcutType);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    #endregion
}
#endif