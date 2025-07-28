using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.AIS;
using Magneto.Contract.AIS.Interface;
using Magneto.Contract.BaseClass;
using Magneto.Device.Parser;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class Ais : DeviceBase
{
    private readonly ConcurrentDictionary<int, AisData> _aisCache = new();

    /// <summary>
    ///     存储设备收到的船舶数据，只存储AIVDM类型包数据
    /// </summary>
    private readonly ConcurrentQueue<string> _data = new();

    /// <summary>
    ///     AIS数据解析类
    /// </summary>
    private readonly AisParser _parse = new();

    private CancellationTokenSource _cts;

    /// <summary>
    ///     AIS数据接收socket
    /// </summary>
    private Socket _recvSoc;

    private Task[] _taskArray;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="guid"></param>
    public Ais(Guid guid) : base(guid)
    {
    }

    /// <summary>
    ///     初始化模块
    /// </summary>
    /// <param name="moduleInfo"></param>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            // 清除资料
            ClearResouce();
            // 初始化网路套接字
            InitSocket();
            // 初始化线程
            InitThread();
            SetHeartBeat(_recvSoc);
        }

        return result;
    }

    /// <summary>
    ///     清理资源
    /// </summary>
    public override void Dispose()
    {
        ClearResouce();
        base.Dispose();
    }

    /// <summary>
    ///     初始化网络套接字
    /// </summary>
    private void InitSocket()
    {
        _recvSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _recvSoc.NoDelay = true;
        _recvSoc.Connect(endPoint);
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitThread()
    {
        _cts = new CancellationTokenSource();
        _taskArray = new[]
        {
            RecvAisDataAsync(_cts.Token),
            SendAisDataProcessAsync(_cts.Token)
        };
    }

    /// <summary>
    ///     发送并处理AIS数据
    /// </summary>
    /// <param name="obj"></param>
    private async Task SendAisDataProcessAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token).ConfigureAwait(false);
            var fragments = new List<string>();
            // if (_data.Count <= 9)
            // {
            //     continue;
            // }
            if (!_data.TryDequeue(out var str)) continue;
            fragments.Add(str);
            try
            {
                var count = int.Parse(fragments[0].Split(',')[1]);
                if (count > 1)
                {
                    if (_data.Count < count) continue;
                    var identifier = fragments[0].Split(',')[3];
                    for (var i = 1; i < count; i++)
                    {
                        _data.TryPeek(out var temp);
                        var tempIdentifier = temp?.Split(',')[3];
                        if (identifier != tempIdentifier)
                        {
                            //如果identifier不相等，则删除前一条数据，有可能是上一条包含多条数据的后面一条数据
                            fragments.RemoveAt(fragments.Count - 1);
                            continue;
                        }

                        _data.TryDequeue(out var d);
                        fragments.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送AIS数据Error:{ex.Message}");
            }

            string encodedMsg;
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
                Console.WriteLine($"AIS 消息解码错误!消息为:{ex.Message}");
            }

            if (ais == null) continue;
            _aisCache.AddOrUpdate(ais.Mmsi,
                m =>
                {
                    var d = new AisData
                    {
                        Mmsi = m
                    };
                    if (ais is IAisStaticData s)
                        d.StaticData = s;
                    else if (ais is IAisPosition p) d.PositionMessage = p;
                    return d;
                },
                (m, o) =>
                {
                    o.Mmsi = m;
                    if (ais is IAisStaticData s)
                    {
                        o.StaticData = s;
                        Console.WriteLine(
                            $"{DateTime.Now:HH:mm:ss.fff}    解码结果:{ais.Mmsi},{ais.MsgId},CallSign:{s.CallSign},ShipName:{s.ShipName}");
                    }
                    else if (ais is IAisPosition p)
                    {
                        o.PositionMessage = p;
                        Console.WriteLine(
                            $"{DateTime.Now:HH:mm:ss.fff}    解码结果:{ais.Mmsi},{ais.MsgId},Lat:{p.Latitude},Lng:{p.Longitude},航速:{p.Sog},航向:{p.Cog}");
                    }

                    return o;
                }
            );
            if (_aisCache.TryGetValue(ais.Mmsi, out var aisp))
            {
                var data = aisp.ToAis();
                if (data != null)
                    // 通过消息通道发送AIS数据
                    SendMessageData(new List<object> { data });
            }
        }
    }

    /// <summary>
    ///     解析AIS数据
    /// </summary>
    /// <param name="ais"></param>
    private List<object> CollectAisData(IAisMessage ais)
    {
        var data = new List<object>();
        try
        {
            if (ais.GetType().Name.Equals("AISPositionA"))
            {
                //A类船舶数据
                var aisA = (AisPositionA)ais;
                data.Add(aisA);
            }
            else if (ais.GetType().Name.Equals("AISBaseStation"))
            {
                //固定基站数据报告
                var aisBase = (AisBaseStation)ais;
                data.Add(aisBase);
            }
            else if (ais.GetType().Name.Equals("AISStaticAVoyage"))
            {
                //静态航向相关数据
                var aisSv = (AisStaticAVoyage)ais;
                data.Add(aisSv);
            }
            else if (ais.GetType().Name.Equals("AISPositionB"))
            {
                //标准B类船舶数据
                var aisB = (AisPositionB)ais;
                data.Add(aisB);
            }
            else if (ais.GetType().Name.Equals("AISPositionExtB"))
            {
                //扩展B类船舶位置数据
                var aisExB = (AisPositionExtB)ais;
                data.Add(aisExB);
            }
            else if (ais.GetType().Name.Equals("AISStaticDataReport"))
            {
                //静态数据报告
                var aisStatic = (AisStaticDataReport)ais;
                data.Add(aisStatic);
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            data = null;
            Console.WriteLine($"解析AIS失败:{ex.Message}");
        }

        return data;
    }

    /// <summary>
    ///     直接解码编码消息体（不包含消息头等信息）
    /// </summary>
    /// <param name="decodeStr"></param>
    private IAisMessage DecodeMsgBody(string decodeStr)
    {
        return AisDecoder.Decode(decodeStr);
    }

    /// <summary>
    ///     直接返回供解码的解码字符串，不包含消息头等信息
    /// </summary>
    /// <param name="param">输入标准的编码字符串字符数组</param>
    private string Combination(params string[] param)
    {
        if (param.Length > 9)
        {
            Console.WriteLine("AVIDM,AVIDO格式最多支持九个子句子!!");
            return null;
        }

        if (param.Any(t => t.Split(',').Length < 7)) return null;
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
            (item1, item2) =>
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
    ///     接收AIS数据
    /// </summary>
    /// <param name="obj"></param>
    private async Task RecvAisDataAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            try
            {
                var aisItem = string.Empty;
                var buffer = new byte[4 * 1024];
                var total = 0;
                while (_recvSoc.Receive(buffer, total, 1, SocketFlags.None) > 0)
                    if (buffer[total++] == '\n')
                        break;
                try
                {
                    aisItem = Encoding.ASCII.GetString(buffer, 0, total).Trim();
                    if (aisItem.Contains("!AIVDM")) // || aisItem.Contains("!AIVDO")),不解析本船信息
                    {
                        Console.WriteLine($"收到AIS信息:{aisItem}");
                        _data.Enqueue(aisItem);
                    }
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收AIS数据Error:{ex.Message}");
            }
        }
    }

    /// <summary>
    ///     清除资源
    /// </summary>
    private void ClearResouce()
    {
        _cts?.Cancel();
        try
        {
            if (_taskArray != null) Task.WhenAll(_taskArray).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            foreach (var ie in ex.Flatten().InnerExceptions) Console.WriteLine(ie.ToString());
        }
        finally
        {
            _cts?.Dispose();
        }

        try
        {
            if (_recvSoc != null)
            {
                _recvSoc.Close();
                _recvSoc.Dispose();
                _recvSoc = null;
            }
        }
        catch
        {
            _recvSoc = null;
        }
    }
}