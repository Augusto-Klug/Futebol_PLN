using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FootballSentiment.Api.Controllers;

[ApiController]
[Route("api/collection")]
public class CollectionController(ICommentCollectorService collectorService) : ControllerBase
{
    [HttpPost("video")]
    public async Task<IActionResult> CollectVideo([FromBody] VideoCollectionRequest request, CancellationToken cancellationToken)
    {
        var result = await collectorService.CollectFromVideoAsync(
            request.VideoId,
            request.Club,
            request.MaxComments,
            request.IncludeReplies,
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("channel")]
    public async Task<IActionResult> CollectChannel([FromBody] ChannelCollectionRequest request, CancellationToken cancellationToken)
    {
        var result = await collectorService.CollectFromChannelAsync(
            request.Club,
            request.MaxVideos,
            request.MaxCommentsPerVideo,
            request.IncludeReplies,
            cancellationToken);
        return Ok(result);
    }
}
