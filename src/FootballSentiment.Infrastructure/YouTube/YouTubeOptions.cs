namespace FootballSentiment.Infrastructure.YouTube;

public class YouTubeOptions
{
    public const string SectionName = "YouTube";

    public string ApiKey { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = "FootballSentimentCollector";
}
