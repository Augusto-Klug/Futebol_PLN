using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Entities;
using FootballSentiment.Infrastructure.Persistence;

namespace FootballSentiment.Infrastructure.YouTube;

public class YouTubeQuotaTracker(FootballSentimentDbContext dbContext) : IYouTubeQuotaTracker
{
    private readonly Dictionary<string, int> _requests = [];
    private int _estimatedUnits;

    public void RegisterRequest(string endpoint, int units)
    {
        _requests[endpoint] = _requests.GetValueOrDefault(endpoint) + 1;
        _estimatedUnits += units;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var usage = dbContext.ApiQuotaUsages.FirstOrDefault(x => x.Date == today && x.Endpoint == endpoint);
        if (usage is null)
        {
            dbContext.ApiQuotaUsages.Add(new ApiQuotaUsage
            {
                Id = Guid.NewGuid(),
                Date = today,
                Endpoint = endpoint,
                RequestCount = 1,
                EstimatedUnits = units
            });
            return;
        }

        usage.RequestCount++;
        usage.EstimatedUnits += units;
    }

    public YouTubeQuotaUsage GetCurrentUsage()
    {
        return new YouTubeQuotaUsage(DateOnly.FromDateTime(DateTime.UtcNow), _estimatedUnits, _requests);
    }
}
