using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.Audio;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.IFMCA;

/// <summary>
///     航空监测
///     单独放在一个类中，与中频多路尽量独立，防止以后两个功能需要拆分
/// </summary>
public class AirMonitoring
{
    private const string DataSuffix = ".dat";
    private const string IndexSuffix = ".idx";
    private const string TempSuffix = ".temp";

    /// <summary>
    ///     计算占用度间隔时间 ms
    /// </summary>
    private const int CalcOccpancyInterval = 1000;

    /// <summary>
    ///     信噪比阈值
    /// </summary>
    private const float SnrThreshold = 12f;

    /// <summary>
    ///     占用度统计阈值
    /// </summary>
    private const float OccupancyThreshold = 1f;

    /// <summary>
    ///     缓存当前正在保存的音频
    ///     键为精确到Hz的中心频率
    /// </summary>
    private readonly ConcurrentDictionary<long, AudioSaveHelper> _audioSaveCache = new();

    private readonly TheoryThreshold _calcThreshold = new();
    private readonly ConcurrentQueue<short[]> _dataCache = new();

    /// <summary>
    ///     缓存频率信息
    ///     键为通道号
    /// </summary>
    private readonly ConcurrentDictionary<int, IfmcaTemplate> _frequenciesCache = new();

    /// <summary>
    ///     存放航空监测数据的根目录
    /// </summary>
    private readonly string _rootDir;

    private AntennaControllerBase _antennaController;

    /// <summary>
    ///     总数据帧数
    /// </summary>
    private int _count;

    private CancellationTokenSource _cts;

    /// <summary>
    ///     当前已经存储的数据的个数
    /// </summary>
    private int _dataCount;

    /// <summary>
    ///     文件全路径（包含根目录）
    /// </summary>
    private string _fileDir = string.Empty;

    private double _frequency;

    /// <summary>
    ///     最大频谱，每个存储周期（1分）清理一次
    /// </summary>
    private short[] _maxData;

    /// <summary>
    ///     占用度计算的最大频谱，每个占用度周期清理一次
    /// </summary>
    private short[] _maxOccData;

    private DeviceBase _monitorDevice;
    private double[] _occupancy;
    private int[] _overTimes;
    private DateTime _preOccTime = DateTime.Now;
    private DateTime _preSaveDataTime = GetMinuteTime(Utils.GetNowTime());
    private Task _processSignalsTask;

    /// <summary>
    ///     相对路径
    /// </summary>
    private string _relativePath;

    private bool _running;
    private float[] _snr;
    private double _span;

    /// <summary>
    ///     每帧数据的点数
    /// </summary>
    private int _total;

    private long[] _totalData;

    public AirMonitoring()
    {
        var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2");
        if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
        _rootDir = Path.Combine(saveDir, PublicDefine.PathAvicg);
        if (!Directory.Exists(_rootDir)) Directory.CreateDirectory(_rootDir);
    }

    public event EventHandler<List<SignalsResult>> SignalsChanged;
    public event EventHandler<SDataAvicgFrequencies> AvicgFrequenciesChanged;

    public void Start(DeviceBase monitor, AntennaControllerBase antennaController)
    {
        _monitorDevice = monitor;
        _antennaController = antennaController;
        _preSaveDataTime = GetMinuteTime(Utils.GetNowTime());
        // 自动门限容差写死
        _calcThreshold.ThresholdMargin = 3;
        _running = true;
        _cts = new CancellationTokenSource();
        _processSignalsTask = new Task(p => ProcessSignalsAsync(p).ConfigureAwait(false), _cts.Token);
        _processSignalsTask.Start();
    }

    public void DdcFrequenciesModify(Dictionary<string, object>[] dics)
    {
        _frequenciesCache.Clear();
        if (dics == null || dics.Length == 0) return;
        var array = Array.ConvertAll(dics, item => (IfmcaTemplate)item);
        for (var i = 0; i < array.Length; i++)
        {
            var freq = (long)(array[i].Frequency * 1e6);
            var bw = array[i].Bandwidth;
            float factor = 0;
            _frequenciesCache.TryAdd(i, array[i]);
            if (_antennaController != null) factor = _antennaController.GetFactor(array[i].Frequency) / 10f;
            if (!_audioSaveCache.ContainsKey(freq))
            {
                var dir = Utils.GetNowTime().ToString("yyyyMMdd");
                var ash = new AudioSaveHelper(_rootDir, dir, GetMinuteTime(Utils.GetNowTime()), freq, factor, bw);
                _audioSaveCache.TryAdd(freq, ash);
            }
        }
    }

    public void Close()
    {
        _running = false;
        // 停止的时候需要将未存储的数据存储
        SaveData(_maxData, _preSaveDataTime);
        // 停止的时候需要将未存储的数据存储
        CompleteAudioData(_preSaveDataTime);
        _audioSaveCache.Clear();
        _frequenciesCache.Clear();
        try
        {
            _cts?.Cancel();
        }
        catch
        {
        }
    }

    public void OnData(List<object> data)
    {
        if (!_running) return;
        if (data.Find(item => item is SDataSpectrum) is SDataSpectrum spectrum)
        {
            if (!Utils.IsNumberEquals(_frequency, spectrum.Frequency)
                || !Utils.IsNumberEquals(_span, spectrum.Span)
                || _maxOccData == null
                || _totalData == null
                || _maxOccData.Length != spectrum.Data.Length)
            {
                Console.WriteLine($"主频谱变化,{_frequency},{_span}||{spectrum.Frequency},{spectrum.Span}");
                _dataCache.Clear();
                _total = spectrum.Data.Length;
                _maxOccData = new short[_total];
                Array.Fill(_maxOccData, (short)-9999);
                _totalData = new long[_total];
                _overTimes = new int[_total];
                _snr = new float[_total];
                Array.Fill(_snr, -9999f);
                _occupancy = new double[_total];
                _maxData = new short[_total];
                Array.Fill(_maxData, (short)-9999);
            }

            _frequency = spectrum.Frequency;
            _span = spectrum.Span;
            _dataCache.Enqueue(spectrum.Data);
        }

        foreach (var item in data)
            if (item is SDataDdc ddc)
                CacheChannelData(ddc);
    }

    private async Task ProcessSignalsAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                if (_dataCache.IsEmpty)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }

                if (!_dataCache.TryDequeue(out var data))
                {
                    await Task.Delay(1).ConfigureAwait(false);
                    continue;
                }

                if (!_running) break;
                var startFrequency = _frequency - _span / 2000;
                var stopFrequency = _frequency + _span / 2000;
                // 由于存在步进为小数的情况，需要限制步进为0.1kHz，因此需要保留一位小数
                var stepFrequency = Math.Round(_span / (data.Length - 1), 1);
                CalcOccupancy(data, startFrequency, stopFrequency, stepFrequency);
                var nowTime = GetMinuteTime(Utils.GetNowTime());
                if (nowTime != _preSaveDataTime && _running)
                {
                    // 一分钟存储一次主通道频谱
                    var saveData = new short[_maxData.Length];
                    Buffer.BlockCopy(_maxData, 0, saveData, 0, sizeof(short) * _maxData.Length);
                    Array.Fill(_maxData, (short)-9999);
                    SaveData(saveData, _preSaveDataTime);
                    // 一分钟存储一次音频数据并发送消息到云端
                    CompleteAudioData(_preSaveDataTime);
                    _preSaveDataTime = nowTime;
                    _audioSaveCache.Clear();
                }
            }
            catch
            {
            }
    }

    #region

    /// <summary>
    ///     占用度计算并提取信号
    /// </summary>
    /// <param name="data"></param>
    /// <param name="startFrequency"></param>
    /// <param name="stopFrequency"></param>
    /// <param name="stepFrequency"></param>
    private void CalcOccupancy(short[] data, double startFrequency, double stopFrequency, double stepFrequency)
    {
        if (data == null || data.Length == 0) return;
        if (!_running) return;
        _count++;
        var tmps = new float[_total];
        for (var i = 0; i < _total; i++)
        {
            tmps[i] = data[i] / 10f;
            if (_maxOccData[i] < data[i]) _maxOccData[i] = data[i];
            if (_maxData[i] < data[i]) _maxData[i] = data[i];
            _totalData[i] += data[i];
        }

        var autoThreshold = _calcThreshold.CalThreshold(tmps, startFrequency, stopFrequency, (float)stepFrequency);
        if (!_running) return;
        for (var i = 0; i < autoThreshold.Length; i++)
        {
            if (tmps[i] >= autoThreshold[i])
            {
                _overTimes[i]++;
                var snr = tmps[i] - autoThreshold[i];
                if (_snr[i] < snr) _snr[i] = snr;
            }

            _occupancy[i] = (float)_overTimes[i] / _count * 100;
        }

        // 计算占用度
        if (DateTime.Now.Subtract(_preOccTime).TotalMilliseconds >= CalcOccpancyInterval)
        {
            var signles = Utils.SignalExtract(_occupancy, _snr, startFrequency, stepFrequency, _maxOccData,
                OccupancyThreshold, SnrThreshold);
            for (var i = 0; i < signles.Count; i++)
            {
                var info = signles[i];
                var freq = info.Frequency;
                if (freq is >= 88 and <= 108) freq = Math.Round(freq, 1);
                info.Frequency = freq;
                signles[i] = info;
            }

            Console.WriteLine($"提取到{signles.Count}个信号");
            SignalsChanged?.Invoke(this, signles);
            _count = 0;
            _totalData = new long[_totalData.Length];
            _overTimes = new int[_overTimes.Length];
            Array.Fill(_maxOccData, (short)-9999);
            Array.Fill(_snr, -9999f);
            _preOccTime = DateTime.Now;
        }
    }

    #endregion

    /// <summary>
    ///     返回精确到分钟的时间
    /// </summary>
    /// <param name="time"></param>
    private static DateTime GetMinuteTime(DateTime time)
    {
        return time.Date.AddMinutes(time.Hour * 60 + time.Minute);
    }

    #region 数据存储

    /// <summary>
    ///     缓存通道数据
    /// </summary>
    /// <param name="ddc"></param>
    private void CacheChannelData(SDataDdc ddc)
    {
        if (!_frequenciesCache.TryGetValue(ddc.ChannelNumber, out var ifmca)) return;
        var freq = (long)(ifmca.Frequency * 1e6);
        if (!_audioSaveCache.TryGetValue(freq, out var save)) return;
        if (ddc.Data.Find(item => item is SDataLevel) is SDataLevel level) save.UpdateLevel(level.Data);
        if (ddc.Data.Find(item => item is SDataAudio) is SDataAudio audio) save.SaveAudio(audio);
    }

    /// <summary>
    ///     频谱数据存储
    /// </summary>
    /// <param name="data"></param>
    /// <param name="time">当前数据的时间（精确到分钟的时间）</param>
    private void SaveData(short[] data, DateTime time)
    {
        var folder = time.ToString("yyyyMMdd");
        _fileDir = Path.Combine(_rootDir, folder);
        _relativePath = Path.Combine("data2", PublicDefine.PathAvicg, folder);
        if (!Directory.Exists(_fileDir)) Directory.CreateDirectory(_fileDir);
        var indexFileName = $"{RunningInfo.EdgeId}_avicg_{time:yyyyMMdd}{IndexSuffix}{TempSuffix}";
        var indexFilePath = Path.Combine(_fileDir, indexFileName);
        if (File.Exists(indexFilePath))
        {
            var fs = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read);
            var bt = new byte[4];
            fs.Read(bt, 0, bt.Length);
            _dataCount = BitConverter.ToInt32(bt, 0);
            fs.Close();
            fs.Dispose();
        }
        else
        {
            _dataCount = 0;
            var header = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, _frequency },
                { ParameterNames.IfBandwidth, _span },
                { "dataLength", _total }
            };
            var summary = Utils.ConvertToMessagePackData(header);
            var fs = new FileStream(indexFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            var bytes = new List<byte>();
            bytes.AddRange(new byte[4]); //总帧数
            bytes.AddRange(BitConverter.GetBytes((uint)summary.Length)); //文件头长度
            bytes.AddRange(summary); //文件头
            fs.Write(bytes.ToArray(), 0, bytes.Count);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }

        // 存储数据
        Console.WriteLine($"存储频谱数据，当前个数:{_dataCount}");
        var buffer = Utils.ConvertToMessagePackData(data);
        var dataFileName = $"{RunningInfo.EdgeId}_avicg_{time:yyyyMMdd}.1{DataSuffix}{TempSuffix}";
        var dataFilePath = Path.Combine(_fileDir, dataFileName);
        var dataStream = new FileStream(dataFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        dataStream.Seek(0, SeekOrigin.End);
        var pos = dataStream.Position;
        dataStream.Write(buffer, 0, buffer.Length);
        dataStream.Flush();
        dataStream.Close();
        dataStream.Dispose();
        // 存储索引
        var idxBytes = Utils.CreateIndexBytes(_dataCount, 1, (int)pos, Utils.GetTimestamp(time));
        var indexStream = new FileStream(indexFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        indexStream.Seek(0, SeekOrigin.End);
        indexStream.Write(idxBytes, 0, idxBytes.Length);
        _dataCount++;
        indexStream.Seek(0, SeekOrigin.Begin);
        var ca = BitConverter.GetBytes(_dataCount);
        indexStream.Write(ca, 0, ca.Length);
        indexStream.Flush();
        indexStream.Close();
        indexStream.Dispose();
        // 重命名
        // 由于同步程序可能会占用文件，因此如果覆盖失败则直接不管
        var ndp = dataFilePath.Replace(TempSuffix, "");
        var idp = indexFilePath.Replace(TempSuffix, "");
        try
        {
            File.Copy(dataFilePath, ndp, true);
        }
        catch
        {
        }

        try
        {
            File.Copy(indexFilePath, idp, true);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     完成音频数据的写入并发送到云端
    /// </summary>
    /// <param name="time"></param>
    private void CompleteAudioData(DateTime time)
    {
        // 发送消息到云端，一分钟发送一包所有数据
        var msg = new SDataAvicgFrequencies
        {
            Frequency = _frequency,
            IfBandwidth = _span,
            RelativePath = _relativePath,
            Timestamp = Utils.GetTimestamp(time)
        };
        List<FrequenciesInfo> list = new();
        foreach (var pair in _audioSaveCache)
        {
            pair.Value.SaveComplete();
            // 包装需要发送到云端的数据
            FrequenciesInfo info = new()
            {
                Frequency = pair.Key / 1e6,
                Bandwidth = pair.Value.Bandwidth,
                File = pair.Value.FileName,
                FieldStrength = pair.Value.FieldStrength,
                Duration = pair.Value.Duration
            };
            list.Add(info);
        }

        msg.Frequencies = list;
        AvicgFrequenciesChanged?.Invoke(this, msg);
    }

    #endregion
}

/// <summary>
///     音频存储帮助类
/// </summary>
internal class AudioSaveHelper
{
    private readonly float _factor;
    private readonly string _folder;
    private readonly string _rootPath;
    private readonly AudioDataSave _save;
    private float _level;

    public AudioSaveHelper(string rootPath, string folder, DateTime time, long frequency, float factor,
        double bandwidth)
    {
        _rootPath = rootPath;
        _folder = folder;
        Frequency = frequency;
        Bandwidth = bandwidth;
        _level = -999f;
        _factor = factor;
        FileName = $"{time:HHmm}_{Frequency}_%_#";
        _save = new AudioDataSave();
    }

    public long Frequency { get; }

    /// <summary>
    ///     文件名 其中最后两位分别为时长与场强
    ///     在最终完成写入时需要重命名，将%与#进行替换
    /// </summary>
    public string FileName { get; private set; }

    public double Bandwidth { get; }
    public float FieldStrength => _level + _factor;
    public float Duration { get; private set; }

    public void UpdateLevel(float level)
    {
        if (_level < level) _level = level;
    }

    public void SaveAudio(SDataAudio data)
    {
        if (_save.SaveStopped) return;
        if (!_save.Running)
            _save.SaveStart(_rootPath, _folder, FileName, Frequency, data.Channels, data.SamplingRate,
                data.BitsPerSample);
        _save.SaveData(data.Data);
    }

    public void SaveComplete()
    {
        if (_save?.Running != true) return;
        Duration = (float)_save.GetDuration();
        var len = (int)Duration;
        var fs = (int)(_level + _factor);
        var newFile = FileName.Replace("%", len.ToString()).Replace("#", fs.ToString());
        FileName = newFile;
        _save.SaveComplete(newFile);
        _save.Dispose();
    }
}

internal class IfmcaTemplate
{
    [Name(ParameterNames.Frequency)] public double Frequency { get; set; }

    [Name(ParameterNames.FilterBandwidth)] public double Bandwidth { get; set; }

    public static explicit operator IfmcaTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new IfmcaTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (dict.ContainsKey(name))
                {
                    object objValue = null;
                    if (property.PropertyType.IsEnum)
                        objValue = Utils.ConvertStringToEnum(dict[name].ToString(), property.PropertyType);
                    else if (property.PropertyType == typeof(Guid))
                        objValue = Guid.Parse(dict[name].ToString() ?? string.Empty);
                    else if (property.PropertyType.IsValueType)
                        objValue = Convert.ChangeType(dict[name], property.PropertyType);
                    else
                        objValue = dict[name]; //Convert.ChangeType(value, prop.PropertyType);
                    property.SetValue(template, objValue, null);
                }
            }
        }
        catch
        {
            // 容错代码
        }

        return template;
    }
}