using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF550;

public partial class Ddf550
{
    #region 解析数据(EB200)

    /// <summary>
    ///     解析CW数据(单频测量任何时候都要订阅这个数据,需要解析电平数据)
    /// </summary>
    private object ToCw(RawCw data)
    {
        if (data == null) return null;
        var datas = new List<object>();
        if (ItuSwitch)
        {
            var frequency = _frequency;
            if (data.FreqOffset[0] >= -1000000000f) frequency = _frequency + data.FmDev[0] / 1000000f;
            var amDepth = data.AmDepth[0] < 0 || data.AmDepth[0] > 100 ? double.MinValue : data.AmDepth[0] / 10f;
            var fmDev = data.FmDev[0] < -1000000000f ? double.MinValue : data.FmDev[0] / 1000f;
            var fmDevPos = data.FmDevPos[0] < -1000000000f ? double.MinValue : data.FmDevPos[0] / 1000f;
            var fmDevNeg = data.FmDevNeg[0] < -1000000000f ? double.MinValue : data.FmDevNeg[0] / 1000f;
            var pmDepth = data.PmDepth[0] == short.MinValue ? double.MinValue : data.PmDepth[0] / 100f;
            double xdBBw;
            double betaBw;
            if (_bandMeasureMode == "XDB")
            {
                xdBBw = data.BandWidth[0] < -1000000000f ? double.MinValue : data.BandWidth[0] / 1000f;
                betaBw = double.MinValue;
            }
            else
            {
                betaBw = data.BandWidth[0] < -1000000000f ? double.MinValue : data.BandWidth[0] / 1000f;
                xdBBw = double.MinValue;
            }

            var sDataItu = new SDataItu
            {
                Modulation = Modulation.Cw,
                Frequency = frequency,
                Misc = new Dictionary<string, object>
                {
                    { ParameterNames.ItuAmDepth, amDepth },
                    { ParameterNames.ItuFmDev, fmDev },
                    { ParameterNames.ItuFmDevPos, fmDevPos },
                    { ParameterNames.ItuFmDevNeg, fmDevNeg },
                    { ParameterNames.ItuPmDepth, pmDepth },
                    { ParameterNames.ItuBeta, betaBw },
                    { ParameterNames.ItuXdb, xdBBw }
                }
            };
            datas.Add(sDataItu);
        }

        // 计算电平值
        var sDataLevel = new SDataLevel
        {
            Frequency = _frequency,
            Bandwidth = _ifbw,
            Data = data.Level[0] / 10f
        };
        datas.Add(sDataLevel);
        return datas;
    }

    /// <summary>
    ///     解析PScan数据
    /// </summary>
    private object ToPScan(RawPScan data)
    {
        var currIndex = Utils.GetCurrIndex(data.FreqOfFirstStep / 1000000d, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var levels = new List<short>();
        levels.AddRange(data.DataCollection);
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        //表示本次扫描结束,此处仅补齐缺少的点
        if (levels.Last() == 2000)
        {
            levels.RemoveAt(levels.Count - 1);
            for (var i = currIndex + levels.Count; i < total; ++i) levels.Add(0);
        }

        var scan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = levels.ToArray()
        };
        return scan;
    }

    /// <summary>
    ///     解析测向数据
    /// </summary>
    private object ToDfpScan(RawDfPscan data)
    {
        // 当前包是否是最后一包
        var isLast = IsLastHop(data.DfStatus);
        // 测向数据是否经过了方位校正
        var isCorrection = IsCorrectionData(data.DfStatus);
        // 判断总点数与发来的数据是否匹配
        var total = (uint)data.LogChannel + (uint)data.NumberOfTraceItems;
        if (CurFeature == FeatureType.ScanDF)
            total = (uint)Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var offset = 0;
        var heading = data.CompassHeadingType >= 0 ? data.CompassHeading / 10f : 0;
        var dFpScanData = new DfpScanData((uint)data.NumberOfTraceItems, data.DataCollection, ref offset,
            data.SelectorFlags, (uint)data.LogChannel, total, isLast, isCorrection, AngleOffset);
        var freq = (long)data.Frequency;
        var freqIndex = (int)((_frequency * 1000000 - freq) /
                              (data.FrequencyStepNumerator / (double)data.FrequencyStepDenominator));
        if (freqIndex > data.NumberOfTraceItems) freqIndex = 0;
        var start = (long)(_frequency * 1000000) - (long)data.Bandwidth / 2;
        var stop = (long)(_frequency * 1000000) + (long)data.Bandwidth / 2;
        var startIndex = (int)((start - freq) / (data.FrequencyStepNumerator / data.FrequencyStepDenominator));
        var stopIndex = (int)((stop - freq) / (data.FrequencyStepNumerator / data.FrequencyStepDenominator));
        float maxQu = 0;
        for (var i = startIndex; i <= stopIndex; i++)
            if (maxQu < dFpScanData.QualityF[i])
            {
                maxQu = dFpScanData.QualityF[i];
                freqIndex = i;
            }

        var azimuth = dFpScanData.AzimuthF[freqIndex];
        var quality = dFpScanData.QualityF[freqIndex];
        if ((_mediaType & (MediaType.Dfind | MediaType.Dfpan | MediaType.Scan)) == 0) return null;
        var result = new List<object>();
        if ((_mediaType & MediaType.Dfind) > 0)
        {
            var dataDFind = new SDataDfind
            {
                Frequency = _frequency,
                BandWidth = _resolutionBandwidth,
                Azimuth = (azimuth - heading + 360) % 360,
                Quality = quality
            };
            if (dataDFind.Quality > QualityThreshold) result.Add(dataDFind);
        }

        if ((_mediaType & MediaType.Spectrum) > 0 && CurFeature != FeatureType.FFDF)
        {
            var dataSpectrum = new SDataSpectrum
            {
                Span = _dfBandwidth, // _curFeature == FeatureType.FFDF ? _fixdfSpectrumSpan : SpectrumSpan;
                Frequency = _frequency,
                Data = dFpScanData.LevelF
            };
            result.Add(dataSpectrum);
        }

        if ((_mediaType & MediaType.Dfpan) > 0)
        {
            var dataWbdf = new SDataDfpan
            {
                Frequency = _frequency,
                Span = _dfBandwidth,
                Azimuths = Array.ConvertAll(dFpScanData.AzimuthF, item => item < 0 ? -1 : (item - heading + 360) % 360),
                Qualities = dFpScanData.QualityF
            };
            result.Add(dataWbdf);
        }

        if ((_mediaType & MediaType.Scan) > 0)
        {
            var dataScanDf = new SDataDfScan
            {
                StartFrequency = StartFrequency,
                StopFrequency = StopFrequency,
                StepFrequency = StepFrequency
            };
            var indexs = new int[data.NumberOfTraceItems];
            if (isLast && data.NumberOfTraceItems + data.LogChannel != total) indexs = new int[total - data.LogChannel];
            var count = isLast ? (int)total - data.LogChannel : data.NumberOfTraceItems;
            for (var i = 0; i < count; i++) indexs[i] = i + data.LogChannel;
            dataScanDf.Offset = indexs[0];
            dataScanDf.Count = indexs.Length;
            dataScanDf.Indices = indexs;
            dataScanDf.Azimuths =
                Array.ConvertAll(dFpScanData.AzimuthF, item => item < 0 ? -1 : (item - heading + 360) % 360);
            dataScanDf.Qualities = dFpScanData.QualityF;
            result.Add(dataScanDf);
            var sDataScan = new SDataScan
            {
                StartFrequency = StartFrequency,
                StopFrequency = StopFrequency,
                StepFrequency = StepFrequency,
                Offset = data.LogChannel,
                Total = data.ChannelsInScanRange,
                SegmentOffset = 0,
                Data = dFpScanData.LevelF
            };
            result.Add(sDataScan);
        }

        if (result.Count > 0) return result;
        return null;
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    private object ToIq(RawIf data)
    {
        if (data == null) return null;
        var rtnFreq = ((data.FrequencyHigh == 0 ? 0 : (long)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d;
        // 校验数据有效性，以及判断当前IQ数据返回的中心频率是否与当前设置一致，不一致则丢弃该IQ数据
        if (data.NumberOfTraceItems <= 0 || !rtnFreq.Equals(Frequency)) return null;
        var dataIq = new SDataIq
        {
            Frequency = rtnFreq,
            Bandwidth = data.Bandwidth / 1000d,
            SamplingRate = data.Samplerate / 1000d,
            Attenuation = data.RxAtt
        };
        if (data.Mode == 1)
            dataIq.Data16 = data.DataCollection16;
        else if (data.Mode == 2) dataIq.Data32 = data.DataCollection32;
        return dataIq;
    }

    /// <summary>
    ///     解析频谱数据
    /// </summary>
    private object ToSpectrum(RawIfPan data)
    {
        if (data == null) return null;
        if (CurFeature == FeatureType.WBDF) return null;
        var dataSpectrum = new SDataSpectrum();
        var freq = (((long)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d;
        dataSpectrum.Frequency = freq;
        dataSpectrum.Span = data.SpanFrequency / 1000d;
        dataSpectrum.Data = data.DataCollection;
        return dataSpectrum;
    }

    /// <summary>
    ///     解析音频数据
    /// </summary>
    private object ToAudio(RawAudio data)
    {
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6,当大于6时，表明该包数据无效
        if (data.FrequencyHigh > 6) return null;
        Consts.AfModes.TryGetValue(data.AudioMode, out var afMode);
        if (afMode == null) return null;
        var audio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = afMode.SamplingRate * 1000,
            BytesPerSecond = afMode.DataRate * 1000,
            BitsPerSample = afMode.BitsPerSample,
            BlockAlign = afMode.LengthPerFrame,
            Channels = afMode.Channels,
            Data = data.DataCollection
        };
        return audio;
    }

    /// <summary>
    ///     解析GPS
    /// </summary>
    private void ToGpsCompass(RawGpsCompass data)
    {
        if (data == null) return;
        var list = new List<object>();
        if (data.CompassHeadingType > 0 && UseCompass)
        {
            SDataCompass compass = new()
            {
                Heading = data.CompassHeading / 10f,
                Rolling = data.AntRoll / 10f,
                Pitch = data.AntElevation / 10f
            };
            list.Add(compass);
        }

        if (data.GpsValid == 1)
        {
            var gps = new SDataGps
            {
                Longitude = data.LonMin / 60 + data.LonDeg,
                Latitude = data.LatMin / 60 + data.LatDeg,
                Altitude = data.Altitude / 100f
            };
            if (data.NoOfSatInView == -1)
                gps.Satellites = 0x00;
            else
                gps.Satellites = (byte)data.NoOfSatInView;
            gps.Speed = (ushort)(data.SpeedOverGround / 10f * 3600 / 1000);
            gps.Heading = (ushort)(data.CompassHeading / 10f);
            list.Add(gps);
        }

        if (list.Count > 0) SendMessageData(list);
    }

    #endregion 解析数据(EB200)
}