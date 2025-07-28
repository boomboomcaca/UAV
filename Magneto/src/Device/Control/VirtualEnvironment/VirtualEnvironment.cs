using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualEnvironment;

public sealed partial class VirtualEnvironment : EnvironmentBase
{
    private const string AcSwitch = "ACSwitch";
    private const string DcSwitch = "DCSwitch";
    private readonly Random _random;
    private readonly Dictionary<string, SwitchState> _switchStateDic = new();
    private bool _isRunning;

    public VirtualEnvironment(Guid deviceId) : base(deviceId)
    {
        var seed = GetHashCode();
        _random = new Random(seed);
        _switchStateDic.Add(Utils.FindJsonNameByPropertyName("WifiSwitch", GetType()), WifiSwitch);
        _switchStateDic.Add(Utils.FindJsonNameByPropertyName("AirConditionSwitch", GetType()), AirConditionSwitch);
        for (var i = 0; i < 6; i++)
        {
            var name = $"{AcSwitch}{i + 1}Module";
            var value = GetSwitchValue<bool>(name);
            var state = value ? SwitchState.Off : SwitchState.Disabled;
            var switchName = $"{AcSwitch}{i + 1}";
            var sn = Utils.FindJsonNameByPropertyName(switchName, GetType());
            _switchStateDic.Add(sn, state);
        }

        for (var i = 0; i < 6; i++)
        {
            var name = $"{DcSwitch}{i + 1}Module";
            var value = GetSwitchValue<bool>(name);
            var state = value ? SwitchState.Off : SwitchState.Disabled;
            var switchName = $"{DcSwitch}{i + 1}";
            var sn = Utils.FindJsonNameByPropertyName(switchName, GetType());
            _switchStateDic.Add(sn, state);
        }
    }

    public override bool Initialized(ModuleInfo device)
    {
        Console.WriteLine("初始化虚拟环境控制");
        return base.Initialized(device);
    }

    public override void SetParameter(string name, object value)
    {
        if (name == "wifiModule")
        {
            var bValue = (bool)value;
            var state = bValue ? SwitchState.Off : SwitchState.Disabled;
            _switchStateDic["wifiSwitch"] = state;
        }
        else if (name == "airConditionModule")
        {
            var bValue = (bool)value;
            var state = bValue ? SwitchState.Off : SwitchState.Disabled;
            _switchStateDic["airConditionSwitch"] = state;
        }
        else if (name == "wifiSwitch"
                 && _switchStateDic["wifiSwitch"] == SwitchState.Disabled)
        {
            return;
        }
        else if (name == "airConditionSwitch"
                 && _switchStateDic["airConditionSwitch"] == SwitchState.Disabled)
        {
            return;
        }
        else if (name.EndsWith("Module"))
        {
            var switchName = name.Replace("Module", "");
            var bValue = (bool)value;
            var state = bValue ? SwitchState.Off : SwitchState.Disabled;
            _switchStateDic[switchName] = state;
        }
        else if (_switchStateDic.ContainsKey(name) && _switchStateDic[name] == SwitchState.Disabled)
        {
            return;
        }

        base.SetParameter(name, value);
    }

    public override void Start(IDataPort dataPort, string edgeId)
    {
        base.Start(dataPort, edgeId);
        _isRunning = true;
        ThreadPool.QueueUserWorkItem(SimData);
    }

    private void SimData(object obj)
    {
        var lastTime = DateTime.Now.AddSeconds(-10);
        const int sendSpan = 5;
        while (_isRunning)
        {
            Thread.Sleep(300);
            var list = new List<object>();
            var alarmList = new List<object>();
            // 报警
            {
                var num = _random.Next(0, 1000);
                if (num < 256)
                {
                    var alarms = new List<EnvInfo>();
                    var str = Convert.ToString(num, 2).PadLeft(8, '0');
                    for (var i = 0; i < 8; i++)
                    {
                        var alarmValue = str[i] == '1';
                        var alarm = (SecurityAlarm)i;
                        var desc = GetEnumDescription(alarm);
                        var env = new EnvInfo
                        {
                            Name = Utils.ConvertEnumToString(alarm),
                            Display = desc,
                            Unit = "",
                            Value = alarmValue,
                            Message = desc
                        };
                        alarms.Add(env);
                    }

                    if (alarms.Count > 0)
                    {
                        var dataSecurityAlarm = new SDataSecurityAlarm
                        {
                            EdgeId = EdgeId,
                            ModuleId = DeviceId,
                            Info = alarms
                        };
                        alarmList.Add(dataSecurityAlarm);
                    }
                }
            }
            // 开关
            {
                if (WifiEnabled)
                {
                    // wifi启用
                    var state = new SDataSwitchState
                    {
                        EdgeId = EdgeId,
                        ModuleId = DeviceId,
                        Display = "WIFI开关",
                        Name = Utils.FindJsonNameByPropertyName("WifiSwitch", GetType()),
                        State = WifiSwitch,
                        SwitchType = SwitchType.AC,
                        Info = new List<EnvInfo>()
                    };
                    if (WifiSwitch == SwitchState.On)
                    {
                        var voltage = new EnvInfo
                        {
                            Name = "voltage",
                            Display = "电压",
                            Unit = "V",
                            Value = 220 + _random.Next(-100, 100) / 10f,
                            Message = ""
                        };
                        var current = new EnvInfo
                        {
                            Name = "current",
                            Display = "电流",
                            Unit = "A",
                            Value = 0.8 + _random.Next(-10, 10) / 10f,
                            Message = ""
                        };
                        state.Info.Add(voltage);
                        state.Info.Add(current);
                    }

                    if (_switchStateDic[state.Name] != state.State
                        || DateTime.Now.Subtract(lastTime).TotalSeconds > sendSpan)
                    {
                        _switchStateDic[state.Name] = state.State;
                        list.Add(state);
                    }
                }

                if (AirConditionEnabled)
                {
                    var state = new SDataSwitchState
                    {
                        EdgeId = EdgeId,
                        ModuleId = DeviceId,
                        Name = Utils.FindJsonNameByPropertyName("AirConditionSwitch", GetType()),
                        Display = "空调开关",
                        SwitchType = SwitchType.AC,
                        State = AirConditionSwitch,
                        Info = new List<EnvInfo>()
                    };
                    if (AirConditionSwitch == SwitchState.On)
                    {
                        var voltage = new EnvInfo
                        {
                            Name = "voltage",
                            Display = "电压",
                            Unit = "V",
                            Value = 220 + _random.Next(-100, 100) / 10f,
                            Message = ""
                        };
                        var current = new EnvInfo
                        {
                            Name = "current",
                            Display = "电流",
                            Unit = "A",
                            Value = 0.8 + _random.Next(-10, 10) / 10f,
                            Message = ""
                        };
                        state.Info.Add(voltage);
                        state.Info.Add(current);
                    }

                    if (_switchStateDic[state.Name] != state.State
                        || DateTime.Now.Subtract(lastTime).TotalSeconds > sendSpan)
                    {
                        _switchStateDic[state.Name] = state.State;
                        list.Add(state);
                    }
                }

                for (var i = 0; i < 6; i++)
                {
                    var name = $"{AcSwitch}{i + 1}Enabled";
                    var value = GetSwitchValue<bool>(name);
                    if (value)
                    {
                        var switchName = $"{AcSwitch}{i + 1}";
                        var switchState = GetSwitchValue<SwitchState>(switchName);
                        var displayPara = $"{AcSwitch}{i + 1}Name";
                        var displayName = GetSwitchValue<string>(displayPara);
                        var state = new SDataSwitchState
                        {
                            EdgeId = EdgeId,
                            ModuleId = DeviceId,
                            Name = Utils.FindJsonNameByPropertyName(switchName, GetType()),
                            Display = displayName,
                            State = switchState,
                            SwitchType = SwitchType.AC,
                            Info = new List<EnvInfo>()
                        };
                        if (switchState == SwitchState.On)
                        {
                            var voltage = new EnvInfo
                            {
                                Name = "voltage",
                                Display = "电压",
                                Unit = "V",
                                Value = 220 + _random.Next(-100, 100) / 10f,
                                Message = ""
                            };
                            var current = new EnvInfo
                            {
                                Name = "current",
                                Display = "电流",
                                Unit = "A",
                                Value = 0.8 + _random.Next(-10, 10) / 10f,
                                Message = ""
                            };
                            state.Info.Add(voltage);
                            state.Info.Add(current);
                        }

                        if (_switchStateDic[state.Name] != state.State
                            || DateTime.Now.Subtract(lastTime).TotalSeconds > sendSpan)
                        {
                            _switchStateDic[state.Name] = state.State;
                            list.Add(state);
                        }
                    }

                    name = $"{DcSwitch}{i + 1}Enabled";
                    value = GetSwitchValue<bool>(name);
                    if (value)
                    {
                        var switchName = $"{DcSwitch}{i + 1}";
                        var switchState = GetSwitchValue<SwitchState>(switchName);
                        var displayPara = $"{DcSwitch}{i + 1}Name";
                        var displayName = GetSwitchValue<string>(displayPara);
                        var state = new SDataSwitchState
                        {
                            EdgeId = EdgeId,
                            ModuleId = DeviceId,
                            Name = Utils.FindJsonNameByPropertyName(switchName, GetType()),
                            Display = displayName,
                            State = switchState,
                            SwitchType = SwitchType.DC,
                            Info = new List<EnvInfo>()
                        };
                        if (switchState == SwitchState.On)
                        {
                            var voltage = new EnvInfo
                            {
                                Name = "voltage",
                                Display = "电压",
                                Unit = "V",
                                Value = 220 + _random.Next(-100, 100) / 10f,
                                Message = ""
                            };
                            var current = new EnvInfo
                            {
                                Name = "current",
                                Display = "电流",
                                Unit = "A",
                                Value = 0.8 + _random.Next(-10, 10) / 10f,
                                Message = ""
                            };
                            state.Info.Add(voltage);
                            state.Info.Add(current);
                        }

                        if (_switchStateDic[state.Name] != state.State
                            || DateTime.Now.Subtract(lastTime).TotalSeconds > sendSpan)
                        {
                            _switchStateDic[state.Name] = state.State;
                            list.Add(state);
                        }
                    }
                }

                if (list?.Count > 0)
                {
                    var totalCurrent = list?
                        .Where(item => item is SDataSwitchState)
                        .Sum(item => ((SDataSwitchState)item)
                            .Info
                            .Where(p => p.Name == "current")
                            .Sum(p => Convert.ToDouble(p.Value))) ?? 0;
                    // 主电压、主电流
                    var state = new SDataSwitchState
                    {
                        EdgeId = EdgeId,
                        ModuleId = DeviceId,
                        Display = "主开关",
                        Name = "main",
                        State = SwitchState.Invalid,
                        Info = new List<EnvInfo>()
                    };
                    var voltage = new EnvInfo
                    {
                        Name = "voltage",
                        Display = "电压",
                        Unit = "V",
                        Value = 220 + _random.Next(-100, 100) / 10f,
                        Message = ""
                    };
                    var current = new EnvInfo
                    {
                        Name = "current",
                        Display = "电流",
                        Unit = "A",
                        Value = totalCurrent,
                        Message = ""
                    };
                    state.Info.Add(voltage);
                    state.Info.Add(current);
                    list.Add(state);
                }
            }
            // 环境
            {
                var environment = new SDataEnvironment
                {
                    EdgeId = EdgeId,
                    ModuleId = DeviceId,
                    Info = new List<EnvInfo>()
                };
                var temperature = new EnvInfo
                {
                    Name = "temperature",
                    Display = "温度",
                    Unit = "℃",
                    Value = _random.Next(30, 40),
                    Message = ""
                };
                var humidity = new EnvInfo
                {
                    Name = "humidity",
                    Display = "湿度",
                    Unit = "%",
                    Value = _random.Next(30, 80),
                    Message = ""
                };
                var airPressure = new EnvInfo
                {
                    Name = "airPressure",
                    Display = "气压",
                    Unit = "hPa",
                    Value = 1013.25 + _random.Next(-5, 5) / 10f,
                    Message = ""
                };
                environment.Info.Add(temperature);
                environment.Info.Add(humidity);
                environment.Info.Add(airPressure);
                if (DateTime.Now.Subtract(lastTime).TotalSeconds > sendSpan) list.Add(environment);
            }
            if (DateTime.Now.Subtract(lastTime).TotalSeconds > sendSpan) lastTime = DateTime.Now;
            if (alarmList.Count > 0 && list.Count > 0) list.AddRange(alarmList);
            if (list.Count > 0) SendData(list);
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