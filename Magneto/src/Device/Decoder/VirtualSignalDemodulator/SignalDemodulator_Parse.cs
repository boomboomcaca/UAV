using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device.VirtualSignalDemodulator;

public partial class SignalDemodulator
{
    private void FastAnalysis(string[] txtFileNames)
    {
        if (txtFileNames.Length == 0) return;
        var isFirst = true;
        while (_cts?.IsCancellationRequested == false)
            try
            {
                if (isFirst)
                {
                    var waitTime = new Random().Next(0, 3) + 3;
                    Task.Delay(waitTime * 1000, _cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    isFirst = false;
                }
                else
                {
                    Task.Delay(500, _cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                var index = new Random().Next(0, txtFileNames.Length);
                var txtFileName = txtFileNames[index];
                var json = File.ReadAllText(txtFileName);
                if (string.IsNullOrWhiteSpace(json))
                    SendData(new List<object> { new SDataSignalDemod { Success = false } });
                var analysisResult = Utils.ConvertFromJson<AnalysisResultData>(json);
                if (analysisResult == null) SendData(new List<object> { new SDataSignalDemod { Success = false } });
                if (_cts?.IsCancellationRequested != false) break;
                if (analysisResult != null)
                {
                    var data = new SDataSignalDemod
                    {
                        Success = true,
                        Spectrum = ToSpectrum(analysisResult.FreqDomainResult?.SpectrumTrace),
                        Spectrogram = ToSpectrogram(analysisResult.FreqDomainResult?.SpectrogramTraces),
                        MathSpectrum = ToMathSpectrum(analysisResult.MathResult),
                        AmplitudeTimeDomain = ToAmplitudeTimeDomain(analysisResult.TimeDomainResult?.TimeTrace),
                        IqTimeDomain = ToIqTimeDomain(analysisResult.TimeDomainResult?.IqTrace),
                        IqConstellation = ToIqConstellation(analysisResult.DemodResult?.IqTrace),
                        VectorError = ToVectorError(analysisResult.DemodResult?.ErrVectTimeTrace),
                        PhaseError = ToPhaseError(analysisResult.DemodResult?.PhaseErrTrace),
                        FreqDomainResult = ToFreqDomainResult(analysisResult.ResultItems),
                        TimeDomainResult = ToTimeDomainResult(analysisResult.ResultItems),
                        DemodResult = ToDemodResult(analysisResult.ResultItems),
                        BusinessTypeResult = ToBusinessTypeResult(analysisResult.ResultItems)
                    };
                    SendData(new List<object> { data });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"解析异常，异常信息：{ex}");
            }
    }

    private static SignalDemodBusinessTypeResult ToBusinessTypeResult(List<ResultItem> resultItems)
    {
        if (resultItems?.Any() != true) return null;
        var item = resultItems.Find(p => p.ItemName == "BusinessResult");
        if (item?.ItemValue is not IEnumerable<object> children) return null;
        var dataItems = GetResultItems(children);
        var data = new SignalDemodBusinessTypeResult
        {
            Data = dataItems.ToArray()
        };
        return data;
    }

    private static SignalDemodDemodResult ToDemodResult(List<ResultItem> resultItems)
    {
        if (resultItems?.Any() != true) return null;
        var item = resultItems.Find(p => p.ItemName == "DemodResult");
        if (item?.ItemValue is not IEnumerable<object> children) return null;
        var dataItems = GetResultItems(children);
        var data = new SignalDemodDemodResult
        {
            Data = dataItems.ToArray()
        };
        return data;
    }

    private static SignalDemodTimeDomainResult ToTimeDomainResult(List<ResultItem> resultItems)
    {
        if (resultItems?.Any() != true) return null;
        var item = resultItems.Find(p => p.ItemName == "TimeDomain");
        if (item?.ItemValue is not IEnumerable<object> children) return null;
        var dataItems = GetResultItems(children);
        var data = new SignalDemodTimeDomainResult
        {
            Data = dataItems.ToArray()
        };
        return data;
    }

    private static SignalDemodFreqDomainResult ToFreqDomainResult(List<ResultItem> resultItems)
    {
        if (resultItems?.Any() != true) return null;
        var item = resultItems.Find(p => p.ItemName == "FreqDomain");
        if (item?.ItemValue is not IEnumerable<object> children) return null;
        var dataItems = GetResultItems(children);
        var data = new SignalDemodFreqDomainResult
        {
            Data = dataItems.ToArray()
        };
        return data;
    }

    private static SignalDemodPhaseError ToPhaseError(TraceTime phaseErrTrace)
    {
        if (phaseErrTrace?.Data == null) return null;
        var data = new SignalDemodPhaseError
        {
            StartTime = phaseErrTrace.StartTime,
            StopTime = phaseErrTrace.StopTime,
            Data = phaseErrTrace.Data
        };
        return data;
    }

    private static SignalDemodVectorError ToVectorError(TraceTime errVectTimeTrace)
    {
        if (errVectTimeTrace?.Data == null) return null;
        var data = new SignalDemodVectorError
        {
            StartTime = errVectTimeTrace.StartTime,
            StopTime = errVectTimeTrace.StopTime,
            Data = errVectTimeTrace.Data
        };
        return data;
    }

    private static SignalDemodIqConstellation ToIqConstellation(TraceIq iQTrace)
    {
        if (iQTrace == null) return null;
        var data = new SignalDemodIqConstellation
        {
            Data = iQTrace.Data,
            QData = iQTrace.QData
        };
        return data;
    }

    private static SignalDemodIqTimeDomain ToIqTimeDomain(TraceIq iQTrace)
    {
        if (iQTrace == null) return null;
        var data = new SignalDemodIqTimeDomain
        {
            Data = iQTrace.Data,
            QData = iQTrace.QData
        };
        return data;
    }

    private static SignalDemodAmplitudeTimeDomain ToAmplitudeTimeDomain(TraceTime timeTrace)
    {
        if (timeTrace?.Data == null) return null;
        var data = new SignalDemodAmplitudeTimeDomain
        {
            StartTime = timeTrace.StartTime,
            StopTime = timeTrace.StopTime,
            Data = timeTrace.Data
        };
        return data;
    }

    private static SignalDemodMathSpectrum ToMathSpectrum(MathResult mathResult)
    {
        if (mathResult?.MathSpectrum?.Data == null) return null;
        var traceSpectrum = mathResult.MathSpectrum;
        var data = new SignalDemodMathSpectrum
        {
            CenterFrequency = traceSpectrum.CenterFreq / 1e6,
            Span = traceSpectrum.Span / 1e3,
            Data = Array.ConvertAll(traceSpectrum.Data, p => p + 107f)
        };
        return data;
    }

    private static SignalDemodSpectrogram ToSpectrogram(List<TraceSpectrum> spectrogramTraces)
    {
        if (spectrogramTraces?.Any() != true) return null;
        var list = new List<SignalDemodSpectrum>();
        spectrogramTraces.ForEach(p =>
        {
            var item = ToSpectrum(p);
            if (item != null) list.Add(item);
        });
        var data = new SignalDemodSpectrogram { Data = list.ToArray() };
        return data;
    }

    private static SignalDemodSpectrum ToSpectrum(TraceSpectrum traceSpectrum)
    {
        if (traceSpectrum?.Data == null) return null;
        var data = new SignalDemodSpectrum
        {
            CenterFrequency = traceSpectrum.CenterFreq / 1e6,
            Span = traceSpectrum.Span / 1e3,
            Data = Array.ConvertAll(traceSpectrum.Data, p => p + 107f)
        };
        return data;
    }

    private static List<SignalDemodResultItem> GetResultItems(IEnumerable<object> items)
    {
        var list = new List<SignalDemodResultItem>();
        foreach (var obj in items)
        {
            if (obj is not ResultItem item) continue;
            var result = new SignalDemodResultItem
            {
                Name = item.ItemName,
                Unit = item.ItemUnit,
                Description = item.ItemDescription
            };
            if (item.ItemValue is IEnumerable<object> children)
                result.Values = GetResultItems(children);
            else
                result.Value = item.ItemValue?.ToString();
            list.Add(result);
        }

        return list;
    }

    private static bool IsBwEqual(string bw1, string bw2)
    {
        var pattern = @"(?<num>\d+(\.\d+)?)(?<unit>[k|M|G]?Hz)";
        var m1 = Regex.Match(bw1, pattern, RegexOptions.IgnoreCase);
        var m2 = Regex.Match(bw2, pattern, RegexOptions.IgnoreCase);
        if (!m1.Success || !m2.Success) return false;
        var num1 = m1.Groups["num"].Value;
        var num2 = m2.Groups["num"].Value;
        var unit1 = m1.Groups["unit"].Value;
        var unit2 = m2.Groups["unit"].Value;
        if (!GetHz(num1, unit1, out var d1) || !GetHz(num2, unit2, out var d2)) return false;
        return d1.EqualTo(d2);
    }

    private static bool GetHz(string sNum, string sUnit, out double dHz)
    {
        dHz = 0d;
        if (string.IsNullOrWhiteSpace(sNum) || string.IsNullOrWhiteSpace(sUnit)) return false;
        if (!double.TryParse(sNum, out dHz)) return false;
        switch (sUnit.ToUpper())
        {
            case "GHZ":
                dHz *= 1e9;
                break;
            case "MHZ":
                dHz *= 1e6;
                break;
            case "KHZ":
                dHz *= 1e3;
                break;
            case "HZ":
                break;
            default:
                return false;
        }

        return true;
    }
}