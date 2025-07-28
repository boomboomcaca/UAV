using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS.Driver;

public partial class Receiver : IDisposable
{
    private readonly int _baudrate;
    private readonly string _comAddress;

    /// <summary>
    ///     串口数据锁
    /// </summary>
    private readonly object _dataLocker = new();

    /// <summary>
    ///     需要解码列表数据锁
    /// </summary>
    private readonly object _decodeLocker = new();

    private readonly List<BcchDataStruct> _historyList = new();
    private readonly List<BcchDataStruct> _needDecodeList = new();

    /// <summary>
    ///     串口消息队列
    /// </summary>
    private readonly List<byte> _queue = new();

    /// <summary>
    ///     查询小区锁
    /// </summary>
    private AutoResetEvent _cellSearchEvent;

    private Task _cellSearchTask;
    private CancellationTokenSource _cellSearchTokenSource;
    private volatile bool _isCellSearch;
    private bool _isDisposed;
    private SerialPort _serialPort;
    private Task _task;
    private CancellationTokenSource _tokenSource;
    public EventHandler<GsmData> OnDataReceived;

    public Receiver(string comAddress, int baudrate)
    {
        _comAddress = comAddress;
        _baudrate = baudrate;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Receiver()
    {
        Dispose(false);
    }

    /// <summary>
    ///     初始化
    /// </summary>
    /// <returns>是否成功</returns>
    public bool Init()
    {
        try
        {
            _serialPort = new SerialPort(_comAddress, _baudrate, Parity.None, 8, StopBits.One);
            _serialPort.Open();
            _serialPort.DataReceived += SerialPort_DataReceived;
            return _serialPort.IsOpen;
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine(ex.Message);
#endif
            return false;
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort is not { IsOpen: true }) return;
        lock (_dataLocker)
        {
            var bytes = new byte[_serialPort.BytesToRead];
            try
            {
                _serialPort.Read(bytes, 0, bytes.Length); //读取数据
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (TimeoutException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }

            _queue.AddRange(bytes);
        }
    }

    /// <summary>
    ///     开始扫描
    /// </summary>
    /// <returns>是否成功</returns>
    public bool StartScan(CellSearchRequest request)
    {
        _cellSearchEvent = new AutoResetEvent(false);
        _searchRequest = request;
        _tokenSource = new CancellationTokenSource();
        _task = new Task(ProcessData, _tokenSource.Token);
        _task.Start();
        _cellSearchTokenSource = new CancellationTokenSource();
        _cellSearchTask = new Task(ExecuteCellSearch, _cellSearchTokenSource.Token);
        _cellSearchTask.Start();
        return CellSearch();
    }

    private bool CellSearch()
    {
        lock (_dataLocker)
        {
            _queue.Clear();
        }

        _cellSearchFlag = 1;
        var cmdBytes = CommandHelper.CreateCellSearchCommand(_searchRequest);
        var result = SendCommand(cmdBytes);
        _isCellSearch = true;
        if (!result) return false;
        return true;
    }

    /// <summary>
    ///     停止扫描
    /// </summary>
    /// <returns>是否成功</returns>
    public bool StopScan()
    {
        var cmdBytes = CommandHelper.CreateStopSearchCommand();
        var result = SendCommand(cmdBytes);
        if (!result) return false;
        _cellSearchEvent?.Dispose();
        CancelTask(_cellSearchTask, _cellSearchTokenSource);
        CancelTask(_task, _tokenSource);
        return true;
    }

    /// <summary>
    ///     关闭
    /// </summary>
    public void Close()
    {
        Dispose();
    }

    private bool SendCommand(byte[] bytes)
    {
        if (_serialPort is not { IsOpen: true }) return false;
        try
        {
            _serialPort.Write(bytes, 0, bytes.Length);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void CancelTask(Task task, CancellationTokenSource tokenSource)
    {
        if (task != null)
            if (tokenSource != null && !task.IsCompleted)
                tokenSource.Cancel();
        tokenSource?.Dispose();
    }

    private void Dispose(bool disposing)
    {
        lock (_dataLocker)
        {
            _queue.Clear();
        }

        if (!_isDisposed)
        {
            if (disposing && _cellSearchEvent != null) _cellSearchEvent.Dispose();
            CancelTask(_cellSearchTask, _cellSearchTokenSource);
            CancelTask(_task, _tokenSource);
            _serialPort?.Dispose();
        }

        _isDisposed = true;
    }

    private void SendData(GsmData data)
    {
        OnDataReceived?.Invoke(this, data);
    }

    private void ProcessData()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            byte[] data = null;
            var crcFlag = -1;
            try
            {
                lock (_dataLocker)
                {
                    if (_queue.Count > 0)
                    {
                        var msgHeader = _queue[0];
                        if (msgHeader == DspHeader)
                        {
                            if (_queue.Count >= Message0Len)
                            {
                                data = new byte[Message0Len];
                                _queue.CopyTo(0, data, 0, Message0Len);
                                _queue.RemoveRange(0, Message0Len);
                                crcFlag = Crc8Helper.Decode(data, Message0Len);
                            }
                        }
                        else
                        {
                            _queue.RemoveAt(0);
                            continue;
                        }
                    }
                }

                if (data == null)
                {
                    Thread.Sleep(500);
                    continue;
                }

                if (crcFlag <= 0) continue;
                if (data[1] == 1 && data[2] == 2 && crcFlag == (int)PacketHeader.PacketHeartBeat) continue;
                ResolveData(data, crcFlag);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }

    private void DecodeCell()
    {
        Trace.WriteLine("开始解码......");
        while (true)
        {
            lock (_dataLocker)
            {
                _queue.Clear();
            }

            lock (_decodeLocker)
            {
                if (_needDecodeList.Count == 0) break;
                _bcchData = _needDecodeList[0];
                _bcchC0 = _bcchData.C0;
                CommandToDSP_SetBCCH_ARFCN(_bcchC0);
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (_bcchData.SIB3 != 1 && stopWatch.ElapsedMilliseconds < 2000) Thread.Sleep(10);
            stopWatch.Stop();
            if (_bcchData.SIB3 == 1)
            {
                _bcchData.C2L = (short)(2 * _bcchData.CELL_RESELECT_OFFSET - _bcchData.RXLEV_ACCESS_MIN);
                int c2L = _bcchData.C2L;
                if (_bcchData is { MNC: <= 1, paging_count: > 10 }) // normal BS
                {
                    _lacList[_bcchData.MNC, (_bcchData.LAC >> 8) & 0xff]++; // normal BS LAC
                    if (c2L + 30 > _c2LWarning[_bcchData.MNC]) _c2LWarning[_bcchData.MNC] = c2L + 30;
                    if (_c2LWarning[_bcchData.MNC] > 123) _c2LWarning[_bcchData.MNC] = 123;
                }

                // check the fake BS condition
                if (_bcchData.MNC <= 1)
                {
                    var lacFlag = 0;
                    var ciFlag = 0;
                    var c2LFlag = 0;
                    for (var i = 0; i < 256; i++)
                        if (_lacList[_bcchData.MNC, i] != 0)
                        {
                            lacFlag = 1;
                            var lacTmp = i * 256 + 128;
                            if (Math.Abs(_bcchData.LAC - lacTmp) < 1024)
                            {
                                lacFlag = 0;
                                break;
                            }
                        }

                    if (c2L > _c2LWarning[_bcchData.MNC] && _c2LWarning[_bcchData.MNC] != -100) c2LFlag = 1;
                    if (_bcchData.CI is 10 or 0xffff) ciFlag = 1;
                    if (lacFlag == 1 || ciFlag == 1 || c2LFlag == 1) _bcchData.fake_bs_flag = 1;
                }

                // 伪基站不能加入小区记录 
                if (_bcchData.fake_bs_flag == 0)
                {
                    lock (_decodeLocker)
                    {
                        _historyList.Add((BcchDataStruct)_bcchData.Clone());
                    }
                }
                else
                {
                    _bcchC0 = _bcchData.C0;
                    OnBnClickedButtonrx();
                    var watch = new Stopwatch();
                    watch.Start();
                    while (_bcchData.SIB3 != 1 && watch.ElapsedMilliseconds < 2000) Thread.Sleep(10);
                    watch.Stop();
                }

                SendData(new GsmData { Data = (BcchDataStruct)_bcchData.Clone() });
            }

            lock (_decodeLocker)
            {
                _needDecodeList.RemoveAll(p => p.C0 == _bcchData.C0);
            }
        }

        Trace.WriteLine("解码完成。");
        _cellSearchEvent.Set();
    }

    private void ExecuteCellSearch()
    {
        while (!_cellSearchTokenSource.IsCancellationRequested)
        {
            try
            {
                _cellSearchEvent.WaitOne();
                CellSearch();
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }

            Trace.WriteLine("开始小区搜索......");
        }
    }

    private void OnBnClickedButtonrx()
    {
        _targetIAslot = -1;
        _cellSearchFlag = 0;
        _bsCcchans = 4;
        _count.TaskSendtoDsp += 2;
        _cellSearchFlag = 0;
        CommandToDSP_SetBCCH_ARFCN(_bcchC0);
        _cellSearchFlag = 0;
        _bcchData = default;
        Array.Clear(_taskSdcch8, 0, _taskSdcch8.Length);
        _count = new CountStruct();
        Array.Clear(_ccchDataBuffer, 0, _ccchDataBuffer.Length);
        _smsShow = 1;
        _sms = 1;
        _speech = 1;
        _pageCall = 1;
        _originatingCall = 1;
        _locationUpdate = 1;
        _tAsum = 0;
        _tAcount = 0;
    }
}