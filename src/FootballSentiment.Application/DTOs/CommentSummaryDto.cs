using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.DTOs;

public record CommentSummaryDto(
    Guid Id,
    Club Club,
    Guid VideoId,
    string YouTubeVideoId,
    string YouTubeCommentId,
    string Text,
    bool IsReply,
    DateTimeOffset PublishedAt,
    SentimentType? Sentiment,
    decimal? SentimentScore);
