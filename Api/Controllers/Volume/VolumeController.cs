using Api.Controllers.Volume.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Volume;

[Route("api/[controller]")]
[ApiController]
public class VolumeController(VolumeService service) : ControllerBase
{
    [HttpPost]
    public IActionResult SetVolume([FromBody] VolumeDto request)
    {   
        if (request.Volume < 0 || request.Volume > 100)
        {
            return BadRequest(new { message = "Громкость должна быть в диапазоне от 0 до 100." });
        }

        service.SetVolume(request.Volume);
        return Ok(new { message = "Громкость установлена", volume = request.Volume });
    }
    
    [HttpGet]
    public IActionResult GetCurrentVolume()
    {
        int currentVolume = service.GetCurrentVolume();
        return Ok(new { volume = currentVolume });
    }
}