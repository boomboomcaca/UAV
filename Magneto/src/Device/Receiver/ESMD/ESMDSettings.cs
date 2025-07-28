using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMD;

public partial class Esmd
{
    #region IFStep/Span

    private void SetSpan(double dstSpan)
    {
        var strIfStep = SendSyncCmd("CALC:IFP:STEP?");
        var ifstep = double.Parse(strIfStep) / 1000.0d;
        var spansTemp = GetSpans(ifstep);
        if (spansTemp.Contains(dstSpan))
        {
            SendCmd($"FREQ:SPAN {dstSpan}kHz");
            return;
        }

        if (dstSpan < spansTemp[0])
        {
            SendCmd($"FREQ:SPAN {spansTemp[0]}kHz");
            Thread.Sleep(10);
            var stepsTemp = GetIfSteps(spansTemp[0]);
            SendCmd($"CALC:IFP:STEP {stepsTemp[0]}kHz");
            Thread.Sleep(10);
            SetSpan(dstSpan);
        }
        else if (dstSpan > spansTemp[^1])
        {
            SendCmd($"FREQ:SPAN {spansTemp[^1]}kHz");
            Thread.Sleep(10);
            var stepsTemp = GetIfSteps(spansTemp[^1]);
            SendCmd($"CALC:IFPAN:STEP {stepsTemp[^1]}kHz");
            Thread.Sleep(10);
            SetSpan(dstSpan);
        }
    }

    private void SetIfStep(double dstIfStep)
    {
        var strSpan = SendSyncCmd("FREQ:SPAN?");
        var span = double.Parse(strSpan) / 1000;
        var stepsTemp = GetIfSteps(span);
        if (stepsTemp.Contains(dstIfStep))
        {
            SendCmd($"CALC:IFP:STEP {dstIfStep}kHz");
            return;
        }

        if (dstIfStep < stepsTemp[0])
        {
            SendCmd($"CALC:IFP:STEP {stepsTemp[0]}kHz");
            Thread.Sleep(10);
            var spansTemp = GetSpans(stepsTemp[0]);
            SendCmd($"FREQ:SPAN {spansTemp[0]}kHz");
            Thread.Sleep(10);
            SetIfStep(dstIfStep);
        }
        else if (dstIfStep > stepsTemp[^1])
        {
            SendCmd($"CALC:IFP:STEP {stepsTemp[^1]}kHz");
            Thread.Sleep(10);
            var spansTemp = GetSpans(stepsTemp[^1]);
            SendCmd($"FREQ:SPAN {spansTemp[^1]}KHz");
            Thread.Sleep(10);
            SetIfStep(dstIfStep);
        }
    }

    private double[] GetSpans(double ifstep)
    {
        var idstep = Array.IndexOf(Consts.ArrayStep, ifstep);
        var isMatch = Consts.Spans.TryGetValue(idstep, out var value);
        if (!isMatch) return null;
        var result = new double[value.end - value.start + 1];
        Array.Copy(Consts.ArraySpan, value.start, result, 0, result.Length);
        return result;
    }

    private double[] GetIfSteps(double span)
    {
        var idspan = Array.IndexOf(Consts.ArraySpan, span);
        var isMatch = Consts.IfSteps.TryGetValue(idspan, out var value);
        if (!isMatch) return null;
        var result = new double[value.end - value.start + 1];
        Array.Copy(Consts.ArrayStep, value.start, result, 0, result.Length);
        return result;
    }

    private double GetDefaultStep(double span)
    {
        var idspan = Array.IndexOf(Consts.ArraySpan, span);
        var isMatch = Consts.DefaultSteps.TryGetValue(idspan, out var idstep);
        if (!isMatch || idstep >= Consts.ArrayStep.Length) return 0;
        return Consts.ArrayStep[idstep];
    }

    #endregion

    #region IFBandwidth/DemMode

    /// <summary>
    ///     单位 kHz
    /// </summary>
    /// <param name="dstFilterBandwidth"></param>
    private void SetFilterBandwidth(double dstFilterBandwidth)
    {
        if (dstFilterBandwidth > 9)
        {
            //当解调模式为CW,USB,LSB时,解调带宽只能<=9kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode is "CW" or "USB" or "LSB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }
        else if (dstFilterBandwidth < 1)
        {
            //当解调模式为ISB时,解调带宽只能>=1kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode == "ISB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:BAND {dstFilterBandwidth} kHz");
    }

    private void SetDemodulation(Modulation dstDemMode)
    {
        if (dstDemMode is Modulation.Cw or Modulation.Usb or Modulation.Lsb or Modulation.Isb)
        {
            //当解调带宽 > 9kHz时，解调带宽不能设置为CW,USB,LSB
            //当解调带宽 < 1kHz时，解调带宽不能设置为ISB
            var strIfBandwidth = SendSyncCmd("SENS:BAND?");
            var ifBandwidth = double.Parse(strIfBandwidth) / 1000;
            if (dstDemMode == Modulation.Isb)
            {
                if (ifBandwidth < 1)
                {
                    SendCmd("SENS:BAND 1 kHz");
                    Thread.Sleep(10);
                }
            }
            else if (ifBandwidth > 9)
            {
                SendCmd("SENS:BAND 9 kHz");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:DEM {dstDemMode}");
    }

    #endregion

    #region 获取TV图片

    private void StartGetTvBmp()
    {
        if (CurFeature != FeatureType.FFM) return;
        if (_demMode != Modulation.Tv) return;
        ThreadPool.QueueUserWorkItem(_ => GetTvBmpAsync().ConfigureAwait(false), null);
    }

    private async Task GetTvBmpAsync()
    {
        var url = $"http://{Ip}/tv.bmp";
        while (TaskState == TaskState.Start)
            try
            {
                var time = DateTime.Now;
                var buffer = await DownloadPictureAsync(url);
                //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tv.bmp");
                //File.WriteAllBytes(path, buffer);
                var data = new SDataTvImage
                {
                    Frequency = _frequency,
                    Bandwidth = _ifBandwidth,
                    Time = time,
                    Data = buffer
                };
                var list = new List<object>
                {
                    data
                };
                SendData(list);
            }
            catch
            {
            }
    }

    /// <summary>
    ///     下载图片
    /// </summary>
    /// <param name="picUrl">图片Http地址</param>
    /// <param name="timeOut">ms Request最大请求时间，如果为-1则无限制</param>
    public async Task<byte[]> DownloadPictureAsync(string picUrl, int timeOut = -1)
    {
        byte[] value = null;
        Stream stream = null;
        try
        {
            var handler = new HttpClientHandler
            {
                //设置是否发送凭证信息，有的服务器需要验证身份，不是所有服务器需要
                UseDefaultCredentials = false
            };
            var client = new HttpClient(handler);
            if (timeOut != -1) client.Timeout = TimeSpan.FromMilliseconds(timeOut);
            stream = await client.GetStreamAsync(picUrl);
            value = SaveBinaryFile(stream);
        }
        finally
        {
            if (stream != null) await stream.DisposeAsync();
        }

        return value;
    }

    private static byte[] SaveBinaryFile(Stream inStream)
    {
        var buffer = new byte[1024];
        Stream outStream = null;
        try
        {
            outStream = new MemoryStream();
            int l;
            do
            {
                l = inStream.Read(buffer, 0, buffer.Length);
                if (l > 0) outStream.Write(buffer, 0, l);
            } while (l > 0);

            var length = outStream.Length;
            var value = new byte[length];
            outStream.Position = 0;
            outStream.Read(value, 0, (int)length);
            return value;
        }
        catch
        {
            return null;
        }
        finally
        {
            outStream?.Close();
            inStream?.Close();
        }
    }

    #endregion
}