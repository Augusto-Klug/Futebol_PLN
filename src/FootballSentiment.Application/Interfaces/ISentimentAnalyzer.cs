using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Interfaces;

public interface ISentimentAnalyzer
{
    Task<SentimentResult> AnalyzeAsync(SentimentAnalysisContext context, CancellationToken cancellationToken);
}

public record SentimentAnalysisContext(
    Club Club,
    string VideoTitle,
    string? VideoDescription,
    string CommentText);
