namespace FootballSentiment.Application.Results;

public record CommentCollectionResult(
    string VideoId,
    bool VideoCreated,
    bool VideoUpdated,
    int ReceivedComments,
    int NewComments,
    int ExistingComments,
    int Replies,
    int EstimatedQuotaUnitsUsed,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);

public record ChannelCollectionResult(
    int VideosReceived,
    int VideosProcessed,
    int NewComments,
    int ExistingComments,
    int EstimatedQuotaUnitsUsed,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt);
