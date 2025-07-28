using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

namespace Magneto.Device.DC1020PMS;

public partial class Dc1020Pms : EnvironmentBase
{
    #region 构造函数

    public Dc1020Pms(Guid id) : base(id)
    {
    }

    #endregion

    #region 设备指令

    private readonly string _queryAllDcSwitch = "DC:STATE:ALL?"; // 查询各个直流开关状态
    private readonly string _queryAllAcSwitch = "AC:STATE:ALL?"; // 查询各个交流通道开关状态
    private readonly string _setFormatedDcSwitch = "DC:STATE:CH{0} {1}"; // 操作直流通道开关命令
    private readonly string _setFormatedAcSwitch = "AC:STATE:CH{0} {1}"; // 操作交流通道开关命令
    private readonly string _queryFormatedDcVoltChannel = "DC:VOLT:CH{0}?"; // 查询直流特定通道电压
    private readonly string _queryFormatedDcCurrChannel = "DC:CURR:CH{0}?"; // 查询直流特定通道电流
    private readonly string _queryFormatedAcVoltChannel = "AC:VOLT:CH{0}?"; // 查询交流特定通道电压
    private readonly string _queryFormatedAcCurrChannel = "AC:CURR:CH{0}?"; // 查询交流特定通道电流
    private readonly string _queryFormatedDcAlertChannel = "DC:ALERT:CH{0}?"; // 查询直流特定通道是否过载
    private readonly string _queryFormatedAcAlertChannel = "AC:ALERT:CH{0}?"; // 查询交流特定通道是否过载
    private readonly string _queryTempInfo = "ENV:TEMP?"; // 查询温度信息
    private readonly string _queryHumInfo = "ENV:HUMI?"; // 查询湿度信息
    private readonly string _querySecurityInfo = "WARN:ALL?"; // 查询安防信息

    #endregion

    #region 成员变量

    private Socket _ctrlChannel; // 发送命令及接收数据套接字
    private NetworkStream _stream; // 网络流
    private TextReader _reader; // 流读取器
    private TextWriter _writer; // 流写入器

    private CancellationTokenSource _cts;

    // 数据更新线程
    private Task _updateDataTask;
    private Task _executeOperationTask;
    private readonly SwitchState[] _acSwitchStatus = new SwitchState[4]; // 缓存4个交流通道的开关状态
    private readonly SwitchState[] _dcSwitchStatus = new SwitchState[8]; // 缓存4个直流通道的开关状态
    private PowerSupplyInfo[] _acPowerSupplyinfo; // 缓存交流电压电流值
    private PowerSupplyInfo[] _dcPowerSupplyinfo; // 缓存直流电压电流信息
    private PowerSupplyInfo[] _powerSupplyinfo; // 缓存框架需要的电压电流信息
    private readonly List<SwitchStatusInfo> _switchStatusInfoCollection = new(); // 缓存框架需要的开关状态
    private SecurityAlert _security = SecurityAlert.None; // 安防报警模块，缓存安防报警信息
    private float _humidity = 57;

    /// 初始化湿度
    private float _temperature = 32; // 初始化温度

    private readonly bool[] _operationAcChannel = new bool[4]; // 缓存4路交流通道操作
    private readonly bool[] _operationDcChannel = new bool[8]; // 缓存8路直流通道操作
    private readonly List<string> _acdcChannelConfig = new(); // 保存客户端配置的
    private string[] _configuredSwitch; // 缓存客户端配置的交直流通道
    private readonly Dictionary<int, int> _acChannelNumIndexDict = new(); // 缓存配置的交流通道号在_acPowerSupplyinfo中的索引
    private readonly Dictionary<int, int> _dcChannelNumIndexDict = new(); // 缓存配置的直流通道号在_dcPowerSupplyinfo中的索引
    private bool _allowPowerSwitch;
    private readonly List<string> _commandCollection = new();
    private readonly Queue<KeyValuePair<bool, string>> _switchCommandQueue = new();
    private readonly object _lockSendCmd = new();

    #endregion

    #region 重写基类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        _allowPowerSwitch = false;
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        ReleaseSource();
        //初始化网络连接
        InitNetworks();
        //初始化参数
        InitMembers();
        ////初始化开关状态
        //InitChannelStatus();
        //初始化数据更新线程
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
        _updateDataTask = new Task(p => RetrieveResultAsync(p).ConfigureAwait(false), token);
        _updateDataTask.Start();
        _executeOperationTask = new Task(p => PollCommandAsync(p).ConfigureAwait(false), token);
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
        _stream = new NetworkStream(_ctrlChannel);
        _reader = new StreamReader(_stream);
        _writer = new StreamWriter(_stream);
    }

    private void InitMembers()
    {
        _cts ??= new CancellationTokenSource();
        var configuredAcChannelNo = 0;
        var configuredDcChannelNo = 0;
        var switchStateConfig = new List<string>();
        var type = GetType();
        for (var index = 0; index < 12; ++index)
        {
            var property = type.GetProperty(index < 4 ? $"AC{index + 1}SwitchEnabled" : $"DC{index - 3}SwitchEnabled");
            var value = property?.GetValue(this);
            if (value is bool enabled)
                switchStateConfig.Add(enabled ? "1" : "0");
            else
                switchStateConfig.Add("0");
        }

        _configuredSwitch = switchStateConfig.ToArray();
        for (var index = 0; index < _configuredSwitch.Length; ++index)
            if (index < 4 && _configuredSwitch[index] == "1")
            {
                var acchannelconfig = $"AC{index + 1}Switch";
                if (!_acChannelNumIndexDict.ContainsKey(index + 1))
                {
                    _acChannelNumIndexDict.Add(index + 1, configuredAcChannelNo++);
                    _acdcChannelConfig.Add(acchannelconfig);
                    var acSwitchInfo = new SwitchStatusInfo
                    {
                        DisplayName = GetChannelAlias(index + 1),
                        Name = $"AC{index + 1}Switch",
                        On = false
                    };
                    _switchStatusInfoCollection.Add(acSwitchInfo);
                }
            }
            else if (index >= 4 && _configuredSwitch[index] == "1")
            {
                var dcchannelconfig = $"DC{index - 3}Switch";
                if (!_dcChannelNumIndexDict.ContainsKey(index - 3))
                {
                    _dcChannelNumIndexDict.Add(index - 3, configuredDcChannelNo++);
                    _acdcChannelConfig.Add(dcchannelconfig);
                    var dcSwitchInfo = new SwitchStatusInfo
                    {
                        DisplayName = GetChannelAlias(index + 1),
                        Name = $"DC{index - 3}Switch",
                        On = false
                    };
                    _switchStatusInfoCollection.Add(dcSwitchInfo);
                }
            }

        _acPowerSupplyinfo = new PowerSupplyInfo[configuredAcChannelNo];
        _dcPowerSupplyinfo = new PowerSupplyInfo[configuredDcChannelNo];
        var i = 0;
        var j = 0;
        foreach (var channelConfig in _acdcChannelConfig)
        {
            var channelCategory = channelConfig.Remove(2, 1);
            if (channelCategory.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
            {
                _acPowerSupplyinfo[i] = new PowerSupplyInfo
                {
                    Name = channelConfig,
                    Ac = true,
                    Current = 0,
                    Voltage = 220
                };
                i++;
            }
            else
            {
                _dcPowerSupplyinfo[j] = new PowerSupplyInfo
                {
                    Name = channelConfig,
                    Ac = false,
                    Current = 0,
                    Voltage = 24
                };
                j++;
            }
        }

        _powerSupplyinfo = new PowerSupplyInfo[_acPowerSupplyinfo.Length + _dcPowerSupplyinfo.Length];
        Array.Copy(_acPowerSupplyinfo, 0, _powerSupplyinfo, 0, _acPowerSupplyinfo.Length);
        Array.Copy(_dcPowerSupplyinfo, 0, _powerSupplyinfo, _acPowerSupplyinfo.Length, _dcPowerSupplyinfo.Length);
        SendCommand("AUTO OFF");
        // RefreshEnvironmentInfo();
    }

    public void ReleaseSource()
    {
        if (_stream != null)
            try
            {
                _stream.Close();
            }
            catch
            {
            }
            finally
            {
                _stream = null;
            }

        if (_reader != null)
            try
            {
                _reader.Close();
            }
            catch
            {
            }
            finally
            {
                _reader = null;
            }

        if (_writer != null)
            try
            {
                _writer.Close();
            }
            catch
            {
            }
            finally
            {
                _writer = null;
            }

        if (_ctrlChannel != null)
            try
            {
                _ctrlChannel.Close();
            }
            catch
            {
            }
            finally
            {
                _ctrlChannel = null;
            }
    }

    #endregion

    #region 发送指令与获取数据

    private DateTime _preSendQueryTime = DateTime.MinValue;
    private DateTime _preSendSwitchStatus = DateTime.MinValue;

    /// <summary>
    ///     发送指令
    /// </summary>
    /// <param name="obj"></param>
    private async Task PollCommandAsync(object obj)
    {
        var token = (CancellationToken)obj;
        _allowPowerSwitch = true;
        // 添加开关动作标记
        // 当前台对开关进行了操作，需要重新获取一次各个状态
        // 刚打开开关的时候，获取到的电压电流仍然为0
        var isSwitchChanged = false;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(50, token).ConfigureAwait(false);
                _commandCollection.Clear();
                var span = DateTime.Now - _preSendSwitchStatus;
                if (span.TotalSeconds > 10.0 || isSwitchChanged)
                {
                    GetAllSwitchStatus();
                    _preSendSwitchStatus = DateTime.Now;
                }

                var span2 = DateTime.Now - _preSendQueryTime;
                if (span2.TotalSeconds >= 10.0)
                {
                    _preSendQueryTime = DateTime.Now;
                    GetAllSubscribedChannelVoltage();
                    GetAllSubscribedChannelCurrent();
                    GetAllSubscribedChannelOverload();
                    GetTemperature();
                    GetHumidity();
                    GetSecurityAlarm();
                }

                isSwitchChanged = false;
                if (_commandCollection.Count > 0 || _switchCommandQueue.Count > 0)
                {
                    if (_switchCommandQueue.Count > 0)
                    {
                        isSwitchChanged = true;
                        // 如果打开开关以后立刻获取电压电流，获取到的仍然为0，因此这里延迟5秒再获取
                        _preSendQueryTime = DateTime.Now.AddSeconds(5);
                    }

                    var flag = false;
                    while (_commandCollection.Count > 0 || _switchCommandQueue.Count > 0)
                    {
                        lock (_lockSendCmd)
                        {
                            if (_switchCommandQueue.Count > 0)
                            {
                                var pair = _switchCommandQueue.Dequeue();
                                SendCommand(pair.Value);
                                _commandCollection.Insert(0, pair.Key ? _queryAllAcSwitch : _queryAllDcSwitch);
                                flag = true;
                                continue;
                            }
                        }

                        if (flag)
                        {
                            flag = false;
                            RefreshSwitchStatusInfo();
                            // RefreshEnvironmentInfo();
                            if (TaskState == 0) SendData(FormatData());
                        }

                        lock (_lockSendCmd)
                        {
                            if (_commandCollection.Count > 0)
                            {
                                SendCommand(_commandCollection[0]);
                                _commandCollection.RemoveAt(0);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is SocketException)
                {
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                    return;
                }

                Debug.WriteLine(exception.StackTrace);
            }
    }

    /// <summary>
    ///     接收指令
    /// </summary>
    private async Task RetrieveResultAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
            try
            {
                //var result = _reader.ReadLine().ToLower().TrimEnd(';');
                var result = await _reader.ReadLineAsync();
                result = result?.ToLower().TrimEnd(';');
                if (string.IsNullOrEmpty(result))
                {
                    await Task.Delay(100, token).ConfigureAwait(false);
                    continue;
                }

                Console.WriteLine($"DC1020PMS收到数据:{result}");
                var arr = result.Split(';');
                if (arr.Length > 0)
                    foreach (var str in arr)
                    {
                        var segments = str.Split(':');
                        if (segments.Length > 1)
                            switch (segments[0])
                            {
                                case "ac":
                                case "dc":
                                    ParseAcdc(str);
                                    break;
                                case "warn":
                                    ParseSecurityAlarm(str);
                                    break;
                                case "temp":
                                    ParseTemperature(str);
                                    break;
                                case "humi":
                                    ParseHumidity(str);
                                    break;
                            }
                        else
                            Thread.Sleep(10);

                        RefreshSwitchStatusInfo();
                        //RefreshEnvironmentInfo();
                    }

                if (TaskState == TaskState.Start) SendData(FormatData());
            }
            catch (Exception ex)
            {
                if (ex is SocketException) return;
                Debug.WriteLine(ex.StackTrace);
            }
    }

    #endregion

    #region 控制指令

    private void SetSwitchStatus(bool ac, int channelNo, SwitchState on)
    {
        var cmd = string.Format(ac ? _setFormatedAcSwitch : _setFormatedDcSwitch, channelNo,
            on == SwitchState.On ? "ON" : "OFF");
        lock (_lockSendCmd)
        {
            _switchCommandQueue.Enqueue(new KeyValuePair<bool, string>(ac, cmd));
        }
    }

    private void GetAllSwitchStatus()
    {
        _commandCollection.AddRange(new List<string> { _queryAllAcSwitch, _queryAllDcSwitch });
    }

    private void GetAllSubscribedChannelVoltage()
    {
        foreach (var channelconfig in _acdcChannelConfig)
        {
            var category = channelconfig.Remove(2, 1);
            var channelNo = int.Parse(channelconfig.Remove(0, 2)[..1]);
            if (category.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
                GetVoltageByChannel(channelNo);
            // Thread.Sleep(1000);
            else
                GetVoltageByChannel(channelNo, false);
            // Thread.Sleep(1000);
        }
    }

    private void GetAllSubscribedChannelCurrent()
    {
        foreach (var channelConfig in _acdcChannelConfig)
        {
            var category = channelConfig.Remove(2, 1);
            var channelNo = int.Parse(channelConfig.Remove(0, 2)[..1]);
            if (category.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
                GetCurrentByChannel(channelNo);
            // Thread.Sleep(1000);
            else
                GetCurrentByChannel(channelNo, false);
            // Thread.Sleep(1000);
        }
    }

    private void GetVoltageByChannel(int channelNo, bool ac = true)
    {
        var cmd = string.Format(ac ? _queryFormatedAcVoltChannel : _queryFormatedDcVoltChannel, channelNo);
        _commandCollection.Add(cmd);
    }

    private void GetCurrentByChannel(int channelNo, bool ac = true)
    {
        var cmd = string.Format(ac ? _queryFormatedAcCurrChannel : _queryFormatedDcCurrChannel, channelNo);
        _commandCollection.Add(cmd);
    }

    private void GetAllSubscribedChannelOverload()
    {
        foreach (var channelConfig in _acdcChannelConfig)
        {
            var category = channelConfig.Remove(2, 1);
            var channelNo = int.Parse(channelConfig.Remove(0, 2)[..1]);
            if (category.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
                GetOverloadByChannel(channelNo);
            // Thread.Sleep(1000);
            else
                GetOverloadByChannel(channelNo, false);
            // Thread.Sleep(1000);
        }
    }

    private void GetOverloadByChannel(int channelNo, bool ac = true)
    {
        var cmd = string.Format(ac ? _queryFormatedAcAlertChannel : _queryFormatedDcAlertChannel, channelNo);
        _commandCollection.Add(cmd);
    }

    private void GetSecurityAlarm()
    {
        _commandCollection.Add(_querySecurityInfo);
    }

    private void GetTemperature()
    {
        _commandCollection.Add(_queryTempInfo);
    }

    private void GetHumidity()
    {
        _commandCollection.Add(_queryHumInfo);
    }

    #endregion

    #region 数据解析

    // 解析交流直流
    private void ParseAcdc(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 3 || segments[0].Equals("ac") || segments[0].Equals("dc"))
            switch (segments[1])
            {
                case "state":
                    ParseSwitchStatus(value);
                    break;
                case "volt":
                    ParseVoltage(value);
                    break;
                case "curr":
                    ParseCurrent(value);
                    break;
                case "alert":
                    ParseOverloadInfo(value);
                    break;
            }
    }

    // 解析开关状态
    private void ParseSwitchStatus(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 3
            || (!segments[0].Equals("ac") && !segments[0].Equals("dc"))
            || !segments[1].Equals("state"))
            return;
        var valueSegments = segments[2].Split(',');
        if (valueSegments.Length == 0) return;
        var dict = new Dictionary<string, string>();
        foreach (var valueSegment in valueSegments)
        {
            var keyValuePair = valueSegment.Split(' ');
            if (keyValuePair.Length < 2
                || string.IsNullOrEmpty(keyValuePair[0])
                || string.IsNullOrEmpty(keyValuePair[^1]))
                continue;
            dict[keyValuePair[0].Trim()] = keyValuePair[^1];
        }

        foreach (var keyValue in dict)
            switch (keyValue.Key)
            {
                case "ch1":
                    if (segments[0].Equals("ac"))
                    {
                        _acSwitchStatus[0] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _ac1Switch = _acSwitchStatus[0];
                    }
                    else
                    {
                        _dcSwitchStatus[0] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc1Switch = _dcSwitchStatus[0];
                    }

                    break;
                case "ch2":
                    if (segments[0].Equals("ac"))
                    {
                        _acSwitchStatus[1] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _ac2Switch = _acSwitchStatus[1];
                    }
                    else
                    {
                        _dcSwitchStatus[1] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc2Switch = _dcSwitchStatus[1];
                    }

                    break;
                case "ch3":
                    if (segments[0].Equals("ac"))
                    {
                        _acSwitchStatus[2] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _ac3Switch = _acSwitchStatus[2];
                    }
                    else
                    {
                        _dcSwitchStatus[2] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc3Switch = _dcSwitchStatus[2];
                    }

                    break;
                case "ch4":
                    if (segments[0].Equals("ac"))
                    {
                        _acSwitchStatus[3] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _ac4Switch = _acSwitchStatus[3];
                    }
                    else
                    {
                        _dcSwitchStatus[3] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc4Switch = _dcSwitchStatus[3];
                    }

                    break;
                case "ch5":
                    if (segments[0].Equals("dc"))
                    {
                        _dcSwitchStatus[4] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc5Switch = _dcSwitchStatus[4];
                    }

                    break;
                case "ch6":
                    if (segments[0].Equals("dc"))
                    {
                        _dcSwitchStatus[5] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc6Switch = _dcSwitchStatus[5];
                    }

                    break;
                case "ch7":
                    if (segments[0].Equals("dc"))
                    {
                        _dcSwitchStatus[6] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc7Switch = _dcSwitchStatus[6];
                    }

                    break;
                case "ch8":
                    if (segments[0].Equals("dc"))
                    {
                        _dcSwitchStatus[7] = keyValue.Value.Equals("on") ? SwitchState.On : SwitchState.Off;
                        _dc8Switch = _dcSwitchStatus[7];
                    }

                    break;
            }
    }

    // 解析电压
    private void ParseVoltage(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 3
            || (!segments[0].Equals("ac") && !segments[0].Equals("dc"))
            || !segments[1].Equals("volt"))
            return;
        var valueSegments = segments[2].Split(',');
        if (valueSegments.Length == 0) return;
        var dict = new Dictionary<string, string>();
        foreach (var valueSegment in valueSegments)
        {
            var keyValuePair = valueSegment.Split(' ');
            if (keyValuePair.Length < 2
                || string.IsNullOrEmpty(keyValuePair[0])
                || string.IsNullOrEmpty(keyValuePair[^1]))
                continue;
            dict[keyValuePair[0].Trim()] = keyValuePair[^1];
        }

        foreach (var keyValue in dict)
        {
            var chn = int.Parse(keyValue.Key.Last().ToString());
            if (segments[0].Equals("ac") && !_acChannelNumIndexDict.ContainsKey(chn))
                continue;
            if (segments[0].Equals("dc") && !_dcChannelNumIndexDict.ContainsKey(chn)) continue;
            float tmp;
            switch (keyValue.Key)
            {
                case "ch1":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[1];
                        _acPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[1];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch2":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[2];
                        _acPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[2];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch3":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[3];
                        _acPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[3];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch4":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[4];
                        _acPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[4];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch5":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[5];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch6":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[6];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch7":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[7];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch8":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[8];
                        _dcPowerSupplyinfo[index].Voltage = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
            }
        }
    }

    // 解析电流
    private void ParseCurrent(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 3
            || (!segments[0].Equals("ac") && !segments[0].Equals("dc"))
            || !segments[1].Equals("curr"))
            return;
        var valueSegments = segments[2].Split(',');
        if (valueSegments.Length == 0) return;
        var dict = new Dictionary<string, string>();
        foreach (var valueSegment in valueSegments)
        {
            var keyValuePair = valueSegment.Split(' ');
            if (keyValuePair.Length < 2
                || string.IsNullOrEmpty(keyValuePair[0])
                || string.IsNullOrEmpty(keyValuePair[^1]))
                continue;
            dict[keyValuePair[0].Trim()] = keyValuePair[^1];
        }

        foreach (var keyValue in dict)
        {
            var chn = int.Parse(keyValue.Key.Last().ToString());
            if (segments[0].Equals("ac") && !_acChannelNumIndexDict.ContainsKey(chn))
                continue;
            if (segments[0].Equals("dc") && !_dcChannelNumIndexDict.ContainsKey(chn)) continue;
            float tmp;
            switch (keyValue.Key)
            {
                case "ch1":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[1];
                        _acPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[1];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch2":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[2];
                        _acPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[2];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch3":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[3];
                        _acPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[3];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch4":
                    if (segments[0].Equals("ac"))
                    {
                        var index = _acChannelNumIndexDict[4];
                        _acPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 220;
                    }
                    else
                    {
                        var index = _dcChannelNumIndexDict[4];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch5":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[5];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch6":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[6];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch7":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[7];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
                case "ch8":
                    if (segments[0].Equals("dc"))
                    {
                        var index = _dcChannelNumIndexDict[8];
                        _dcPowerSupplyinfo[index].Current = float.TryParse(keyValue.Value, out tmp) ? tmp : 24;
                    }

                    break;
            }
        }
    }

    // 解析过载信息
    private void ParseOverloadInfo(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 3
            || (!segments[0].Equals("ac") && !segments[0].Equals("dc"))
            || !segments[1].Equals("alert"))
            return;
        var valueSegments = segments[2].Split(',');
        if (valueSegments.Length == 0) return;
        var dict = new Dictionary<string, string>();
        foreach (var valueSegment in valueSegments)
        {
            var keyValuePair = valueSegment.Split(' ');
            if (keyValuePair.Length < 2
                || string.IsNullOrEmpty(keyValuePair[0])
                || string.IsNullOrEmpty(keyValuePair[^1]))
                continue;
            dict[keyValuePair[0].Trim()] = keyValuePair[^1];
        }

        var overloaded = false;
        foreach (var keyValue in dict)
            if (keyValue.Value.Equals("1"))
            {
                overloaded = true;
                break;
            }

        _security = overloaded ? _security | SecurityAlert.CurrentOverload : _security & ~SecurityAlert.CurrentOverload;
    }

    // 解析安防信息
    private void ParseSecurityAlarm(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 2 || !segments[0].Equals("warn")) return;
        var valueSegments = segments[1].Split(',');
        if (valueSegments.Length == 0) return;
        var dict = new Dictionary<string, string>();
        foreach (var valueSegment in valueSegments)
        {
            var keyValuePair = valueSegment.Split(' ');
            if (keyValuePair.Length < 2
                || string.IsNullOrEmpty(keyValuePair[0])
                || string.IsNullOrEmpty(keyValuePair[^1]))
                continue;
            dict[keyValuePair[0].Trim()] = keyValuePair[^1];
        }

        foreach (var keyValue in dict)
            switch (keyValue.Key)
            {
                case "ch1":
                    _security = keyValue.Value.Equals("1")
                        ? _security | SecurityAlert.GateAccess
                        : _security & ~SecurityAlert.GateAccess;
                    break;
                case "ch2":
                    _security = keyValue.Value.Equals("1")
                        ? _security | SecurityAlert.Flooding
                        : _security & ~SecurityAlert.Flooding;
                    break;
                case "ch3":
                    _security = keyValue.Value.Equals("1")
                        ? _security | SecurityAlert.Infrared
                        : _security & ~SecurityAlert.Infrared;
                    break;
                case "ch4":
                    _security = keyValue.Value.Equals("1")
                        ? _security | SecurityAlert.Smoke
                        : _security & ~SecurityAlert.Smoke;
                    break;
            }
    }

    // 解析温度
    private void ParseTemperature(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 2 || !segments[0].Equals("temp")) return;
        if (float.TryParse(segments[1], out var temp)) _temperature = temp;
    }

    // 解析湿度
    private void ParseHumidity(string value)
    {
        var valueString = value.ToLower();
        var segments = valueString.Split(':');
        if (segments.Length != 2 || !segments[0].Equals("humi")) return;
        if (float.TryParse(segments[1], out var temp)) _humidity = temp;
    }

    #endregion

    #region Helper

    private void SendCommand(string command)
    {
        if (_ctrlChannel == null || _stream == null || _writer == null)
        {
            // 完全是基于性能方面的考虑，避免调用的方法出现空循环
            Thread.Sleep(10);
            return;
        }

        if (string.IsNullOrEmpty(command)) throw new ArgumentNullException(nameof(command), "指令格式不对");
        try
        {
            _writer.WriteLine(command);
            _writer.Flush();
            Thread.Sleep(80);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }
    }

    private string GetChannelAlias(int channelNo)
    {
        var alias = string.Empty;
        switch (channelNo)
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
                alias = Dc1SwitchName;
                break;
            case 6:
                alias = Dc2SwitchName;
                break;
            case 7:
                alias = Dc3SwitchName;
                break;
            case 8:
                alias = Dc4SwitchName;
                break;
            case 9:
                alias = Dc5SwitchName;
                break;
            case 10:
                alias = Dc6SwitchName;
                break;
            case 11:
                alias = Dc7SwitchName;
                break;
            case 12:
                alias = Dc8SwitchName;
                break;
        }

        return alias;
    }

    private SwitchState GetSwitchStatusByChannel(bool ac, int channelNo)
    {
        var result = SwitchState.On;
        if (ac)
            switch (channelNo)
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
            }
        else
            switch (channelNo)
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
                case 7:
                    result = _dc7Switch;
                    break;
                case 8:
                    result = _dc8Switch;
                    break;
            }

        return result;
    }

    private void UpdateSwitchStatus(int channelNo, bool ac)
    {
        if (TaskState != TaskState.Start && !_allowPowerSwitch) return;
        if (ac)
        {
            if (_operationAcChannel[channelNo])
            {
                var on = GetSwitchStatusByChannel(true, channelNo + 1);
                SetSwitchStatus(true, channelNo + 1, on);
                if (on == _acSwitchStatus[channelNo]) _operationAcChannel[channelNo] = false;
            }
        }
        else if (_operationDcChannel[channelNo])
        {
            var on = GetSwitchStatusByChannel(false, channelNo + 1);
            SetSwitchStatus(false, channelNo + 1, on);
            if (on == _dcSwitchStatus[channelNo]) _operationDcChannel[channelNo] = false;
        }
    }

    private void RefreshSwitchStatusInfo()
    {
        foreach (var switchStatusInfo in _switchStatusInfoCollection)
        {
            var name = switchStatusInfo.Name.ToLower();
            var index = _switchStatusInfoCollection.IndexOf(switchStatusInfo);
            if (name.StartsWith("ac", StringComparison.OrdinalIgnoreCase))
            {
                var channelNo = int.Parse(name.Remove(0, 2)[..1]);
                _switchStatusInfoCollection[index].On = _acSwitchStatus[channelNo - 1] == SwitchState.On;
            }
            else if (name.StartsWith("dc", StringComparison.OrdinalIgnoreCase))
            {
                var channelNo = int.Parse(name.Remove(0, 2)[..1]);
                _switchStatusInfoCollection[index].On = _dcSwitchStatus[channelNo - 1] == SwitchState.On;
            }
        }
    }

    private List<object> FormatData()
    {
        var formateDataCollection = new List<object>();
        foreach (var switchInfo in _switchStatusInfoCollection)
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
                if (_powerSupplyinfo?.First(item => item.Name.Equals(switchInfo.Name)) is { } power)
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
        Console.WriteLine($"发送状态数据:{string.Join(',', formateDataCollection.Select(item => item.ToString()))}");
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
        return value.ToString();
    }

    #endregion
}