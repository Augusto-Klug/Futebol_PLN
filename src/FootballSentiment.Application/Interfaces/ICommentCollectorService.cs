using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Interfaces;

public interface ICommentCollectorService
{
    Task<CommentCollectionResult> CollectFromVideoAsync(string videoId, Club club, int? maxComments, bool includeReplies, CancellationToken cancellationToken);
    Task<ChannelCollectionResult> CollectFromChannelAsync(Club club, int maxVideos, int? maxCommentsPerVideo, bool includeReplies, CancellationToken cancellationToken);
}
