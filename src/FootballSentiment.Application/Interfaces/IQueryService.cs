using FootballSentiment.Application.DTOs;
using FootballSentiment.Application.Requests;
using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Interfaces;

public interface IQueryService
{
    Task<IReadOnlyCollection<Club>> GetClubsAsync(CancellationToken cancellationToken);
    Task<PagedResult<VideoSummaryDto>> GetVideosAsync(VideoQuery query, CancellationToken cancellationToken);
    Task<VideoSummaryDto?> GetVideoAsync(Guid id, CancellationToken cancellationToken);
    Task<VideoSummaryDto?> GetVideoByYouTubeIdAsync(string youtubeVideoId, CancellationToken cancellationToken);
    Task<PagedResult<CommentSummaryDto>> GetCommentsAsync(CommentQuery query, CancellationToken cancellationToken);
    Task<CommentStatisticsResult> GetCommentStatisticsAsync(Club? club, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken);
    Task<VideoStatisticsResult> GetVideoStatisticsAsync(Club? club, CancellationToken cancellationToken);
}
