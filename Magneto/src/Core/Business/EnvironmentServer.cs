using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using Magneto.Protocol.Interface;

namespace Core.Business;

public partial class ControlServer : IEnvironment
{
    public void SetSwitches(Guid moduleId, List<Parameter> parameters)
    {
        EnvironmentManager.Instance.SetParameters(moduleId, parameters);
    }
}