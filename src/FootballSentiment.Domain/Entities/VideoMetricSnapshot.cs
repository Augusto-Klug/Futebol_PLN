namespace FootballSentiment.Domain.Entities;

public class VideoMetricSnapshot
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public long? CommentCount { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
}
