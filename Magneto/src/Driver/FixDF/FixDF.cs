using System;
using System.Collections.Generic;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.FixDF;

public partial class FixDf : DriverBase
{
    private DfBearingStatistics _dfBearingStatistics;
    private float _levelData;
    private float _optimalAzimuth = -1f;

    public FixDf(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _optimalAzimuth = -1;
        _dfBearingStatistics = new DfBearingStatistics();
        _dfBearingStatistics.Resolution = 0.1f;
        _dfBearingStatistics.ProbabilityChanged += DfBearingStatistics_ProbabilityChanged;
        _dfBearingStatistics.AngleStatisticsChanged += DfBearingStatistics_AngleStatisticsChanged;
        _dfBearingStatistics.Start();
        (Receiver as DeviceBase)?.Start(FeatureType.FFDF, this);
        return true;
    }

    public override bool Stop()
    {
        base.Stop();
        _optimalAzimuth = -1;
        (Receiver as DeviceBase)?.Stop();
        _dfBearingStatistics?.Clear();
        _dfBearingStatistics?.Stop();
        if (_dfBearingStatistics != null)
        {
            _dfBearingStatistics.ProbabilityChanged -= DfBearingStatistics_ProbabilityChanged;
            _dfBearingStatistics.AngleStatisticsChanged -= DfBearingStatistics_AngleStatisticsChanged;
        }

        _dfBearingStatistics?.Dispose();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name is ParameterNames.Frequency or ParameterNames.DfBandwidth) _dfBearingStatistics?.Clear();
    }

    public override void OnData(List<object> data)
    {
        if (data.Exists(item => item is SDataDfind)) CanPause = true;
        var level = (SDataLevel)data.Find(item => item is SDataLevel);
        var dfind = (SDataDfind)data.Find(item => item is SDataDfind);
        if (level != null) _levelData = level.Data;
        if (dfind != null)
        {
            if (dfind.Azimuth > 0)
            {
                // dfind.Azimuth += RunningInfo.CompassData.Heading;
                _dfBearingStatistics.AddData(new SdFindData(dfind.Frequency)
                {
                    Azimuth = dfind.Azimuth + RunningInfo.BufCompassData.Heading,
                    Level = _levelData,
                    Quality = dfind.Quality,
                    TimeStamp = DateTime.Now
                });
                // dfind.OptimalAzimuth = _dfBearingStatistics.MaxProbability;
                if (_optimalAzimuth >= 0)
                {
                    var opt = _optimalAzimuth - RunningInfo.BufCompassData.Heading;
                    dfind.OptimalAzimuth = (opt + 360) % 360;
                }
            }
            else
            {
                data.Remove(dfind);
            }
        }

        SendData(data);
    }

    private void DfBearingStatistics_AngleStatisticsChanged(object sender, BearingStatisticsArgs e)
    {
    }

    private void DfBearingStatistics_ProbabilityChanged(object sender, BearingStatisticsArgs e)
    {
        _optimalAzimuth = (e.MaxProbability + 360) % 360;
    }
}