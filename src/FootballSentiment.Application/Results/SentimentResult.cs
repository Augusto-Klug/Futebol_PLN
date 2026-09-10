using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Results;

public record SentimentResult(SentimentType Sentiment, decimal Score);
