using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;
    public string YouTubeCommentId { get; set; } = null!;
    public string? ParentYouTubeCommentId { get; set; }
    public string Text { get; set; } = null!;
    public string? AuthorChannelId { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string? AuthorProfileImageUrl { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset CollectedAt { get; set; }
    public long LikeCount { get; set; }
    public bool IsReply { get; set; }
    public SentimentType? Sentiment { get; set; }
    public decimal? SentimentScore { get; set; }
}
