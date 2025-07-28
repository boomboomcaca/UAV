using System;
using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS.Driver;

public class GsmData : EventArgs
{
    public BcchDataStruct Data { get; set; }
}