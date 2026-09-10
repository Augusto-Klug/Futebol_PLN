using FootballSentiment.Application.DTOs;
using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Requests;
using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Entities;
using FootballSentiment.Domain.Enums;
using FootballSentiment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballSentiment.Infrastructure.Services;

public class QueryService(FootballSentimentDbContext dbContext) : IQueryService
{
    public Task<IReadOnlyCollection<Club>> GetClubsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Club> clubs = Enum.GetValues<Club>();
        return Task.FromResult(clubs);
    }

    public async Task<PagedResult<VideoSummaryDto>> GetVideosAsync(VideoQuery query, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var page = Math.Max(query.Page, 1);
        var videos = ApplyVideoFilters(dbContext.Videos.AsNoTracking(), query);
        var total = await videos.CountAsync(cancellationToken);
        var items = await videos
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new VideoSummaryDto(x.Id, x.Club, x.YouTubeVideoId, x.Title, x.PublishedAt, x.ThumbnailUrl, x.ViewCount, x.LikeCount, x.CommentCount, x.Comments.Count))
            .ToListAsync(cancellationToken);
        return new PagedResult<VideoSummaryDto>(items, page, pageSize, total);
    }

    public Task<VideoSummaryDto?> GetVideoAsync(Guid id, CancellationToken cancellationToken)
    {
        return ProjectVideos(dbContext.Videos.AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<VideoSummaryDto?> GetVideoByYouTubeIdAsync(string youtubeVideoId, CancellationToken cancellationToken)
    {
        return ProjectVideos(dbContext.Videos.AsNoTracking().Where(x => x.YouTubeVideoId == youtubeVideoId)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<CommentSummaryDto>> GetCommentsAsync(CommentQuery query, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var page = Math.Max(query.Page, 1);
        var comments = dbContext.Comments.AsNoTracking().Include(x => x.Video).AsQueryable();
        if (query.Club is not null) comments = comments.Where(x => x.Video.Club == query.Club);
        if (query.VideoId is not null) comments = comments.Where(x => x.VideoId == query.VideoId);
        if (!string.IsNullOrWhiteSpace(query.YouTubeVideoId)) comments = comments.Where(x => x.Video.YouTubeVideoId == query.YouTubeVideoId);
        if (query.From is not null) comments = comments.Where(x => x.PublishedAt >= query.From);
        if (query.To is not null) comments = comments.Where(x => x.PublishedAt <= query.To);
        if (query.IsReply is not null) comments = comments.Where(x => x.IsReply == query.IsReply);
        if (query.Sentiment is not null) comments = comments.Where(x => x.Sentiment == query.Sentiment);

        var total = await comments.CountAsync(cancellationToken);
        var items = await comments
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CommentSummaryDto(x.Id, x.Video.Club, x.VideoId, x.Video.YouTubeVideoId, x.YouTubeCommentId, x.Text, x.IsReply, x.PublishedAt, x.Sentiment, x.SentimentScore))
            .ToListAsync(cancellationToken);
        return new PagedResult<CommentSummaryDto>(items, page, pageSize, total);
    }

    public async Task<CommentStatisticsResult> GetCommentStatisticsAsync(Club? club, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        var comments = dbContext.Comments.AsNoTracking().Include(x => x.Video).AsQueryable();
        if (club is not null) comments = comments.Where(x => x.Video.Club == club);
        if (from is not null) comments = comments.Where(x => x.PublishedAt >= from);
        if (to is not null) comments = comments.Where(x => x.PublishedAt <= to);

        return new CommentStatisticsResult(
            club,
            await comments.CountAsync(cancellationToken),
            await comments.CountAsync(x => !x.IsReply, cancellationToken),
            await comments.CountAsync(x => x.IsReply, cancellationToken),
            await comments.Select(x => x.VideoId).Distinct().CountAsync(cancellationToken),
            from is null ? null : DateOnly.FromDateTime(from.Value.Date),
            to is null ? null : DateOnly.FromDateTime(to.Value.Date));
    }

    public async Task<VideoStatisticsResult> GetVideoStatisticsAsync(Club? club, CancellationToken cancellationToken)
    {
        var videos = dbContext.Videos.AsNoTracking().AsQueryable();
        if (club is not null) videos = videos.Where(x => x.Club == club);

        var count = await videos.CountAsync(cancellationToken);
        var totalViews = await videos.SumAsync(x => x.ViewCount ?? 0, cancellationToken);
        var totalLikes = await videos.SumAsync(x => x.LikeCount ?? 0, cancellationToken);
        return new VideoStatisticsResult(
            club,
            count,
            totalViews,
            totalLikes,
            await videos.SumAsync(x => x.CommentCount ?? 0, cancellationToken),
            await videos.SelectMany(x => x.Comments).CountAsync(cancellationToken),
            count == 0 ? 0 : totalViews / (decimal)count,
            count == 0 ? 0 : totalLikes / (decimal)count);
    }

    private static IQueryable<Video> ApplyVideoFilters(IQueryable<Video> videos, VideoQuery query)
    {
        if (query.Club is not null) videos = videos.Where(x => x.Club == query.Club);
        if (query.From is not null) videos = videos.Where(x => x.PublishedAt >= query.From);
        if (query.To is not null) videos = videos.Where(x => x.PublishedAt <= query.To);
        if (!string.IsNullOrWhiteSpace(query.Title)) videos = videos.Where(x => x.Title.ToLower().Contains(query.Title.ToLower()));
        return videos;
    }

    private static IQueryable<VideoSummaryDto> ProjectVideos(IQueryable<Video> videos)
    {
        return videos.Select(x => new VideoSummaryDto(x.Id, x.Club, x.YouTubeVideoId, x.Title, x.PublishedAt, x.ThumbnailUrl, x.ViewCount, x.LikeCount, x.CommentCount, x.Comments.Count));
    }
}
