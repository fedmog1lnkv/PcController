using Api.Controllers.Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Api.Controllers.Application;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController(ApplicationService applicationService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetApplications()
    {
        var applications = applicationService.GetApplications();
        return Ok(applications);
    }

    [HttpPost("start")]
    public IActionResult StartApplication([FromBody] StartApplicationDto request)
    {
        var applicationName = request.Name;
        var app = applicationService.GetApplications()
            .FirstOrDefault(a => a.Name.Equals(applicationName, StringComparison.OrdinalIgnoreCase));

        if (app == null)
        {
            return NotFound(new { message = $"Приложение с именем {applicationName} не найдено" });
        }

        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = app.Path,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                });

            return Ok(new { message = $"Приложение {applicationName} запущено" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Ошибка при запуске приложения: {ex.Message}" });
        }
    }
}