using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device.N9917A;

public partial class N9917A
{
    private void SendLevel()
    {
        var result = string.Empty;
        var data = new List<object>();
        if (_spectrumSwitch)
        {
            SendCmd("INIT:IMM;*OPC?");
            result = RecvResult('\n');
            if (!result.Equals("1\n"))
                return;
        }

        if (_getLevel.Equals("PEAK")) SendCmd("CALC:MARK1:FUNC:MAX"); //将marker移到峰值
        SendCmd("CALC:MARK1:Y?");
        result = RecvResult('\n');
        if (result != "")
        {
            var level = float.Parse(result);
            if (level is > -999f and < 1000f)
            {
                var generalLevel = new SDataLevel
                {
                    Bandwidth = _ifBandwidth,
                    Frequency = _frequency,
                    Data = level
                };
                data.Add(generalLevel);
            }

            if (_isRunning)
                SendData(data);
        }
    }

    private void SendSpectrum()
    {
        try
        {
            var data = new List<object>();
            GetSpectrum(out var spectrumData);
            if (spectrumData == null) return;
            var generalSpectrum = new SDataSpectrum
            {
                Span = _ifBandwidth,
                Frequency = _frequency,
                Data = Array.ConvertAll(spectrumData, item => (short)(item * 10))
            };
            data.Add(generalSpectrum);
            if (_isRunning)
                SendData(data);
        }
        catch
        {
        }
    }

    private void SendScanData()
    {
        var count = Utils.GetTotalCount(_startFrequency, _stopFrequency, StepFrequency);
        var sdScan = new SDataScan
        {
            StartFrequency = _startFrequency,
            StepFrequency = StepFrequency,
            StopFrequency = _stopFrequency,
            Offset = 0,
            Total = count
        };
        var startFreq = _startFrequency;
        while (_isRunning)
            if (count - 10001 <= 0)
            {
                SendCmd("SENS:FREQ:STAR " + startFreq + "MHz");
                SendCmd("SENS:FREQ:STOP " + _stopFrequency + "MHz");
                SendCmd("SENS:SWE:POIN " + count);
                var spData = new List<object>();
                GetSpectrum(out var spectrumData);
                if (spectrumData != null && spectrumData.Length == count)
                {
                    sdScan.Data = Array.ConvertAll(spectrumData, item => (short)(item * 10));
                    spData.Add(sdScan);
                    if (_isRunning)
                    {
                        SendData(spData);
                        break;
                    }
                }
            }
            else
            {
                count -= 10001;
                SendCmd("SENS:FREQ:STAR " + startFreq + "MHz");
                var tempStop = startFreq + StepFrequency / 1000 * 10000;
                SendCmd("SENS:FREQ:STOP " + tempStop + "MHz");
                SendCmd("SENS:SWE:POIN " + "10001");
                GetSpectrum(out var spectrumData);
                var spData = new List<object>();
                if (spectrumData is { Length: 10001 })
                {
                    sdScan.Data = Array.ConvertAll(spectrumData, item => (short)(item * 10));
                    spData.Add(sdScan);
                    if (_isRunning)
                    {
                        SendData(spData);
                        startFreq = tempStop + StepFrequency;
                    }
                }
            }
    }

    private void GetSpectrum(out float[] spectrumData)
    {
        spectrumData = null;
        var result = string.Empty;
        if (_spectrumSwitch)
        {
            SendCmd("INIT:IMM;*OPC?");
            result = RecvResult('\n');
            if (!result.Equals("1\n"))
                return;
        }

        SendCmd("TRAC:DATA?;*OPC?");
        result = RecvResult('\n');
        if (result != "")
            if (result.Split(';')[1] == "1\n")
            {
                var temp = result.Split(';')[0].Split(',');
                try
                {
                    spectrumData = Array.ConvertAll(temp,
                        float.Parse);
                }
                catch
                {
                    spectrumData = null;
                }
            }
    }

    private string RecvResult(int endflag)
    {
        var total = 0;
        var buffer = new byte[1024 * 1024];
        var result = string.Empty;
        while (_cmdSock.Receive(buffer, total, 1, SocketFlags.None) > 0)
            if (buffer[total++] == endflag)
                break;
        result = Encoding.ASCII.GetString(buffer, 0, total);
        buffer = null;
        return result;
    }
}