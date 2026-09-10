using FootballSentiment.Application.DTOs;

namespace FootballSentiment.Application.Interfaces;

public interface IYouTubeService
{
    Task<YouTubeChannelDto?> GetChannelAsync(string channelId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<YouTubeVideoDto>> GetChannelVideosAsync(string channelId, int maxVideos, CancellationToken cancellationToken);
    Task<YouTubeVideoDto?> GetVideoAsync(string videoId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<YouTubeCommentDto>> GetVideoCommentsAsync(string videoId, int? maxComments, bool includeReplies, CancellationToken cancellationToken);
}
