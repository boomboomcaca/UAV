using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SSE;

public partial class Sse : DriverBase
{
    public Sse(Guid driverId) : base(driverId)
    {
    }

    public override void OnData(List<object> data)
    {
        //如果有且只有音频数据，则直接发送
        //若有GPS和Compass数据则缓存，没有则取缓存发送
        if (data.All(item => item is SDataAudio))
        {
            SendData(data);
            return;
        }

        var sse = (SDataSse)data.Find(item => item is SDataSse);
        if (sse != null)
        {
            CanPause = true;
            var zoom = (float)sse.Data.Length / 360;
            // 这里不需要发送正北示向度
            // var newData = new float[sse.Data.Length];
            // for (int i = 0; i < sse.Data.Length; i++)
            // {
            //     if ((i + calib) >= sse.Data.Length)
            //     {
            //         newData[i + calib - sse.Data.Length] = sse.Data[i];
            //     }
            //     else
            //     {
            //         newData[i + calib] = sse.Data[i];
            //     }
            // }
            // sse.Data = newData;
            var results = PeakAlgorithm.GetPeak(sse.Data, sse.AzimuthCount);
            sse.Results = results.Select(i => (i / zoom + 360) % 360).ToArray();
            // var str = "";
            // foreach (var item in sse.Results)
            // {
            //     str += $"{item}°,";
            // }
            // Console.WriteLine($"估算示向度：{str}");
        }

        SendData(data);
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency
            && double.TryParse(value.ToString(), out var freq)
            && AntennaController is IAntennaController controller)
            controller.Frequency = freq;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        CanPause = false;
        if (!base.Start(dataPort, mediaType)) return false;
        try
        {
            (DFinder as DeviceBase)?.Start(FeatureType.SSE, this);
            if (Receivers?.Length > 0)
                foreach (var recv in Receivers)
                {
                    if (Equals(recv, DFinder)) continue;
                    (recv as DeviceBase)?.Start(FeatureType.IFOUT, this);
                }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool Stop()
    {
        (DFinder as DeviceBase)?.Stop();
        if (Receivers?.Length > 0)
            foreach (var recv in Receivers)
            {
                if (Equals(recv, DFinder)) continue;
                (recv as DeviceBase)?.Stop();
            }

        return base.Stop();
    }
}