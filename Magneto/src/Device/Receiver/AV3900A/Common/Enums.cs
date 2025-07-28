namespace Magneto.Device.AV3900A.Common;

public enum SalErrorType
{
    /// <summary>
    ///     No Error
    /// </summary>
    SalErrNone = 0,

    /// <summary>
    ///     This functionality is not implemented yet.
    /// </summary>
    SalErrNotimplemented = -1,

    /// <summary>
    ///     Error of unspecified type
    /// </summary>
    SalErrUnknown = -2,

    /// <summary>
    ///     The system is busy
    /// </summary>
    SalErrBusy = -3,

    /// <summary>
    ///     Unspecified error
    /// </summary>
    SalErrTruncated = -4,

    /// <summary>
    ///     The measurement was aborted
    /// </summary>
    SalErrAborted = -5,

    /// <summary>
    ///     The server accepted the call but returned no result
    /// </summary>
    SalErrRpcNoresult = -6,

    /// <summary>
    ///     The RPC call to the server failed completely
    /// </summary>
    SalErrRpcFail = -7,

    /// <summary>
    ///     Incorrect parameter in call.
    /// </summary>
    SalErrParam = -8,

    /// <summary>
    ///     Another measurement is currently in progress
    /// </summary>
    SalErrMeasInProgress = -9,

    /// <summary>
    ///     No result was returned
    /// </summary>
    SalErrNoResult = -10,

    /// <summary>
    ///     The sensor name specified already exists
    /// </summary>
    SalErrSensorNameExists = -11,

    /// <summary>
    ///     The calibration file has an invalid format
    /// </summary>
    SalErrInvalidCalFile = -12,

    /// <summary>
    ///     The antenna path specified does not exist
    /// </summary>
    SalErrNoSuchAntennapath = -13,

    /// <summary>
    ///     The sensor name specified does not exist
    /// </summary>
    SalErrInvalidSensorName = -14,

    /// <summary>
    ///     The given measurement ID is not valid
    /// </summary>
    SalErrInvalidMeasurementId = -15,

    /// <summary>
    ///     Internal system error
    /// </summary>
    SalErrInvalidRequest = -16,

    /// <summary>
    ///     You need to specify map coordinates
    /// </summary>
    SalErrMissingMapParameters = -17,

    /// <summary>
    ///     The measurement arrived at the sensor too late
    /// </summary>
    SalErrTooLate = -18,

    /// <summary>
    ///     An HTTP error occurred when trying to talk to the sensors
    /// </summary>
    SalErrHttpTransport = -19,

    /// <summary>
    ///     No sensors available for measurement
    /// </summary>
    SalErrNoSensors = -20,

    /// <summary>
    ///     Not enough timeseries in measurement
    /// </summary>
    SalErrNotEnoughTimeseries = -21,

    /// <summary>
    ///     Error in native code
    /// </summary>
    SalErrNative = -22,

    /// <summary>
    ///     Invalid sensor location
    /// </summary>
    SalErrBadSensorLocation = -23,

    /// <summary>
    ///     Data Channel already open
    /// </summary>
    SalErrDataChannelOpen = -24,

    /// <summary>
    ///     Data Channel not open
    /// </summary>
    SalErrDataChannelNotOpen = -25,

    /// <summary>
    ///     Socket error
    /// </summary>
    SalErrSocketError = -26,

    /// <summary>
    ///     Sensor not connected
    /// </summary>
    SalErrSensorNotConnected = -27,

    /// <summary>
    ///     No data available
    /// </summary>
    SalErrNoDataAvailable = -28,

    /// <summary>
    ///     No SMS Available
    /// </summary>
    SalErrNoSms = -29,

    /// <summary>
    ///     User data buffer too small for data
    /// </summary>
    SalErrBufferTooSmall = -30,

    /// <summary>
    ///     A diagnostic error occurred
    /// </summary>
    SalErrDiagnostic = -31,

    /// <summary>
    ///     No more msgs in the Error Queue
    /// </summary>
    SalErrQueueEmpty = -32,

    /// <summary>
    ///     Sensor set to the wrong service (see salSetService())
    /// </summary>
    SalErrWrongService = -33,

    /// <summary>
    ///     Could not allocate memory
    /// </summary>
    SalErrMemory = -34,

    /// <summary>
    ///     User supplied handle was invalid
    /// </summary>
    SalErrInvalidHandle = -35,

    /// <summary>
    ///     Attempt to connect to sensor failed
    /// </summary>
    SalErrSensorConnect = -36,

    /// <summary>
    ///     SMS refused to issue token
    /// </summary>
    SalErrSmsNoToken = -37,

    /// <summary>
    ///     Sensor command failed
    /// </summary>
    SalErrCommandFailed = -38,

    /// <summary>
    ///     Could not get locate result history
    /// </summary>
    SalErrNoLocateHistory = -39,

    /// <summary>
    ///     Measurement timed out
    /// </summary>
    SalErrTimeout = -40,

    /// <summary>
    ///     Requested location image size too big
    /// </summary>
    SalErrImageSize = -41,

    /// <summary>
    ///     Requested antenna type not valid
    /// </summary>
    SalErrInvalidAntenna = -42,

    /// <summary>
    ///     Input string too long
    /// </summary>
    SalErrStringTooLong = -43,

    /// <summary>
    ///     Requested timeout value not valid
    /// </summary>
    SalErrInvalidTimeout = -44,

    /// <summary>
    ///     Sensor index not valid
    /// </summary>
    SalErrInvalidSensorIndex = -45,

    /// <summary>
    ///     Requested trigger type not valid
    /// </summary>
    SalErrInvalidTriggerType = -46,

    /// <summary>
    ///     Requested Doppler compensation not valid
    /// </summary>
    SalErrInvalidDopplerComp = -47,

    /// <summary>
    ///     Maximum number of sensors already added to group
    /// </summary>
    SalErrNumSensors = -48,

    /// <summary>
    ///     Operation not valid on empty sensor group
    /// </summary>
    SalErrEmptyGroup = -49,

    /// <summary>
    ///     Handle can not be closed because it is in use
    /// </summary>
    SalErrHandleInUse = -50,

    /// <summary>
    ///     Requested salDataType not valid for measurement
    /// </summary>
    SalErrDataType = -52,

    /// <summary>
    ///     Sensor measurement server communications error
    /// </summary>
    SalErrSensorServer = -53,

    /// <summary>
    ///     Request for time data that is not in sensor memory
    /// </summary>
    SalErrTimeNotInStream = -54,

    /// <summary>
    ///     Requested frequency is outside of current tuner range
    /// </summary>
    SalErrFreqNotInStream = -55,

    /// <summary>
    ///     Measurement requires sensor in lookback mode
    /// </summary>
    SalErrNotInLookback = -56,

    /// <summary>
    ///     Error authorizing current application and user on the sensor
    /// </summary>
    SalErrAuthorization = -57,

    /// <summary>
    ///     Could not obtain a lock on tuner resource
    /// </summary>
    SalErrTunerLock = -58,

    /// <summary>
    ///     Could not obtain a lock on FFT resource
    /// </summary>
    SalErrFftLock = -59,

    /// <summary>
    ///     Could not obtain a lock on requested resource
    /// </summary>
    SalErrLockFailed = -60,

    /// <summary>
    ///     RF Sensor data stream terminated unexpectedly
    /// </summary>
    SalErrSensorDataEnd = -61,

    /// <summary>
    ///     Requested measurement span is not valid
    /// </summary>
    SalErrInvalidSpan = -62,

    /// <summary>
    ///     Requested geolocation algorithm is not available
    /// </summary>
    SalErrInvalidAlgorithm = -63,

    /// <summary>
    ///     License error
    /// </summary>
    SalErrLicense = -64,

    /// <summary>
    ///     End of list reached
    /// </summary>
    SalErrListEnd = -65,

    /// <summary>
    ///     The measurement failed of timed out with no results
    /// </summary>
    SalErrMeasFailed = -66,

    /// <summary>
    ///     Function not supported in embedded apps.
    /// </summary>
    SalErrEmbedded = -67,

    /// <summary>
    ///     Exception in SMS processing
    /// </summary>
    SalErrSmsException = -68,

    /// <summary>
    ///     SDRAM overflow in sensor
    /// </summary>
    SalSdramOverflow = -69,

    /// <summary>
    ///     NO free DMA Buffers in sensor
    /// </summary>
    SalNoDmaBuffer = -70,

    /// <summary>
    ///     DMA FIFO Underflow in sensor
    /// </summary>
    SalDmaFifoUnderflow = -71,

    /// <summary>
    ///     FFT Setup Error
    /// </summary>
    SalFftSetupError = -72,

    /// <summary>
    ///     Measurement trigger timeout in sensor
    /// </summary>
    SalTriggerTimeout = -73,

    /// <summary>
    ///     Measurement stream problem in sensor
    /// </summary>
    SalNoStreamData = -74,

    /// <summary>
    ///     Measurement data available timeout in sensor
    /// </summary>
    SalDataAvailTimeout = -75,

    /// <summary>
    ///     Tuner not streaming in sensor
    /// </summary>
    SalTunerNotStreaming = -76,

    /// <summary>
    ///     this should ALWAYS EQUAL the last valid error message
    /// </summary>
    SalErrNum = -76
}

public enum SalFftDataType
{
    /// <summary>
    ///     dBm data from sensor
    /// </summary>
    FftDataDb,

    /// <summary>
    ///     v^2 data from sensor
    /// </summary>
    FftDataMag
}

public enum SalMonitorMode
{
    /// <summary>
    ///     Do not use monitor mode
    /// </summary>
    MonitorModeOff,

    /// <summary>
    ///     If there is an FFT measurement running on the sensor,send data in "eavesdrop mode"
    /// </summary>
    MonitorModeOn
}

public enum SalOverlapType
{
    /// <summary>
    ///     Use overlap averaging. Note that enum value = 0 for backward comapatability.
    /// </summary>
    OverlapOn,

    /// <summary>
    ///     Do not use overlap averaging.
    /// </summary>
    OverlapOff
}

public enum SalWindowType
{
    /// <summary>
    ///     Hann/Hanning window ( conversion from RBW to FFT bin spacing: 1.5 )
    /// </summary>
    WindowHann,

    /// <summary>
    ///     Gausstop window ( conversion from RBW to FFT bin spacing: 2.215349684 )
    /// </summary>
    WindowGaussTop,

    /// <summary>
    ///     Flattop window  ( conversion from RBW to FFT bin spacing: 3.822108760 )
    /// </summary>
    WindowFlatTop,

    /// <summary>
    ///     Uniform window  ( conversion from RBW to FFT bin spacing: 1.0 )
    /// </summary>
    WindowUniform,
    WindowUnknown
}

public enum SalSweepCommand
{
    /// <summary>
    ///     Stop a sweep when the sweep is finished
    /// </summary>
    Stop,

    /// <summary>
    ///     Stop a sweep as soon as possible
    /// </summary>
    Abort,

    /// <summary>
    ///     Flush the sweep backlog
    /// </summary>
    Flush
}

public enum SalTimeDataCmd
{
    /// <summary>
    ///     Stop a time data request, but keep sends data acquired so far
    /// </summary>
    TimeDataCmdStop,

    /// <summary>
    ///     Stop a time data request and discard any data not sent
    /// </summary>
    TimeDataCmdAbort
}

/// <summary>
///     天线类型
///     todo: 需要验证枚举值含义
/// </summary>
public enum SalAntennaType
{
    Antenna1,
    Antenna2
}

/// <summary>
///     平均类型
///     todo: 需要验证枚举值含义
/// </summary>
public enum SalAverageType
{
    Off,
    Rms,
    Peak,
    Unknown
}

public enum SalLocalization
{
    /// <summary>
    ///     English language
    /// </summary>
    English
}

public enum GpsMode
{
    GpsFixMode,
    GpsPvtMode
}

public enum SalDemodulation
{
    /// <summary>
    ///     No Demodulation
    /// </summary>
    None = 0,

    /// <summary>
    ///     AM Demodulation
    /// </summary>
    Am = 1,

    /// <summary>
    ///     FM Demodulation
    /// </summary>
    Fm = 2
}

public enum SalDemodCmd
{
    DemodCmdStop,
    DemodCmdAbort
}

public enum SalTriggerSlope
{
    /// <summary>
    ///     rising edge trigger
    /// </summary>
    Rising = 0,

    /// <summary>
    ///     falling edge trigger
    /// </summary>
    Falling = 1,

    /// <summary>
    ///     trigger on either edge
    /// </summary>
    Either = 2
}

public enum SalTimeTrigType
{
    TimeTrigNone,
    TimeTrigAbstime,
    TimeTrigReltime
}

public enum SalLevelTrigType
{
    LevelTrigNone,
    LevelTrigAbslevel,
    LevelTrigRising,
    LevelTrigFalling
}

public enum SalSensorMode
{
    /// <summary>
    ///     No Measurement
    /// </summary>
    SensorModeNone = 0,

    /// <summary>
    ///     TDOA measurement
    /// </summary>
    SensorModeTdoa = 3,

    /// <summary>
    ///     TDOA measurement
    /// </summary>
    SensorModeLookback = 4,

    /// <summary>
    ///     E3238s or IQ measurement
    /// </summary>
    SensorModeDefault = 100,

    /// <summary>
    ///     Error mode
    /// </summary>
    ErrMode = -1
}