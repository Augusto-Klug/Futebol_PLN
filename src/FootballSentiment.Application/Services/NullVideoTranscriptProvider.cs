using FootballSentiment.Application.Interfaces;

namespace FootballSentiment.Application.Services;

public class NullVideoTranscriptProvider : IVideoTranscriptProvider
{
    public Task<VideoTranscript?> GetTranscriptAsync(string videoId, CancellationToken cancellationToken)
    {
        return Task.FromResult<VideoTranscript?>(null);
    }
}
