#if DEMO
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Magneto.Contract.BaseClass;
using Magneto.Contract.UavDef;
using static Magneto.Device.Nvt.SDK.GlobalMembers;

namespace Magneto.Device.Nvt;

public struct ObjRect
{
    public int X1;
    public int Y1;
    public int X2;
    public int Y2;
}

public partial class Nvt : DeviceBase
{
    public Nvt(Guid deviceId) : base(deviceId)
    {
        _ = _webSocketServer.StartAsync();
    }

    #region Global Members

    private readonly WebSocketServer _webSocketServer = new();

    /// <summary>
    ///     录制文件的唯一标志；会记录到数据库和文件名上。
    /// </summary>
    private Guid _recordFileIdentity;

    private readonly UdpClient _udpClient = new();
    private DateTime _startTime;
    private readonly object _pdzLocker = new();
    private readonly IntPtr _preView = IntPtr.Zero;
    private readonly IntPtr _login = IntPtr.Zero;
    private TmAiSelObjectT _stSelectObj;
    private TmAiVideoInCfgT _stAiVin;
    private TmTraceParamT _stTracerParam;

    #endregion

    #region Private Methods

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
            TMCC_PtzOpen(_login, 0, true);
            TMCC_PtzLock(_login, 0);
            TMCC_PtzControl(_login, dwPtzCommand, dwPtzControl, dwSpeed);
            TMCC_PtzClose(_login);
        }
    }

    private void TurnLaser(bool turnOn)
    {
        const int iChannel = 0;
        const uint dwPtzCmd = PtzLightPwrOn;
        var dwStart = turnOn ? 1u : 0u; /*1--开启 0--关闭*/
        const uint dwPtzSpeed = 40u;
        TMCC_PtzOpen(_login, iChannel, true);
        TMCC_PtzLock(_login, iChannel);
        TMCC_PtzControl(_login, dwPtzCmd, dwStart, dwPtzSpeed);
        TMCC_PtzClose(_login);
    }

    private void TurnTrack()
    {
        /*停止跟踪*/
        var cmdInfo = new TmCommandInfoT();
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
    }

    private void TurnVisibleLight(bool isVisibleLight)
    {
        _stAiVin.dwSize = (uint)Marshal.SizeOf(_stAiVin);
        /*修改*/
        _stAiVin.byVideoId = isVisibleLight ? (byte)1 : (byte)2;
        _stAiVin.byEnable = 1;
        var spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stAiVin));
        Marshal.StructureToPtr(_stAiVin, spObj, false);
        var cmdInfo = new TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdAiVideoCfg;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stAiVin);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stAiVin);
        cmdInfo.dwResult = 0;
        TMCC_SetConfig(_login, ref cmdInfo);
    }

    private void TurnHideTrackRect(bool hideTrackingRect)
    {
        /*先获取原来的配置参数*/
        _stTracerParam.dwSize = (uint)Marshal.SizeOf(_stTracerParam);
        var spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stTracerParam));
        Marshal.StructureToPtr(_stTracerParam, spObj, false);
        var cmdInfo = new TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        TMCC_GetConfig(_login, ref cmdInfo);
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
        TMCC_SetConfig(_login, ref cmdInfo);
        /*保存配置，保证设备断电重启还生效*/
        TMCC_SaveConfig(_login);
    }

    private void TurnHideIdentifyRect(bool hideIdentifyRect)
    {
        /*先获取原来的配置参数*/
        _stTracerParam.dwSize = (uint)Marshal.SizeOf(_stTracerParam);
        var spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stTracerParam));
        Marshal.StructureToPtr(_stTracerParam, spObj, false);
        var cmdInfo = new TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
        cmdInfo.dwMajorCommand = TmccMajorCmdTracerCfg;
        cmdInfo.dwMinorCommand = TmccMinorCmdParameter;
        cmdInfo.iChannel = 0;
        cmdInfo.iStream = 0;
        cmdInfo.pCommandBuffer = spObj;
        cmdInfo.iCommandBufferLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.iCommandDataLen = Marshal.SizeOf(_stTracerParam);
        cmdInfo.dwResult = 0;
        TMCC_GetConfig(_login, ref cmdInfo);
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
        TMCC_SetConfig(_login, ref cmdInfo);
        /*保存配置，保证设备断电重启还生效*/
        TMCC_SaveConfig(_login);
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
        var cmdInfo = new TmCommandInfoT();
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
        return ret is TmccErrSuccess;
    }

    private void ScreenShot()
    {
        var guid = Guid.NewGuid();
        var path = "/videos/" + guid + ".jpeg";
        if (TMCC_CapturePictureToFile(_preView, path, "JPEG").Equals(0))
            SendData([
                new PlaybackFile
                {
                    FilePath = "/videos/",
                    CreatedAt = DateTime.Now,
                    FileType = FileType.Image,
                    FileName = guid + ".jpeg",
                    UpDatedAt = DateTime.Now
                }
            ]);
    }

    private void SetTraceAuto(bool isAuto)
    {
        /*先获取原来的配置参数*/
        _stTracerParam.dwSize = (uint)Marshal.SizeOf(_stTracerParam);
        var spObj = Marshal.AllocCoTaskMem(Marshal.SizeOf(_stTracerParam));
        Marshal.StructureToPtr(_stTracerParam, spObj, false);

        var cmdInfo = new TmCommandInfoT();
        cmdInfo.dwSize = (uint)Marshal.SizeOf(cmdInfo);
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
        //_stTracerParam = (TmTraceParamT)BytesToStruct(pByte, _stTracerParam.GetType());

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
        TMCC_SetConfig(_login, ref cmdInfo);
        _ = TMCC_SaveConfig(_login);
    }

    private void Record(bool isRunning)
    {
        if (isRunning)
        {
            _recordFileIdentity = Guid.NewGuid();
            TMCC_StartRecord(_preView,
                AppDomain.CurrentDomain.BaseDirectory + "/videos/" + Guid.NewGuid() + ".avi",
                "mp4");
            _startTime = DateTime.Now;
        }
        else
        {
            TMCC_StopRecord(_preView);
            _ = "/videos/" + Guid.NewGuid();
            SendData([
                new PlaybackFile
                {
                    FilePath = "/videos/",
                    CreatedAt = _startTime,
                    FileType = FileType.Video,
                    FileName = _recordFileIdentity + ".mp4",
                    UpDatedAt = DateTime.Now,
                    Duration = DateTime.Now - _startTime
                }
            ]);
        }
    }

    #endregion
}
#endif