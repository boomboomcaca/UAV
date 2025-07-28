using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.YaesuG1000DXC;

public partial class YaesuG1000Dxc : DeviceBase
{
    #region 构造函数

    public YaesuG1000Dxc(Guid id) : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     获取转台数据串口
    /// </summary>
    private SerialPort _serialport;

    /// <summary>
    ///     获取方位角数据线程
    /// </summary>
    private Task _getAzimuthTask;

    /// <summary>
    ///     比幅测向功能线程
    /// </summary>
    private Task _ampdfTask;

    private CancellationTokenSource _getAzimuthTokenSource;
    private CancellationTokenSource _ampdfTokenSource;

    /// <summary>
    ///     同步锁
    /// </summary>
    private readonly object _locker = new();

    /// <summary>
    ///     缓存当前转台方位角
    /// </summary>
    private int _currentAzimuth;

    /// <summary>
    ///     比幅测向时，控制转台右转动信号
    /// </summary>
    private readonly AutoResetEvent _turnright = new(false);

    /// <summary>
    ///     比幅测向时，控制转台左转动信号
    /// </summary>
    private readonly AutoResetEvent _turnleft = new(false);

    /// <summary>
    ///     缓存比幅测向时次数
    /// </summary>
    private int _measuretimes;

    /// <summary>
    ///     比幅测向转动标志
    /// </summary>
    private bool _beginRotate;

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo device)
    {
        try
        {
            var result = base.Initialized(device);
            if (!result) return false;
            //释放资源
            ReleaseResources();
            //初始化网络连接
            InitNetwork();
            //初始化设备
            InitDevice();
            //初始化任务
            InitAllTask();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            ReleaseResources();
            return false;
        }
    }

    public override void Dispose()
    {
        ReleaseResources();
        GC.SuppressFinalize(this);
        base.Dispose();
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        ExecuteCmd();
    }

    public override void Stop()
    {
        StopMove();
        base.Stop();
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    private void ReleaseResources()
    {
        if (_serialport != null)
        {
            try
            {
                if (_serialport.IsOpen) _serialport.Close();
                _serialport.Dispose();
            }
            catch
            {
            }

            _serialport = null;
        }

        Utils.CancelTask(_getAzimuthTask, _getAzimuthTokenSource);
        Utils.CancelTask(_ampdfTask, _ampdfTokenSource);
    }

    /// <summary>
    ///     初始化网络
    /// </summary>
    private void InitNetwork()
    {
        _serialport = new SerialPort
        {
            PortName = SerialPortNum,
            BaudRate = BaudRate,
            DataBits = 8,
            StopBits = StopBits.One,
            Parity = Parity.None,
            Handshake = Handshake.None
        };
        if (!_serialport.IsOpen) _serialport.Open();
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        lock (_locker)
        {
            _serialport.WriteLine("X1\r\n"); //匹配从串口获取数据的频率，此处固定转速X1,避免频繁获取相同的数据，造成不必要的开销
        }
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitAllTask()
    {
        _getAzimuthTokenSource = new CancellationTokenSource();
        _getAzimuthTask = new Task(GetAzimuth, _getAzimuthTokenSource.Token);
        _getAzimuthTask.Start();
    }

    #endregion

    #region 辅助函数

    /// <summary>
    ///     比幅测向线程方法
    /// </summary>
    private void AmpdfTest()
    {
        _measuretimes = MeasureCount;
        //转台归零
        lock (_locker)
        {
            _serialport.WriteLine("M000\r\n");
        }

        _beginRotate = false;
        for (var i = 0; i < MeasureCount; i++)
        {
            _turnright.WaitOne();
            lock (_locker)
            {
                _serialport.WriteLine("M360\r\n");
            }

            _measuretimes--;
            if (_measuretimes == 0) return;
            _turnleft.WaitOne();
            lock (_locker)
            {
                _serialport.WriteLine("M000\r\n");
            }

            _measuretimes--;
            if (_measuretimes == 0) return;
        }
    }

    /// <summary>
    ///     执行客户端命令
    /// </summary>
    private void ExecuteCmd()
    {
        switch (Movement)
        {
            case 0:
                StopMove();
                break;
            case 1:
                RecoverAntenna();
                break;
            case 2:
                StartAmpdf();
                break;
            case 3:
                RotateAzimuth();
                break;
        }
    }

    /// <summary>
    ///     转台停止转动
    /// </summary>
    private void StopMove()
    {
        lock (_locker)
        {
            _serialport.WriteLine("A\r\n");
        }

        Thread.Sleep(500);
    }

    /// <summary>
    ///     收天线(使天线回到0°位置)
    /// </summary>
    private void RecoverAntenna()
    {
        lock (_locker)
        {
            _serialport.WriteLine("M000\r\n");
        }

        Thread.Sleep(500);
    }

    /// <summary>
    ///     启动比幅测向功能
    /// </summary>
    private void StartAmpdf()
    {
        _ampdfTokenSource = new CancellationTokenSource();
        _ampdfTask = new Task(AmpdfTest, _ampdfTokenSource.Token);
        _ampdfTask.Start();
    }

    /// <summary>
    ///     获取方位角并心跳检测
    /// </summary>
    private void GetAzimuth()
    {
        //用于记录未获取到数据的次数，如果连续5次未获取到数据，则认为网络断连
        var checkConnectionCount = 0;
        //检查收天线时转台调整的次数
        var checkRecoverAntennaCount = 0;
        //检查转动角度时转台调整的次数
        var checkRotateAzimuthCount = 0;
        //检查比幅测向时，转台转到起始位置时调整的次数
        var checkStartPointCount = 0;
        //检查比幅测向时，转台转动到终点位置时调整的次数
        var checkStopPointCount = 0;
        //检查比幅测向时，比幅测向最后转动到终点时转台调整的次数
        var checkAmpdfCompleteCount = 0;
        while (!_getAzimuthTokenSource.IsCancellationRequested)
        {
            #region 查询数据

            lock (_locker)
            {
                _serialport.WriteLine("C\r\n");
            }

            Thread.Sleep(300); //匹配设备转动速度
            string receiveData;
            lock (_locker)
            {
                receiveData = _serialport.ReadExisting();
            }

            if (receiveData.Length >= 6 && receiveData.StartsWith("AZ"))
                try
                {
                    //解决偶发性出现数据紊乱如AZ=01的问题，实际正确的数据格式应为AZ=001的问题
                    _currentAzimuth = int.Parse(receiveData.Substring(3, 3));
                }
                catch
                {
                    //如果受环境影响，出现不规则数据，则直接丢掉此包数据
                    continue;
                }
            else
                continue;

            #endregion

            #region 如果连续5次获取不到数据，则认为网络断连（此设备会出现串口能连接，但是设备不返回数据）

            if (receiveData is "" or "") checkConnectionCount++;
            if (checkConnectionCount == 5)
            {
                var info = new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                };
                SendMessage(info);
            }

            #endregion

            var data = new List<object>();
            var dataAngle = new SDataAngle
            {
                Azimuth = _currentAzimuth,
                Elevation = 0,
                Polarization = 0,
                Completed = false
            };

            #region 比幅测向功能

            //Movement==2 比幅测向功能
            if (Movement == 2 && TaskState.ToString().Equals(TaskState.Start.ToString()))
            {
                if (Math.Abs(_currentAzimuth - 0) < OffSet && !_beginRotate) checkStartPointCount++;
                if (Math.Abs(_currentAzimuth - 360) < OffSet && _beginRotate) checkStopPointCount++;
                if (_measuretimes > 0 && !_beginRotate && checkStartPointCount > 5)
                {
                    _turnright.Set();
                    checkStartPointCount = 0;
                    lock (_locker)
                    {
                        _beginRotate = true;
                    }
                }

                if (_measuretimes > 0 && _beginRotate && checkStopPointCount > 5)
                {
                    _turnleft.Set();
                    checkStopPointCount = 0;
                    lock (_locker)
                    {
                        _beginRotate = false;
                    }
                }

                //判断比幅测向是否完成
                if (_measuretimes == 0)
                {
                    if (Math.Abs(_currentAzimuth - 0) < OffSet && MeasureCount % 2 == 0 && checkAmpdfCompleteCount > 5)
                    {
                        dataAngle.Completed = true;
                        lock (_locker)
                        {
                            _beginRotate = false;
                        }

                        checkAmpdfCompleteCount = 0;
                        checkStartPointCount = 0;
                    } //判断是否转动到360°时，当转动角度和360°相差2°以内，且5次仍未调整到360°，则认为已经到360°
                    else if (Math.Abs(_currentAzimuth - 360) < OffSet && MeasureCount % 2 == 1 &&
                             checkAmpdfCompleteCount > 5)
                    {
                        dataAngle.Completed = true;
                        lock (_locker)
                        {
                            _beginRotate = false;
                        }

                        checkAmpdfCompleteCount = 0;
                        checkStopPointCount = 0;
                    }

                    checkAmpdfCompleteCount++;
                }

                data.Add(dataAngle);
                SendData(data);
            }

            #endregion

            #region 转动角度功能

            //Movement==3 转动角度功能
            else if (Movement == 3)
            {
                if (Math.Abs(_currentAzimuth - AzimuthAngle) < OffSet) checkRotateAzimuthCount++;
                //如果转台连续5次仍未调整到需要转动的角度，则认为此次转动角度完成，发送当前真实的角度值
                if (Math.Abs(AzimuthAngle - _currentAzimuth) < 1e-9 || checkRotateAzimuthCount > 5)
                {
                    dataAngle.Completed = true;
                    checkRotateAzimuthCount = 0;
                }

                data.Add(dataAngle);
                SendData(data);
            }

            #endregion

            #region 收天线功能

            //Movement==1 收天线功能
            else if (Movement == 1)
            {
                if (Math.Abs(_currentAzimuth - 0) < OffSet) checkRecoverAntennaCount++;
                //由于转台转动固有误差，如果连续转动5次仍未调整到0°处，则认为转台已经转动到0°处，发送当前的真实角度值
                if (_currentAzimuth == 0 || checkRecoverAntennaCount > 10)
                {
                    dataAngle.Completed = true;
                    checkRecoverAntennaCount = 0;
                }

                data.Add(dataAngle);
                SendData(data);
            }

            #endregion
        }
    }

    /// <summary>
    ///     转台调整角度
    /// </summary>
    private void RotateAzimuth()
    {
        if (AzimuthAngle >= 100)
        {
            Thread.Sleep(500); //设置YeasuG1000DXC参数需要一定的时间，延时500ms保证参数正确设置到YeasuG1000DXC中
            lock (_locker)
            {
                _serialport.WriteLine($"M{AzimuthAngle}\r\n");
            }
        }
        else if (AzimuthAngle >= 10)
        {
            Thread.Sleep(500); //设置YeasuG1000DXC参数需要一定的时间，延时500ms保证参数正确设置到YeasuG1000DXC中
            lock (_locker)
            {
                _serialport.WriteLine($"M0{AzimuthAngle}\r\n");
            }
        }
        else
        {
            Thread.Sleep(500); //设置YeasuG1000DXC参数需要一定的时间，延时500ms保证参数正确设置到YeasuG1000DXC中
            lock (_locker)
            {
                _serialport.WriteLine($"M00{AzimuthAngle}\r\n");
            }
        }
    }

    #endregion
}