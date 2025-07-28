using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Magneto.Device.AP510;

public partial class Ap510
{
    private void ReceiveCmdResult()
    {
        while (_cmdReceiveCts?.IsCancellationRequested == false)
        {
            var buffer = new byte[Buffersize];
            try
            {
                var cnt = _cmdSocket.Receive(buffer);
                if (cnt > 0)
                    for (var i = 0; i < cnt; i++)
                        _cmdQueue.Enqueue(buffer[i]);
                else
                    Thread.Sleep(50);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"SingleCall通道接收数据异常，异常信息：{ex}");
#endif
            }
        }
    }

    private void ReceiveData()
    {
        while (_dataReceiveCts?.IsCancellationRequested == false)
        {
            var buffer = new byte[Buffersize];
            try
            {
                var cnt = _dataSocket.Receive(buffer);
                if (cnt > 0)
                    for (var i = 0; i < cnt; i++)
                        _dataQueue.Enqueue(buffer[i]);
                else
                    Thread.Sleep(50);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"ServiceCall通道接收数据异常，异常信息：{ex}");
#endif
            }
        }
    }

    private void ProcessCmdResult()
    {
        var headOffset = -1;
        var loadLen = 0;
        var dataLen = 0;
        var listDataLen = new List<byte>();
        var list = new List<byte>();
        while (_cmdProcessCts?.IsCancellationRequested == false)
            try
            {
                var b = _cmdQueue.TryDequeue(out var bt);
                if (!b)
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (headOffset < 0)
                {
                    if (bt == '#')
                    {
                        headOffset = 0;
                        loadLen = 0;
                        dataLen = 0;
                        list.Clear();
                        listDataLen.Clear();
                    }

                    continue;
                }

                headOffset++;
                if (headOffset == 1)
                {
                    loadLen = bt - '0';
                }
                else if (headOffset > 1 && headOffset <= loadLen + 1)
                {
                    listDataLen.Add(bt);
                    if (headOffset == loadLen + 1)
                    {
                        var s = _encodingDefault.GetString(listDataLen.ToArray());
                        if (!int.TryParse(s, out dataLen)) dataLen = 0;
                    }
                }
                else if (dataLen > 0 && loadLen > 0 && headOffset == loadLen + dataLen + 1)
                {
                    list.Add(bt);
                    var s = _encodingDefault.GetString(list.ToArray());
                    Trace.WriteLine($"==> {s}");
                    _cmdCache.Enqueue(s);
                    headOffset = -1;
                }
                else
                {
                    list.Add(bt);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"解析SingleCall通道数据异常，异常信息：{ex}");
#endif
            }
    }

    private void ProcessData()
    {
        var headOffset = -1;
        var loadLen = 0;
        var dataLen = 0;
        var listDataLen = new List<byte>();
        var list = new List<byte>();
        while (_dataProcessCts?.IsCancellationRequested == false)
            try
            {
                var b = _dataQueue.TryDequeue(out var bt);
                if (!b)
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (headOffset < 0)
                {
                    if (bt == '#')
                    {
                        headOffset = 0;
                        loadLen = 0;
                        dataLen = 0;
                        list.Clear();
                        listDataLen.Clear();
                    }

                    continue;
                }

                headOffset++;
                if (headOffset == 1)
                {
                    loadLen = bt - '0';
                }
                else if (headOffset > 1 && headOffset <= loadLen + 1)
                {
                    listDataLen.Add(bt);
                    if (headOffset == loadLen + 1)
                    {
                        var s = _encodingDefault.GetString(listDataLen.ToArray());
                        if (!int.TryParse(s, out dataLen)) dataLen = 0;
                    }
                }
                else if (dataLen > 0 && loadLen > 0 && headOffset == loadLen + dataLen + 1)
                {
                    list.Add(bt);
                    var data = ScpiDataStruct.Parse(loadLen, dataLen, list.ToArray(), Encoding.Default);
                    _dataCache.Enqueue(data);
                    headOffset = -1;
                }
                else
                {
                    list.Add(bt);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"解析ServiceCall通道数据异常，异常信息：{ex}");
#endif
            }
    }

    private static short[] ToDataBlocks(Dictionary<string, ScpiMapItem> dic, string key)
    {
        if (!dic.TryGetValue(key, out var data)) return null;
        if (data is not ScpiArrayMapItem item) return null;
        try
        {
            var bytes = item.Data;
            var array = new short[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
            return array;
        }
        catch
        {
            return null;
        }
    }

    private static float ToSingle(Dictionary<string, ScpiMapItem> dic, string key, float defaultValue = float.NaN)
    {
        if (!dic.TryGetValue(key, out var data)) return defaultValue;
        if (data is not ScpiStringMapItem item) return defaultValue;
        var s = item.Data;
        if (!float.TryParse(s, out var val)) return defaultValue;
        return val;
    }

    private static double ToDouble(Dictionary<string, ScpiMapItem> dic, string key, double defaultValue = double.NaN)
    {
        if (!dic.TryGetValue(key, out var data)) return defaultValue;
        if (data is not ScpiStringMapItem item) return defaultValue;
        var s = item.Data;
        if (!double.TryParse(s, out var val)) return defaultValue;
        return val;
    }

    private static string ToValueString(Dictionary<string, ScpiMapItem> dic, string key)
    {
        if (!dic.TryGetValue(key, out var data)) return null;
        if (data is not ScpiStringMapItem item) return null;
        var s = item.Data;
        return s;
    }
}