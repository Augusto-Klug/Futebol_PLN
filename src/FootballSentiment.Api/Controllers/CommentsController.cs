using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Requests;
using FootballSentiment.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FootballSentiment.Api.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController(IQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Club? club,
        [FromQuery] Guid? videoId,
        [FromQuery] string? youtubeVideoId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] bool? isReply,
        [FromQuery] SentimentType? sentiment,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await queryService.GetCommentsAsync(
            new CommentQuery(club, videoId, youtubeVideoId, from, to, isReply, sentiment, page, pageSize),
            cancellationToken));
    }
}
