using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CCC;
using Core;
using Core.Configuration;
using Core.Define;
using Core.Tasks;
using Core.Utils;
using Magneto.Contract;
using Magneto.Protocol.Define;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Utils = Magneto.Contract.Utils;

namespace Magneto.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public bool State { get; set; }
    public string StateInfo { get; set; }
    public string EdgeName { get; set; }
    public List<ModuleInfo> DeviceInfo { get; set; }
    public List<ModuleInfo> DriverInfo { get; set; }
    public List<TaskInfo> TaskInfo { get; set; }
    public List<BatchedCrondInfo> PlanInfo { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public int ClientCount { get; set; }
    public int TaskCount { get; set; }
    public List<ClientInfo> ClientList { get; set; }
    public string Version { get; set; }
    public string Time { get; set; }
    public string IpAddress { get; set; }
    public decimal Compass { get; set; }
    public string CpuUseage { get; set; }
    public string HddUseage { get; set; }
    public string MemoryUseage { get; set; }
    public EdgeCapacity Capacity { get; set; }

    public void OnGet()
    {
        State = RunningInfo.CloudState;
        if (State)
            StateInfo = "连接正常";
        else
            StateInfo = "未连接";
        if (StationConfig.Instance.Station == null)
            EdgeName = "云端未连接，请配置并连接云端以获取边缘端配置";
        else
            EdgeName = StationConfig.Instance.Station.Name;
        DeviceInfo = Manager.Instance.GetDeviceState();
        TaskInfo = Manager.Instance.GetTaskList();
        DriverInfo = DriverConfig.Instance.Drivers;
        PlanInfo = Manager.Instance.GetPlanList();
        var gps = RunningInfo.BufGpsData;
        if (gps != null)
        {
            var ew = gps.Longitude > 0 ? "E" : "W";
            var ns = gps.Latitude > 0 ? "N" : "S";
            Longitude = $"{Math.Abs(gps.Longitude):0.000000}{ew}";
            Latitude = $"{Math.Abs(gps.Latitude):0.000000}{ns}";
        }
        else
        {
            Longitude = "-";
            Latitude = "-";
        }

        ClientList = Server.GetClients();
        ClientCount = ClientList?.Count ?? 0;
        TaskCount = TaskInfo?.Count ?? 0;
        Version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        IpAddress = RunningInfo.EdgeIp;
        Time = Utils.GetNowTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
        Compass = decimal.Round(new decimal(RunningInfo.BufCompassData.Heading), 2);
        CpuUseage = $"{RunningInfo.CpuUseage}%";
        HddUseage = $"{RunningInfo.HddUsed}/{RunningInfo.HddTotal}GB ({RunningInfo.DataDir})";
        MemoryUseage = $"{RunningInfo.MemoryUsed}/{RunningInfo.MemoryTotal}GB";
        Capacity = Manager.Instance.GetEdgeCapacity();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        _ = Task.Run(SystemControl.Restart).ConfigureAwait(false);
        return RedirectToPage("./Index");
    }
}