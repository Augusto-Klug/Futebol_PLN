using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Services;

public class NullSentimentAnalyzer : ISentimentAnalyzer
{
    public Task<SentimentResult> AnalyzeAsync(SentimentAnalysisContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SentimentResult(SentimentType.Neutral, 0));
    }
}
