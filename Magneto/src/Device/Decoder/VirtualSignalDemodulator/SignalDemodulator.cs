using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Contract.SignalDemod;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualSignalDemodulator;

public partial class SignalDemodulator : DeviceBase
{
    private readonly List<(string rat, string bw, string fileName)> _fileList = new();
    private readonly string _templatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sgldecTemplates");
    private Guid _analysisId;
    private CancellationTokenSource _cts;
    private volatile bool _running;
    private Task _task;

    public SignalDemodulator(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var success = base.Initialized(device);
        if (!success) return false;
        _fileList.Clear();
        var files = Directory.GetFiles(_templatesFolder, "*.txt");
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var match = Regex.Match(name, @"(?<rat>\w+)_(?<bw>\d+[k|M|G]?Hz)", RegexOptions.IgnoreCase);
            if (!match.Success) continue;
            var rat = match.Groups["rat"].Value;
            var bw = match.Groups["bw"].Value;
            _fileList.Add((rat, bw, file));
        }

        if (ExactModeSwitch)
        {
            AnalysisService.Instance.Disconnected += Instance_Disconnected;
            try
            {
                var b = AnalysisService.Instance.InitAsync(IpAddress, Port, CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                if (!b)
                {
                    Trace.WriteLine("核心分析服务不存在");
                    return false;
                }

                b = AnalysisService.Instance.IsAvaliableAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (!b)
                    b = AnalysisService.Instance.StartAnalysisCoreAsync().ConfigureAwait(false).GetAwaiter()
                        .GetResult();
                if (!b)
                {
                    Trace.WriteLine("启动核心分析进程失败");
                    return false;
                }

                AnalysisService.Instance.AnalysisCompleted += Instance_AnalysisCompleted;
                Trace.WriteLine("核心分析服务运行正常");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"初始化异常，异常信息:{ex}");
                return false;
            }
        }

        return true;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (feature != FeatureType.SGLDEC) throw new Exception("不支持的功能");
        if (string.IsNullOrWhiteSpace(IqFileName)) throw new Exception("不合法的文件名");
        if (!File.Exists(IqFileName)) throw new Exception("文件不存在");
        _running = true;
        if (!AnalysisMode && ExactModeSwitch)
        {
            _analysisId = Guid.NewGuid();
            var result = AnalysisService.Instance
                .AnalysisDataAsync(IqFileName, CalculateSymbolRate, _analysisId, RunningInfo.EdgeId,
                    DeviceId.ToString("N")).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!result)
            {
                _running = false;
                throw new Exception("启动分析失败");
            }
        }
        else
        {
            var fileName = Path.GetFileNameWithoutExtension(IqFileName);
            {
                var match = Regex.Match(fileName, @"(?<bw>\d+(\.\d+)?[k|M|G]?Hz)$");
                string[] txtFileNames = null;
                if (match.Success)
                {
                    var bw = match.Groups["bw"].Value;
                    txtFileNames = _fileList.Where(p => IsBwEqual(p.bw, bw)).Select(p => p.fileName).ToArray();
                }

                if (txtFileNames?.Any() != true) throw new Exception("未找到对应带宽的模板文件");
                _cts = new CancellationTokenSource();
                _task = new Task(() => FastAnalysis(txtFileNames), _cts.Token);
            }

            _task.Start();
        }
    }

    public override void Stop()
    {
        _running = false;
        Utils.CancelTask(_task, _cts);
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Utils.CancelTask(_task, _cts);
    }

    private void Instance_AnalysisCompleted(object sender, AnalysisResult e)
    {
        if (e == null || e.EdgeId != RunningInfo.EdgeId || e.DeviceId != DeviceId.ToString("N") || !_running ||
            _analysisId != e.Id) return;
        SendData(new List<object> { e.Data });
    }

    private void Instance_Disconnected(object sender, EventArgs e)
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
}