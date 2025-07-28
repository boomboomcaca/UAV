using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.PR100;

public partial class Pr100
{
    private void CapturePacket(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        var socket = _dataSock;
        var queue = _udpDataQueue;
        var buffer = new byte[1024 * 1024];
        socket.ReceiveBufferSize = buffer.Length;
        while (!token.IsCancellationRequested)
            try
            {
                var receivedCount = socket.Receive(buffer);
                if (receivedCount <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var receivedBuffer = new byte[receivedCount];
                Buffer.BlockCopy(buffer, 0, receivedBuffer, 0, receivedCount);
                if (TaskState == TaskState.Start) queue.Enqueue(receivedBuffer);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    private void DispatchPacket(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        var queue = _udpDataQueue;
        while (!token.IsCancellationRequested)
            try
            {
                if (queue.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                queue.TryDequeue(out var buffer);
                if (buffer == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var packet = RawPacket.Parse(buffer, 0);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardPacket(packet);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine(e.ToString());
#endif
            }
    }

    private void ForwardPacket(RawPacket packet)
    {
        if (packet == null) return;
        var result = new List<object>();
        object obj = null;
        foreach (var data in packet.DataCollection)
        {
            switch ((DataType)data.Tag)
            {
                case DataType.Audio:
                    obj = ToAudio(data as RawAudio);
                    break;
                case DataType.Ifpan:
                    obj = ToSpectrum(data as RawIfPan);
                    break;
                case DataType.If:
                    obj = ToIq(data as RawIf);
                    break;
                case DataType.Fscan:
                    obj = ToFScan(data as RawFScan);
                    break;
                case DataType.Mscan:
                    obj = ToMScan(data as RawMScan);
                    break;
                case DataType.Pscan:
                    obj = ToPScan(data as RawPScan);
                    break;
            }

            if (obj != null)
            {
                if (obj is List<object> list)
                    result.AddRange(list);
                else
                    result.Add(obj);
            }
        }

        result = result.Where(item => item != null).ToList();
        if (result.Count > 0 && TaskState == TaskState.Start)
            //result.Add(_deviceId);
            SendData(result);
    }

    private void DispatchLevelData(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送，否则界面参数列表刷新时也会出现卡顿
        //此处处理为2秒左右发送一次到客户端
        while (!token.IsCancellationRequested)
            try
            {
                if ((_mediaType & (MediaType.Level | MediaType.Itu)) == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var datas = new List<object>();
                //获取电平数据和ITU数据
                var result = SendSyncCmd("SENS:DATA?");
                var values = result.Split(',');
                if ((_mediaType & MediaType.Level) > 0)
                {
                    var level = float.Parse(values[0]);
                    var datalevel = new SDataLevel
                    {
                        Frequency = _frequency,
                        Bandwidth = _filterBandwidth,
                        Data = level
                    };
                    datas.Add(datalevel);
                }

                if (datas.Count > 0 && TaskState == TaskState.Start) SendData(datas);
                Thread.Sleep(200);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    #region 解析业务数据

    /// <summary>
    ///     解析音频数据
    /// </summary>
    private object ToAudio(RawAudio data)
    {
        if (data == null) return null;
        var audio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = 32 * 1000,
            BytesPerSecond = 64 * 1000,
            BitsPerSample = 16,
            BlockAlign = 2,
            Channels = 1,
            Data = data.DataCollection
        };
        if (data.ChannelNumber > 0)
        {
            var list = new List<object> { audio };
            return new SDataDdc
            {
                ChannelNumber = data.ChannelNumber - 1,
                Data = list
            };
        }

        return audio;
    }

    /// <summary>
    ///     解析频谱数据
    /// </summary>
    private object ToSpectrum(RawIfPan data)
    {
        if (data == null) return null;
        var spectrum = new short[data.NumberOfTraceItems];
        for (var i = 0; i < spectrum.Length; i++)
        {
            spectrum[i] = data.DataCollection[i];
            if (spectrum[i] > 1200 || spectrum[i] < -9990) return null;
        }

        return new SDataSpectrum
        {
            Frequency = data.Frequency / 1000000d,
            Span = data.SpanFrequency / 1000d,
            Data = spectrum
        };
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    private object ToIq(RawIf data)
    {
        if (data == null) return null;
        return new SDataIq
        {
            Frequency = data.Frequency / 1000000d,
            Bandwidth = data.Bandwidth / 1000d,
            SamplingRate = data.Samplerate / 1000d,
            Attenuation = data.RxAtt,
            Data16 = Array.ConvertAll(data.DataCollection, item => (short)item)
        };
    }

    /// <summary>
    ///     解析FSCAN扫描数据
    /// </summary>
    private object ToFScan(RawFScan data)
    {
        if (data?.DataCollection == null) return null;
        var levels = data.DataCollection;
        var frequencies = new double[data.NumberOfTraceItems];
        for (var index = 0; index < data.NumberOfTraceItems; ++index)
            frequencies[index] = data.FreqCollection[index] / 1000000d;
        return CalculateData(frequencies, levels);
    }

    /// <summary>
    ///     解析PSCAN扫描数据
    /// </summary>
    private object ToPScan(RawPScan data)
    {
        if (data?.DataCollection == null) return null;
        var levels = new float[data.NumberOfTraceItems];
        var frequencies = new double[data.NumberOfTraceItems];
        var currIndex = 0;
        for (var index = 0; index < data.NumberOfTraceItems; ++index)
        {
            levels[index] = data.DataCollection[index] / 10.0f;
            frequencies[index] = data.FreqCollection[index] / 1000000d;
            currIndex = Utils.GetCurrIndex(frequencies[index], StartFrequency, StepFrequency);
        }

        return CalculateDscanData(levels.ToList(), currIndex);
    }

    /// <summary>
    ///     解析离散扫描数据
    /// </summary>
    private object ToMScan(RawMScan data)
    {
        if (data?.DataCollection == null) return null;
        var levels = data.DataCollection;
        var frequencies = new double[data.NumberOfTraceItems];
        for (var index = 0; index < data.NumberOfTraceItems; ++index)
            frequencies[index] = data.FreqCollection[index] / 1000000d;
        return CalculateData(frequencies, levels);
    }

    private object CalculateData(double[] frequencies, short[] levels)
    {
        var dataScan = new SDataScan
        {
            SegmentOffset = 0,
            StepFrequency = StepFrequency,
            Total = _cacheData.Length
        };
        if (ScanMode == ScanMode.MScan)
        {
            dataScan.StartFrequency = _scanFreqs[0];
            dataScan.StopFrequency = _scanFreqs.Last();
        }
        else
        {
            dataScan.StartFrequency = StartFrequency;
            dataScan.StopFrequency = StopFrequency;
        }

        var data = new List<object>();
        for (var i = 0; i < frequencies.Length; ++i)
        {
            var currIndex = ScanMode == ScanMode.MScan
                ? _scanFreqs.IndexOf(frequencies[i])
                : Utils.GetCurrIndex(frequencies[i], StartFrequency, StepFrequency);
            if (currIndex < 0 || currIndex >= _cacheData.Length) continue;
            //检查数据是否丢包，若有丢包则丢包部分取缓存数据发送
            int count;
            if (currIndex >= _index)
            {
                //非最后一包数据丢包
                count = currIndex - _index; //丢包点数
                dataScan.Data = new short[count + 1];
                if (count > 0) Array.Copy(_cacheData, _index, dataScan.Data, 0, count);
                dataScan.Data[count] = levels[i];
                dataScan.Offset = _index;
                data.Clear();
                data.Add(dataScan);
                if (TaskState == TaskState.Start) SendData(data);
            }
            else if (currIndex < _index)
            {
                //最后一包数据丢失
                //若currIndex大于0，则该情况很可能是本次扫描最后一包数据丢失，下次扫描第一包数据丢失
                //考虑到多频段扫描，此处只补充本次扫描数据，故统一处理为发送最后一包的缓存数据
                count = _cacheData.Length - _index; //丢包点数
                dataScan.Data = new short[count];
                Array.Copy(_cacheData, _index, dataScan.Data, 0, count);
                dataScan.Offset = _index;
                data.Clear();
                data.Add(dataScan);
                if (TaskState == TaskState.Start) SendData(data);
            }

            //更新索引和缓存数据
            _index = _index + dataScan.Data.Length == _cacheData.Length ? 0 : _index + dataScan.Data.Length;
            _cacheData[currIndex] = levels[i];
        }

        return null;
    }

    /// <summary>
    ///     数字扫描数据校验
    /// </summary>
    /// <param name="scandata">电平值</param>
    /// <param name="currIndex">本包数据索引</param>
    private object CalculateDscanData(List<float> scandata, int currIndex)
    {
        if (currIndex < 0 || currIndex >= _cacheData.Length) return null;
        //先检查本次扫描是否完成
        if (Math.Abs(scandata.Last() - 200) < 1e-9) //表示本次扫描数据完成
        {
            scandata.RemoveAt(scandata.Count - 1);
            //若本段扫描点数与软件框架计算的总点数少，则补0
            for (var i = currIndex + scandata.Count; i < _cacheData.Length; ++i) scandata.Add(0);
            //若本段扫描点数比总点数多，则去掉多余的点
            for (var i = currIndex + scandata.Count; i > _cacheData.Length; --i) scandata.RemoveAt(scandata.Count - 1);
        }

        var dataScan = new SDataScan
        {
            SegmentOffset = 0,
            StartFrequency = StartFrequency,
            StepFrequency = StepFrequency,
            StopFrequency = StopFrequency,
            Total = _cacheData.Length
        };
        var data = new List<object>();
        var count = 0; //丢包点数
        //检查数据是否丢包，若有丢包情况则丢包部分取缓存数据发送
        if (currIndex >= _index)
        {
            //非最后一包数据丢失情况
            count = currIndex - _index;
            dataScan.Data = new short[count + scandata.Count];
            if (count > 0) Array.Copy(_cacheData, _index, dataScan.Data, 0, count);
            Array.Copy(scandata.ToArray(), 0, dataScan.Data, count, scandata.Count);
            dataScan.Offset = _index;
            data.Clear();
            data.Add(dataScan);
            if (TaskState == TaskState.Start) SendData(data);
        }
        else if (currIndex < _index)
        {
            //最后一包数据丢失
            //若currIndex大于0，则该情况很可能是本次扫描最后一包数据丢失，下次扫描第一包数据丢失
            //考虑到多频段扫描，此处只补充本次扫描数据，故统一处理为发送最后一包的缓存数据
            count = _cacheData.Length - _index;
            dataScan.Data = new short[count];
            Array.Copy(_cacheData, _index, dataScan.Data, 0, count);
            dataScan.Offset = _index;
            data.Clear();
            data.Add(dataScan);
            if (TaskState == TaskState.Start) SendData(data);
        }

        //更新索引和缓存数据
        _index = _index + dataScan.Data.Length == _cacheData.Length ? 0 : _index + dataScan.Data.Length;
        Array.Copy(scandata.ToArray(), 0, _cacheData, currIndex, scandata.Count);
        return null;
    }

    #endregion
}