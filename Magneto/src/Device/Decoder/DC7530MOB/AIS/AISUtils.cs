/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\DC7530MOB\AISUtils.cs
 *
 * 作    者:    王 喜 进
 *
 * 创作日期:    2019-3-12
 *
 * 备    注:	AIS信息处理综合类，增加此类，将原有的模块移植过来，不改变原有的代码。
 *
 *********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

/// <summary>
///     AIS 综合类
/// </summary>
internal sealed class AisUtils : IDisposable
{
    /// <summary>
    ///     存储设备收到的船舶数据，只存储AIVDM类型包数据
    /// </summary>
    private readonly ConcurrentQueue<string> _data;

    /// <summary>
    ///     AIS数据解析类
    /// </summary>
    private readonly AisParser _parse;

    private bool _disposed;

    /// <summary>
    ///     发送AIS数据线程
    /// </summary>
    private Task _sendTask;

    private CancellationTokenSource _sendTokenSource;

    /// <summary>
    ///     构造函数
    /// </summary>
    public AisUtils()
    {
        _data = new ConcurrentQueue<string>();
        _parse = new AisParser();
    }

    /// <summary>
    ///     清理资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AisUtils()
    {
        Dispose(false);
    }

    /// <summary>
    ///     初始化
    /// </summary>
    public void Initilize()
    {
        _sendTokenSource = new CancellationTokenSource();
        _sendTask = new Task(SendAisDataProcess, _sendTokenSource.Token);
        _sendTask.Start();
    }

    /// <summary>
    ///     入队列船舶数据
    /// </summary>
    /// <param name="data">AIS船舶数据</param>
    public void Enqueue(string data)
    {
        _data.Enqueue(data);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        _data.Clear();
        if (disposing)
        {
        }

        CancelTask(_sendTask, _sendTokenSource);
        _disposed = true;
    }

    /// <summary>
    ///     发送并处理AIS数据
    /// </summary>
    private void SendAisDataProcess()
    {
        while (_sendTokenSource?.IsCancellationRequested == false)
        {
            Thread.Sleep(1);
            var fragments = new List<string>();
            try
            {
                if (_data.Count > 9)
                {
                    if (!_data.TryDequeue(out var fragment)) continue;
                    fragments.Add(fragment);
                    var count = int.Parse(fragments[0].Split(',')[1]);
                    if (count > 1)
                    {
                        if (_data.Count >= count)
                        {
                            var identifier = fragments[0].Split(',')[3];
                            for (var i = 1; i < count; i++)
                            {
                                if (!_data.TryPeek(out var temp)) continue;
                                var tempIdentifier = temp.Split(',')[3];
                                if (identifier != tempIdentifier)
                                {
                                    //如果identifier不相等，则删除前一条数据，有可能是上一条包含多条数据的后面一条数据
                                    fragments.RemoveAt(fragments.Count - 1);
                                    continue;
                                }

                                if (!_data.TryDequeue(out var item)) continue;
                                fragments.Add(item);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                string encodedMsg = null;
                if (fragments.Count > 1)
                {
                    encodedMsg = Combination(fragments.ToArray());
                }
                else
                {
                    //有可能在一条不正确数据删除之后fragments长度为0
                    if (fragments.Count > 0)
                        encodedMsg = fragments[0];
                    else
                        continue;
                }

                var data = new List<object>();
                IAisMessage ais = null;
                try
                {
                    if (encodedMsg != null)
                    {
                        if (encodedMsg.Contains("!AIVDM") || encodedMsg.Contains("!AIVDO"))
                            ais = _parse.Parse(encodedMsg);
                        else
                            ais = DecodeMsgBody(encodedMsg);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"AIS 消息解码错误!消息为：{encodedMsg}\r\n{ex.Message}");
                    continue;
                }

                if (ais == null)
                    continue;
                data = CollectAisData(ais);
                if (data is { Count: > 0 })
                {
                    // 通过消息通道发送AIS数据
                    //SendMessage(MessageDomain.Network, MessageType.AIS, data[0]);
                    //_7530mob.SendMessageEx(MessageDomain.Network, MessageType.AIS, data[0]);
                }
            }
            catch
            {
                // 程序在第一个处理由于数据堆积可能会导致异常，此异常不做处理   王喜进 2019.3.21
            }
        }
    }

    /// <summary>
    ///     直接返回供解码的解码字符串，不包含消息头等信息
    /// </summary>
    /// <param name="param">输入标准的编码字符串字符数组</param>
    /// <returns></returns>
    private string Combination(params string[] param)
    {
        if (param.Length > 9)
        {
            Trace.WriteLine("AVIDM,AVIDO格式最多支持九个子句子！！");
            return null;
        }

        if (param.Any(_ => param[0].Split(',').Length < 7)) return null;
        var sentences = new List<Avidm>();
        try
        {
            foreach (var item in param)
            {
                var temp = item.Split(',');
                sentences.Add(new Avidm(temp[0], int.Parse(temp[1]), int.Parse(temp[2]), temp[3], temp[4], temp[5],
                    temp[6]));
            }
        }
        catch
        {
            return null;
        }

        sentences.Sort(
            delegate(Avidm item1, Avidm item2)
            {
                var flag = item1.SegCount.CompareTo(item2.SegCount);
                return flag == 0 ? item1.Fragment.CompareTo(item2.Fragment) : flag;
            });
        var sb = new StringBuilder();
        for (var j = 0; j < sentences[0].SegCount - 1; j++)
        {
            sb.Append(sentences[j].Body);
            while (sentences[j + 1].SegCount == sentences[j].SegCount &&
                   sentences[j + 1].Fragment == sentences[j].Fragment + 1)
            {
                sb.Append(sentences[j + 1].Body);
                j++;
                if (j >= sentences[j].SegCount - 1) break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     直接解码编码消息体（不包含消息头等信息）
    /// </summary>
    /// <param name="decodeStr"></param>
    /// <returns></returns>
    private IAisMessage DecodeMsgBody(string decodeStr)
    {
        IAisMessage ais = null;
        ais = AisDecoder.Decode(decodeStr);
        return ais;
    }

    /// <summary>
    ///     解析AIS数据
    /// </summary>
    /// <param name="ais"></param>
    /// <returns></returns>
    private List<object> CollectAisData(IAisMessage ais)
    {
        var data = new List<object>();
        if (ais is AisBroadcast)
            return null;
        data.Add(ais);
        return data;
    }

    private void CancelTask(Task task, CancellationTokenSource tokenSource)
    {
        if (tokenSource == null) return;
        try
        {
            if (task?.IsCompleted == false) tokenSource.Cancel();
        }
        catch (AggregateException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            tokenSource.Dispose();
        }
    }
}