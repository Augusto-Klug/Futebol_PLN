using FootballSentiment.Application.Interfaces;
using FootballSentiment.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FootballSentiment.Api.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController(IQueryService queryService) : ControllerBase
{
    [HttpGet("comments")]
    public async Task<IActionResult> GetCommentStatistics(
        [FromQuery] Club? club,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        return Ok(await queryService.GetCommentStatisticsAsync(club, from, to, cancellationToken));
    }

    [HttpGet("videos")]
    public async Task<IActionResult> GetVideoStatistics([FromQuery] Club? club, CancellationToken cancellationToken)
    {
        return Ok(await queryService.GetVideoStatisticsAsync(club, cancellationToken));
    }
}
