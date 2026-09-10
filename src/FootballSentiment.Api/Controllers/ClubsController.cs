using FootballSentiment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FootballSentiment.Api.Controllers;

[ApiController]
[Route("api/clubs")]
public class ClubsController(IQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return Ok(await queryService.GetClubsAsync(cancellationToken));
    }
}
