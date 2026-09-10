namespace FootballSentiment.Domain.Entities;

public class VideoTag
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;
    public string Value { get; set; } = null!;
}
