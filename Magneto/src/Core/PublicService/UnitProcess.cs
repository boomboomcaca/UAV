using System;
using System.Collections.Generic;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.PublicService;

public class UnitProcess
{
    private readonly List<SDataFactor> _factor = new();

    /// <summary>
    ///     单位切换
    /// </summary>
    protected int UnitSelection;

    public void OnData(List<object> data)
    {
        if (data.Exists(item => item is SDataFactor)) _factor.Clear();
        foreach (var item in data)
            if (item is SDataFactor f)
                _factor.Add(f);
        var spectrum = (SDataSpectrum)data.Find(item => item is SDataSpectrum);
        var level = (SDataLevel)data.Find(item => item is SDataLevel);
        // 0-dBμV;1-dBμV/m;2-dBm
        // dBμV-107=dBm
        // dBμV+天线因子=dBμV/m
        if (UnitSelection == 1)
        {
            short factor = 0;
            if (_factor.Count > 0 && _factor[0].Data?.Length > 0) factor = _factor[0].Data[0];
            if (level != null)
            {
                level.IsStrengthField = true;
                level.Data += factor / 10f;
            }

            if (spectrum != null)
            {
                spectrum.IsStrengthField = true;
                if (spectrum.Data != null)
                {
                    var len = spectrum.Data.Length;
                    for (var i = 0; i < len; i++)
                    {
                        spectrum.Data[i] += factor;
                        if (spectrum.Maximum != null) spectrum.Maximum[i] += factor;
                        if (spectrum.Minimum != null) spectrum.Minimum[i] += factor;
                        if (spectrum.Mean != null) spectrum.Mean[i] += factor;
                    }
                }
            }
        }

        var scan = (SDataScan)data.Find(item => item is SDataScan);
        if (scan != null && UnitSelection == 1)
        {
            scan.IsStrengthField = true;
            var factor = _factor.Find(item => Magneto.Contract.Utils.IsNumberEquals(item.StartFrequency, scan.StartFrequency)
                                              && Magneto.Contract.Utils.IsNumberEquals(item.StopFrequency, scan.StopFrequency)
                                              && Magneto.Contract.Utils.IsNumberEquals(item.StepFrequency, scan.StepFrequency)
                                              && item.SegmentOffset == scan.SegmentOffset
                                              && item.Total == scan.Total
                                              && item.Data.Length >= scan.Total);
            var array = new short[scan.Data.Length];
            Array.Copy(factor.Data, scan.Offset, array, 0, array.Length);
            for (var i = 0; i < scan.Data.Length; i++)
            {
                scan.Data[i] += array[i];
                if (scan.Maximum != null) scan.Maximum[i] += array[i];
                if (scan.Minimum != null) scan.Minimum[i] += array[i];
                if (scan.Mean != null) scan.Mean[i] += array[i];
                if (scan.Threshold != null) scan.Threshold[i] += array[i];
            }
        }
    }

    public void SetParameter(Parameter parameter)
    {
        if (parameter.Name == ParameterNames.UnitSelection
            && int.TryParse(parameter.Value.ToString(), out var unit))
            UnitSelection = unit;
    }
}