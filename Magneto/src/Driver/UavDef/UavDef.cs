using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Contract.Storage;
using Magneto.Contract.UavDef;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magneto.Driver.UavDef;

public partial class UavDef : ScanBase
{
    private const int RecordWaitMinutes = 5;
    private readonly DfBearingStatistics _bearingStatistics;
    private readonly List<SDataScan> _bufScanData = [];

    private float[] _bufAzimuths = [];
    private bool _isRunning;

    /// <summary>
    ///     For judge is one record,in 15 minute.
    /// </summary>
    private DateTime _lastRecordTime = DateTime.MinValue;

    private float _maxProbability = -1;

    // 上次的GPS数据
    private SDataGps _prevGps = new();

    /// <summary>
    ///     Polling for alarm messages.
    /// </summary>
    private Task _recordTask;

    private Guid _scanTaskId = Guid.Empty;

    public UavDef(Guid functionId) : base(functionId)
    {
        IsSupportMultiSegments = true;
        _bearingStatistics = new DfBearingStatistics(45f, 0.0f);
    }

    private ConcurrentQueue<AlarmMessage> AlarmMessagesQueue { get; } = new();

    /// <summary>
    ///     启动原始数据保存
    /// </summary>
    private void StartSaveData()
    {
        SegmentList.ForEach(f =>
        {
            var sDataScan = new SDataScan
            {
                StartFrequency = f.StartFrequency,
                StopFrequency = f.StopFrequency,
                StepFrequency = f.StepFrequency,
                Total = f.Total,
                SegmentOffset = f.Index,
                Data = Enumerable.Repeat<short>(-9999, f.Total).ToArray()
            };
            _bufScanData.Add(sDataScan);
        });
        _scanTaskId = Guid.NewGuid();
        var summary = new FileDataSummary
        {
            EdgeId = RunningInfo.EdgeId,
            TaskId = _scanTaskId.ToString(),
            PluginId = "",
            DriverId = Module.Id.ToString(),
            DeviceId = Decoder.Id.ToString()
        };
        var parameters = Module.Parameters.Select(item => item.Clone()).ToList();
        if (RawDataStorage.Instance.IsTaskWriting(_scanTaskId))
            RawDataStorage.Instance.ChangeParameters(_scanTaskId, parameters, true);
        else
            RawDataStorage.Instance.Create(summary, FeatureType.UavDef, DateTime.Now, parameters);
    }

    /// <summary>
    ///     停止保存
    /// </summary>
    private void StopSaveData()
    {
        try
        {
            if (!RawDataStorage.Instance.TaskFileManagerInfoList.TryGetValue(_scanTaskId, out var taskFile)) return;
            RawDataStorage.Instance.StopSaveFile(_scanTaskId);
            RawDataStorage.Instance.Complete(_scanTaskId);
            //读参数文件
            var filePath = taskFile!.FileNotificationInfo.RootPath + "/" +
                           Path.Combine(taskFile.FileNotificationInfo.RelativePath,
                               taskFile.FileNotificationInfo.FileName + ".params");
            using var paramsStream = File.OpenRead(filePath);
            var parametersInfo = MessagePackSerializer.Deserialize<ParametersInfo>(paramsStream);
            filePath = taskFile!.FileNotificationInfo.RootPath + "/" +
                       Path.Combine(taskFile.FileNotificationInfo.RelativePath,
                           taskFile.FileNotificationInfo.FileName + ".idx");
            //读索引文件
            using var idxStream = File.OpenRead(filePath);
            idxStream.Seek(0, SeekOrigin.Begin);
            var frameTotalArray = new byte[4];
            _ = idxStream.Read(frameTotalArray, 0, frameTotalArray.Length);
            var frameTotal = BitConverter.ToUInt32(frameTotalArray);
            JArray segments = [];
            _bufScanData.ForEach(f =>
            {
                segments.Add(new JObject
                {
                    new JProperty("startFrequency", f.StartFrequency),
                    new JProperty("stopFrequency", f.StopFrequency),
                    new JProperty("stepFrequency", f.StepFrequency),
                    new JProperty("count", f.Total)
                });
            });
            var startTime = DateTime.FromFileTimeUtc((long)parametersInfo.StorageTimeSegments[0].StartTime);
            OnData([
                new PlaybackFile
                {
                    CreatedAt = startTime,
                    FileType = FileType.ScanData,
                    FileName = taskFile.FileNotificationInfo.FileName,
                    FilePath = taskFile.FileNotificationInfo.RelativePath,
                    UpDatedAt = DateTime.Now,
                    Duration = DateTime.Now - startTime,
                    Segments = JsonConvert.SerializeObject(segments),
                    TotalFrames = (int)frameTotal
                }
            ]);
            _scanTaskId = Guid.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task LoopRecordAlarmMessageAsync()
    {
        var bufRecord = new Record();
        var bufEvidence = new List<Evidence>();
        while (_isRunning)
            try
            {
                if (AlarmMessagesQueue.TryDequeue(out var almMsg))
                {
                    var djiInfo = almMsg.Details.FindAll(v => v is SDataDjiFlightInfoStr).Cast<SDataDjiFlightInfoStr>()
                        .ToArray();
                    if (almMsg.Timestamp - _lastRecordTime > TimeSpan.FromSeconds(RecordWaitMinutes) &&
                        djiInfo.Length > 0)
                    {
                        bufRecord = new Record();
                        bufEvidence = [];
                        // A new record.
                        bufRecord = new Record
                        {
                            Time = almMsg.Timestamp,
                            DetectionEquipments = [almMsg.Source],
                            InvasionArea = InvasionAreaType.AlertArea
                        };
                        bufRecord.Id = UavDefDataBase.Insert(bufRecord);
                        // Star record scan data and video,Track Nvt.
                        StartSaveData();
                        foreach (var imageRecognizer in ImageRecognizers?.Cast<DeviceBase>())
                        {
                            if (djiInfo.Length == 0)
                                continue;
                            var guide = new Dictionary<string, object>[]
                            {
                                new()
                                {
                                    { "latitude", djiInfo[0].DroneLatitude },
                                    { "longitude", djiInfo[0].DroneLongitude },
                                    { "height", djiInfo[0].Height }
                                }
                            };
                            imageRecognizer?.SetParameter("targetPosition", guide);
                            imageRecognizer?.SetParameter("isRecording", true);
                        }
                    }

                    // Keep adding information form every alarmMessage Details.
                    bufRecord.Duration = DateTime.Now - bufRecord.Time;
                    foreach (var oneDji in djiInfo)
                    {
                        var evd = bufEvidence.FirstOrDefault(e =>
                            e.ElectronicFingerprint.ToString()
                                .Equals(oneDji.DroneSerialNum));
                        if (evd is null)
                        {
                            bufRecord.NumOfFlyingObjects += 1;
                            // Add new evidence.
                            var newEvd = new Evidence
                            {
                                RecordId = bufRecord.Id,
                                Model = oneDji.ProductTypeStr,
                                Type = "uav",
                                LastFlightAltitude = oneDji.Altitude,
                                LastFlightLatitude = oneDji.DroneLatitude,
                                LastFlightLongitude = oneDji.DroneLongitude,
                                PilotLatitude = oneDji.PilotLatitude,
                                PilotLongitude = oneDji.PilotLongitude,
                                ReturnLatitude = oneDji.HomeLatitude,
                                ReturnLongitude = oneDji.HomeLongitude,
                                LastFlightHorizontalSpeed = oneDji.EastSpeed,
                                LastFlightVerticalSpeed = oneDji.NorthSpeed,
                                ElectronicFingerprint = oneDji.DroneSerialNum,
                                RadioFrequency = 2400d,
                                LastFlightBearing = 36,
                                EvdAndFiles = new List<EvdAndFile>()
                            };
                            newEvd.Id = UavDefDataBase.Insert(newEvd);
                            newEvd.EvdAndFiles.Add(new EvdAndFile { EvdId = newEvd.Id });
                            bufEvidence.Add(newEvd);
                        }
                        else
                        {
                            // Modify evidence
                            evd.LastFlightAltitude = oneDji.Altitude;
                            evd.LastFlightLatitude = oneDji.DroneLatitude;
                            evd.LastFlightLongitude = oneDji.DroneLongitude;
                            evd.PilotLatitude = oneDji.PilotLatitude;
                            evd.PilotLongitude = oneDji.PilotLongitude;
                            evd.ReturnLatitude = oneDji.HomeLatitude;
                            evd.ReturnLongitude = oneDji.HomeLongitude;
                            evd.LastFlightHorizontalSpeed = oneDji.EastSpeed;
                            evd.LastFlightVerticalSpeed = oneDji.NorthSpeed;
                        }

                        var uavPath = new UavPath
                        {
                            RecordId = bufRecord.Id,
                            Latitude = oneDji.DroneLatitude,
                            Longitude = oneDji.DroneLongitude,
                            UavSerialNum = oneDji.DroneSerialNum
                        };
                        UavDefDataBase.Insert(uavPath);
                    }

                    var disposals = almMsg.Details.FindAll(f => f is Disposal);
                    foreach (var disposal in disposals.Cast<Disposal>())
                    {
                        disposal.RecordId = bufRecord.Id;
                        UavDefDataBase.Insert(disposal);
                    }

                    // Record EvidenceFiles
                    var playbackFiles = almMsg.Details.FindAll(f => f is PlaybackFile);
                    foreach (var f in playbackFiles.Cast<PlaybackFile>())
                    {
                        f.Id = UavDefDataBase.Insert(f);
                        bufEvidence.ForEach(bf =>
                        {
                            foreach (var objEvdAndFile in bf.EvdAndFiles)
                            {
                                objEvdAndFile.FileId = f.Id;
                                UavDefDataBase.Insert(objEvdAndFile);
                            }
                        });
                    }

                    if (playbackFiles.Count > 0 || disposals.Count > 0) continue;
                    _lastRecordTime = DateTime.Now;
                    continue;
                }

                if (DateTime.Now - _lastRecordTime > TimeSpan.FromMinutes(RecordWaitMinutes) &&
                    bufRecord.NumOfFlyingObjects > 0)
                {
                    #region One record is over,Update Record and Evidence.

                    UavDefDataBase.Update(bufRecord);
                    bufEvidence.ForEach(UavDefDataBase.Update);
                    // Stop record scan data and video.
                    StopSaveData();
                    foreach (var imageRecognizer in ImageRecognizers?.Cast<DeviceBase>())
                        imageRecognizer?.SetParameter("isRecording", false);

                    #endregion

                    // Don't stop next time.
                    bufRecord.NumOfFlyingObjects = 0;
                    SendData([
                        ..new object[]
                        {
                            new AlarmMessage
                            {
                                Timestamp = DateTime.Now,
                                Status = AlarmStatus.AllClear,
                                Details = []
                            }
                        }
                    ]);
                }

                await Task.Delay(10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
    }

    protected override void StartDevice()
    {
        foreach (var receiver in Receivers.Cast<DeviceBase>())
            receiver.Start(FeatureType.ScanDf, this);
    }

    protected override void StopDevice()
    {
        foreach (var receiver in Receivers.Cast<DeviceBase>())
            receiver.Stop();
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        ((DeviceBase)Decoder).Start(FeatureType.Scan, this);
        foreach (var imageRecognizer in ImageRecognizers.Cast<DeviceBase>())
            imageRecognizer.Start(FeatureType.UavDef, this);
        _bearingStatistics.ProbabilityChanged += BearingStatistics_ProbabilityChanged;
        _bearingStatistics.Start();
        _isRunning = true;
        _recordTask = Task.Run(LoopRecordAlarmMessageAsync);

        if (_startTime == DateTime.MinValue) _startTime = Utils.GetNowTime().ToUniversalTime();
        SetFunctionSwitch();
        if (_dataProcess != null)
        {
            _dataProcess.DataProcessComplete += ProcessDataComplete;
            _dataProcess.Start();
        }

        StartMultiSegments();
        return true;
    }

    public override bool Stop()
    {
        if (!base.Stop()) return false;
        ((DeviceBase)Decoder).Stop();
        foreach (var imageRecognizer in ImageRecognizers.Cast<DeviceBase>()) imageRecognizer.Stop();
        foreach (var receiver in Receivers.Cast<DeviceBase>()) receiver.Stop();
        _isRunning = false;
        _recordTask?.Wait(1000);
        _bearingStatistics.ProbabilityChanged -= BearingStatistics_ProbabilityChanged;
        _bearingStatistics.Stop();
        _bearingStatistics.Clear();

        _saveDataSign = false;
        _saveDataHandle.Reset();
        _isGetTemplateParameters = false;
        if (_dataProcess != null)
        {
            _dataProcess.Stop();
            _dataProcess.DataProcessComplete -= ProcessDataComplete;
        }

        if (FunctionSwitch)
        {
            _ = Task.Run(async () =>
            {
                await UpdateResultDataToCloudAsync();
                _saveDataSign = true;
                _saveDataHandle.Set();
            });
            _saveDataHandle.WaitOne(5000);
            if (!_saveDataSign) Trace.WriteLine($"保存模板{TemplateId}的比对数据超时!");
        }
        else
        {
            _saveDataHandle.WaitOne(5000);
            if (!_saveDataSign) Trace.WriteLine($"保存模板{TemplateId}超时!");
        }

        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.StepFrequency) return;
        SetParameterInternal(name, value);
        if ("omniSuppressing".Equals(name))
            SendData([
                new AlarmMessage
                {
                    Status = AlarmStatus.Processed,
                    Timestamp = DateTime.Now,
                    Details =
                    [
                        new Disposal
                        {
                            Content = "全向压制",
                            DateTime = DateTime.Now
                        }
                    ]
                }
            ]);
    }

    private void BearingStatistics_ProbabilityChanged(object sender, BearingStatisticsArgs args)
    {
        var dlt = new DelegateProbabilityChanged(e =>
        {
            _maxProbability = NormalizeAngle(e.MaxProbability);
            var list = new List<object>();
            var sDataDfind = new SDataDfind
            {
                Azimuth = _maxProbability
            };
            list.Add(sDataDfind);
            SendData(list);
        });
        dlt.Invoke(args);
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle < 0) angle += 360;
        return angle;
    }

    private readonly List<object> _details = [];
    public override void OnData(List<object> data)
    {
        if (data.Exists(e => e is SDataGps))
            _prevGps = (SDataGps)data.Find(f => f is SDataGps);
        if (data.Exists(e => e is SDataCompass))
        {
        }

        var alarmMessages = data.FindAll(f => f is AlarmMessage);
        if (alarmMessages.Any())
        {
            alarmMessages.ForEach(f =>
            {
                ((AlarmMessage)f).Details.ForEach(d =>
                {
                    if (d is not SDataDjiFlightInfoStr) return;
                    _details?.RemoveAll(e => e is null || ((SDataDjiFlightInfoStr)e).DroneSerialNum.Equals(((SDataDjiFlightInfoStr)d).DroneSerialNum));
                    _details?.Add(d);
                });
                ((AlarmMessage)f).Details = _details;
            });
        }
        var evidenceFiles = data.FindAll(f => f is PlaybackFile);
        if (evidenceFiles.Any())
        {
            var buf = new AlarmMessage
            {
                Timestamp = DateTime.Now,
                Description = "Video",
                Details = []
            };
            buf.Details.AddRange(evidenceFiles);
            alarmMessages.Add(buf);
        }

        var sDataScan = data.FindAll(f => f is SDataScan);
        if (sDataScan.Count > 0)
        {
            SendDataWithSpan(data);
            if (RawDataStorage.Instance.IsTaskWriting(_scanTaskId))
            {
                //if (_bufScanData.Count.Equals(2))
                //{
                //    var scanData = data.FindAll(f => f is SDataScan).Select(s => (SDataScan)s);
                //    foreach (var dataScan in scanData)
                //        Array.Copy(dataScan.Data, 0, _bufScanData[dataScan.SegmentOffset].Data, dataScan.Offset,
                //            dataScan.Data.Length);
                //    if (_bufScanData.All(a => !a.Data.Contains((short)-9999)))
                //    {
                //        RawDataStorage.Instance.Write(_scanTaskId, new List<object> { _bufScanData });
                //        _bufScanData.ForEach(f => f.Data = f.Data.Select(_ => (short)-9999).ToArray());
                //    }
                //}
            }

            var sDataDfScan = (SDataDfScan)data.Find(f => f is SDataDfScan);
            if (_markScanDf is { Segment: 0, StartIndex: 0, StopIndex: 0 }) return;
            if (!_markScanDf.Segment.Equals(sDataDfScan.SegmentOffset)) return;
            var (startFreq, stopFreq) = GetFrequencyIntersection(sDataDfScan.Offset, sDataDfScan.Offset + sDataDfScan.Count,
                _markScanDf.StartIndex,
                _markScanDf.StopIndex);
            _bufAzimuths = _bufAzimuths.Concat(sDataDfScan.Azimuths.Skip(startFreq - sDataDfScan.Offset)
                .Take(stopFreq - startFreq)).ToArray();
            if (_markScanDf.StopIndex.Equals(stopFreq))
            {
                _bufAzimuths = ExtractMaxProbability(_bufAzimuths.Where(w => w > -1f).ToArray());
                foreach (var bufAzimuth in _bufAzimuths)
                {
                    if (bufAzimuth < 0) continue;
                    _ = Task.Run(() => _bearingStatistics.AddData(new SdFindData
                    {
                        Azimuth = NormalizeAngle(bufAzimuth / 10f), // 这里加上罗盘使出来的数据不再做场地模式判断
                        Level = 20f,
                        Quality = 20f,
                        TimeStamp = DateTime.Now,
                        Lng = _prevGps.Longitude,
                        Lat = _prevGps.Latitude
                    }));
                }

                _bufAzimuths = [];
            }
        }

        alarmMessages.ForEach(f =>
        {
            AlarmMessagesQueue.Enqueue((AlarmMessage)f);
            var djiInfo = ((AlarmMessage)f).Details.FindAll(v => v is SDataDjiFlightInfoStr)
                .Cast<SDataDjiFlightInfoStr>()
                .ToArray();
            if (djiInfo.Length <= 0) return;
            var location = ((DeviceBase)Receivers[0]).GetParameter<string>("Address").Split(',');
            var angle = Bearing.Complex(double.Parse(location[1]), double.Parse(location[0]),
                djiInfo[0].DroneLatitude, djiInfo[0].DroneLongitude);
            var randomValue = new Random().NextDouble() * 3 - 1.5;
            djiInfo[0].YawAngle = (short)Math.Round(angle + randomValue);
            if (ImageRecognizers is null) return;
            foreach (var imageRecognizer in ImageRecognizers?.Cast<DeviceBase>()!)
            {
                if (djiInfo.Length == 0)
                    continue;
                var guide = new Dictionary<string, object>[]
                {
                    new()
                    {
                        { "latitude", djiInfo[0].DroneLatitude },
                        { "longitude", djiInfo[0].DroneLongitude },
                        { "height", djiInfo[0].Height }
                    }
                };
                imageRecognizer?.SetParameter("targetPosition", guide);
            }
        });
        SendData(data);

        try
        {
            var scan = (SDataScan)data.Find(item => item is SDataScan);
            if (scan != null && FunctionSwitch && SetSimData)
            {
                var num1 = scan.Total / 4;
                var num2 = scan.Total / 2;
                var num3 = scan.Total / 4 * 3;
                var num11 = num1 - scan.Offset;
                var num22 = num2 - scan.Offset;
                var num33 = num3 - scan.Offset;
                if (num11 >= 0 && num11 < scan.Data.Length) scan.Data[num11] += 200;
                if (num22 >= 0 && num22 < scan.Data.Length) scan.Data[num22] += 200;
                if (num33 >= 0 && num33 < scan.Data.Length) scan.Data[num33] += 200;
            }
        }
        catch
        {
            // 容错代码
        }

        SendDataWithSpan(data);
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private static float[] ExtractMaxProbability(float[] azimuths)
    {
        azimuths = azimuths.Where(val => val > -1f).ToArray();
        var bufAzimuths = azimuths.Select(s =>
        {
            s /= 10f;
            if (s > 180)
                return s - 360;
            return s;
        }).ToArray();
        var sumAzimuth = bufAzimuths.Sum();
        var meanAzimuth = sumAzimuth / bufAzimuths.Length;
        var variance = bufAzimuths.Sum(s => Math.Pow(s - meanAzimuth, 2));
        var standardDeviation = Math.Sqrt(variance / bufAzimuths.Length);
        if (standardDeviation > 100)
            for (var i = 0; i < bufAzimuths.Length; i++)
                azimuths[i] = -1f; // I assume this is what you meant by azimuthsrstd;:get<0>(item)7 12
        else
            for (var i = 0; i < bufAzimuths.Length; i++)
                if (Math.Abs(bufAzimuths[i] - meanAzimuth) > 1.5 * standardDeviation && standardDeviation > 0)
                    azimuths[i] = -1f; // I assume this is what you meant by azimuths[std::get<0>(item)]-14
        return azimuths;
    }

    public static (int startFreq, int stopFreq) GetFrequencyIntersection(int srcStartFreq, int srcStopFreq,
        int destStartFreq, int destStopFreq)
    {
        var startFreq = Math.Max(srcStartFreq, destStartFreq);
        var stopFreq = Math.Min(srcStopFreq, destStopFreq);
        return startFreq > stopFreq ? (-1, -1) : (startFreq, stopFreq);
    }

    private delegate void DelegateProbabilityChanged(BearingStatisticsArgs args);
}