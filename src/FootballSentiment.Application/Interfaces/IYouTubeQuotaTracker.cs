using FootballSentiment.Application.Results;

namespace FootballSentiment.Application.Interfaces;

public interface IYouTubeQuotaTracker
{
    void RegisterRequest(string endpoint, int units);
    YouTubeQuotaUsage GetCurrentUsage();
}
