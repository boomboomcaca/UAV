using System;
using System.Collections.Generic;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Core.Statistics;

public delegate void DataStatisticsDataArrivedHandler(List<object> data);

public class DataStatistics
{
    private readonly IDataProcess _dataProcess;
    private bool _isRunning;
    private Guid _taskId;

    public DataStatistics(FeatureType feature, Guid taskId, IAntennaController controller)
    {
        _taskId = taskId;
        switch (feature)
        {
            case FeatureType.Ffm:
                _dataProcess = new FfmProcess(controller);
                break;
            case FeatureType.Ffdf:
                _dataProcess = new FfdfProcess(controller);
                break;
            case FeatureType.Scan:
            case FeatureType.Nsic:
            case FeatureType.Ese:
                // _dataProcess = new ScanOccupancyProcess(controller, _feature);
                break;
            case FeatureType.Ifmca:
                _dataProcess = new IfmcaProcess(controller);
                break;
            case FeatureType.MScan:
                _dataProcess = new MScanProcess(controller);
                break;
            default:
                _dataProcess = new DataProcessBase(controller);
                break;
        }
    }

    public event DataStatisticsDataArrivedHandler StatisticsDataArrived;

    public void Start()
    {
        if (_isRunning) return;
        if (_dataProcess != null)
        {
            _dataProcess.Start();
            _dataProcess.DataProcessComplete += DataProcessComplate;
        }

        _isRunning = true;
    }

    public void Stop()
    {
        _dataProcess?.Stop();
        if (_dataProcess != null) _dataProcess.DataProcessComplete -= DataProcessComplate;
        _isRunning = false;
    }

    public void ChangeParameter(Parameter parameter)
    {
        _dataProcess?.SetParameter(parameter);
    }

    public void RealtimeDataProcess(List<object> data)
    {
        _dataProcess?.OnData(data);
    }

    public void ModulationRecognition()
    {
    }

    private void DataProcessComplate(object sender, List<object> e)
    {
        if (e?.Count <= 0) return;
        StatisticsDataArrived?.Invoke(e);
    }
}