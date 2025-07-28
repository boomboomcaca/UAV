using Magneto.Protocol.Define;

namespace Magneto.Device.N9030A;

public partial class N9030A
{
    /// <summary>
    ///     获取监测数据的线程
    /// </summary>
    /// <param name="obj">启动标志</param>
    private void CollectData(object obj)
    {
        var media = (FeatureType)obj;
        _prevAbility = media;
        while (!_dataTokenSource.IsCancellationRequested)
            try
            {
                if ((media & FeatureType.FFM) > 0)
                {
                    SendLevel();
                    if (SpectrumSwitch) SendSpectrum();
                    //SendITU();
                }

                if ((media & FeatureType.SCAN) > 0) SendScan();
                if ((media & FeatureType.MScan) > 0) SendMscan();
                if ((media & FeatureType.FASTEMT) > 0) ReadSpectrumScanData();
            }
            catch
            {
            }
    }
}