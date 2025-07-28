using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.XE_VUHF.Common;

internal class TelnetSocket : IDisposable
{
    private CancellationTokenSource _cts;

    /// <summary>
    ///     一个Socket套接字
    /// </summary>
    private Socket _socket;

    private Task _task;
    public bool Connected { get; set; }

    /// <summary>
    ///     启动socket 进行telnet操作
    /// </summary>
    public void Connect(string ip, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(ip, port);
        Connected = true;
        _cts = new CancellationTokenSource();
        _task = new Task(OnRecievedData, _cts.Token);
        _task.Start();
    }

    private void OnRecievedData()
    {
        while (_cts?.IsCancellationRequested == false)
            try
            {
                var nRecvBytes = _socket.Receive(_recvBuffer, SocketFlags.None);
                if (nRecvBytes > 0)
                {
                    //将接收到的数据转个码,顺便转成string型
                    Encoding.GetEncoding("utf-8").GetString(_recvBuffer, 0, nRecvBytes);
                    //声明一个字符串,用来存储解析过的字符串
                    var strLine = "";
                    //遍历Socket接收到的字符
                    /*
                     * 此循环用来调整linux 和 windows在换行上标记的区别
                     * 最后将调整好的字符赋予给 strLine
                     */
                    for (var i = 0; i < nRecvBytes; i++)
                    {
                        var ch = Convert.ToChar(_recvBuffer[i]);
                        switch (ch)
                        {
                            case '\r':
                                strLine += Convert.ToString("\r\n");
                                break;
                            case '\n':
                                break;
                            default:
                                strLine += Convert.ToString(ch);
                                break;
                        }
                    }

                    try
                    {
                        //如果长度为零
                        if (strLine.Length == 0)
                            //则返回"\r\n" 即回车换行
                            strLine = Convert.ToString("\r\n");
                        //建立一个流,把接收的信息(转换后的)存进 bytes 中
                        var bytes = new byte[strLine.Length];
                        for (var i = 0; i < strLine.Length; i++) bytes[i] = Convert.ToByte(strLine[i]);
                        // Process the incoming data
                        //对接收的信息进行处理,包括对传输过来的信息的参数的存取和
                        var strOutText = ProcessOptions(bytes);
                        //解析命令后返回 显示信息(即除掉了控制信息)
                        if (strOutText != "") Console.Write(strOutText);
                        //Connected = true;
                        // Respond to any incoming commands
                        //接收完数据,处理完字符串数据等一系列事物之后,开始回发数据
                        RespondToOptions();
                    }
                    catch (Exception /*ex*/)
                    {
                    }
                }
                else
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception /*ex*/)
            {
            }
    }

    /// <summary>
    ///     发送数据的函数
    /// </summary>
    private void RespondToOptions()
    {
        try
        {
            //声明一个字符串,来存储 接收到的参数
            string strOption;
            /*
             * 此处的控制信息参数,是之前接受到信息之后保存的
             * 例如 255   253   23   等等
             * 具体参数的含义需要去查telnet 协议
             */
            for (var i = 0; i < _listOptions.Count; i++)
            {
                //获得一个控制信息参数
                strOption = _listOptions[i];
                //根据这个参数,进行处理
                ArrangeReply(strOption);
            }

            //申请一个与字符串相当长度的char流
            var smk = new byte[_strResponse.Length];
            for (var i = 0; i < _strResponse.Length; i++)
            {
                //解析字符串,将其存储到char流中去
                var ss = Convert.ToByte(_strResponse[i]);
                smk[i] = ss;
            }

            _socket.Send(smk);
            _strResponse = "";
            _listOptions.Clear();
        }
        catch (Exception /*ex*/)
        {
            //Console.WriteLine("错错了,在回发数据的时候 " + ex.Message);
        }
    }

    /// <summary>
    ///     解析接收的数据,生成最终用户看到的有效文字,同时将附带的参数存储起来
    /// </summary>
    /// <param name="bytesToProcess">收到的处理后的数据</param>
    /// <returns></returns>
    private string ProcessOptions(byte[] bytesToProcess)
    {
        var dispalyText = "";
        var strTemp = "";
        var strOption = "";
        var bDone = false;
        var ndx = 0;
        var ldx = 0;
        char ch;
        try
        {
            //把数据从byte[] 转化成string
            for (var i = 0; i < bytesToProcess.Length; i++)
            {
                var ss = Convert.ToChar(bytesToProcess[i]);
                strTemp += Convert.ToString(ss);
            }

            //此处意义为,当没描完数据前,执行扫描
            while (bDone != true)
            {
                //获得长度
                var lensmk = strTemp.Length;
                //之后开始分析指令,因为每条指令为255 开头,故可以用此来区分出每条指令
                ndx = strTemp.IndexOf(Convert.ToString(_iac), StringComparison.Ordinal);
                //此处为出错判断,本无其他含义
                if (ndx > lensmk) ndx = strTemp.Length;
                //此处为,如果搜寻到IAC标记的telnet 指令,则执行以下步骤
                if (ndx != -1)
                {
                    #region 如果存在IAC标志位

                    // 将 标志位IAC 的字符 赋值给最终显示文字
                    dispalyText += strTemp.Substring(0, ndx);
                    // 此处获得命令码
                    ch = strTemp[ndx + 1];
                    //如果命令码是253(DO) 254(DONT)  521(WILL) 252(WONT) 的情况下
                    if (ch == _do || ch == _dont || ch == _will || ch == _wont)
                    {
                        //将以IAC 开头3个字符组成的整个命令存储起来
                        strOption = strTemp.Substring(ndx, 3);
                        _listOptions.Add(strOption);
                        // 将 标志位IAC 的字符 赋值给最终显示文字
                        dispalyText += strTemp.Substring(0, ndx);
                        //将处理过的字符串删去
                        var txt = strTemp.Substring(ndx + 3);
                        strTemp = txt;
                    }
                    //如果IAC后面又跟了个IAC (255)
                    else if (ch == _iac)
                    {
                        //则显示从输入的字符串头开始,到之前的IAC 结束
                        dispalyText = strTemp.Substring(0, ndx);
                        //之后将处理过的字符串排除出去
                        strTemp = strTemp.Substring(ndx + 1);
                    }
                    //如果IAC后面跟的是SB(250)
                    else if (ch == _sb)
                    {
                        dispalyText = strTemp.Substring(0, ndx);
                        ldx = strTemp.IndexOf(Convert.ToString(_se), StringComparison.Ordinal);
                        strOption = strTemp.Substring(ndx, ldx);
                        _listOptions.Add(strOption);
                        strTemp = strTemp.Substring(ldx);
                    }

                    #endregion
                }
                //若字符串里已经没有IAC标志位了
                else
                {
                    //显示信息累加上strTemp存储的字段
                    dispalyText = dispalyText + strTemp;
                    bDone = true;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("解析传入的字符串错误:" + ex.Message);
        }

        //输出人看到的信息
        return dispalyText;
    }

    #region 一些telnet的数据定义,先没看懂没关系

    /// <summary>
    ///     标志符,代表是一个TELNET 指令
    /// </summary>
    private readonly char _iac = Convert.ToChar(255);

    /// <summary>
    ///     表示一方要求另一方使用，或者确认你希望另一方使用指定的选项。
    /// </summary>
    private readonly char _do = Convert.ToChar(253);

    /// <summary>
    ///     表示一方要求另一方停止使用，或者确认你不再希望另一方使用指定的选项。
    /// </summary>
    private readonly char _dont = Convert.ToChar(254);

    /// <summary>
    ///     表示希望开始使用或者确认所使用的是指定的选项。
    /// </summary>
    private readonly char _will = Convert.ToChar(251);

    /// <summary>
    ///     表示拒绝使用或者继续使用指定的选项。
    /// </summary>
    private readonly char _wont = Convert.ToChar(252);

    /// <summary>
    ///     表示后面所跟的是对需要的选项的子谈判
    /// </summary>
    private readonly char _sb = Convert.ToChar(250);

    /// <summary>
    ///     子谈判参数的结束
    /// </summary>
    private readonly char _se = Convert.ToChar(240);

    private const char Is = '0';
    private const char Send = '1';
    private const char Info = '2';
    private const char Var = '0';
    private const char Value = '1';
    private const char Esc = '2';
    private const char Uservar = '3';

    /// <summary>
    ///     流
    /// </summary>
    private readonly byte[] _recvBuffer = new byte[100000];

    /// <summary>
    ///     收到的控制信息
    /// </summary>
    private readonly List<string> _listOptions = new();

    /// <summary>
    ///     存储准备发送的信息
    /// </summary>
    private string _strResponse;

    #endregion

    #region magic Function

    /// <summary>
    ///     解析传过来的参数,生成回发的数据到m_strResp
    /// </summary>
    /// <param name="strOption"></param>
    private void ArrangeReply(string strOption)
    {
        try
        {
            char modifier;
            char ch;
            var bDefined = false;
            //排错选项,无啥意义
            if (strOption.Length < 3) return;
            //获得命令码
            var verb = strOption[1];
            //获得选项码
            var option = strOption[2];
            //如果选项码为 回显(1) 或者是抑制继续进行(3)
            if (option == 1 || option == 3) bDefined = true;
            // 设置回发消息,首先为标志位255
            _strResponse += _iac;
            //如果选项码为 回显(1) 或者是抑制继续进行(3) ==true
            if (bDefined)
            {
                #region 继续判断

                //如果命令码为253 (DO)
                if (verb == _do)
                {
                    //我设置我应答的命令码为 251(WILL) 即为支持 回显或抑制继续进行
                    ch = _will;
                    _strResponse += ch;
                    _strResponse += option;
                }

                //如果命令码为 254(DONT)
                if (verb == _dont)
                {
                    //我设置我应答的命令码为 252(WONT) 即为我也会"拒绝启动" 回显或抑制继续进行
                    ch = _wont;
                    _strResponse += ch;
                    _strResponse += option;
                }

                //如果命令码为251(WILL)
                if (verb == _will)
                {
                    //我设置我应答的命令码为 253(DO) 即为我认可你使用回显或抑制继续进行
                    ch = _do;
                    _strResponse += ch;
                    _strResponse += option;
                    //break;
                }

                //如果接受到的命令码为251(WONT)
                if (verb == _wont)
                {
                    //应答  我也拒绝选项请求回显或抑制继续进行
                    ch = _dont;
                    _strResponse += ch;
                    _strResponse += option;
                    //    break;
                }

                //如果接受到250(sb,标志子选项开始)
                if (verb == _sb)
                {
                    /*
                     * 因为启动了子标志位,命令长度扩展到了4字节,
                     * 取最后一个标志字节为选项码
                     * 如果这个选项码字节为1(send)
                     * 则回发为 250(SB子选项开始) + 获取的第二个字节 + 0(is) + 255(标志位IAC) + 240(SE子选项结束)
                     */
                    modifier = strOption[3];
                    if (modifier == Send)
                    {
                        ch = _sb;
                        _strResponse += ch;
                        _strResponse += option;
                        _strResponse += Is;
                        _strResponse += _iac;
                        _strResponse += _se;
                    }
                }

                #endregion
            }
            else //如果选项码不是1 或者3
            {
                #region 底下一系列代表,无论你发那种请求,我都不干

                if (verb == _do)
                {
                    ch = _wont;
                    _strResponse += ch;
                    _strResponse += option;
                }

                if (verb == _dont)
                {
                    ch = _wont;
                    _strResponse += ch;
                    _strResponse += option;
                }

                if (verb == _will)
                {
                    ch = _dont;
                    _strResponse += ch;
                    _strResponse += option;
                }

                if (verb == _wont)
                {
                    ch = _dont;
                    _strResponse += ch;
                    _strResponse += option;
                }

                #endregion
            }
        }
        catch (Exception ex)
        {
            throw new Exception("解析参数时出错:" + ex.Message);
        }
    }

    public void Dispose()
    {
        Utils.CancelTask(_task, _cts);
        _socket?.Dispose();
        Connected = false;
    }

    #endregion
}