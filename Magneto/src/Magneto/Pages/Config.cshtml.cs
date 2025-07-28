using System;
using System.IO;
using System.Threading.Tasks;
using Core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Magneto.Pages;

// [Authorize]
public class ConfigModel(ILogger<ConfigModel> logger) : PageModel
{
    private readonly ILogger<ConfigModel> _logger = logger;

    [BindProperty] public string EdgeId { get; set; }

    [BindProperty] public string IpAddress { get; set; }

    [BindProperty] public int Port { get; set; }

    [BindProperty] public string CloudIp { get; set; }

    [BindProperty] public int CloudPort { get; set; }

    [BindProperty] public int Type { get; set; }

    [BindProperty] public string CloudUser { get; set; }

    [BindProperty] public string CloudPassword { get; set; }

    public IActionResult OnGet()
    {
        EdgeId = Program.Settings.EdgeId;
        Port = Program.Settings.Port;
        CloudIp = Program.Settings.CloudIpAddress;
        CloudPort = Program.Settings.CloudPort;
        Type = Program.Settings.ServerType;
        IpAddress = Program.Settings.IpAddress;
        CloudUser = Program.Settings.CloudUser;
        CloudPassword = Program.Settings.CloudPassword;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        if (string.IsNullOrEmpty(CloudPassword)) CloudPassword = Program.Settings.CloudPassword;
        var settings = Program.Settings;
        settings.EdgeId = EdgeId;
        settings.Port = Port;
        settings.IpAddress = IpAddress;
        settings.CloudIpAddress = CloudIp;
        settings.CloudPort = CloudPort;
        settings.ServerType = Type;
        settings.CloudUser = CloudUser;
        settings.CloudPassword = CloudPassword;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);
        _ = Task.Run(SystemControl.Restart).ConfigureAwait(false);
        return RedirectToPage("./Index");
    }
}