using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Requests;

public record VideoQuery(
    Club? Club,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Title,
    int Page = 1,
    int PageSize = 50);

public record CommentQuery(
    Club? Club,
    Guid? VideoId,
    string? YouTubeVideoId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    bool? IsReply,
    SentimentType? Sentiment,
    int Page = 1,
    int PageSize = 50);
