namespace Magneto.Contract;

public static class PublicDefine
{
    public const int DataReturnId = 0;

    /// <summary>
    ///     数据发送间隔（ms）
    /// </summary>
    public static int DataSpan = 30;

    #region 文件夹与配置定义

    /// <summary>
    ///     配置文件保存位置
    /// </summary>
    public const string PathConfig = "configuration";

    public const string ComparisonTemplate = "ComparisonTemplate";

    /// <summary>
    ///     动态库所在文件夹
    /// </summary>
    public const string PathLibrary = "library";

    /// <summary>
    ///     持久化数据保存位置
    /// </summary>
    public const string PathSavedata = "data";

    /// <summary>
    ///     持久化IQ数据保存位置
    /// </summary>
    public const string PathSaveiqdata = "iqData";

    /// <summary>
    ///     航空监测数据保存位置
    /// </summary>
    public const string PathAvicg = "avicg";

    /// <summary>
    ///     日志保存位置
    /// </summary>
    public const string PathLog = "log";

    /// <summary>
    ///     视频文件存放位置
    /// </summary>
    public const string PathVideo = "videos";

    /// <summary>
    ///     天线因子存放位置
    /// </summary>
    public const string PathAntfactor = "antFactor";

    /// <summary>
    ///     信号解调数据存放位置
    /// </summary>
    public const string PathSgldec = "sgldec";

    /// <summary>
    ///     站点配置文件
    /// </summary>
    public const string FileConfigStation = "station.json";

    /// <summary>
    ///     设备配置文件
    /// </summary>
    public const string FileConfigDevice = "device.json";

    /// <summary>
    ///     功能配置文件
    /// </summary>
    public const string FileConfigDriver = "driver.json";

    /// <summary>
    ///     计划任务配置文件
    /// </summary>
    public const string FileConfigCrondtask = "crondTask.json";

    /// <summary>
    ///     功能文件夹
    /// </summary>
    public const string PathDriver = "Driver";

    /// <summary>
    ///     设备文件夹
    /// </summary>
    public const string PathDevice = "Device";

    public const string PathTask = "CrondTasks";

    #endregion

    #region 存储相关

    /// <summary>
    ///     日报月报文件扩展名
    /// </summary>
    public const string ExtensionReport = ".mth";

    /// <summary>
    ///     信号普查文件扩展名
    /// </summary>
    public const string ExtensionSignalCensus = ".cus";

    /// <summary>
    ///     临时文件扩展名
    /// </summary>
    public const string ExtensionTemporary = ".temp";

    #endregion
}