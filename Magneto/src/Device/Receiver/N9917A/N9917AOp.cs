using Magneto.Protocol.Define;

namespace Magneto.Device.N9917A;

public partial class N9917A
{
    private void SendMornitorData()
    {
        while (_isRunning)
            if ((CurFeature & FeatureType.FFM) > 0)
            {
                if (LevelSwitch) SendLevel();
                if (_spectrumSwitch)
                {
                    SendCmd("SWE:POIN?");
                    int.TryParse(RecvResult('\n'), out var pointCount);
                    if (pointCount == Points) SendSpectrum();
                }
            }
            else if ((CurFeature & FeatureType.SCAN) > 0)
            {
                SendScanData();
            }
    }
}