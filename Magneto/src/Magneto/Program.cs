using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using CCC;
using CommandLine;
using Core;
using Core.Business;
using Core.PublicService;
using Magneto.Contract;
using Magneto.Contract.Http;
using Magneto.Definition;
using Magneto.Protocol.Define;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;

namespace Magneto;

public static class Program
{
    private static int _port;
    private static string _jsonPath = string.Empty;
    private static WebApplication _webApp;
    public static AppSettings Settings { get; private set; }

    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        // args = new string[] { "template", "-g", "-t", "-p" };
        //加载.json配置文件
        _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        if (!File.Exists(_jsonPath) && args?.Length == 0)
        {
            Settings = new AppSettings
            {
                EdgeId = "00001",
                IpAddress = "0.0.0.0",
                Port = 5001,
                CloudIpAddress = "127.0.0.1",
                CloudPort = 1000,
                ServerType = 0,
                CloudUser = "dc_admin",
                CloudPassword = "456789",
                AudioRecogAddress = "127.0.0.1",
                AudioRecogPort = 11011,
                AudioRecogServerKey = "decentest_1234567890"
            };
            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(_jsonPath, json);
        }

        CheckLibrary();
        ReadSettings();
        //有启动参数
        if (args?.Length > 0)
        {
            var checkResult = CheckArgs(args);
            if (checkResult == CommandType.Duplicate)
            {
                Trace.WriteLine("Command不能重复");
                return;
            }

            //解析参数
            Parser.Default.Settings.MaximumDisplayWidth = Console.WindowWidth < 120 ? 120 : Console.WindowWidth;
            var exitsResult = Parser.Default.ParseArguments<TemplateOptions, MainOptions, EnvOptions>(args).MapResult(
                (TemplateOptions temp) => RunTemplate(temp),
                (MainOptions config) => RunConfig(config),
                (EnvOptions config) => RunEnvConfig(config),
                HandleParseError);
            if (exitsResult == 0) return;
            if (checkResult is CommandType.Help or CommandType.Template) return;
        }

        if (Settings == null)
        {
            Trace.WriteLine("缺少配置信息，无法正常启动");
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSavedata);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        DisplayInfo(); //打印信息
        RunProcess(); //启动进程
        // Console.ReadLine(); 在服务模式以及docker下 此条语句无效
    }

    private static void ReadSettings()
    {
        if (!File.Exists(_jsonPath)) return;
        var json = File.ReadAllText(_jsonPath);
        // _settings = JsonConvert.DeserializeObject<AppSettings>(json);
        var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        var isWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var target = isWindow ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.Process;
        Settings = new AppSettings();
        if (settings.TryGetValue("ipAddress", out var setting))
        {
            var value = setting.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.IpAddress = env;
        }

        if (settings.TryGetValue("port", out var setting1))
        {
            var value = setting1.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            if (int.TryParse(env, out var port)) Settings.Port = port;
        }

        if (settings.TryGetValue("cloudIpAddress", out var setting2))
        {
            var value = setting2.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.CloudIpAddress = env;
        }

        if (settings.TryGetValue("cloudPort", out var setting3))
        {
            var value = setting3.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            if (int.TryParse(env, out var port)) Settings.CloudPort = port;
        }

        if (settings.TryGetValue("edgeID", out var setting4))
        {
            var value = setting4.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.EdgeId = env;
        }

        if (settings.TryGetValue("type", out var setting5))
        {
            var value = setting5.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            if (int.TryParse(env, out var type)) Settings.ServerType = type;
        }

        if (settings.TryGetValue("ipType", out var setting6))
        {
            var value = setting6.ToString();
            var env = value;
            if (value != null && value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            if (int.TryParse(env, out var type)) Settings.IpType = type;
        }

        if (settings.TryGetValue("cloudUser", out var setting7))
        {
            var value = setting7.ToString();
            var env = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.CloudUser = env;
        }

        if (settings.TryGetValue("cloudPassword", out var setting8))
        {
            var value = setting8.ToString();
            var env = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.CloudPassword = env;
        }

        if (settings.TryGetValue("audioRecogAddress", out var setting9))
        {
            var value = setting9.ToString();
            var env = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.AudioRecogAddress = env;
        }

        if (settings.TryGetValue("audioRecogPort", out var setting10))
        {
            var value = setting10.ToString();
            var env = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            if (int.TryParse(env, out var port)) Settings.AudioRecogPort = port;
        }

        if (settings.TryGetValue("audioRecogServerKey", out var setting11))
        {
            var value = setting11.ToString();
            var env = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                env = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.AudioRecogServerKey = env;
        }

        if (settings.TryGetValue("computerId", out var setting12))
        {
            var value = setting12.ToString();
            var computerId = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                computerId = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.ComputerId = computerId;
        }

        if (settings.TryGetValue("timeout", out var setting13))
        {
            var value = setting13.ToString();
            var timeout = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                timeout = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.Timeout = int.TryParse(timeout, out var num) ? num : 15;
        }

        if (settings.TryGetValue("frameDynamic", out var setting14))
        {
            var value = setting14.ToString();
            var frameDynamic = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                frameDynamic = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.FrameDynamic = !bool.TryParse(frameDynamic, out var bl) || bl;
        }

        if (settings.TryGetValue("frameSpan", out var setting15))
        {
            var value = setting15.ToString();
            var frameSpan = value;
            if (value.StartsWith("$"))
            {
                value = value[1..];
                frameSpan = Environment.GetEnvironmentVariable(value, target);
            }

            Settings.FrameSpan = int.TryParse(frameSpan, out var num) ? num : 30;
        }

        // if (settings.ContainsKey("dataDir"))
        // {
        //     var value = settings["dataDir"].ToString();
        //     var dataDir = value;
        //     if (value.StartsWith("$"))
        //     {
        //         value = value[1..];
        //         dataDir = Environment.GetEnvironmentVariable(value, target);
        //     }
        //     _settings.DataDir = dataDir;
        // }
        if (string.IsNullOrEmpty(Settings.CloudUser)) Settings.CloudUser = "dc_admin";
        if (string.IsNullOrEmpty(Settings.CloudPassword)) Settings.CloudPassword = "456789";
        if (string.IsNullOrEmpty(Settings.AudioRecogAddress)) Settings.AudioRecogAddress = Settings.CloudIpAddress;
        if (Settings.AudioRecogPort == 0) Settings.AudioRecogPort = 11011;
        if (string.IsNullOrEmpty(Settings.AudioRecogServerKey)) Settings.AudioRecogServerKey = "decentest_1234567890";
        if (string.IsNullOrEmpty(Settings.ComputerId)) Settings.ComputerId = Settings.EdgeId;
    }

    /// <summary>
    ///     启动时检查动态库文件夹
    ///     windows下可以不用管
    ///     但是在linux下，如果存在dll，则加载同名的so文件会失败
    /// </summary>
    private static void CheckLibrary()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathLibrary);
            if (!Directory.Exists(dir)) return;
            ClearLibrary(dir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理库文件失败:{ex}");
        }
    }

    private static void ClearLibrary(string dir)
    {
        var info = new DirectoryInfo(dir);
        foreach (var file in info.GetFiles())
            if (file.Extension == ".dll")
            {
                Console.WriteLine($"清理文件:{file.FullName}");
                file.Delete();
            }
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var error = e.ExceptionObject as Exception;
        MessageManager.Instance.Error("Main", error?.Message, error);
    }

    /// <summary>
    ///     展示信息
    /// </summary>
    private static void DisplayInfo()
    {
        Trace.WriteLine("**********************************************************");
        Trace.WriteLine("");
        Trace.WriteLine($"服务地址:ws://{Settings.IpAddress}:{Settings.Port}");
        Trace.WriteLine($"侦听IP:{Settings.IpAddress}");
        Trace.WriteLine($"侦听Port:{Settings.Port}");
        Trace.WriteLine($"云端IP:{Settings.CloudIpAddress}");
        Trace.WriteLine($"云端Port:{Settings.CloudPort}");
        Trace.WriteLine($"标识号:{Settings.EdgeId}");
        Trace.WriteLine($"IP类型:{Settings.IpType}");
        Trace.WriteLine($"服务类型:{Settings.ServerType}");
        Trace.WriteLine($"电脑ID:{Settings.ComputerId}");
        Trace.WriteLine("");
        Trace.WriteLine("**********************************************************");
    }

    private static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Trace.WriteLine("检测到Ctrl+C，程序退出...");
        Environment.Exit(0);
    }

    private static CommandType CheckArgs(string[] args)
    {
        if (args.Contains("-h") || args.Contains("--help")) return CommandType.Help;
        if (args.Contains("template"))
        {
            if (args.Contains("main") || args.Contains("env")) return CommandType.Duplicate;
            return CommandType.Template;
        }

        if (args.Contains("main"))
        {
            if (args.Contains("env")) return CommandType.Duplicate;
            return CommandType.Main;
        }

        if (args.Contains("env")) return CommandType.Env;
        //输入错误，显示帮助信息
        return CommandType.Help;
    }

    private static void ProcessExit(object sender, EventArgs e)
    {
        Trace.WriteLine("程序退出...");
        Manager.Instance.Close();
        // _webHost?.Dispose();
        _ = Server.CloseServerAsync(_port);
    }

    /// <summary>
    ///     解析Template
    /// </summary>
    /// <param name="options"></param>
    private static int RunTemplate(TemplateOptions options)
    {
        var newList = new List<string>();
        if (options.Path?.Any() != true || (options.Path.Count() == 1 && options.Path.FirstOrDefault()?.Length == 0))
        {
            newList =
            [
                TemplateServer.Instance.GetDefaultDevicePath(),
                TemplateServer.Instance.GetDefaultDriverPath()
            ];
            Trace.WriteLine("模板文件使用默认路径");
        }
        else
        {
            var count = options.Path.Count();
            var tempList = new List<string>(options.Path);
            for (var i = 0; i < count; i++)
                //绝对路径
                if (Directory.Exists(tempList[i]))
                {
                    newList.Add(tempList[i]);
                }
                else if (File.Exists(tempList[i]))
                {
                    newList.Add(tempList[i]);
                }
                else
                {
                    //相对目录
                    var newPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempList[i]);
                    if (Directory.Exists(newPath))
                    {
                        newList.Add(newPath);
                    }
                    else if (File.Exists(newPath))
                    {
                        newList.Add(newPath);
                    }
                    else
                    {
                        Trace.WriteLine("路径输入有误，请重新输入");
                        return 0;
                    }
                }
        }

        if (options.Uri != null)
        {
            //上传
            if (options.Uri?.Length == 0)
            {
                if (Settings != null)
                {
                    Trace.WriteLine($"采用默认的URL: http://{Settings.CloudIpAddress}:{Settings.CloudPort}/Template/add");
                    //TODO 自己拼接
                    options.Uri = "/Template/add";
                }
                else
                {
                    Trace.WriteLine("云端URI不能为空");
                    return 0;
                }
            }
            else if (options.Uri == "/Template/add")
            {
            }
            else if (options.Uri != null &&
                     !options.Uri.StartsWith($"http://{Settings.CloudIpAddress}:{Settings.CloudPort}"))
            {
                Trace.WriteLine("URL不正确，请检查后重新输入");
                return 0;
            }
            else
            {
                options.Uri = options.Uri?.Replace($"http://{Settings.CloudIpAddress}:{Settings.CloudPort}", "");
            }
        }

        var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template");
        if (!string.IsNullOrEmpty(options.OutputPath)) output = options.OutputPath;
        TemplateServer.Instance.UpdateOutputPath(output);
        options.Path = newList;
        string msg;
        if (options.Generate == null)
        {
            //不生成 一定上传
            TemplateServer.Instance.UploadTemplateToCloud(options.Path, options.Uri);
            msg = "模板上传完成";
        }
        else
        {
            //生成
            TemplateServer.Instance.GenerateTemplate(options.Path);
            msg = "模板生成完成";
            if (options.Uri != null)
            {
                //上传
                TemplateServer.Instance.UploadTemplateToCloud(options.Path, options.Uri);
                msg = "模板生成及上传完成";
            }
        }

        Trace.WriteLine($"{msg}，按任意键继续...");
        Console.ReadLine();
        return 1;
    }

    /// <summary>
    ///     解析Main命令
    /// </summary>
    /// <param name="options"></param>
    private static int RunConfig(MainOptions options)
    {
        return ParseConfig(options);
    }

    /// <summary>
    ///     解析Env命令
    /// </summary>
    /// <param name="options"></param>
    private static int RunEnvConfig(EnvOptions options)
    {
        return ParseConfig(options);
    }

    /// <summary>
    ///     解析参数
    /// </summary>
    /// <param name="options"></param>
    private static int ParseConfig(MainOptions options)
    {
        if (options.Config != null)
        {
            if (options.Config?.Length == 0)
            {
                Trace.WriteLine("指定的配置文件路径不能为空");
                return 0;
            }

            //此处只管配置文件，且不保存
            if (File.Exists(options.Config))
            {
                try
                {
                    var json = File.ReadAllText(options.Config);
                    Settings = JsonConvert.DeserializeObject<AppSettings>(json);
                }
                catch (Exception)
                {
                    Trace.WriteLine("json文件格式错误，无法启动");
                    return 0;
                }
            }
            else
            {
                Trace.WriteLine("配置文件不存在或路径错误，无法启动");
                return 0;
            }

            return 1;
        }

        if (Settings == null)
        {
            //不存在配置文件，且第一次启动，默认是边缘端
            //如果config文件不为空，则必须正确，且必须为绝对路径（不考虑相对路径）
            //如果config不为空，则不再解析其他参数
            Settings = new AppSettings
            {
                IpAddress = "0.0.0.0",
                ServerType = 0
            };
            if (string.IsNullOrEmpty(options.RemoteAddress) || !CheckHelper.IsIpAddress(options.RemoteAddress))
            {
                Trace.WriteLine("请正确配置云端IP");
                return 0;
            }

            if (string.IsNullOrEmpty(options.RemotePort) || !CheckHelper.IsInt(options.RemotePort))
            {
                Trace.WriteLine("请正确配置云端Port");
                return 0;
            }

            if (string.IsNullOrEmpty(options.LocalPort) || !CheckHelper.IsInt(options.LocalPort))
            {
                Trace.WriteLine("请正确配置本地监听Port");
                return 0;
            }

            if (string.IsNullOrEmpty(options.Identifier) || !CheckHelper.IsIdentifier())
            {
                Trace.WriteLine("请正确配置边缘端唯一标识符");
                return 0;
            }

            Settings.CloudIpAddress = options.RemoteAddress;
            Settings.CloudPort = int.Parse(options.RemotePort);
            Settings.EdgeId = options.Identifier;
            Settings.Port = int.Parse(options.LocalPort);
            //保存信息
            File.WriteAllText(_jsonPath, JsonConvert.SerializeObject(Settings));
        }
        else
        {
            if (Settings.ServerType == 1)
            {
                Trace.WriteLine("不能使用main指令启动环境控制，请使用env或-h指令");
                return 0;
            }

            if (options.RemoteAddress != null)
            {
                if (!CheckHelper.IsIpAddress(options.RemoteAddress))
                {
                    Trace.WriteLine("请正确配置云端IP");
                    return 0;
                }

                Settings.CloudIpAddress = options.RemoteAddress;
            }

            if (options.RemotePort != null)
            {
                if (!CheckHelper.IsInt(options.RemotePort))
                {
                    Trace.WriteLine("请正确配置云端Port");
                    return 0;
                }

                Settings.CloudPort = int.Parse(options.RemotePort);
            }

            if (options.LocalPort != null)
            {
                if (!CheckHelper.IsInt(options.LocalPort))
                {
                    Trace.WriteLine("请正确配置本地监听Port");
                    return 0;
                }

                Settings.Port = int.Parse(options.LocalPort);
            }

            if (options.Identifier != null)
            {
                if (!CheckHelper.IsIdentifier())
                {
                    Trace.WriteLine("请正确配置边缘端唯一标识符");
                    return 0;
                }

                Settings.EdgeId = options.Identifier;
            }
        }

        return 1;
    }

    private static int HandleParseError(IEnumerable<Error> errs)
    {
        //TODO  do something interesting
        return 1;
    }

    /// <summary>
    ///     启动
    /// </summary>
    private static void RunProcess()
    {
        StartEdge(Settings.ServerType);
    }

    /// <summary>
    ///     启动边缘端
    /// </summary>
    /// <param name="type"></param>
    private static void StartEdge(int type)
    {
        _port = Settings.Port;
        RunningInfo.EdgeId = Settings.EdgeId;
        RunningInfo.Port = Settings.Port;
        RunningInfo.CloudIpAddress = Settings.CloudIpAddress;
        RunningInfo.CloudPort = Settings.CloudPort;
        RunningInfo.ServerType = Settings.ServerType;
        RunningInfo.EdgeIp = Settings.IpAddress;
        RunningInfo.LocalIpAddress = Settings.IpAddress;
        RunningInfo.IpType = Settings.IpType;
        RunningInfo.CloudTokenUser = Settings.CloudUser;
        RunningInfo.CloudTokenPassword = Settings.CloudPassword;
        RunningInfo.AudioRecognitionAddress = Settings.AudioRecogAddress;
        RunningInfo.AudioRecognitionPort = Settings.AudioRecogPort;
        RunningInfo.AudioRecognitionServerKey = Settings.AudioRecogServerKey;
        RunningInfo.ComputerId = Settings.ComputerId;
        RunningInfo.DataDir = Settings.DataDir;
        RunningInfo.Timeout = Settings.Timeout;
        RunningInfo.FrameDynamic = Settings.FrameDynamic;
        PublicDefine.DataSpan = Settings.FrameSpan;
        _webApp = Server.CreateHostBuilder(null).Build();
        Server.CreateJsonRpcServer(_webApp, "0.0.0.0", Settings.Port, FormatterType.CustomMessagePackFormatter);
        if (type == 0)
        {
            Server.MapServer<ControlServer>(Settings.Port, Maps.MapControl);
            Server.MapServer<DataServer>(Settings.Port, Maps.MapTask);
            Server.MapServer<ControlServer>(Settings.Port, Maps.MapAtomic);
        }
        else
        {
            Server.MapServer<ControlServer>(Settings.Port, Maps.MapControl);
            Server.MapServer<ControlServer>(Settings.Port, Maps.MapAtomic);
        }

        HttpHelper.Instance.Initialized(Settings.CloudIpAddress, Settings.CloudPort);
        MessageManager.Instance.Initialized();
        Manager.Instance.Initialized();
        Debug.WriteLine("程序启动成功!");
        AppDomain.CurrentDomain.ProcessExit += ProcessExit;
        Console.CancelKeyPress += CancelKeyPress;
        Server.RunServer(_port);
    }

    private static class CheckHelper
    {
        public static bool IsIpAddress(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        public static bool IsInt(string value)
        {
            return int.TryParse(value, out var _);
        }

        public static bool IsIdentifier()
        {
            return true;
        }
    }
}