using System;
using Magneto.Contract.BaseClass;

namespace Magneto.Device.DemoAntennaController;

public partial class DemoAntennaController : AntennaControllerBase
{
    public DemoAntennaController(Guid deviceId) : base(deviceId)
    {
    }

    public override bool SendControlCode(string code)
    {
        Console.WriteLine($"发送打通码{code}");
        return true;
    }
}