using System.Diagnostics;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;
using Protocol.Define;

namespace Magneto.Driver.UAVS;

public partial class Uavs
{
    private void RegisterSwitch()
    {
        if (SwitchArray is ISwitchCallback callback)
        {
            var rmdSwitch = callback.Register(SwitchUsage.RadioMonitoring, OnSwitchChanged);
            var rsdSwitch = callback.Register(SwitchUsage.RadioSuppressing, OnSwitchChanged);
            _registeredSwitches.AddRange(new[] { rmdSwitch, rsdSwitch });
        }
    }

    private void UnRegisterSwitch()
    {
        if (SwitchArray is ISwitchCallback callback)
            _registeredSwitches.ForEach(item => callback.UnRegister(item, OnSwitchChanged));
    }

    private void OnSwitchChanged(SwitchInfo switchInfo)
    {
        _enableRadioSuppressingSwitch = switchInfo.Usage == SwitchUsage.RadioSuppressing;
        Trace.WriteLine(
            $"任务状态：{IsTaskRunning}, 开关编号：{switchInfo.Index}, 开关名称：{switchInfo.Name}, 开关用途：{switchInfo.Usage}");
        if (IsTaskRunning) ScheduleDeviceWork();
    }

    private void ScheduleDeviceWork()
    {
        if (_enableRadioSuppressingSwitch)
        {
            StopMonitoring();
            StartSuppressing();
        }
        else
        {
            StopSuppressing();
            StartMonitoring();
        }
    }
}