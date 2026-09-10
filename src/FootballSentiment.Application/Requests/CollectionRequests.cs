using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Requests;

public record VideoCollectionRequest(
    string VideoId,
    Club Club,
    int? MaxComments = 100,
    bool IncludeReplies = true);

public record ChannelCollectionRequest(
    Club Club,
    int MaxVideos = 10,
    int? MaxCommentsPerVideo = 200,
    bool IncludeReplies = true);

public record SocialCollectionRequest(
    string Source,
    string ResourceId,
    int? MaxItems);
