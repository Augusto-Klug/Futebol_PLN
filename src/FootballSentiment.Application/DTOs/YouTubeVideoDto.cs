namespace FootballSentiment.Application.DTOs;

public record YouTubeVideoDto(
    string Id,
    string ChannelId,
    string ChannelTitle,
    string Title,
    string? Description,
    DateTimeOffset PublishedAt,
    IReadOnlyCollection<string> Tags,
    string? CategoryId,
    string? DefaultLanguage,
    string? DefaultAudioLanguage,
    string? LiveBroadcastContent,
    TimeSpan? Duration,
    string? Definition,
    string? Dimension,
    bool? HasCaption,
    bool? LicensedContent,
    string? Projection,
    string? ThumbnailUrl,
    long? ViewCount,
    long? LikeCount,
    long? CommentCount);
