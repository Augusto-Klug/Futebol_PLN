using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Requests;
using FootballSentiment.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FootballSentiment.Api.Controllers;

[ApiController]
[Route("api/videos")]
public class VideosController(IQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Club? club,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? title,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await queryService.GetVideosAsync(new VideoQuery(club, from, to, title, page, pageSize), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var video = await queryService.GetVideoAsync(id, cancellationToken);
        return video is null ? NotFound() : Ok(video);
    }

    [HttpGet("youtube/{youtubeVideoId}")]
    public async Task<IActionResult> GetByYouTubeId(string youtubeVideoId, CancellationToken cancellationToken)
    {
        var video = await queryService.GetVideoByYouTubeIdAsync(youtubeVideoId, cancellationToken);
        return video is null ? NotFound() : Ok(video);
    }
}
