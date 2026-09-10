namespace FootballSentiment.Application.DTOs;

public record YouTubeCommentDto(
    string Id,
    string Text,
    string? AuthorChannelId,
    string? AuthorDisplayName,
    string? AuthorProfileImageUrl,
    DateTimeOffset PublishedAt,
    DateTimeOffset? UpdatedAt,
    long LikeCount,
    bool IsReply,
    string? ParentCommentId);
