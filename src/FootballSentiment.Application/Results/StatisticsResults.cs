using FootballSentiment.Domain.Enums;

namespace FootballSentiment.Application.Results;

public record CommentStatisticsResult(
    Club? Club,
    int TotalComments,
    int TopLevelComments,
    int Replies,
    int Videos,
    DateOnly? From,
    DateOnly? To);

public record VideoStatisticsResult(
    Club? Club,
    int Videos,
    long TotalViews,
    long TotalLikes,
    long TotalCommentsReportedByYouTube,
    int CommentsStored,
    decimal AverageViewsPerVideo,
    decimal AverageLikesPerVideo);
