namespace FootballSentiment.Application.DTOs;

public record SocialCommentDto(
    string ExternalId,
    string Text,
    DateTimeOffset PublishedAt,
    string? ParentExternalId);
