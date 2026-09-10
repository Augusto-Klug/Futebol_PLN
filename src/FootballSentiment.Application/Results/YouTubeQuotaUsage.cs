namespace FootballSentiment.Application.Results;

public record YouTubeQuotaUsage(
    DateOnly Date,
    int EstimatedUnits,
    IReadOnlyDictionary<string, int> Requests);
