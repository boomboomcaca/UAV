using System.Collections.Generic;
using Magneto.Contract.Algorithm;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Statistics;

public class FfmProcess : DataProcessBase
{
    private readonly Recognize _recognize;

    /// <summary>
    ///     调制解调开关
    /// </summary>
    private bool _mrSwitch;

    public FfmProcess(IAntennaController antennaController) : base(antennaController)
    {
        _recognize = new Recognize();
        _recognize.RecognizeResultArrived += OnRecognizeResultArrived;
    }

    public override void Start()
    {
        base.Start();
        _recognize?.Start();
        _recognize?.Clear();
    }

    public override void Stop()
    {
        base.Stop();
        _recognize?.Stop();
    }

    public override void SetParameter(Parameter parameter)
    {
        base.SetParameter(parameter);
        if (parameter.Name == ParameterNames.MrSwitch
            && bool.TryParse(parameter.Value.ToString(), out var mrSwitch))
            _mrSwitch = mrSwitch;
    }

    public override void OnData(List<object> data)
    {
        base.OnData(data);
        if (_mrSwitch)
        {
            var iqData = (SDataIq)data.Find(item => item is SDataIq);
            if (iqData != null) _recognize.SetIqData(iqData);
        }

        data.RemoveAll(item => item is SDataIq);
    }

    private void OnRecognizeResultArrived(SDataRecognize recognize)
    {
        if (!_mrSwitch) return;
        var data = new List<object> { recognize };
        SendData(data);
    }
}