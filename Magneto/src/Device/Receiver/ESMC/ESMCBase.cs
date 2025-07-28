using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.ESMC.SDK;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMC;

public partial class Esmc
{
    #region 初始化和释放资源

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        _gpib = new GpibClass();
        var intReturn = DeviceStart();
        //_Wav = new GetWave();
        //_Wav.RecordQuality = Quality.low;
        //_Wav.AudioDataArrived += _Wav_AudioDataArrived;
        //_Wav.ErrorEvent += _Wav_ErrorEvent;
        try
        {
            //int result = 0;
            //_audioRecorder = AudioRecorder.Create(WaveFormat.PCM_MONO, AudioProc, ref result);
            //if (_audioRecorder == null)
            //{
            //    string msg = string.Format(" OpenAudioDriver failed! error code {0}", result);
            //    LogManager.Add(new LogItem(LogType.Error, "ESMC:", msg));
            //}
        }
        catch
        {
        }

        if (intReturn == -1)
        {
            var err = "所选择的GPIB设备未开机或连接不正常，控制器无法与设备通讯。";
            throw new Exception(err);
        }

        if (_checkConnectTask == null)
        {
            _checkConnectTokenSource = new CancellationTokenSource();
            _checkConnectTask = new Task(CheckConnect, _checkConnectTokenSource.Token);
            _checkConnectTask.Start();
        }
    }

/*
    private void AudioProc(IntPtr lpData, int cbSize)
    {
        if (_audioSwitch)
        {
            var data = new byte[cbSize];
            Marshal.Copy(lpData, data, 0, cbSize);
            var audioData = new SDataAudio
            {
                Data = data,
                Format = AudioFormat.PCMONO
            };
            //audioData.SamplingRate = _frequency;
            var datas = new List<object> { audioData };
            SendData(datas);
        }
    }
*/

/*
    private void Wav_AudioDataArrived(byte[] data)
    {
        try
        {
            if (_audioSwitch == false) return;
            var audioData = new SDataAudio
            {
                // audioData.SamplingRate = _frequency;
                Format = AudioFormat.PCMONO,
                Data = data
            };
            var list = new List<object> { audioData };
            SendData(list);
        }
        catch
        {
        }
    }
*/

    private void InitFeature()
    {
        //初始化接收机恢复默认状态
        SendCommand("*RST");
        //由于默认状态下音量不为0，此处将音量关闭
        SendCommand("SYSTEM:AUDIO:VOLUME 0");
        SendCommand("OUTP:TONE OFF");
        SendCommand("FORM:BORD NORM"); //SWAP
        SendCommand("SENS:FREQ:AFC OFF"); //不使用自动频率控制
        SendCommand("MEAS:TIME DEF"); //测量时间为默认
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResources()
    {
        Utils.CancelTask(_checkConnectTask, _checkConnectTokenSource);
        Utils.CancelTask(_sendSpectrumTask, _sendSpectrumTokenSource);
        Utils.CancelTask(_fScanTask, _fScanTokenSource);
        Utils.CancelTask(_sendScanDataTask, _sendScanDataTokenSource);
        //if (_audioRecorder != null)
        //{
        //    _audioRecorder.Dispose();
        //    _audioRecorder = null;
        //}
        //if (_Wav != null)
        //{
        //}
    }

    #endregion

    #region 设备启动、重连

    private int DeviceStart()
    {
        var intReturn = 0;
        _gpib.BoardNumber = (short)GpibCode;
        _gpib.PrimaryAddress = (short)GpibAddress;
        _gpib.SecondaryAddress = (short)GpibAddress2;
        try
        {
            _gpib.Configure();
            if (_gpib.IsOnline())
            {
                _gpib.Clear();
                _gpib.DataAsString = true;
                _gpib.EotMode = true;
                _gpib.EosChar = "0";
                SendCommand("SYST:DISP:UPD ON");
                SendCommand("FORMAT ASCII");
                GetSysOpt(); // 获取系统配置
            }
        }
        catch (Exception)
        {
            return -1;
        }

        return intReturn;
    }

    private void CheckConnect()
    {
        Thread.Sleep(3000);
        try
        {
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            while (!_checkConnectTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(5000);
                _gpib.IsOnline();
            }
        }
        catch (Exception ex)
        {
            if (ex is not ThreadAbortException)
                SendMessage(new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                });
        }
        finally
        {
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
        }
    }

    private void Restart()
    {
        try
        {
            if (_gpib.IsOnline())
            {
                _gpib.Reset();
                DeviceStart();
            }
            else
            {
                DeviceStart();
            }
        }
        catch (Exception)
        {
            _gpib.Reset();
            DeviceStart();
        }
    }

    private string GetSysOpt()
    {
        var tmpstr = "";
        var tmpdata = CommandControl("*OPT?", 1000);
        if (tmpdata.IndexOf("SU", StringComparison.Ordinal) != -1)
        {
            tmpstr += "内置中频全景组件；";
            _haveSu = true;
        }

        if (tmpdata.IndexOf("BP", StringComparison.Ordinal) != -1) tmpstr += "电池快；";
        //_haveBp = true;
        if (tmpdata.IndexOf("DS", StringComparison.Ordinal) != -1) tmpstr += "射频频谱数字扫描；";
        //_haveDs = true;
        if (tmpdata.IndexOf("ER", StringComparison.Ordinal) != -1) tmpstr += "扩展RAM组件；";
        //_haveEr = true;
        if (tmpdata.IndexOf("HF", StringComparison.Ordinal) != -1) tmpstr += "高频组件；";
        //_haveHf = true;
        if (tmpdata.IndexOf("CW", StringComparison.Ordinal) != -1) tmpstr += "覆盖测量；";
        //_haveCw = true;
        if (tmpdata.IndexOf("FS", StringComparison.Ordinal) != -1) tmpstr += "场强测量；";
        //_haveFs = true;
        return tmpstr;
    }

    #endregion

    #region 单频测量

    private void SendSpectrum()
    {
        while (!_sendSpectrumTokenSource.IsCancellationRequested)
        {
            var datas = new List<object>();
            if (_haveSu) // 有中频全景选件
            {
                var spectrumData = GetSpectrum();
                if (spectrumData != null) datas.Add(spectrumData);
                var levelData = GetLevel();
                if (levelData != null) datas.Add(levelData);
            }
            else
            {
                var levelData = GetLevel();
                if (levelData != null) datas.Add(levelData);
            }

            if (datas.Count > 0) SendData(datas);
        }
    }

    /// <summary>
    ///     读取频谱数据
    /// </summary>
    /// <returns></returns>
    private object GetSpectrum()
    {
        var tmpStr = CommandControl("trace? IFPAN", 10000);
        var specList = new List<float>();
        if (string.IsNullOrEmpty(tmpStr))
        {
            var specArr = tmpStr?.Split(',');
            if (specArr is { Length: > 0 }) specList.AddRange(specArr.Select(t => float.Parse(t) * 10));
        }

        if (specList.Count > 0)
        {
            var specData = new SDataSpectrum
            {
                Frequency = _frequency,
                Span = 200,
                Data = Array.ConvertAll(specList.ToArray(), item => (short)(item * 10))
            };
            return specData;
        }

        return null;
    }

    /// <summary>
    ///     读取电平数据
    /// </summary>
    /// <returns></returns>
    private object GetLevel()
    {
        short level = 0;
        GetLevel(ref level);
        if (level < 2000)
        {
            var data = new SDataLevel
            {
                Frequency = _frequency,
                Bandwidth = _ifBandWidth,
                Data = level
            };
            return data;
        }

        return null;
    }

    /// <summary>
    ///     获得当前频率电平
    /// </summary>
    /// <returns></returns>
    private void GetLevel(ref short level)
    {
        try
        {
            var tempLevle = GetCurLevel();
            if (tempLevle == 0)
            {
                level = Convert.ToInt16(_preLevel);
            }
            else
            {
                //TempLevle *= 10;     // allen
                _preLevel = tempLevle;
                level = Convert.ToInt16(tempLevle);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    ///     取电平值
    /// </summary>
    /// <returns></returns>
    private float GetCurLevel()
    {
        var getData = 0f;
        try
        {
            var tmpStr = "";
            tmpStr = CommandControl("SENS:DATA?", 1000);
            if (!string.IsNullOrEmpty(tmpStr)) getData = float.Parse(tmpStr);
            return getData;
        }
        catch
        {
        }

        return getData;
    }

    #endregion

    #region 频段扫描

    private void StartScan()
    {
        if (_scanMode == ScanMode.Fscan)
        {
            CurWorkMode = WorkMode.Swe;
            Thread.Sleep(200);
            InitFScan();
            StartFscan();
        }
        else
        {
            InitMscan();
            StartMscan();
        }
    }

    private void StopScan()
    {
        if (_scanMode == ScanMode.Fscan)
            StopFScan();
        else
            StopMscan();
        Utils.CancelTask(_fScanTask, _fScanTokenSource);
    }

    #region FScan

    /// <summary>
    ///     初始化频段扫描
    /// </summary>
    private void InitFScan()
    {
        StopFScan();
        SetFscanCount(10000);
        SetFscanDirectUp(true);
        SetDwellTime(0f);
        SetHoldTime(0f);
        SetAfc("OFF");
        SetAgc(true);
        _currentMeasureDataCount =
            (int)((_stopFrequency * 1000000 - _startFrequency * 1000000) / (_stepFrequency * 1000));
    }

    /// <summary>
    ///     开始频段扫描
    /// </summary>
    private void StartFscan()
    {
        try
        {
            _fScanNum = 0;
            _currNum = 0;
            _totalCount = (int)((_stopFrequency * 1000000 - _startFrequency * 1000000) / (_stepFrequency * 1000)) + 1;
            SendCommand("TRACE SSTART,0");
            SendCommand("TRACE SSIOP,0");
            SendCommand("TRACE:FEED:CONTROL ITRACE,NEVER");
            SendCommand("TRACE:FEED:CONTROL IFPAN,NEVER");
            SendCommand("SENSE:FREQ:MODE SWEEP");
            SendCommand("TRACE:FEED:CONTROL MTRACE,ALWAYS");
            SendCommand("SENSE:SWEEP:COUNT 1");
            SendCommand("INIT");
            _fScanTokenSource = new CancellationTokenSource();
            _fScanTask = new Task(RunFScanProcess, _fScanTokenSource.Token);
            _fScanTask.Start();
            _sendScanDataTokenSource = new CancellationTokenSource();
            _sendScanDataTask = new Task(SendScanData, _sendScanDataTokenSource.Token);
            _sendScanDataTask.Start();
        }
        catch
        {
            _currNum = 0;
            _totalCount = 0;
        }
    }

    private void StopFScan()
    {
        SendCommand("ABORT");
        SendCommand("ABORT");
        SendCommand("ABORT");
        SendCommand("TRACE:FEED:CONTROL MTRACE,NEVER");
        SendCommand("SENSE:FREQUENCY:MODE CW");
        //停止数据发送线程
        Utils.CancelTask(_sendScanDataTask, _sendScanDataTokenSource);
    }

    private void RunFScanProcess()
    {
        var startFreq = _startFrequency;
        var stopFreq = _stopFrequency;
        var scanStep = (float)_stepFrequency;
        while (!_fScanTokenSource.IsCancellationRequested)
        {
            Thread.Sleep(100);
            try
            {
                var tmpStr = CommandControl("TRACE:DATA? MTRACE", 100000);
                if (tmpStr != "")
                {
                    if (tmpStr.IndexOf("9.91E37", StringComparison.Ordinal) != -1 && _fScanNum >= 2)
                    {
                        _fScanNum = 0;
                        //CurrNum = 0;
                        _totalCount = (int)((stopFreq * 1000000 - startFreq * 1000000) / (scanStep * 1000)) + 1; //***
                        SendCommand("TRACE SSTART,0");
                        SendCommand("TRACE SSIOP,0");
                        SendCommand("TRACE:FEED:CONTROL ITRACE,NEVER");
                        SendCommand("TRACE:FEED:CONTROL IFPAN,NEVER");
                        SendCommand("SENSE:FREQ:MODE SWEEP");
                        SendCommand("TRACE:FEED:CONTROL MTRACE,ALWAYS");
                        SendCommand("SENSE:SWEEP:COUNT 1");
                        SendCommand("INIT");
                    }
                    else
                    {
                        if (tmpStr.IndexOf("9.91E37", StringComparison.Ordinal) != -1)
                        {
                            _fScanNum++;
                        }
                        else
                        {
                            var mData = MySplit(tmpStr);
                            var dataCount = mData.Length;
                            var startIndex = _currNum;
                            _currNum += dataCount;
                            //m_Value = new short[DataCount - 1];
                            var mValue = new float[dataCount];
                            for (var i = 0; i <= dataCount - 1; i++) mValue[i] = float.Parse(mData[i]);
                            if (_currNum >= _totalCount)
                            {
                                //FSCAN
                                SendFScanPackage(startIndex, mValue);
                                startIndex = 0;
                                _currNum = 0;
                            }
                            else
                            {
                                SendFScanPackage(startIndex, mValue);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }

    private void SendFScanPackage(int startIndex, float[] levels)
    {
        var dataScan = new SDataScan
        {
            Data = Array.ConvertAll(levels, item => (short)(item * 10)),
            Total = _totalCount,
            Offset = startIndex,
            SegmentOffset = 0, // todo 频段索引
            StartFrequency = _startFrequency,
            StopFrequency = _stopFrequency,
            StepFrequency = _stepFrequency
        };
        _scanQueue.EnQueue(dataScan);
    }

    #endregion

    #region MScan

    /// <summary>
    ///     停止离散扫描
    /// </summary>
    private void StopMscan()
    {
        SendCommand("SYSTEM:KLOCK OFF");
        SendCommand("TRACE:FEED:CONTROL ITRACE,NEVER:TRACE:FEED:CONTROL MTRACE,NEVER");
        SendCommand("SENSE:FREQ:MODE CW");
        SendCommand("ABORT");
        SendCommand("MEM:CLEAR MEM0,MAX");
        //停止数据发送线程
        Utils.CancelTask(_sendScanDataTask, _sendScanDataTokenSource);
    }

    /// <summary>
    ///     初始化离散扫描
    /// </summary>
    private void InitMscan()
    {
        SetSqu(false);
        SetSquUse(false);
        SetMScanDwellTime(0);
        SetMScanHoldTime(0);
        SetMscanCount(10000);
        var mDd = new MemChanel();
        _currentMeasureDataCount = MscanPoints.Length;
        mDd.Act = true;
        mDd.Afc = false;
        mDd.AntNo = 1;
        mDd.Att = false;
        mDd.AutoAtt = true;
        mDd.Squ = false;
        mDd.Threth = 0;
        for (var i = 0; i < MscanPoints.Length; i++)
        {
            mDd.Freq = (double)MscanPoints[i]["frequency"];
            mDd.Mode = MscanPoints[i]["demMode"].ToString();
            mDd.SngBand = Convert.ToSingle(MscanPoints[i]["ifBandwidth"]);
            SetMscanChanel(i, mDd);
        }
    }

    /// <summary>
    ///     开始离散扫描
    /// </summary>
    private void StartMscan()
    {
        _fScanNum = 0;
        _currNum = 0;
        _totalCount = _currentMeasureDataCount;
        SendCommand("SYSTEM:KLOCK ON");
        SendCommand("SENSE:FREQ:MODE CW");
        SendCommand("TRACE:FCONTROL ITRACE,NEVER");
        SendCommand("TRACE:FEED:CONTROL IFPAN,NEVER");
        SendCommand("SENSE:FREQ:MODE MSCAN");
        SendCommand("TRACE:FEED:CONTROL MTRACE,ALWAYS");
        SendCommand("SENSE:MSCAN:DIR UP");
        SendCommand("SENSE:MSCAN:COUNT 1");
        SendCommand("INIT");
        _fScanTokenSource = new CancellationTokenSource();
        _fScanTask = new Task(RunMScanProcess, _fScanTokenSource.Token);
        _fScanTask.Start();
        SetAgc(true);
        _sendScanDataTokenSource = new CancellationTokenSource();
        _sendScanDataTask = new Task(SendScanData, _sendScanDataTokenSource.Token);
        _sendScanDataTask.Start();
    }

    /// <summary>
    ///     MSCAN扫描取数过程
    /// </summary>
    private void RunMScanProcess()
    {
        while (!_fScanTokenSource.IsCancellationRequested)
        {
            Thread.Sleep(100);
            try
            {
                var tmpStr = CommandControl("TRACE:DATA? MTRACE", 100000);
                if (tmpStr != "")
                {
                    if (tmpStr.IndexOf("9.91E37", StringComparison.Ordinal) != -1 && _fScanNum >= 2)
                    {
                        _fScanNum = 0;
                        //CurrNum = 0;
                        //_TotalCount =(uint)(CurrentMeasureDataCount);
                        SendCommand("TRACE:FEED:CONTROL ITRACE,NEVER");
                        SendCommand("TRACE:FEED:CONTROL IFPAN,NEVER");
                        SendCommand("SENSE:FREQ:MODE MSCAN");
                        SendCommand("TRACE:FEED:CONTROL MTRACE,ALWAYS");
                        SendCommand("SENSE:MSCAN:COUNT 1");
                        SendCommand("INIT");
                    }
                    else
                    {
                        if (tmpStr.IndexOf("9.91E37", StringComparison.Ordinal) != -1)
                        {
                            _fScanNum++;
                        }
                        else
                        {
                            var mData = MySplit(tmpStr);
                            var dataCount = mData.Length;
                            _currNum += dataCount;
                            //m_Value = new short[DataCount - 1];
                            var mValue = new float[dataCount];
                            for (var i = 0; i <= dataCount - 1; i++)
                                //m_Value[i] = float.Parse(m_data[i]) * 10;    // Allen
                                mValue[i] = float.Parse(mData[i]);
                            if (_currNum >= _totalCount)
                            {
                                SendMScanPackage(mValue, true);
                                _currNum = 0;
                            }
                            else
                            {
                                SendMScanPackage(mValue, false);
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    ///     设置一个信号的离散扫描命令
    /// </summary>
    /// <param name="index"></param>
    /// <param name="mVal"></param>
    private void SetMscanChanel(int index, MemChanel mVal)
    {
        var dbmSign = char.ToString('"');
        var tmpstr = "MEM:CONT MEM" + index;
        tmpstr = tmpstr + ", " + mVal.Freq + "MHz," + mVal.Threth + ","
                 + mVal.Mode + "," + mVal.SngBand + "KHz," + "45,(@" + mVal.AntNo + "),0" + ",";
        if (mVal.Att)
            tmpstr += "1,";
        else
            tmpstr += "0,";
        if (mVal.AutoAtt)
            tmpstr += "1,";
        else
            tmpstr += "0,";
        tmpstr += "AGC,POS,0,1,0,";
        if (mVal.Squ)
            tmpstr += "1,";
        else
            tmpstr += "0,";
        tmpstr += "abs,";
        if (mVal.Afc)
            tmpstr += "1,";
        else
            tmpstr += "0,";
        tmpstr = tmpstr + dbmSign + "br3" + dbmSign + ",2,";
        if (mVal.Act)
            tmpstr += "1";
        else
            tmpstr += "0";
        SendCommand(tmpstr);
    }

    private void SendMScanPackage(float[] levels, bool isEnd)
    {
        var dataScan = new SDataScan
        {
            Data = Array.ConvertAll(levels, item => (short)(item * 10)),
            Total = _totalCount,
            Offset = isEnd ? 0 : _currNum
        };
        _scanQueue.EnQueue(dataScan);
    }

    #endregion

    /// <summary>
    ///     发送扫描数据
    /// </summary>
    private void SendScanData()
    {
        while (!_sendScanDataTokenSource.IsCancellationRequested)
        {
            var obj = _scanQueue.DeQueue();
            if (obj == null)
            {
                Thread.Sleep(2);
                continue;
            }

            var objList = new List<object> { obj };
            SendData(objList);
        }
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     发送命令
    /// </summary>
    /// <param name="cmdst"></param>
    private void SendCommand(string cmdst)
    {
        try
        {
            Thread.Sleep(30);
            CommandControl(cmdst, -1);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     程序中所有的向ESMC发送的命令函数
    /// </summary>
    /// <param name="commandString">命令字符串</param>
    /// <param name="buffer"></param>
    /// <returns>从设备返回的数据</returns>
    private string CommandControl(string commandString, int buffer)
    {
        var tempString = "";
        try
        {
            lock (this)
            {
                if (_gpib.IsOnline() == false) Restart();

                if (commandString != "") _gpib.Send(commandString);
                if (buffer > 0) tempString = _gpib.Read(32000).ToString();
                return tempString;
            }
        }
        catch
        {
        }

        return tempString;
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     设置频段扫描次数
    /// </summary>
    /// <param name="count"></param>
    private void SetFscanCount(int count)
    {
        if (count >= 1000)
            SendCommand("SENS:SWE:COUN INF");
        else
            SendCommand("SENS:SWE:COUN " + count);
    }

    /// <summary>
    ///     设置扫描方向是向前还是往后
    /// </summary>
    /// <param name="value"></param>
    private void SetFscanDirectUp(bool value)
    {
        SendCommand("SENS:SWE:dir " + (value ? "UP" : "DOWN"));
    }

    private void SetDwellTime(float dwellTime)
    {
        SendCommand("SWE:DWEL  " + dwellTime);
    }

    private void SetHoldTime(float value)
    {
        SendCommand("SWE:HOLD:TIME  " + value);
    }

    private void SetAgc(bool value)
    {
        SendCommand(value ? "SENS:GCON:MODE AGC" : "SENS:GCON:MODE MGC");
    }

    /// <summary>
    ///     设置自动频率控制
    /// </summary>
    /// <param name="isOpen"></param>
    private void SetAfc(string isOpen)
    {
        SendCommand(isOpen == "ON" ? "SENS:FREQ:AFC ON" : "SENS:FREQ:AFC OFF");
    }

    private static string[] MySplit(string dataStr)
    {
        string[] tmpData = null;
        if (dataStr != null) tmpData = dataStr.Split(',');
        return tmpData;
    }

    /// <summary>
    ///     设置是否开启静噪门限
    /// </summary>
    private void SetSqu(bool mVal)
    {
        try
        {
            SendCommand(mVal ? "OUTP:SQU:STAT ON" : "OUTP:SQU:STAT OFF");
        }
        catch
        {
        }
    }

    /// <summary>
    ///     设置静噪门限是否使用
    /// </summary>
    /// <param name="use"></param>
    private void SetSquUse(bool use)
    {
        SendCommand("OUTPUT:SQUELCH:CONTROL " + (use ? "MEM" : "NONE"));
    }

    private void SetMScanDwellTime(float value)
    {
        SendCommand("SENSE:MSCAN:DWELL " + value);
    }

    private void SetMScanHoldTime(float value)
    {
        SendCommand("SENSE:MSCAN:HOLD " + value);
    }

    /// <summary>
    ///     设置离散扫描次数
    /// </summary>
    /// <param name="count"></param>
    private void SetMscanCount(int count)
    {
        if (count >= 1000)
            SendCommand("SENSE:MSCAN:COUNT INF");
        else
            SendCommand("SENSE:MSCAN:COUNT " + count);
    }

    #endregion
}