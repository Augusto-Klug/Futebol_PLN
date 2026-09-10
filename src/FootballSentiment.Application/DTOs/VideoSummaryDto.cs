using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.DTOs;

public record VideoSummaryDto(
    Guid Id,
    Club Club,
    string YouTubeVideoId,
    string Title,
    DateTimeOffset PublishedAt,
    string? ThumbnailUrl,
    long? ViewCount,
    long? LikeCount,
    long? CommentCount,
    int StoredComments);
