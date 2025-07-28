using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Driver.UAVD.Algorithm;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UAVD;

public partial class Uavd : ScanBase
{
    #region 构造函数

    public Uavd(Guid functionId) : base(functionId)
    {
        IsSupportMultiSegments = false;
    }

    #endregion

    #region 无人机识别业务功能

    private void CaptureUav(object obj)
    {
        while (_uavCaptureCts?.IsCancellationRequested == false)
            try
            {
                Thread.Sleep(1);
                if (_spectraQueue.IsEmpty) continue;
                var b = _spectraQueue.TryDequeue(out var spectraCollection);
                if (!b || spectraCollection == null || spectraCollection.Count == 0) continue;
                if (!_spectraQueue.IsEmpty) //保证取出的数据是最新的一条
                    continue;
                var droneBytesLength = 1024 * 20;
                var droneBytes = new byte[droneBytesLength];
                var spectraList = new List<Spectra>();
                spectraCollection.ForEach(spectra =>
                {
                    var dataPtr = Marshal.AllocHGlobal(spectra.Length * Marshal.SizeOf<float>());
                    Marshal.Copy(spectra, 0, dataPtr, spectra.Length);
                    spectraList.Add(new Spectra { Length = spectra.Length, Data = dataPtr });
                });
                try
                {
                    UavDll.AttachEnhancedUav(_startFrequency, _stopFrequency, _stepFrequency, SnrThreshold,
                        spectraList.ToArray(), spectraCollection.Count, droneBytes, ref droneBytesLength);
                }
                finally
                {
                    spectraList.ForEach(p => Marshal.FreeHGlobal(p.Data)); //及时释放
                }

                var droneArrayStr = Encoding.UTF8.GetString(droneBytes, 0, droneBytesLength);
                if (string.IsNullOrWhiteSpace(droneArrayStr)) continue;
                var drones = droneArrayStr.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (drones.Any() != true || (drones.Length == 1 &&
                                             string.Equals("none", drones[0], StringComparison.OrdinalIgnoreCase)))
                    continue;
                foreach (var droneInfo in drones)
                {
                    var fields = droneInfo.Split(';');
                    if (fields.Length < 5) continue;
                    var name = fields[1];
                    _uavCounter.AddOrUpdate(name, new SDataUavd
                    {
                        Model = name,
                        FirstTime = Utils.GetNowTimestamp(),
                        LastTime = Utils.GetNowTimestamp(),
                        Azimuth = (Azimuth + new Random().NextSingle() * 5) % 360,
                        Occurrences = 1,
                        Details = GetDroneDataCollectionByModel(name)
                    }, (_, v) =>
                    {
                        v.LastTime = Utils.GetNowTimestamp();
                        v.Azimuth = (Azimuth + new Random().NextSingle() * 5) % 360;
                        v.Occurrences++;
                        return v;
                    });
                }

                SendData(new List<object>(_uavCounter.Values));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    #endregion

    #region 辅助方法

    private List<DroneData> GetDroneDataCollectionByModel(string model)
    {
        lock (_droneTemplateLocker)
        {
            if (_droneTemplates?.Any() != true) return null;
        }

        var droneDataCollection = new List<DroneData>();
        lock (_droneTemplateLocker)
        {
            foreach (var droneTemplate in _droneTemplates)
                if (droneTemplate.Name.Equals(model, StringComparison.OrdinalIgnoreCase))
                    droneDataCollection.Add(new DroneData
                    {
                        Model = droneTemplate.Name,
                        StartFrequency = droneTemplate.StartFrequency,
                        StopFrequency = droneTemplate.StopFrequency,
                        LowerBandwidth = droneTemplate.Bandwidth,
                        UpperBandwidth = droneTemplate.Bandwidth,
                        FrequencyHoppingStep = droneTemplate.HoppingStep1
                    });
        }

        return droneDataCollection;
    }

    #endregion

    #region 成员变量

    private readonly double _ePsilon = 1.0E-7;
    private readonly object _lockCachedScan = new();
    private double _startFrequency;
    private double _stopFrequency;
    private double _stepFrequency;
    private float[] _cachedScan;
    private readonly ConcurrentDictionary<string, SDataUavd> _uavCounter = new();
    private readonly ConcurrentQueue<List<float[]>> _spectraQueue = new();
    private List<float[]> _latestSpectraCollection = new();
    private CancellationTokenSource _uavCaptureCts;

    private Task _uavCaptureTask;

    //private readonly ManualResetEvent _startTaskEvent = new ManualResetEvent(false);
    private readonly List<DroneModel> _droneTemplates = new();
    private readonly object _droneTemplateLocker = new();

    #endregion

    #region 任务相关

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        var droneTemplateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"uav_enhanced_template.json");
        if (File.Exists(droneTemplateFile))
        {
            var droneTemplateString = File.ReadAllText(droneTemplateFile);
            try
            {
                var templates = Utils.ConvertFromJson<DroneModel[]>(droneTemplateString);
                if (templates?.Any() != true) return;
                lock (_droneTemplateLocker)
                {
                    _droneTemplates.Clear();
                    _droneTemplates.AddRange(templates);
                    UavDll.InitEnhancedUavSchema(templates.Select(p => p.ToDrone()).ToArray(), templates.Length);
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        UpdateAntennaControllerFrequency(_startFrequency);
        StartMultiSegments();
        var scanLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
        _cachedScan = new float[scanLength];
        _uavCaptureCts = new CancellationTokenSource();
        _uavCaptureTask = new Task(CaptureUav, _uavCaptureCts.Token);
        _uavCaptureTask.Start();
        return true;
    }

    public override bool Stop()
    {
        base.Stop();
        Utils.CancelTask(_uavCaptureTask, _uavCaptureCts);
        lock (_lockCachedScan)
        {
            _latestSpectraCollection.Clear();
            _cachedScan = null;
        }

        _uavCounter.Clear();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        if (name.Equals(ParameterNames.ScanSegments))
            // 无人机不支持按频段下载
            return;
        base.SetParameter(name, value);
        if (name.Equals(ParameterNames.StartFrequency)) _startFrequency = Convert.ToDouble(value);
        if (name.Equals(ParameterNames.StopFrequency)) _stopFrequency = Convert.ToDouble(value);
        if (name.Equals(ParameterNames.StepFrequency)) _stepFrequency = Convert.ToDouble(value);
        lock (_lockCachedScan)
        {
            if (IsTaskRunning
                && _startFrequency > 0
                && _stopFrequency > 0
                && _stepFrequency > 0)
            {
                var scanLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
                if (scanLength > 0 && (_cachedScan == null || _cachedScan.Length != scanLength))
                    _cachedScan = new float[scanLength];
            }
        }

        if (name.Equals(ParameterNames.StartFrequency)
            || name.Equals(ParameterNames.StopFrequency)
            || name.Equals(ParameterNames.StepFrequency)
            || name.Equals("snrthreshold", StringComparison.OrdinalIgnoreCase)
            || name.Equals("integralTimes", StringComparison.OrdinalIgnoreCase))
        {
            UpdateScanSegments();
            _spectraQueue.Clear();
            _uavCounter.Clear();
        }
    }

    public override void OnData(List<object> data)
    {
        if (data.Find(item => item is SDataScan) is not SDataScan scan
            || Math.Abs(scan.StartFrequency - _startFrequency) > _ePsilon
            || Math.Abs(scan.StopFrequency - _stopFrequency) > _ePsilon
            || Math.Abs(scan.StepFrequency - _stepFrequency) > _ePsilon)
            return;
        SendDataWithSpan(data);
        lock (_lockCachedScan)
        {
            var scanData = Array.ConvertAll(scan.Data, item => item / 10f);
            Array.Copy(scanData, 0, _cachedScan, scan.Offset, scanData.Length);
            if (scan.Offset + scan.Data.Length == scan.Total)
                //var dataPtr = Marshal.AllocHGlobal(_cachedScan.Length * Marshal.SizeOf<float>());
                //Marshal.Copy(_cachedScan, 0, dataPtr, _cachedScan.Length);
                _latestSpectraCollection.Add(_cachedScan);
            if (_latestSpectraCollection.Count == IntegrationTimes)
            {
                _spectraQueue.Enqueue(_latestSpectraCollection);
                _latestSpectraCollection = new List<float[]>();
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        Utils.CancelTask(_uavCaptureTask, _uavCaptureCts);
    }

    protected override void StartDevice()
    {
        (Receiver as DeviceBase)?.Start(FeatureType.SCAN, this);
    }

    protected override void StopDevice()
    {
        (Receiver as DeviceBase)?.Stop();
    }

    #endregion
}