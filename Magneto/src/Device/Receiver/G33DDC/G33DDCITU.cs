using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.Algorithm;
using Magneto.Device.G33DDC.SDK;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC;

public partial class G33Ddc
{
    private const float LevelDataCalibration = 3f;
    private readonly object _iqDataLock = new();
    private short[] _iqDataForItu;
    private CancellationTokenSource _ituCts;
    private Task _ituProcessTask;

    private void StartItu()
    {
        _ituCts = new CancellationTokenSource();
        _ituProcessTask = new Task(p => ItuMeasureAsync(p).ConfigureAwait(false), _cts.Token);
        _ituProcessTask.Start();
    }

    private void StopItu()
    {
        try
        {
            _ituCts?.Cancel();
        }
        catch
        {
        }

        try
        {
            _ituProcessTask?.Dispose();
        }
        catch
        {
        }

        lock (_iqDataLock)
        {
            _iqDataForItu = null;
        }
    }

    private async Task ItuMeasureAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(1000, token).ConfigureAwait(false);
                short[] iq = null;
                lock (_iqDataLock)
                {
                    if (_iqDataForItu != null)
                    {
                        iq = new short[_iqDataForItu.Length];
                        Buffer.BlockCopy(_iqDataForItu, 0, iq, 0, _iqDataForItu.Length * sizeof(short));
                    }
                }

                if (iq == null) continue;
                var res = ItuMeasure.MeasureItu(iq, (int)(_device.Ddc1SampleRate * 1000), XdB, BetaValue,
                    _frequency * 1000000, out var result);
                if (!res) continue;
                SDataItu itu = new()
                {
                    Frequency = result.Frequency / 1e6,
                    Bandwidth = _ifBandwidth,
                    Modulation = ConvertMoulation(),
                    Misc = new Dictionary<string, object>
                    {
                        [ParameterNames.ItuLevel] = _level,
                        //itu.Misc[ParameterNames.ITU_AmDepth] = _device?.GetAMDepth(0);
                        //itu.Misc[ParameterNames.ITU_FmDev] = _device?.GetDeviation(0) / 1000d;
                        [ParameterNames.ItuBeta] = result.BetaBwPsd / 1e3,
                        [ParameterNames.ItuXdb] = result.XdBBwPsd / 1e3,
                        [ParameterNames.ItuFrequency] = result.Frequency / 1e6,
                        [ParameterNames.ItuAmDepth] = result.AmMod * 100,
                        [ParameterNames.ItuFmDev] = result.FmMod / 1e3,
                        [ParameterNames.ItuFmDevPos] = result.FmPos / 1e3,
                        [ParameterNames.ItuFmDevNeg] = Math.Abs(result.FmNeg / 1e3)
                    }
                };
                if (result.PmMod is > -2 * Math.PI and < 2 * Math.PI)
                    itu.Misc[ParameterNames.ItuPmDepth] = result.PmMod;
                SendData(new List<object> { itu });
            }
            catch
            {
            }
    }

    private Modulation ConvertMoulation()
    {
        var mode = (DemodulatorMode)_demMode;
        return mode switch
        {
            DemodulatorMode.G3XddcModeCw => Modulation.Cw,
            DemodulatorMode.G3XddcModeAm => Modulation.Am,
            DemodulatorMode.G3XddcModeFm => Modulation.Cw,
            DemodulatorMode.G3XddcModeLsb => Modulation.Lsb,
            DemodulatorMode.G3XddcModeUsb => Modulation.Usb,
            DemodulatorMode.G3XddcModeAms => Modulation.Am,
            DemodulatorMode.G3XddcModeIsb => Modulation.Isb,
            _ => Modulation.Iq
        };
    }

    private short[] Iq32ToIq16(int[] iq)
    {
        var max = iq.Max();
        var min = iq.Min();
        var scale = 1;
        if (max > 1000 || min < -1000)
        {
            var sMax = max / 1000;
            var sMin = min / -1000;
            scale = sMax > sMin ? sMax : sMin;
        }

        var iqShort = new short[iq.Length];
        for (var i = 0; i < iq.Length; i++) iqShort[i] = (short)(iq[i] / scale);
        return iqShort;
    }
}