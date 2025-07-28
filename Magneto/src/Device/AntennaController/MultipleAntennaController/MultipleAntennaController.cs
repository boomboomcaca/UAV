using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

/*********************************************************************************************
 *
 * 文件名称:		..\Tracker800 V9\Server\Source\Device\AntennaController\RSAntennaControllerEX\RSAntennaControllerEX.cs
 *
 * 作    者:		蒋波
 *
 * 创作日期:		2020-07-13
 *
 * 修    改:		无
 *
 * 备    注:		使用了多个德辰公司自研网口天线控制器时使用(目前支持两个天线控制器串联控制)
 *
 *********************************************************************************************/

namespace Magneto.Device;

public partial class MultipleAntennaController : AntennaControllerBase
{
    #region 构造函数

    public MultipleAntennaController(Guid id) : base(id)
    {
    }

    #endregion

    /// <summary>
    ///     发送控制码，码值配置成A_B，A为控制器1码值，B为控制器2码
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public override bool SendControlCode(string code)
    {
        try
        {
            var tempCodes = code.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (tempCodes.Length == 1)
            {
                //发送接收机内部天线切换器天线码
                SendControllerCode1(tempCodes[0]);
            }
            else if (tempCodes.Length == 2)
            {
                //发送接收机内部天线切换器天线码
                SendControllerCode1(tempCodes[0]);
                //发送外部天线切换器天线码
                SendControllerCode2(tempCodes[1]);
            }
            else
            {
                throw new Exception("天线码值配置有误,请检查！");
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #region 初始化

    private void InitNetwork()
    {
        //初始化与天线控制器1连接
        IPEndPoint ep = new(IPAddress.Parse(AntController1Ip), Port1);
        _controllerSocket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        _controllerSocket1.Connect(ep);
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     用于控制天线切换器1
    /// </summary>
    private Socket _controllerSocket1;

    /// <summary>
    ///     用于控制天线切换器2
    /// </summary>
    private Socket _controllerSocket2;

    private CancellationTokenSource _cts;

    /// <summary>
    ///     天线控制器2连接线程
    ///     主要用于实现重连，因为天线控制器2的断线不体现该设备的连接状态，以避免网络断线恢复后只能重启服务端才能恢复
    /// </summary>
    private Task _controller2ConnectTask;

    // 天线控制器控制1流对象，TCP套接字
    private Stream _stream1;

    // 天线控制器1字节写入流对象
    private BinaryWriter _writer1;

    //天线控制器2控制流对象，TCP套接字
    private Stream _stream2;

    //天线控制器2字节写入对象
    private BinaryWriter _writer2;

    #endregion

    #region DeviceBase

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (result)
        {
            ReleaseResource();
            InitNetwork();
            SetHeartBeat(_controllerSocket1);
            TryConnectToExtAntController();
        }

        return result;
    }

    public override void Dispose()
    {
        ReleaseResource();
        base.Dispose();
    }

    #endregion

    #region 外部天线控制器重连

    private void TryConnectToExtAntController()
    {
        if (string.IsNullOrWhiteSpace(AntController2Ip)) return;
        _cts = new CancellationTokenSource();
        _controller2ConnectTask = new Task(p => ExtAntControllerKeepAliveAsync(p).ConfigureAwait(false), _cts.Token);
        _controller2ConnectTask.Start();
    }

    private async Task ExtAntControllerKeepAliveAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(2000, token).ConfigureAwait(false);
            try
            {
                if (_controllerSocket2?.Connected == true) continue;
                _controllerSocket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };
                await _controllerSocket2.ConnectAsync(AntController2Ip, Port2);
            }
            catch
            {
                _controllerSocket2 = null;
            }
        }
    }

    #endregion

    #region 辅助函数

    private void ReleaseResource()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
        }

        _controller2ConnectTask = null;
        if (_controllerSocket1 != null)
        {
            try
            {
                _controllerSocket1.Close();
            }
            catch
            {
            }

            _controllerSocket1 = null;
        }

        if (_controllerSocket2 != null)
        {
            try
            {
                _controllerSocket2.Close();
            }
            catch
            {
            }

            _controllerSocket2 = null;
        }
    }

    /// <summary>
    ///     发送天线控制器1指令
    /// </summary>
    /// <param name="code"></param>
    private bool SendControllerCode1(string code)
    {
        if (_stream1 == null)
        {
            if (_controllerSocket1 == null) return false;
            _stream1 = new NetworkStream(_controllerSocket1);
        }

        if (_stream1 != null && _writer1 == null) _writer1 = new BinaryWriter(_stream1);
        if (_writer1 == null) return false;
        //发送天线码
        SendCode(_writer1, code);
        return true;
    }

    /// <summary>
    ///     发送天线控制器2指令
    /// </summary>
    /// <param name="code"></param>
    private bool SendControllerCode2(string code)
    {
        if (_controllerSocket2?.Connected == true)
            try
            {
                _stream2 ??= new NetworkStream(_controllerSocket2);
                if (_stream2 != null && _writer2 == null) _writer2 = new BinaryWriter(_stream2);
                if (_writer2 == null) return false;
                return SendCode(_writer2, code);
            }
            catch
            {
            }

        return false;
    }

    /// <summary>
    ///     发送天线码
    /// </summary>
    /// <param name="receiverWriter"></param>
    /// <param name="code">以“|”分隔的十六进制字节码</param>
    /// <returns>成功返回True，否则返回False</returns>
    private bool SendCode(BinaryWriter receiverWriter, string code)
    {
        var pattern = @"^((0[xX])?[0-9A-Fa-f]{1,2}\|)*(0[xX])?[0-9A-Fa-f]{1,2}$"; // 只匹配以“|”分隔的16进制数据，已考虑大小写情况
        var reg = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
        if (!reg.IsMatch(code)) throw new FormatException("天线控制码格式有误");
        if (receiverWriter == null || string.IsNullOrEmpty(code)) return false;
        var codeArray = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        var commands = new byte[codeArray.Length];
        for (var index = 0; index < codeArray.Length; ++index)
            commands[index] = Convert.ToByte(codeArray[index], 16); // 无论字符串是否带有“0x”前缀，都可以转换为对应的16进制数据
        receiverWriter.Write(commands, 0, commands.Length);
        receiverWriter.Flush();
        return true;
    }

    #endregion
}