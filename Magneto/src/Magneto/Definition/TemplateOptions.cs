using System.Collections.Generic;
using CommandLine;

namespace Magneto.Definition;

[Verb("template", HelpText = "生成和上传模板文件（-g和-t至少必须存在一个，如果都存在，则先生成，后上传，路径由-p指定）")]
public class TemplateOptions
{
    [Option('g', "generate", Order = 1, Group = "gAndt", Required = false,
        HelpText = "生成模板，通过-p指定dll路径或目录，若未指定路径，则为当前目录")]
    public string Generate { get; set; }

    [Option('t', "transfer", Order = 2, Group = "gAndt", Required = false,
        HelpText = "指定云端URL，并上传模版，通过-p指定dll路径或目录，若未指定路径，则为当前目录")]
    public string Uri { get; set; }

    [Option('p', "path", Order = 3, Required = false, HelpText = "指定生成或上传模版的本地路径，可以是文件或目录")]
    public IEnumerable<string> Path { get; set; }

    [Option('o', "output", Order = 4, Required = false, HelpText = "指定生成的目标路径，为空代表程序根目录")]
    public string OutputPath { get; set; }
}