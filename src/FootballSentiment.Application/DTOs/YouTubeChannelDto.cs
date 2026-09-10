namespace FootballSentiment.Application.DTOs;

public record YouTubeChannelDto(
    string Id,
    string Title,
    string? UploadsPlaylistId);
