using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Media
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController(MediaService mediaService) : ControllerBase
    {
        [HttpPost("playpause")]
        public IActionResult PlayPause()
        {
            mediaService.PlayPause();
            return Ok("Play/Pause command sent.");
        }

        [HttpPost("next")]
        public IActionResult Next()
        {
            mediaService.Next();
            return Ok("Next track command sent.");
        }

        [HttpPost("previous")]
        public IActionResult Previous()
        {
            mediaService.Previous();
            return Ok("Previous track command sent.");
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            mediaService.Stop();
            return Ok("Stop command sent.");
        }
    }
}