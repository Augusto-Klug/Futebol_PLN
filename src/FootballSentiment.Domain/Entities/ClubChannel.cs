using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Domain.Entities;

public class ClubChannel
{
    public Guid Id { get; set; }
    public Club Club { get; set; }
    public string YouTubeChannelId { get; set; } = null!;
    public string ChannelName { get; set; } = null!;
    public string? UploadsPlaylistId { get; set; }
    public bool Active { get; set; } = true;
}
