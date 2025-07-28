using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoReceiver;

public partial class DemoReceiver
{
    #region 数据产生器

    private readonly Dictionary<double, double> _sampleRateDic =
        new()
        {
            { 40000, 51200 },
            { 20000, 25600 },
            { 10000, 25600 },
            { 5000, 6400 },
            { 1000, 2560 },
            { 500, 640 },
            { 250, 640 },
            { 200, 256 },
            { 120, 256 },
            { 100, 256 },
            { 50, 64 },
            { 25, 64 },
            { 12.5, 64 },
            { 6.25, 12.8 },
            { 3.125, 6.4 }
        };

    private float _angle;
    private readonly int _levelMax = 45;
    private readonly int _levelMin = 40;

    private SDataIq GetIq()
    {
        var iq = new SDataIq
        {
            Frequency = Frequency,
            Timestamp = (long)Utils.GetNowTimestamp(),
            Attenuation = _ifAttenuation,
            Bandwidth = IfBandwidth,
            SamplingRate = _sampleRateDic[FilterBandwidth],
            Data16 = new short[2048 * 2]
        };
        // 赋值
        // for (int i = 0; i < iq.Data16.Length; i++)
        // {
        //     iq.Data16[i] = (short)_random.Next(0, 65535);
        // }
        var dataCnt = iq.Data16.Length / 2;
        Random random = new(Guid.NewGuid().GetHashCode());
        for (var i = 0; i < dataCnt; i++)
        {
            var level = (double)random.Next(_levelMin * 10, _levelMax * 10) / 10;
            //double radian = (2 * Math.PI) * _random.NextDouble();
            var radian = Math.PI / 180 * _angle;
            _angle++;
            if (_angle >= 360) _angle = 0;
            var iValue = Math.Sqrt(Math.Pow(10, level / 10)) * Math.Cos(radian);
            var qValue = Math.Sqrt(Math.Pow(10, level / 10)) * Math.Sin(radian);
            var iData = (short)iValue;
            var qData = (short)qValue;
            iq.Data16[i * 2] = iData;
            iq.Data16[i * 2 + 1] = qData;
        }

        return iq;
    }

    private SDataLevel GetLevel(double frequency, double bandwidth, Random random)
    {
        var level = new SDataLevel
        {
            Frequency = frequency,
            Bandwidth = bandwidth,
            Data = 20f + random.Next(-20, 20) / 10.0f
        };
        if (RfMode == RfMode.LowDistort)
            level.Data += 10;
        else if (RfMode == RfMode.LowNoise) level.Data -= 10;
        if (SimPulse) level.Data += 40;
        return level;
    }

    private SDataSpectrum GetSpectrum(double frequency, double bandwidth, Random random)
    {
        var spectrum = new SDataSpectrum
        {
            Frequency = frequency,
            Span = bandwidth,
            Data = new short[1601]
        };
        float delta = 0;
        if (RfMode == RfMode.LowDistort)
            delta = 10;
        else if (RfMode == RfMode.LowNoise) delta -= 10;
        // if (mid < 0)
        // {
        //     mid = 400;
        // }
        var idx = new[]
        {
            100, 300, 650, 1100, 1450
        };
        var lvl = new[]
        {
            35f, 40f, 45f, 47f, 37f
        };
        for (var i = 0; i < 1601; ++i)
        {
            spectrum.Data[i] = (short)((random.Next(-50, 10) / 10.0f + delta) * 10);
            if (i is < 820 and > 780 && i != 800)
                spectrum.Data[i] = (short)((51f / Math.Abs(i - 800) - 2 + random.Next(-10, 9) / 10.0f + delta) * 10);
            if (i == 800) spectrum.Data[i] = (short)((50 + delta) * 10);
            for (var j = 0; j < 5; j++)
                if (i == idx[j])
                    spectrum.Data[i] = (short)((lvl[j] + delta) * 10 + random.Next(-10, 10));
                else if (i > idx[j] - 20 && i < idx[j] + 20)
                    spectrum.Data[i] =
                        (short)(((lvl[j] + 1f) / Math.Abs(i - idx[j]) - 2 + random.Next(-10, 9) / 10.0f + delta) * 10);
        }

        // spectrum.Data[mid - 1] = 27.5f + (random.Next(-25, 25) / 10.0f) + delta;
        // spectrum.Data[mid] = 50 + (random.Next(-10, 10) / 10.0f) + delta;
        // spectrum.Data[mid + 1] = 27.5f + (random.Next(-25, 25) / 10.0f) + delta;
        return spectrum;
    }

    private DateTime _preItuTime = DateTime.Now;

    protected SDataItu GetItu(double frequency, double bandwidth, Random random)
    {
        if (DateTime.Now.Subtract(_preItuTime).TotalMilliseconds < 3000) return null;
        _preItuTime = DateTime.Now;
        var freq = frequency + random.NextDouble() * 2 - 1;
        var rbw = random.NextDouble() * 2 - 1 + bandwidth;
        var level = 48 + random.NextDouble() * 10;
        var itu = new SDataItu
        {
            Frequency = frequency,
            // FieldStrength = level,
            Bandwidth = rbw,
            Modulation = Modulation.Fm,
            Misc = new Dictionary<string, object>()
        };
        var misc = new ItuMisc
        {
            FrequencyStat = new ItuStatData("中心频率", "MHz")
            {
                Value = freq
            },
            LevelStat = new ItuStatData("电平", "dBμV")
            {
                Value = level
            },
            FieldStrengthStat = new ItuStatData("场强", "dBμV/m")
            {
                Value = level
            },
            AmDepthStat = new ItuStatData("AM调幅度", "%")
            {
                Value = random.NextDouble() * 50 + 50
            },
            AmDepthPosStat = new ItuStatData("AM正调幅度", "%")
            {
                Value = random.NextDouble() * 50 + 50
            },
            AmDepthNegStat = new ItuStatData("AM负调幅度", "%")
            {
                Value = random.NextDouble() * 50 + 50
            },
            FmDevStat = new ItuStatData("FM频偏", "kHz")
            {
                Value = random.NextDouble() * 10 - 5
            },
            FmDevPosStat = new ItuStatData("FM正频偏", "kHz")
            {
                Value = random.NextDouble() * 10 - 5
            },
            FmDevNegStat = new ItuStatData("FM负频偏", "kHz")
            {
                Value = random.NextDouble() * 10 - 5
            },
            PmDepthStat = new ItuStatData("PM调制度", "rad")
            {
                Value = random.NextDouble() * 50 + 50
            },
            XdbStat = new ItuStatData("X dB带宽", "kHz")
            {
                Value = random.NextDouble() * 2 - 1 + bandwidth
            },
            BetaStat = new ItuStatData("β带宽", "kHz")
            {
                Value = random.NextDouble() * 2 - 1 + bandwidth
            }
        };
        itu.Misc = misc.ToDictionary(1);
        return itu;
    }

    private readonly Random _random = new();

    private SDataDfind GetDfind()
    {
        var random = new Random();
        var dfind = new SDataDfind
        {
            Frequency = Frequency,
            BandWidth = DfBandwidth,
            Azimuth = SimAzimuth + random.Next(-150, 150) / 10.0f,
            Quality = 80 + random.Next(-200, 199) / 10.0f
        };
        dfind.Azimuth = (dfind.Azimuth + 360) % 360;
        Thread.Sleep(10);
        return dfind;
    }

    #region 频段扫描数据模拟

    private readonly double[] _scanFreqs = [88, 89, 90, 95, 100, 101, 105, 109, 115, 402, 1000, 3000, 5000, 7000];
    private readonly float[] _scanValues = [75, 80, 85, 70, 55, 85, 80, 60, 65, 90, 50, 55, 60, 50];
    private readonly double[] _noiseFreqs = [20, 40, 60, 80, 100, 120, 300, 1000];
    private readonly float[] _noiseValues = [40, 40, 35, 15, 10, 10, 5, 0];

    /// <summary>
    ///     加工生成频段扫描数据
    /// </summary>
    private SDataScan GetScan()
    {
        try
        {
            if (StartFrequency >= StopFrequency) return null;
            var scan = new SDataScan
            {
                StartFrequency = StartFrequency,
                StopFrequency = StopFrequency,
                StepFrequency = StepFrequency,
                Total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency)
            };
            //本包数据长度
            var dataLen = 0;
            if (ScanMode == ScanMode.Pscan)
            {
                //以20M 25Khz 即801个点为标准, 小于25K步进的每包数据长度取800/(25/step) 个点
                var span = StopFrequency - StartFrequency;
                const int baseLen = 800;
                var stepMultiple = 25.0d / StepFrequency;
                var packageLen = (int)(stepMultiple > 0 ? baseLen / stepMultiple : 20.0d * 1000 / StepFrequency);
                packageLen = Math.Abs(span - 20.0d) < 1e-9 ? packageLen + 1 : packageLen;
                dataLen = scan.Total - _index >= packageLen ? packageLen : scan.Total - _index;
            }
            else if (ScanMode.Fscan == ScanMode)
            {
                dataLen = scan.Total / 10;
                if (dataLen < 1) dataLen = 1;
                if (dataLen > 800) dataLen = 800;
                if (scan.Total - _index < dataLen) dataLen = scan.Total - _index;
            }

            if (dataLen < 0) return null;
            scan.Offset = _index;
            scan.Data = new short[dataLen];
            _index = _index + dataLen == scan.Total ? 0 : _index + dataLen;
            //用于修正射频模式的影响
            float delta = 0;
            if (RfMode == RfMode.LowDistort)
                delta = 10;
            else if (RfMode == RfMode.LowNoise) delta = -10;
            var random = new Random();
            //用于穿插小点
            random.Next(0, 9);
            var start = StartFrequency + StepFrequency / 1000d * scan.Offset;
            for (var i = 0; i < dataLen; ++i)
            {
                var freq = start + StepFrequency / 1000d * i;
                var value = GetFreqValue(freq, StepFrequency / 1000d, random);
                scan.Data[i] = (short)((value + delta) * 10);
                // if (isNoise && i > ((dataLen / 2) - 3) && i < ((dataLen / 2) + 3))
                // {
                //     scan.Data[i] += random.NextSingle() * 10;
                // }
            }

            //模拟数据产生时间
            Thread.Sleep(10);
            return scan;
        }
        catch
        {
            return null;
        }
    }

    private float GetNoise(double freq, double step, Random random, float refValue = 0)
    {
        float range = 20;
        var rd = random == null ? 0 : random.NextSingle() * range - range / 2 + refValue;
        if (freq < _noiseFreqs[0]) return _noiseValues[0] + rd;
        for (var i = 0; i < _noiseFreqs.Length; i++)
            if (i >= 1 && i < _noiseFreqs.Length && freq >= _noiseFreqs[i - 1] && freq <= _noiseFreqs[i])
            {
                var x = (float)(freq - _noiseFreqs[i - 1]) / step;
                var xm = (_noiseFreqs[i] - _noiseFreqs[i - 1]) / step;
                var ym = _noiseValues[i] - _noiseValues[i - 1];
                var p = (100 - xm * xm) / (2 * ym);
                var y = (100 - x * x) / (2 * p);
                return (float)(_noiseValues[i - 1] + y + rd);
            }

        return _noiseValues[^1] + rd;
    }

    private float GetFreqValue(double freq, double step, Random random)
    {
        var rd = random.NextSingle() * 4 - 2;
        for (var i = 0; i < _scanFreqs.Length; i++)
        {
            var value = _scanValues[i];
            var mx = _scanFreqs[i];
            if (Math.Abs(_scanFreqs[i] - freq) < 1e-9) return value + rd;
            if (i < _scanFreqs.Length - 1 && freq > _scanFreqs[i + 1]) continue;
            if (Math.Abs(freq - _scanFreqs[i]) > step * 20) continue;
            var x = Math.Abs(freq - mx) / step;
            var m = value - 2;
            if (i < _scanFreqs.Length - 1 && freq > mx)
            {
                var num = Math.Abs(freq - _scanFreqs[i + 1]) / step;
                if (num < x)
                {
                    x = num;
                    m = _scanValues[i + 1] - 2;
                }
            }

            var n = GetNoise(mx + 20 * step, step, null);
            var a = 20 * n / (m - n);
            var b = a * m;
            return (float)(b / (x + a)) + rd;
        }

        return GetNoise(freq, step, random);
    }

    #endregion

    /// <summary>
    ///     加工生成离散扫描数据,统一用一个结构体
    ///     生成离散扫描数据，统一用一个结构体
    /// </summary>
    private SDataScan GetDwellScan()
    {
        var scan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = _index,
            Data = new short[1]
        };
        // Console.WriteLine($"Index:{_index}");
        if (ScanMode == ScanMode.Fscan)
            scan.Total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        else if (ScanMode == ScanMode.MScan)
            lock (_lockMscanPoints)
            {
                if (_mscanPoints == null) return null;
                scan.Total = _mscanPoints.Length;
            }

        // 用于修正射频模式的影响
        var delta = 0.0f;
        if (RfMode == RfMode.LowDistort)
            // 低失真
            delta = 10.0f;
        else if (RfMode == RfMode.LowNoise)
            // 低噪声
            delta = -10.0f;
        if (CurFeature == FeatureType.FScne)
        {
            if (_index == scan.Total / 2)
                scan.Data[0] = (short)((50 + _random.Next(-15, 15) / 10.0f + delta) * 10);
            else
                scan.Data[0] = (short)((_random.Next(-150, 100) / 10.0f + delta) * 10);
        }
        else
        {
            scan.Data[0] = (short)((50 + _random.Next(-15, 15) / 10.0f + delta) * 10);
        }

        // 模块数据产生时间
        Thread.Sleep(10);
        return scan;
    }

    private SDataAudio GetAudio()
    {
        SDataAudio audio = null;
        if (!SquelchSwitch || (SquelchSwitch && MaxLevel >= SquelchThreshold))
        {
            audio = new SDataAudio
            {
                Format = AudioFormat.Pcm,
                SamplingRate = 22050,
                // 从音频文件读音频数据
                Data = new byte[AudioPacketLen]
            };
            if (_indexAudio < _listAudio.Count)
                Buffer.BlockCopy(_listAudio[_indexAudio], 0, audio.Data, 0, AudioPacketLen);
            _indexAudio++;
            if (_indexAudio >= _listAudio.Count) _indexAudio = 0;
        }

        return audio;
    }

    private SDataSse GetSseData()
    {
        var dfind = new SDataSse
        {
            Frequency = Frequency,
            Bandwidth = DfBandwidth,
            AzimuthCount = _sseAzimuthCount
        };
        const int len = 720;
        var buffer = new float[len];
        var mn = _random.Next(-50, 50) / 10f;
        if (dfind.AzimuthCount > 0)
        {
            const float max = 80f;
            const float min = 40f;
            var period = (float)360 / dfind.AzimuthCount;
            const float span = max - min;
            var startAngle = SimAzimuth;
            var ω = 2 * Math.PI / (period * Math.PI / 180);
            var φ = Math.PI * startAngle / 180;
            // var file = "test.csv";
            // using var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
            // using var sw = new StreamWriter(fs);
            var minCount = 0;
            var a = max;
            for (var i = 0; i < len; i++)
            {
                var angle = (float)i * 360 / len;
                // var p = (int)Math.Abs(angle - (startAngle - (period / 2))) % 360 / period;
                // var a = (span * (count - p) / count) + min;
                var x = Math.PI * angle / 180;
                var c = Math.Cos(ω * (x - φ));
                if (Math.Abs(c - -1) < 1e-9)
                {
                    minCount++;
                    if (minCount == dfind.AzimuthCount) minCount = 0;
                    a = span * (dfind.AzimuthCount - minCount) / dfind.AzimuthCount + min;
                }

                var y = a / 2 * c + a / 2;
                buffer[i] = (float)y + mn;
                // sw.WriteLine($"{i},{y}");
            }
        }
        else
        {
            for (var i = 0; i < len; i++) buffer[i] = 50;
        }

        var rd = _random.Next(0, 10);
        var nb = new float[len + 20];
        Buffer.BlockCopy(buffer, 0, nb, 5 * sizeof(float), len * sizeof(float));
        Buffer.BlockCopy(buffer, (len - 5) * sizeof(float), nb, 0, 5 * sizeof(float));
        Buffer.BlockCopy(buffer, 0, nb, (len + 5) * sizeof(float), 5 * sizeof(float));
        Buffer.BlockCopy(nb, rd * sizeof(float), buffer, 0, len * sizeof(float));
        dfind.Data = buffer;
        // sw.Flush();
        // sw.Close();
        // fs.Close();
        Thread.Sleep(50);
        return dfind;
    }

    private SDataIq GetTdoaData()
    {
        lock (_lockTdoa)
        {
            if (_indexTdoa >= _tdoaData.Count) _indexTdoa = 0;
            var data = _tdoaData[_indexTdoa];
            _indexTdoa++;
            return new SDataIq
            {
                Timestamp = data.TimeStamp,
                Frequency = Frequency,
                Bandwidth = IfBandwidth,
                SamplingRate = data.SamplingRate,
                Attenuation = data.Attenuation,
                Data16 = data.Data16,
                Data32 = data.Data32
            };
        }
    }

    /// <summary>
    ///     随机三个信号频点，在1/3, 1/2, 3/4处
    /// </summary>
    private SDataDfpan GetDFindPan()
    {
        var nPoint = DfSamplingCount;
        var data = new SDataDfpan
        {
            Azimuths = new float[nPoint],
            Qualities = new float[nPoint]
        };
        for (var i = 0; i < nPoint; ++i)
        {
            data.Azimuths[i] = -1;
            data.Qualities[i] = -1;
        }

        var index1 = nPoint / 3;
        var index2 = nPoint / 2;
        var index3 = nPoint * 3 / 4;
        data.Azimuths[index1] = 295 + _random.Next(-30, 30) / 10.0f;
        data.Qualities[index1] = 70 + _random.Next(-8, 8);
        data.Azimuths[index2] = 92 + _random.Next(-15, 15) / 10.0f;
        data.Qualities[index2] = 83 + _random.Next(-3, 3);
        data.Azimuths[index3] = 190 + _random.Next(-20, 20) / 10.0f;
        data.Qualities[index3] = 76 + _random.Next(-5, 5);
        data.Span = DfBandwidth;
        data.Frequency = Frequency;
        return data;
    }

    /// <summary>
    ///     随机三个信号频点，在1/3, 1/2, 3/4处
    /// </summary>
    private SDataSpectrum GetWbdfSpectrum()
    {
        var nPoint = DfSamplingCount;
        var spectrum = new SDataSpectrum
        {
            Frequency = Frequency,
            Span = IfBandwidth,
            Data = new short[nPoint]
        };
        var index1 = nPoint / 3;
        var index2 = nPoint / 2;
        var index3 = nPoint * 3 / 4;
        float delta = 0;
        if (RfMode == RfMode.LowDistort)
            delta = 10;
        else if (RfMode == RfMode.LowNoise) delta = -10;
        for (var i = 0; i < nPoint; ++i) spectrum.Data[i] = (short)((_random.Next(-150, 5) / 10.0f + delta) * 10);
        spectrum.Data[index1] = (short)((40 + _random.Next(-25, 25) / 10.0f + delta) * 10);
        spectrum.Data[index2] = (short)((60 + _random.Next(-25, 25) / 10.0f + delta) * 10);
        spectrum.Data[index3] = (short)((53 + _random.Next(-25, 25) / 10.0f + delta) * 10);
        return spectrum;
    }

    /// <summary>
    ///     加工生成扫描测向数据
    /// </summary>
    private SDataDfScan GetScanDf()
    {
        var data = new SDataDfScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency
        };
        var nPoint = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        data.Offset = 0;
        data.Count = nPoint;
        data.Azimuths = new float[nPoint];
        data.Indices = new int[nPoint];
        data.Qualities = new float[nPoint];
        var index1 = nPoint * 2 / 5;
        var index2 = nPoint * 3 / 5;
        for (var i = 0; i < nPoint; i++)
        {
            data.Indices[i] = i;
            data.Azimuths[i] = _random.Next(0, 3600) / 10f;
            data.Qualities[i] = _random.Next(0, 50);
            if (i > index1 && i < index2)
            {
                data.Azimuths[i] = _random.Next(900, 1200) / 10f;
                data.Qualities[i] = _random.Next(80, 99);
            }
        }

        data.Azimuths[index1] = 295 + _random.Next(-90, 90) / 10.0f;
        data.Qualities[index1] = _random.Next(70, 99);
        return data;
    }

    /// <summary>
    ///     加工生成扫描测向电平数据
    /// </summary>
    private SDataScan GetScanDfLevel()
    {
        var scan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency
        };
        var nPoint = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        scan.Total = nPoint;
        scan.Offset = 0;
        scan.Data = new short[nPoint];
        var index1 = nPoint * 2 / 5;
        var index2 = nPoint * 3 / 5;
        for (var i = 0; i < nPoint; i++)
        {
            scan.Data[i] = (short)_random.Next(0, 200);
            if (i > index1 && i < index2) scan.Data[i] = (short)(500 + _random.Next(-150, 150));
        }

        return scan;
    }

    private SDataMScanDf GetMScanDf(double freq, int total, int index)
    {
        var qt = 80 + _random.Next(-200, 199) / 10.0;
        var data = new SDataMScanDf
        {
            Frequency = freq,
            Total = (ushort)total,
            Index = (ushort)index,
            Azimuth = 160 + _random.Next(-150, 150) / 10.0,
            Quality = qt,
            Elevation = 0,
            Level = 48 + _random.Next(-200, 200) / 10.0
        };
        return data;
    }

    #endregion
}