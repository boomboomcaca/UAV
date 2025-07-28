using Magneto.Definition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Magneto.Pages;

public class RunningLogModel(ILogger<RunningLogModel> logger) : PageModel
{
    private readonly ILogger<RunningLogModel> _logger = logger;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        RunningLog.Instance.Clear();
        return RedirectToPage("./RunningLog");
    }
}