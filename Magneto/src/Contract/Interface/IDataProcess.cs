using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Interface;

public interface IDataProcess
{
    void Start();
    void Stop();
    void SetParameter(Parameter parameter);
    void OnData(List<object> data);
    event EventHandler<List<object>> DataProcessComplete;
}