using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.GWGJ_PDU.Common;
using Magneto.Device.GWGJ_PDU.Models;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.GWGJ_PDU;

public partial class Pdu : EnvironmentBase
{
    private const string AcSwitch = "ACSwitch";
    private readonly Dictionary<SecurityAlarm, bool> _alarmCache = new();
    private readonly ConcurrentDictionary<string, SwitchState> _switchStateDic = new();
    private string _ip;
    private volatile bool _isRunning;
    private GwgjPdu _pdu;
    private int _port = 4600;

    /// <summary>
    ///     存放当前开启的开关
    /// </summary>
    private bool[] _switchCache = new bool[8];

    public Pdu(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        _alarmCache.Clear();
        foreach (SecurityAlarm alarm in Enum.GetValues(typeof(SecurityAlarm))) _alarmCache.Add(alarm, false);
        var success = base.Initialized(device);
        _ip = device.Parameters.Find(p => p.Name == "ipAddress")?.Value?.ToString() ?? IpAddress;
        var sPort = device.Parameters.Find(p => p.Name == "port")?.Value?.ToString();
        if (!int.TryParse(sPort, out var port)) port = Port;
        _port = port;
        _pdu = new GwgjPdu(_ip, _port);
        success = _pdu.Connect();
        if (!success)
        {
            Console.WriteLine("连接GWGJ_PDU失败");
            _pdu.Dispose();
            return false;
        }

        Trace.WriteLine("连接GWGJ_PDU成功");
        var state = _pdu.Login(LoginUser, LoginPwd);
        success = state == LoginState.Success;
        if (!success)
        {
            Console.WriteLine($"登录GWGJ_PDU失败，失败原因：{state}");
            _pdu.Dispose();
            return false;
        }

        _pdu.UpdateDate();
        _pdu.UpdateSocketConfig(new[]
        {
            new SocketConfig
            {
                Name = AcSwitch1Name,
                Action = ActionMode.KeepLastState,
                IcoId = 1
            },
            new SocketConfig
            {
                Name = AcSwitch2Name,
                Action = ActionMode.KeepLastState,
                IcoId = 2
            },
            new SocketConfig
            {
                Name = AcSwitch3Name,
                Action = ActionMode.KeepLastState,
                IcoId = 3
            },
            new SocketConfig
            {
                Name = AcSwitch4Name,
                Action = ActionMode.KeepLastState,
                IcoId = 4
            }
        });
        var b = _pdu.GetSocketState(out var states);
        if (b)
        {
            for (var i = 0; i < 8; i++)
            {
                var name = $"{AcSwitch}{i + 1}";
                var sw = states[i] ? SwitchState.On : SwitchState.Off;
                _switchStateDic.AddOrUpdate(name, sw, (_, _) => sw);
                SetPropertyValue(name, sw);
            }
        }
        else
        {
            _switchStateDic.AddOrUpdate($"{AcSwitch}1", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}2", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}3", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}4", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}5", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}6", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}7", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
            _switchStateDic.AddOrUpdate($"{AcSwitch}8", SwitchState.Invalid, (_, _) => SwitchState.Invalid);
        }

        return true;
    }

    public override void Start(IDataPort dataPort, string edgeId)
    {
        base.Start(dataPort, edgeId);
        _isRunning = true;
        ThreadPool.QueueUserWorkItem(SimData);
    }

    public override void Stop()
    {
        _isRunning = false;
        base.Stop();
    }

    public override void Dispose()
    {
        _pdu?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        var prop = Utils.FindPropertyByName(name, GetType());
        if (prop == null) return;
        var propName = prop.Name;
        if (!_switchStateDic.ContainsKey(propName)) return;
        var pattern = $"^{AcSwitch}(?<index>\\d+)$";
        var match = Regex.Match(propName, pattern);
        if (!match.Success) return;
        var sIndex = match.Groups["index"].Value;
        if (!int.TryParse(sIndex, out var index)) return;
        if (index <= 0) return;
        // var swStates = new bool[8];
        var state = string.Equals("on", value?.ToString(), StringComparison.OrdinalIgnoreCase);
        _switchCache[index - 1] = state;
        var status = _pdu.SocketControl(_switchCache, true);
        if (status == OpStatus.Success)
        {
            var swState = string.Equals("on", value?.ToString(), StringComparison.OrdinalIgnoreCase)
                ? SwitchState.On
                : SwitchState.Off;
            _switchStateDic.AddOrUpdate(propName, swState, (_, _) => swState);
            var list = new List<object>();
            for (var i = 0; i < 4; i++)
            {
                var switchName = $"{AcSwitch}{i + 1}";
                if (!_switchStateDic.TryGetValue(switchName, out var sw)) sw = SwitchState.Off;
                var displayPara = $"{switchName}Name";
                var displayName = GetSwitchValue<string>(displayPara);
                var stateData = new SDataSwitchState
                {
                    EdgeId = EdgeId,
                    ModuleId = DeviceId,
                    Name = Utils.FindJsonNameByPropertyName(switchName, GetType()),
                    SwitchType = SwitchType.AC,
                    Display = displayName,
                    State = sw,
                    Info = new List<EnvInfo>()
                };
                list.Add(stateData);
            }

            SendData(list);
        }
    }

    private void SimData(object obj)
    {
        const int sendInterval = 5;
        while (_isRunning)
        {
            if (_pdu == null) break;
            if (!_pdu.State) break;
            var list = new List<object>();
            var b = _pdu.GetSocketState(out var states);
            _switchCache = states;
            if (b)
                for (var i = 0; i < 8; i++)
                {
                    var sw = states[i] ? SwitchState.On : SwitchState.Off;
                    var switchName = $"{AcSwitch}{i + 1}";
                    _switchStateDic.AddOrUpdate($"{AcSwitch}{i + 1}", sw, (_, _) => sw);
                    var displayPara = $"{AcSwitch}{i + 1}Name";
                    var displayName = GetSwitchValue<string>(displayPara);
                    var state = new SDataSwitchState
                    {
                        EdgeId = EdgeId,
                        ModuleId = DeviceId,
                        Name = Utils.FindJsonNameByPropertyName(switchName, GetType()),
                        Display = displayName,
                        SwitchType = SwitchType.AC,
                        State = sw,
                        Info = new List<EnvInfo>()
                    };
                    list.Add(state);
                }

            var success = _pdu.GetPvcInfo(out var pvcInfo);
            if (success)
            {
                var voltage = new EnvInfo
                {
                    Name = "voltage",
                    Display = "电压",
                    Unit = "V",
                    Value = pvcInfo.Voltage,
                    Message = ""
                };
                var current = new EnvInfo
                {
                    Name = "current",
                    Display = "电流",
                    Unit = "A",
                    Value = pvcInfo.Current,
                    Message = ""
                };
                if (pvcInfo.Voltage.CompareWith(VoltageMax) >= 0)
                    _alarmCache[SecurityAlarm.VoltageOverLoad] = true;
                // string desc = GetEnumDescription(SecurityAlarm.VoltageOverLoad);
                // var env = new EnvInfo()
                // {
                //     Name = Utils.ConvertEnumToString(SecurityAlarm.VoltageOverLoad),
                //     Display = desc,
                //     Unit = "",
                //     Value = true,
                //     Message = desc
                // };
                // alarms.Add(env);
                else
                    _alarmCache[SecurityAlarm.VoltageOverLoad] = false;
                if (pvcInfo.Current.CompareWith(CurrentMax) >= 0)
                    _alarmCache[SecurityAlarm.CurrentOverload] = true;
                // string desc = GetEnumDescription(SecurityAlarm.CurrentOverload);
                // var env = new EnvInfo()
                // {
                //     Name = Utils.ConvertEnumToString(SecurityAlarm.CurrentOverload),
                //     Display = desc,
                //     Unit = "",
                //     Value = true,
                //     Message = desc
                // };
                // alarms.Add(env);
                else
                    _alarmCache[SecurityAlarm.CurrentOverload] = false;
                var switchStateData = new SDataSwitchState
                {
                    EdgeId = EdgeId,
                    ModuleId = DeviceId,
                    Name = "main",
                    State = SwitchState.Invalid,
                    Display = "主开关",
                    Info = new List<EnvInfo> { voltage, current }
                };
                list.Add(switchStateData);
            }

            if (TemperatureAvailable)
            {
                var bSuccess = _pdu.GetTemperature(out var temperatureInfo);
                if (bSuccess)
                {
                    if (temperatureInfo.Temperature.CompareWith(TemperatureMax) >= 0)
                        _alarmCache[SecurityAlarm.OverTemperature] = true;
                    // string desc = GetEnumDescription(SecurityAlarm.OverTemperature);
                    // var env = new EnvInfo()
                    // {
                    //     Name = Utils.ConvertEnumToString(SecurityAlarm.OverTemperature),
                    //     Display = desc,
                    //     Unit = "",
                    //     Value = true,
                    //     Message = desc
                    // };
                    // alarms.Add(env);
                    else
                        _alarmCache[SecurityAlarm.OverTemperature] = false;
                    var temperature = new EnvInfo
                    {
                        Name = "temperature",
                        Display = "温度",
                        Unit = "℃",
                        Value = temperatureInfo.Temperature,
                        Message = ""
                    };
                    var humidity = new EnvInfo
                    {
                        Name = "humidity",
                        Display = "湿度",
                        Unit = "%",
                        Value = temperatureInfo.Humidity,
                        Message = ""
                    };
                    var environment = new SDataEnvironment
                    {
                        EdgeId = EdgeId,
                        ModuleId = DeviceId,
                        Info = new List<EnvInfo> { temperature, humidity }
                    };
                    list.Add(environment);
                }
            }

            // if (alarms.Count > 0)
            {
                var alarms = _alarmCache.Select(p =>
                {
                    var desc = GetEnumDescription(p.Key);
                    return new EnvInfo
                    {
                        Name = Utils.ConvertEnumToString(p.Key),
                        Display = desc,
                        Unit = "",
                        Value = p.Value,
                        Message = desc
                    };
                }).ToList();
                var dataSecurityAlarm = new SDataSecurityAlarm
                {
                    EdgeId = EdgeId,
                    ModuleId = DeviceId,
                    Info = alarms
                };
                list.Add(dataSecurityAlarm);
            }
            if (list.Count > 0) SendData(list);
            if (_isRunning) Thread.Sleep(sendInterval * 1000);
        }
    }

    private T GetSwitchValue<T>(string name)
    {
        var type = GetType();
        var info = Utils.FindPropertyByName(name, type);
        if (info == null) return default;
        var value = info.GetValue(this);
        if (value is not T tValue) return default;
        return tValue;
    }

    private void SetPropertyValue(string name, object value)
    {
        var type = GetType();
        var info = type.GetProperty(name);
        if (info == null) return;
        info.SetValue(this, value);
    }

    private string GetEnumDescription(object value)
    {
        var enumType = value.GetType();
        if (!enumType.IsEnum) return value.ToString();
        var field = enumType.GetField(value.ToString() ?? string.Empty);
        if (field == null) return value.ToString();
        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            return attribute.Description;
        return value.ToString(); //string.Empty;
    }
}