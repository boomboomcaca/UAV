using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC1000PMS;

public partial class Dc1000Pms : EnvironmentBase
{
    #region 构造函数

    public Dc1000Pms(Guid id) : base(id)
    {
    }

    #endregion

    #region 设备指令 具体说明请查阅DC1000PMS系统综合管理控制箱上位机软件需求及指令描述

    // 查询各个直流开关状态
    private readonly string _queryDcSwitchStatus = "0x01010201AAAAAAAAAAAAAAAA";

    // 查询各个交流通道开关状态
    private readonly string _queryAcSwitchStatus = "0x01010101AAAAAAAAAAAAAAAA";

    // 查询直流零号通道电压电流
    private readonly string _queryDcChannelInfo = "0x01015000AAAAAAAAAAAAAAAA";

    // 查询交流零号通道电压电流
    private readonly string _queryAcChannelInfo = "0x01014000AAAAAAAAAAAAAAAA";

    // 查询空调开关状态(安装空调模块时有效)
    private readonly string _queryAirConditionSwitchStatus = "0x01010401AAAAAAAAAAAAAAAA";

    // 查询wifi开关状态
    private readonly string _queryWifiSwitchStatus = "0x01010501AAAAAAAAAAAAAAAA";

    // 查询温湿度信息(当空调及温湿度模块存在时有效)
    private readonly string _queryTempHumiInfo = "0x01010403AAAAAAAAAAAAAAAA";

    // 查询安防信息(第一个字节为保留不用管，第二个字节为烟雾安防信息，第三个字节为人体红外安防信息，第四个字节为浸水安防信息，第五个字节为门禁安防信息，后续三个字节不用关心)
    private readonly string _querySecurityInfo = "0x01013001AAAAAAAAAAAAAAAA";

    // 操作直流通道开关命令
    private readonly string _setDcSwitch = "0x01020201AAAAAAAAAAAAAAAA";

    // 操作交流通道开关命令
    private readonly string _setAcSwitch = "0x01020101AAAAAAAAAAAAAAAA";

    // 打开空调指令
    private readonly string _setAirConditionOn = "0x01020401F0AAAAAAAAAAAAAA";

    // 关闭空调指令
    private readonly string _setAirConditionOff = "0x010204010FAAAAAAAAAAAAAA";

    // 打开wifi指令
    private readonly string _setWifiSwitchOn = "0x01020501F0AAAAAAAAAAAAAA";

    // 关闭wifi指令
    private readonly string _setWifiSwitchOff = "0x010205010FAAAAAAAAAAAAAA";

    #endregion

    #region 全局变量

    // 发送命令及接收数据套接字
    private Socket _ctrlChannel;

    // 取消令牌
    private CancellationTokenSource _cts;

    // 数据更新线程
    private Task _updateDataTask;

    private Task _executeOperationTask;

    // 获取数据线程
    private readonly SwitchState[] _dcChannelStatus = new SwitchState[6];

    // 缓存8个交流通道的开关状态
    private readonly SwitchState[] _acChannelStatus = new SwitchState[8];

    // 缓存交流电压电流值(当前设备可用的交流通道)
    private PowerSupplyInfo[] _acpowersupplyinfo;

    // 缓存直流电压电流信息(当前设备可用的直流通道)
    private PowerSupplyInfo[] _dcpowersupplyinfo;

    // 缓存框架需要的电压电流信息
    private PowerSupplyInfo[] _powersupplyinfo;

    // 缓存框架需要的开关状态(系统开关、空调开关、WIFI开关)
    private readonly List<SwitchStatusInfo> _switchInfo = new();

    // 安防报警模块，缓存安防报警信息
    private SecurityAlert _security = SecurityAlert.None;

    // wifi开关标志
    private SwitchState _wifistatus = SwitchState.Off;

    // 空调开关标志
    private SwitchState _aircondictionStatus = SwitchState.Off;

    // 初始化湿度
    private float _humidity = 50;

    // 初始化温度
    private float _temperature = 32;

    // 缓存8路交流通道操作
    private readonly bool[] _operationAcChannel = new bool[8];

    // 缓存6路直流通道操作
    private readonly bool[] _operationDcChannel = new bool[6];

    // 是否修改wifi开关(保证只修改一次)
    private bool _modifywifiswitch;

    // 是否修改空调开关(保证只修改一次)
    private bool _modifyairconditionswitch;

    // 保存客户端配置的
    private readonly List<string> _acdcChannelConfig = new();

    // 下发一次命令，修改wifi状态的间隔次数
    private int _modifywifitimes = 3;

    // 下发一次命令，修改空调状态的间隔次数
    private int _modifyairconditiontimes = 3;

    // 缓存客户端配置的交直流通道
    private string[] _powerSwitchConfig;

    // 缓存配置的交流通道号在_acpowersupplyinfo中的索引
    private readonly Dictionary<int, int> _acChannelInfo = new();

    // 缓存配置的交流通道号在_dcpowersupplyinfo中的索引 
    private readonly Dictionary<int, int> _dcChannelInfo = new();

    // 是否允许开关切换 
    private bool _allowPowerSwitch;
    private readonly List<byte[]> _lstsendCmd = new();
    private readonly Queue<KeyValuePair<bool, byte[]>> _lstPowerSwitchCmd = new();
    private readonly object _lockSendCmd = new();
    private int _retryCount;
    private readonly int _maxRetryCount = 20;

    #endregion

    #region 重写基类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        _allowPowerSwitch = false;
        _retryCount = 0;
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        ReleaseSource();
        //初始化网络连接
        InitNetworks();
        //初始化参数
        InitMembers();
        //初始化开关状态
        InitChannelStatus(3);
        return true;
    }

    public override void Dispose()
    {
        ReleaseSource();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Start(IDataPort dataPort, string edgeId)
    {
        base.Start(dataPort, edgeId);
        var token = _cts.Token;
        _updateDataTask = new Task(p => UpdateDataAsync(p).ConfigureAwait(false), token);
        _updateDataTask.Start();
        _executeOperationTask = new Task(p => ExecuteOperationAsync(p).ConfigureAwait(false), token);
        _executeOperationTask.Start();
    }

    public override void Stop()
    {
        _cts?.Cancel();
        try
        {
            _updateDataTask?.Dispose();
        }
        catch
        {
        }

        try
        {
            _executeOperationTask?.Dispose();
        }
        catch
        {
        }

        _cts?.Dispose();
        base.Stop();
    }

    #endregion

    #region 初始化

    // 初始化网络连接
    private void InitNetworks()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _ctrlChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            SendTimeout = 30000,
            ReceiveTimeout = 30000
        };
        _ctrlChannel.Connect(ep);
    }

    // 初始化参数(开关名称、状态、温湿度、报警信息)
    private void InitMembers()
    {
        _cts ??= new CancellationTokenSource();
        var acChannelConfigNum = 0;
        var dcChannelConfigNum = 0;
        var switchStateConfig = new List<string>();
        var type = GetType();
        for (var index = 0; index < 14; ++index)
        {
            var property = type.GetProperty(index < 8 ? $"AC{index + 1}SwitchEnabled" : $"DC{index - 7}SwitchEnabled");
            var value = property?.GetValue(this);
            if (value is bool enabled)
                switchStateConfig.Add(enabled ? "1" : "0");
            else
                switchStateConfig.Add("0");
        }

        _powerSwitchConfig = switchStateConfig.ToArray();
        try
        {
            for (var i = 0; i < _powerSwitchConfig.Length; i++)
                //如果是交流通道1-8
                if (i <= 7 && _powerSwitchConfig[i] == "1")
                {
                    var acchannelconfig = $"AC{i + 1}Switch";
                    if (!_acChannelInfo.ContainsKey(i + 1))
                    {
                        _acChannelInfo.Add(i + 1, acChannelConfigNum);
                        _acdcChannelConfig.Add(acchannelconfig);
                        var acSwitchInfo = new SwitchStatusInfo
                        {
                            DisplayName = GetChannelAlias(i + 1),
                            On = false,
                            Name = $"AC{i + 1}Switch"
                        };
                        _switchInfo.Add(acSwitchInfo);
                    }

                    acChannelConfigNum++;
                }
                else if (i >= 8 && _powerSwitchConfig[i] == "1") //如果是直流通道1-6
                {
                    var dcchannelconfig = $"DC{i + 1 - 8}Switch";
                    if (!_dcChannelInfo.ContainsKey(i + 1 - 8))
                    {
                        _dcChannelInfo.Add(i + 1 - 8, dcChannelConfigNum);
                        _acdcChannelConfig.Add(dcchannelconfig);
                        var dcSwitchInfo = new SwitchStatusInfo
                        {
                            DisplayName = GetChannelAlias(i + 1),
                            On = false, //初始化时，默认都为关
                            Name = $"DC{i + 1 - 8}Switch"
                        };
                        _switchInfo.Add(dcSwitchInfo);
                    }

                    dcChannelConfigNum++;
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }

        _acpowersupplyinfo = new PowerSupplyInfo[acChannelConfigNum];
        _dcpowersupplyinfo = new PowerSupplyInfo[dcChannelConfigNum];
        var k = 0;
        var m = 0;
        //解析并给配置的通道初始化(电压电流),由于设备可能第一次不返回值，此处必须初始化
        try
        {
            foreach (var channelconfig in _acdcChannelConfig)
            {
                var channelCategory = channelconfig.Remove(2, 1);
                if (channelCategory.StartsWith("ac", StringComparison.OrdinalIgnoreCase)) //交流默认220V 0A
                {
                    _acpowersupplyinfo[k] = new PowerSupplyInfo
                    {
                        Name = channelconfig,
                        Ac = true,
                        Current = 0
                    };
                    _acpowersupplyinfo[k++].Voltage = 220;
                }
                else
                {
                    _dcpowersupplyinfo[m] = new PowerSupplyInfo
                    {
                        Name = channelconfig,
                        Ac = false,
                        Current = 0
                    };
                    _dcpowersupplyinfo[m++].Voltage = 24;
                }
            }

            _powersupplyinfo = new PowerSupplyInfo[_acpowersupplyinfo.Length + _dcpowersupplyinfo.Length];
            Array.Copy(_acpowersupplyinfo, 0, _powersupplyinfo, 0, _acpowersupplyinfo.Length);
            Array.Copy(_dcpowersupplyinfo, 0, _powersupplyinfo, _acpowersupplyinfo.Length, _dcpowersupplyinfo.Length);
        }
        catch (Exception ex1)
        {
            Debug.WriteLine(ex1.StackTrace);
        }

        if (AirConditionSwitchEnabled)
        {
            //有空调选件,初始化空调开关
            var airconditionSwitchInfo = new SwitchStatusInfo
            {
                DisplayName = "空调开关",
                Name = "AirConditionSwitch",
                //初始化时，如果没有获取到正确的空调开关状态，就默认配置时的空调开关状态（无法知道设备什么时候返回正确数据，坑坑坑！）
                On = _aircondictionStatus == SwitchState.On
            };
            _switchInfo.Add(airconditionSwitchInfo);
        }

        if (WifiSwitchEnabled)
        {
            //有WIFI模块，初始化WIFI开关
            var wifiSwitchInfo = new SwitchStatusInfo
            {
                DisplayName = "WIFI开关",
                Name = "WIFISwitch",
                //初始化时，如果没有获取到正确的空调开关状态，就默认配置时WIFI开关状态
                On = false
            };
            _switchInfo.Add(wifiSwitchInfo);
        }
    }

    // 初始化时获取交直流开关状态(便于获取需要设置的开关命令,由于一次可能获取到的开关状态和实际状态不一致，此处设置多次查询)
    private void InitChannelStatus(int count)
    {
        for (var j = 0; j < count; j++)
        {
            //查询交流开关通道
            var acChannelStatus = SendAndReceive(_queryAcSwitchStatus);
            for (var i = 4; i < acChannelStatus.Length; i++)
            {
                if (acChannelStatus[i].Equals(240))
                    _acChannelStatus[i - 4] = SwitchState.On;
                else if (acChannelStatus[i].Equals(15)) _acChannelStatus[i - 4] = SwitchState.Off;
                switch (i - 4)
                {
                    case 0:
                        _ac1Switch = _acChannelStatus[i - 4];
                        break;
                    case 1:
                        _ac2Switch = _acChannelStatus[i - 4];
                        break;
                    case 2:
                        _ac3Switch = _acChannelStatus[i - 4];
                        break;
                    case 3:
                        _ac4Switch = _acChannelStatus[i - 4];
                        break;
                    case 4:
                        _ac5Switch = _acChannelStatus[i - 4];
                        break;
                    case 5:
                        _ac6Switch = _acChannelStatus[i - 4];
                        break;
                    case 6:
                        _ac7Switch = _acChannelStatus[i - 4];
                        break;
                    case 7:
                        _ac8Switch = _acChannelStatus[i - 4];
                        break;
                }
            }

            //查询直流开关状态
            var dcChannelStatus = SendAndReceive(_queryDcSwitchStatus);
            for (var i = 4; i < dcChannelStatus.Length; i++)
            {
                if (dcChannelStatus[i].Equals(240))
                    _dcChannelStatus[i - 4] = SwitchState.On;
                else if (dcChannelStatus[i].Equals(15)) _dcChannelStatus[i - 4] = SwitchState.Off;
                switch (i - 4)
                {
                    case 0:
                        _dc1Switch = _dcChannelStatus[i - 4];
                        break;
                    case 1:
                        _dc2Switch = _dcChannelStatus[i - 4];
                        break;
                    case 2:
                        _dc3Switch = _dcChannelStatus[i - 4];
                        break;
                    case 3:
                        _dc4Switch = _dcChannelStatus[i - 4];
                        break;
                    case 4:
                        _dc5Switch = _dcChannelStatus[i - 4];
                        break;
                    case 5:
                        _dc6Switch = _dcChannelStatus[i - 4];
                        break;
                }
            }
        }
    }

    // 获取交直流通道别名
    private string GetChannelAlias(int i)
    {
        var alias = string.Empty;
        switch (i)
        {
            case 1:
                alias = Ac1SwitchName;
                break;
            case 2:
                alias = Ac2SwitchName;
                break;
            case 3:
                alias = Ac3SwitchName;
                break;
            case 4:
                alias = Ac4SwitchName;
                break;
            case 5:
                alias = Ac5SwitchName;
                break;
            case 6:
                alias = Ac6SwitchName;
                break;
            case 7:
                alias = Ac7SwitchName;
                break;
            case 8:
                alias = Ac8SwitchName;
                break;
            case 9:
                alias = Dc1SwitchName;
                break;
            case 10:
                alias = Dc2SwitchName;
                break;
            case 11:
                alias = Dc3SwitchName;
                break;
            case 12:
                alias = Dc4SwitchName;
                break;
            case 13:
                alias = Dc5SwitchName;
                break;
            case 14:
                alias = Dc6SwitchName;
                break;
        }

        return alias;
    }

    // 执行各种操作(查询开关状态、电压电流值、温湿度值、报警信息)
    private Task ExecuteOperationAsync(object obj)
    {
        var token = (CancellationToken)obj;
        _allowPowerSwitch = true;
        while (!token.IsCancellationRequested)
            try
            {
                _lstsendCmd.Clear();
                GetSwithStatus();
                GetTemperatureHumidity();
                GetSecurityAlarm();
                GetVoltageCurrentInfo();
                UpdateSwitchSetting();
                if (_lstsendCmd.Count > 0 || _lstPowerSwitchCmd.Count > 0)
                {
                    var blPowerChange = false;
                    while (_lstsendCmd.Count > 0 || _lstPowerSwitchCmd.Count > 0) //发送其它指令
                    {
                        lock (_lockSendCmd)
                        {
                            if (_lstPowerSwitchCmd.Count > 0)
                            {
                                var power = _lstPowerSwitchCmd.Dequeue();
                                var bCmd = power.Value;
                                SendCmd(bCmd);
                                Debug.Print("******************向设备发送电源开关指令*****************************");
                                //发送开关后马上发查询状态指令
                                _lstsendCmd.Insert(0,
                                    power.Key
                                        ? ConvertHexStringToBytes(_queryAcSwitchStatus)
                                        : ConvertHexStringToBytes(_queryDcSwitchStatus));
                                blPowerChange = true;
                                continue;
                            }
                        }

                        //如果开关过电源，发送电源开关状态数据
                        if (blPowerChange)
                        {
                            blPowerChange = false;
                            UpdateSWitchStatus();
                            if (TaskState == TaskState.Start) SendData(FormatData());
                        }

                        lock (_lockSendCmd)
                        {
                            if (_lstsendCmd.Count > 0)
                            {
                                var bCmd = _lstsendCmd[0];
                                _lstsendCmd.RemoveAt(0);
                                SendCmd(bCmd);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                    return Task.CompletedTask;
                Debug.WriteLine(ex.StackTrace);
            }

        return Task.CompletedTask;
    }

    // 释放所有非托管资源
    private void ReleaseSource()
    {
        if (_ctrlChannel != null)
        {
            try
            {
                _ctrlChannel.Close();
            }
            catch
            {
                // ignored
            }

            _ctrlChannel = null;
        }
    }

    #endregion

    #region 更新数据

    // 更新数据线程方法并做心跳检测使用
    private Task UpdateDataAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
            try
            {
                var total = 12;
                var offset = 0;
                var buffer = new byte[12];
                while (offset < total)
                {
                    var count = _ctrlChannel.Receive(buffer, offset, total, SocketFlags.None);
                    offset += count;
                    total -= count;
                }

                var flag = BitConverter.ToInt32(buffer, 0);
                switch (flag)
                {
                    case (int)CmdFlag.AcChannel1Power:
                    case (int)CmdFlag.AcChannel2Power:
                    case (int)CmdFlag.AcChannel3Power:
                    case (int)CmdFlag.AcChannel4Power:
                    case (int)CmdFlag.AcChannel5Power:
                    case (int)CmdFlag.AcChannel6Power:
                    case (int)CmdFlag.AcChannel7Power:
                    case (int)CmdFlag.AcChannel8Power:
                        ParseAcdcChannelVoltageCurrentInfo(true, buffer);
                        break;
                    case (int)CmdFlag.DcChannel1Power:
                    case (int)CmdFlag.DcChannel2Power:
                    case (int)CmdFlag.DcChannel3Power:
                    case (int)CmdFlag.DcChannel4Power:
                    case (int)CmdFlag.DcChannel5Power:
                    case (int)CmdFlag.DcChannel6Power:
                        ParseAcdcChannelVoltageCurrentInfo(false, buffer);
                        break;
                    case (int)CmdFlag.WiFiStatus:
                        ParseWifiSwitchStatus(buffer);
                        break;
                    case (int)CmdFlag.AcChannelStatus:
                        ParseAcdcSwitchStatus(true, buffer);
                        break;
                    case (int)CmdFlag.DcChannelStatus:
                        ParseAcdcSwitchStatus(false, buffer);
                        break;
                    case (int)CmdFlag.SecurityAlarmInfo:
                        ParseSecurityAlarmInfo(buffer);
                        break;
                    case (int)CmdFlag.TemperatureHumidyInfo:
                        ParseTemperatureHumidyInfo(buffer);
                        break;
                    case (int)CmdFlag.AirconditionStatus:
                        ParseAirConditionSwitchStatus(buffer);
                        break;
                }

                //更新开关状态
                UpdateSWitchStatus();
                Debug.Print("从设备查询数据时更新电源状态");
                PrintSwitchInfo();
                if (TaskState == TaskState.Start) SendData(FormatData());
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                {
                    _retryCount++;
                    if (_retryCount > _maxRetryCount)
                    {
                        // 重试几次以后判断设备异常，需要告诉上层重连
                        var info = new SDataMessage
                        {
                            LogType = LogType.Warning,
                            ErrorCode = (int)InternalMessageType.DeviceRestart,
                            Description = DeviceId.ToString(),
                            Detail = DeviceInfo.DisplayName
                        };
                        SendMessage(info);
                        return Task.CompletedTask;
                    }

                    Thread.CurrentThread.Join(1000);
                }
                else
                {
                    Debug.WriteLine(ex.StackTrace);
                }
            }

        return Task.CompletedTask;
    }

    private void PrintSwitchInfo()
    {
        var strB = new StringBuilder();
        foreach (var switchInfo in _switchInfo)
        {
            var name = switchInfo.Name.ToLower();
            var index = _switchInfo.IndexOf(switchInfo);
            if (name.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
                strB.Append(name).Append(": ").Append(_switchInfo[index].On).AppendLine();
            else if (name.StartsWith("dc", StringComparison.OrdinalIgnoreCase))
                strB.Append(name).Append(": ").Append(_switchInfo[index].On).AppendLine();
            else if (name.StartsWith("wifi", StringComparison.OrdinalIgnoreCase))
                strB.Append("Wifi:").AppendLine(_wifistatus.ToString());
            else if (name.StartsWith("aircondition", StringComparison.OrdinalIgnoreCase))
                strB.Append("空调:").AppendLine(_wifistatus.ToString());
        }

        Debug.Print(strB.ToString());
    }

    // 更新客户端对开关操作
    private void UpdateSwitchSetting()
    {
        if (_modifywifiswitch && WifiSwitchEnabled)
        {
            //此处主要为了处理两个问题:
            //问题1：客户端向设备发送开关命令但设备未设置成功，由于框架限制，当客户端再次下发命令时，设备将永远不会收到开关命令，故设备处理为永远设置直到命令设置成功为止
            //问题2：当一直向设备发送同样的开关命令，设备会响应缓慢，不能做出开关动作，故此处间隔3次下发命令一次
            if (_modifywifitimes == 3) SetWiFi(_wifiSwitch);
            _modifywifitimes--;
            if (_modifywifitimes == 0) _modifywifitimes = 3;
            if (_wifiSwitch == _wifistatus)
            {
                _modifywifiswitch = false;
                _modifywifitimes = 3;
            }
        }

        if (_modifyairconditionswitch && AirConditionSwitchEnabled)
        {
            //此处主要为了处理两个问题:
            //问题1：客户端向设备发送开关命令但设备未设置成功，由于框架Task.cs限制，当客户端再次下发命令时，设备将永远不会收到开关命令，故设备处理为永远设置直到命令设置成功为止
            //问题2：根据实际测试所得，当一直向设备发送同样的开关命令，设备会响应缓慢，不能做出开关动作，故此处间隔3次下发命令一次
            if (_modifyairconditiontimes == 3) SetAirCondition(_airconditionSwitch);
            _modifyairconditiontimes--;
            if (_modifyairconditiontimes == 0) _modifyairconditiontimes = 3;
            if (_airconditionSwitch == _aircondictionStatus)
            {
                _modifyairconditionswitch = false;
                _modifyairconditiontimes = 3;
            }
        }
    }

    // 更改客户端各通道电源开关操作
    private void UpdatePowerSwitchSetting(int channelIndex, bool isAc)
    {
        if (TaskState != TaskState.Start && !_allowPowerSwitch) return;
        if (isAc)
        {
            if (_operationAcChannel[channelIndex])
            {
                var cmd = GetSetSwitchValue(true, channelIndex + 1);
                SetSwitch(true, channelIndex + 1, cmd);
                if (cmd == _acChannelStatus[channelIndex]) _operationAcChannel[channelIndex] = false;
                //_acChannelStatus[channelIndex] = switchState;
            }
        }
        else if (_operationDcChannel[channelIndex])
        {
            var cmd = GetSetSwitchValue(false, channelIndex + 1);
            SetSwitch(false, channelIndex + 1, cmd);
            if (cmd == _dcChannelStatus[channelIndex]) _operationDcChannel[channelIndex] = false;
            //_dcChannelStatus[channelIndex] = switchState;
        }
    }

    // 更新开关状态（包括交直流通道开关、wifi开关、空调开关等配置开关）
    private void UpdateSWitchStatus()
    {
        foreach (var switchInfo in _switchInfo)
        {
            var name = switchInfo.Name.ToLower();
            var index = _switchInfo.IndexOf(switchInfo);
            if (name.Contains("ac", StringComparison.OrdinalIgnoreCase))
            {
                var channelnum = int.Parse(name.Remove(0, 2)[..1]);
                _switchInfo[index].On = _acChannelStatus[channelnum - 1] == SwitchState.On;
            }
            else if (name.Contains("dc", StringComparison.OrdinalIgnoreCase))
            {
                var channelnum = int.Parse(name.Remove(0, 2)[..1]);
                _switchInfo[index].On = _dcChannelStatus[channelnum - 1] == SwitchState.On;
            }
            else if (name.Contains("wifi", StringComparison.OrdinalIgnoreCase))
            {
                _switchInfo[index].On = _wifistatus == SwitchState.On;
            }
            else if (name.Contains("aircondition", StringComparison.OrdinalIgnoreCase))
            {
                _switchInfo[index].On = _aircondictionStatus == SwitchState.On;
            }
        }
    }

    // 查询客户端传递过来需要操控开关的值 true:开  false:关
    private SwitchState GetSetSwitchValue(bool ac, int channel)
    {
        var result = SwitchState.On;
        if (ac)
            switch (channel)
            {
                case 1:
                    result = _ac1Switch;
                    break;
                case 2:
                    result = _ac2Switch;
                    break;
                case 3:
                    result = _ac3Switch;
                    break;
                case 4:
                    result = _ac4Switch;
                    break;
                case 5:
                    result = _ac5Switch;
                    break;
                case 6:
                    result = _ac6Switch;
                    break;
                case 7:
                    result = _ac7Switch;
                    break;
                case 8:
                    result = _ac8Switch;
                    break;
            }
        else
            switch (channel)
            {
                case 1:
                    result = _dc1Switch;
                    break;
                case 2:
                    result = _dc2Switch;
                    break;
                case 3:
                    result = _dc3Switch;
                    break;
                case 4:
                    result = _dc4Switch;
                    break;
                case 5:
                    result = _dc5Switch;
                    break;
                case 6:
                    result = _dc6Switch;
                    break;
            }

        return result;
    }

    // 获取安防报警信息
    private void GetSecurityAlarm()
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(_querySecurityInfo));
        //SendCmd(ConvertHexStringToBytes(QUERY_SECURITY_INFO));
    }

    // 更新开关状态，包括系统总开关以及WIFI、空调等分支开关
    private void GetSwithStatus()
    {
        //查询直流通道开关状态
        GetChannelSwitchStatus(false);
        //查询交流通道开关状态
        GetChannelSwitchStatus();
        if (WifiSwitchEnabled) GetWiFiStatus();
        if (AirConditionSwitchEnabled) GetAirConditionStatus();
    }

    // 解析交直流开关状态
    private void ParseAcdcSwitchStatus(bool isAcChannel, byte[] buffer)
    {
        Debug.Print("解析设备查询到的开关数据");
        for (var i = 4; i < buffer.Length; i++)
            if (isAcChannel)
            {
                if (buffer[i] == 240)
                    _acChannelStatus[i - 4] = SwitchState.On;
                else if (buffer[i] == 15) _acChannelStatus[i - 4] = SwitchState.Off;
            }
            else if (buffer[i] == 240)
            {
                _dcChannelStatus[i - 4] = SwitchState.On;
            }
            else if (buffer[i] == 15)
            {
                _dcChannelStatus[i - 4] = SwitchState.Off;
            }
    }

    // 解析wifi开关状态
    private void ParseWifiSwitchStatus(byte[] buffer)
    {
        if (buffer[4] == 240)
            _wifistatus = SwitchState.On;
        else if (buffer[4] == 15) _wifistatus = SwitchState.Off;
    }

    // 解析空调开关状态
    private void ParseAirConditionSwitchStatus(byte[] buffer)
    {
        if (buffer[4] == 240)
            _aircondictionStatus = SwitchState.On;
        else if (buffer[4] == 15) _aircondictionStatus = SwitchState.Off;
    }

    // 解析交直流各个通道的电压电流信息
    private void ParseAcdcChannelVoltageCurrentInfo(bool isAcChannel, byte[] buffer)
    {
        var data = new byte[4];
        Buffer.BlockCopy(buffer, 4, data, 0, data.Length);
        if (data.All(item => item != 0xAA))
        {
            int channelNum = buffer[3];
            if (isAcChannel)
            {
                var index = _acChannelInfo[channelNum];
                _acpowersupplyinfo[index].Current = ((data[2] - 1) * 256 + (data[3] - 1)) / 100.0f;
                _acpowersupplyinfo[index].Voltage = ((data[0] - 1) * 256 + (data[1] - 1)) / 10.0f;
            }
            else
            {
                var index = _dcChannelInfo[channelNum];
                _dcpowersupplyinfo[index].Current = ((data[2] - 1) * 256 + (data[3] - 1)) / 100.0f;
                _dcpowersupplyinfo[index].Voltage = ((data[0] - 1) * 256 + (data[1] - 1)) / 10.0f;
            }
        }
        //当一直返回无效数据时，则发送null，客户端不显示电流电压值
        // _powersupplyinfo = null;
    }

    // 解析安防报警信息
    private void ParseSecurityAlarmInfo(byte[] buffer)
    {
        _security = SecurityAlert.None;
        // 安装配置环境监控功能时，需要确认有报警传感器的类型 根据协议 index=5:烟雾报警；index=6 红外报警;index=7 浸水报警;index=8 门磁报警
        if (buffer[5] == 240) _security |= SecurityAlert.Smoke;
        if (buffer[6] == 240) _security |= SecurityAlert.Infrared;
        if (buffer[7] == 240) _security |= SecurityAlert.Flooding;
        if (buffer[8] == 240) _security |= SecurityAlert.GateAccess;
    }

    // 解析温湿度信息
    private void ParseTemperatureHumidyInfo(byte[] buffer)
    {
        if (buffer[4] != 0xAA)
        {
            if ((buffer[4] & 0x80) > 0)
                _temperature = -(buffer[4] & 0x7F);
            else
                _temperature = buffer[4] & 0x7F;
            _humidity = buffer[5];
        }
    }

    // 设置交直流通道开关
    private void SetSwitch(bool ac, int channelNo, SwitchState cmd)
    {
        byte[] sendData;
        if (ac)
        {
            sendData = ConvertHexStringToBytes(_setAcSwitch);
            //遍历配置的交流通道，并给客户端未控制的直流通道赋控制命令，规避嵌入式系统老版本没有给未控制的通道赋控制命令，改通道返回无效的数据的问题（坑，嵌入式系统版本管理问题，最新版本无此问题）
            for (var i = 0; i < _powerSwitchConfig.Length - 6; i++)
                if (_powerSwitchConfig[i].Equals("1"))
                    sendData[i + 4] = GetSetSwitchValue(true, i + 1) == SwitchState.On ? (byte)240 : (byte)15;
        }
        else
        {
            sendData = ConvertHexStringToBytes(_setDcSwitch);
            //遍历配置的直流通道,并给客户端未控制的直流通道赋控制命令，规避嵌入式系统老版本没有给未控制的通道赋控制命令，改通道返回无效的数据的问题（坑，嵌入式系统版本管理问题,最新版本无此问题）
            for (var i = 8; i < _powerSwitchConfig.Length; i++)
                if (_powerSwitchConfig[i].Equals("1"))
                    sendData[i - 8 + 4] = GetSetSwitchValue(false, i - 7) == SwitchState.On ? (byte)240 : (byte)15;
        }

        //给真正需要控制的通道赋值控制命令
        if (cmd == SwitchState.On)
            sendData[channelNo + 3] = 240;
        else
            sendData[channelNo + 3] = 15;
        lock (_lockSendCmd)
        {
            //_lstPowerSwitchCmd.Insert(0, sendData);
            _lstPowerSwitchCmd.Enqueue(new KeyValuePair<bool, byte[]>(ac, sendData));
        }
        //SendCmd(sendData);
    }

    // 设置wifi开关
    private void SetWiFi(SwitchState on = SwitchState.Off)
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(on == SwitchState.On ? _setWifiSwitchOn : _setWifiSwitchOff));
        //SendCmd(ConvertHexStringToBytes(on ? SET_WIFI_SWITCH_ON : SET_WIFI_SWITCH_OFF));
    }

    // 设置空调开关
    private void SetAirCondition(SwitchState on = SwitchState.Off)
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(on == SwitchState.On ? _setAirConditionOn : _setAirConditionOff));
        //SendCmd(ConvertHexStringToBytes(on ? SET_AIR_CONDITION_ON : SET_AIR_CONDITION_OFF));
    }

    // 按通道号获取特定交直流通道的电压电流信息
    private void GetVoltageCurrentByChannel(int channel, bool ac = true)
    {
        var sentData = ConvertHexStringToBytes(ac ? _queryAcChannelInfo : _queryDcChannelInfo);
        sentData[3] = (byte)channel;
        _lstsendCmd.Add(sentData);
        //SendCmd(sentData);
    }

    // 更新电压电流，由于数据只能表示一组电压电流信息，因此，多路交直流电压电流信息采用轮询的方式更新数据
    private void GetVoltageCurrentInfo()
    {
        foreach (var channelconfig in _acdcChannelConfig)
        {
            var channelCategory = channelconfig.Remove(2, 1);
            var channelNum = int.Parse(channelconfig.Remove(0, 2)[..1]);
            if (channelCategory.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
            {
                //获取交流通道电流电压信息,只获取配置好的通道的电压电流信息
                GetVoltageCurrentByChannel(channelNum);
                Thread.Sleep(1000);
            }
            else
            {
                //获取交流通道电流电压信息,只获取配置好的通道的电压电流信息
                GetVoltageCurrentByChannel(channelNum, false);
                Thread.Sleep(1000);
            }
        }
    }

    // 更新环境温度与湿度
    private void GetTemperatureHumidity()
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(_queryTempHumiInfo));
        //SendCmd(ConvertHexStringToBytes(QUERY_TEMP_HUMI_INFO));
    }

    // 获取交直流通道开关状态
    private void GetChannelSwitchStatus(bool acChannel = true)
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(acChannel ? _queryAcSwitchStatus : _queryDcSwitchStatus));
        //SendCmd(ConvertHexStringToBytes(acChannel ? QUERY_AC_SWITCH_STATUS : QUERY_DC_SWITCH_STATUS));
    }

    // 获取WIFI开关状态
    private void GetWiFiStatus()
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(_queryWifiSwitchStatus));
        //SendCmd(ConvertHexStringToBytes(QUERY_WIFI_SWITCH_STATUS));
    }

    // 获取空调开关状态
    private void GetAirConditionStatus()
    {
        _lstsendCmd.Add(ConvertHexStringToBytes(_queryAirConditionSwitchStatus));
        //SendCmd(ConvertHexStringToBytes(QUERY_AIR_CONDITION_SWITCH_STATUS));
    }

    #endregion

    #region Helper

    // 发送并获取数据
    // 注：有的指令发送了却没有反馈，此时只有通过超时的方式跳过，并返回空数据
    private void SendCmd(byte[] commandBuffer)
    {
        if (_ctrlChannel == null)
        {
            // 完全是基于性能方面的考虑，避免调用的方法出现空循环
            Thread.Sleep(10);
            return;
        }

        if (commandBuffer == null) throw new ArgumentNullException(nameof(commandBuffer), "指令格式不对");
        if (commandBuffer.Length != 12) // 当前协议定义的指令长度为12个字节
            throw new FormatException("无效的指令");
        try
        {
            var sentbuffer = new byte[commandBuffer.Length];
            Buffer.BlockCopy(commandBuffer, 0, sentbuffer, 0, commandBuffer.Length);
            // 发送指令
            var total = 12;
            var offset = 0;
            while (offset < total)
            {
                var count = _ctrlChannel.Send(sentbuffer, offset, total, SocketFlags.None);
                offset += count;
                total -= count;
            }

            //出于设备响应来考虑问题,每次发送命令后延时1s
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            if (ex is not SocketException) Debug.WriteLine(ex.StackTrace);
        }
    }

    // 查询
    private byte[] SendAndReceive(string cmd)
    {
        var sendCmd = ConvertHexStringToBytes(cmd);
        var total = 12;
        var offset = 0;
        while (offset < total)
        {
            var count = _ctrlChannel.Send(sendCmd, offset, total, SocketFlags.None);
            offset += count;
            total -= count;
        }

        //便于设备能够返回有效数据
        Thread.Sleep(1000);
        var acChannelStatus = new byte[12];
        _ctrlChannel.Receive(acChannelStatus, acChannelStatus.Length, SocketFlags.None);
        return acChannelStatus;
    }

    // 将16进制的n长度（不包含前缀）的字符串转换n/2长度的字节数组
    private static byte[] ConvertHexStringToBytes(string hexString, string prefix = "0x")
    {
        hexString = hexString[prefix.Length..];
        var hexArray = new byte[hexString.Length / 2];
        for (var index = 0; index < hexArray.Length; ++index)
            hexArray[index] = Convert.ToByte(hexString.Substring(index * 2, 2), 16);
        return hexArray;
    }

    private List<object> FormatData()
    {
        // _remoteInfo.Switches = _switchInfo.ToArray();
        // _remoteInfo.Powers = _powersupplyinfo;
        // _remoteInfo.SecurityAlert = _security;
        var formateDataCollection = new List<object>();
        foreach (var switchInfo in _switchInfo)
        {
            var name = switchInfo.Name.ToLower();
            if (name.StartsWith("wifi") || name.StartsWith("aircondition"))
            {
                var switchState = new SDataSwitchState
                {
                    EdgeId = EdgeId,
                    ModuleId = DeviceId,
                    Display = switchInfo.DisplayName,
                    Name = Utils.FindJsonNameByPropertyName(switchInfo.Name, GetType()),
                    SwitchType = SwitchType.AC,
                    State = switchInfo.On ? SwitchState.On : SwitchState.Off
                };
                formateDataCollection.Add(switchState);
            }
            else if (name.StartsWith("ac") || name.StartsWith("dc"))
            {
                var switchState = new SDataSwitchState
                {
                    EdgeId = EdgeId,
                    ModuleId = DeviceId,
                    Display = switchInfo.DisplayName,
                    Name = Utils.FindJsonNameByPropertyName(switchInfo.Name, GetType()),
                    State = switchInfo.On ? SwitchState.On : SwitchState.Off
                };
                if (_powersupplyinfo?.First(item => item.Name.Equals(switchInfo.Name)) is { } power)
                {
                    switchState.SwitchType = power.Ac ? SwitchType.AC : SwitchType.DC;
                    switchState.Info = new List<EnvInfo>
                    {
                        new()
                        {
                            Name = "voltage",
                            Display = "电压",
                            Unit = "V",
                            Value = power.Voltage
                        },
                        new()
                        {
                            Name = "current",
                            Display = "电流",
                            Unit = "A",
                            Value = power.Current
                        }
                    };
                }

                formateDataCollection.Add(switchState);
            }
        }

        var environment = new SDataEnvironment
        {
            EdgeId = EdgeId,
            ModuleId = DeviceId,
            Info = new List<EnvInfo>
            {
                new()
                {
                    Name = "temperature",
                    Display = "温度",
                    Unit = "℃",
                    Value = _temperature
                },
                new()
                {
                    Name = "humidity",
                    Display = "湿度",
                    Unit = "%",
                    Value = _humidity
                }
            }
        };
        formateDataCollection.Add(environment);
        // if (_security != SecurityAlert.None)
        {
            var securityAlarm = new SDataSecurityAlarm
            {
                EdgeId = EdgeId,
                ModuleId = DeviceId,
                Info = new List<EnvInfo>()
            };
            if ((_security & SecurityAlert.Fire) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Fire),
                    Display = GetEnumDescription(SecurityAlarm.Fire),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Fire),
                    Display = GetEnumDescription(SecurityAlarm.Fire),
                    Value = false
                });
            if ((_security & SecurityAlert.Flooding) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Flooding),
                    Display = GetEnumDescription(SecurityAlarm.Flooding),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Flooding),
                    Display = GetEnumDescription(SecurityAlarm.Flooding),
                    Value = false
                });
            if ((_security & SecurityAlert.GateAccess) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.GateAccess),
                    Display = GetEnumDescription(SecurityAlarm.GateAccess),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.GateAccess),
                    Display = GetEnumDescription(SecurityAlarm.GateAccess),
                    Value = false
                });
            if ((_security & SecurityAlert.Infrared) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Infrared),
                    Display = GetEnumDescription(SecurityAlarm.Infrared),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Infrared),
                    Display = GetEnumDescription(SecurityAlarm.Infrared),
                    Value = false
                });
            if ((_security & SecurityAlert.Smoke) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Smoke),
                    Display = GetEnumDescription(SecurityAlarm.Smoke),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.Smoke),
                    Display = GetEnumDescription(SecurityAlarm.Smoke),
                    Value = false
                });
            if ((_security & SecurityAlert.Overtemperature) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.OverTemperature),
                    Display = GetEnumDescription(SecurityAlarm.OverTemperature),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.OverTemperature),
                    Display = GetEnumDescription(SecurityAlarm.OverTemperature),
                    Value = false
                });
            if ((_security & SecurityAlert.VoltageOverload) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.VoltageOverLoad),
                    Display = GetEnumDescription(SecurityAlarm.VoltageOverLoad),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.VoltageOverLoad),
                    Display = GetEnumDescription(SecurityAlarm.VoltageOverLoad),
                    Value = false
                });
            if ((_security & SecurityAlert.CurrentOverload) > 0)
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.CurrentOverload),
                    Display = GetEnumDescription(SecurityAlarm.CurrentOverload),
                    Value = true
                });
            else
                securityAlarm.Info.Add(new EnvInfo
                {
                    Name = Utils.ConvertEnumToString(SecurityAlarm.CurrentOverload),
                    Display = GetEnumDescription(SecurityAlarm.CurrentOverload),
                    Value = false
                });
            formateDataCollection.Add(securityAlarm);
        }
        return formateDataCollection;
    }

    private static string GetEnumDescription(object value)
    {
        var enumType = value.GetType();
        if (!enumType.IsEnum) return value.ToString();
        var field = enumType.GetField(value.ToString() ?? string.Empty);
        if (field == null) return value.ToString();
        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            return attribute.Description;
        return value.ToString(); //string.Empty;
    }

    #endregion
}