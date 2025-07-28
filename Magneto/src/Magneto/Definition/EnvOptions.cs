using CommandLine;

namespace Magneto.Definition;

[Verb("env", HelpText = "动环控制配置信息管理")]
public class EnvOptions
{
    [Option('l', "local-port", Order = 1, Required = false, HelpText = "边缘端端口号")]
    public string LocalPort { get; init; }

    [Option('a', "remote-address", Order = 2, Required = false, HelpText = "云端IP地址")]
    public string RemoteAddress { get; init; }

    [Option('r', "remote-port", Order = 3, Required = false, HelpText = "云端端口号")]
    public string RemotePort { get; init; }

    [Option('i', "identifier", Order = 4, Required = false, HelpText = "边缘端唯一标识号")]
    public string Identifier { get; init; }

    [Option('c', "config", Order = 5, Required = false, HelpText = "边缘端配置文件路径（json格式）")]
    public string Config { get; init; }

    public static implicit operator MainOptions(EnvOptions options)
    {
        var temp = new MainOptions
        {
            Config = options.Config,
            Identifier = options.Identifier,
            LocalPort = options.LocalPort,
            RemoteAddress = options.RemoteAddress,
            RemotePort = options.RemotePort
        };
        return temp;
    }
}