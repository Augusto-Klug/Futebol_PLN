using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Domain.Entities;

public class Video
{
    public Guid Id { get; set; }
    public Club Club { get; set; }
    public string YouTubeVideoId { get; set; } = null!;
    public string ChannelId { get; set; } = null!;
    public string ChannelTitle { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public string? CategoryId { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? DefaultAudioLanguage { get; set; }
    public string? LiveBroadcastContent { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Definition { get; set; }
    public string? Dimension { get; set; }
    public bool? HasCaption { get; set; }
    public bool? LicensedContent { get; set; }
    public string? Projection { get; set; }
    public string? ThumbnailUrl { get; set; }
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long? CommentCount { get; set; }
    public DateTimeOffset CollectedAt { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public ICollection<VideoTag> Tags { get; set; } = new List<VideoTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<VideoMetricSnapshot> MetricSnapshots { get; set; } = new List<VideoMetricSnapshot>();
}
