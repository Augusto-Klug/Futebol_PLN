namespace FootballSentiment.Application.Interfaces;

public interface IVideoTranscriptProvider
{
    Task<VideoTranscript?> GetTranscriptAsync(string videoId, CancellationToken cancellationToken);
}

public record VideoTranscript(string VideoId, string Language, string Text);
