using FootballSentiment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FootballSentiment.Api.Controllers;

[ApiController]
[Route("api/youtube")]
public class YouTubeController(IYouTubeQuotaTracker quotaTracker) : ControllerBase
{
    [HttpGet("quota")]
    public IActionResult GetQuota()
    {
        return Ok(quotaTracker.GetCurrentUsage());
    }
}
