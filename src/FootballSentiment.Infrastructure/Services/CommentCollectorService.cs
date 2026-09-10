using FootballSentiment.Application.DTOs;
using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Entities;
using FootballSentiment.Domain.Enums;
using FootballSentiment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballSentiment.Infrastructure.Services;

public class CommentCollectorService(
    FootballSentimentDbContext dbContext,
    IYouTubeService youTubeService,
    IYouTubeQuotaTracker quotaTracker) : ICommentCollectorService
{
    public async Task<CommentCollectionResult> CollectFromVideoAsync(string videoId, Club club, int? maxComments, bool includeReplies, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var videoDto = await youTubeService.GetVideoAsync(videoId, cancellationToken)
            ?? throw new InvalidOperationException($"Video '{videoId}' not found.");

        var (video, created, updated) = await UpsertVideoAsync(videoDto, club, cancellationToken);
        var comments = await youTubeService.GetVideoCommentsAsync(videoId, maxComments, includeReplies, cancellationToken);

        var existingIds = await dbContext.Comments
            .Where(x => comments.Select(c => c.Id).Contains(x.YouTubeCommentId))
            .Select(x => x.YouTubeCommentId)
            .ToListAsync(cancellationToken);
        var existingSet = existingIds.ToHashSet(StringComparer.Ordinal);

        var newComments = 0;
        var existingComments = 0;
        foreach (var commentDto in comments)
        {
            var existing = await dbContext.Comments.FirstOrDefaultAsync(x => x.YouTubeCommentId == commentDto.Id, cancellationToken);
            if (existing is null)
            {
                dbContext.Comments.Add(MapComment(commentDto, video.Id));
                newComments++;
                continue;
            }

            existing.Text = commentDto.Text;
            existing.AuthorChannelId = commentDto.AuthorChannelId;
            existing.AuthorDisplayName = commentDto.AuthorDisplayName;
            existing.AuthorProfileImageUrl = commentDto.AuthorProfileImageUrl;
            existing.UpdatedAt = commentDto.UpdatedAt;
            existing.LikeCount = commentDto.LikeCount;
            existingComments++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var finishedAt = DateTimeOffset.UtcNow;
        return new CommentCollectionResult(
            videoId,
            created,
            updated,
            comments.Count,
            newComments,
            existingComments,
            comments.Count(x => x.IsReply),
            quotaTracker.GetCurrentUsage().EstimatedUnits,
            startedAt,
            finishedAt);
    }

    public async Task<ChannelCollectionResult> CollectFromChannelAsync(Club club, int maxVideos, int? maxCommentsPerVideo, bool includeReplies, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var channel = await dbContext.ClubChannels.FirstOrDefaultAsync(x => x.Club == club && x.Active, cancellationToken)
            ?? throw new InvalidOperationException($"No active channel configured for club '{club}'.");
        var videos = await youTubeService.GetChannelVideosAsync(channel.YouTubeChannelId, maxVideos, cancellationToken);

        var newComments = 0;
        var existingComments = 0;
        foreach (var video in videos)
        {
            var result = await CollectFromVideoAsync(video.Id, club, maxCommentsPerVideo, includeReplies, cancellationToken);
            newComments += result.NewComments;
            existingComments += result.ExistingComments;
        }

        return new ChannelCollectionResult(
            videos.Count,
            videos.Count,
            newComments,
            existingComments,
            quotaTracker.GetCurrentUsage().EstimatedUnits,
            startedAt,
            DateTimeOffset.UtcNow);
    }

    private async Task<(Video Video, bool Created, bool Updated)> UpsertVideoAsync(YouTubeVideoDto dto, Club club, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var video = await dbContext.Videos.Include(x => x.Tags).FirstOrDefaultAsync(x => x.YouTubeVideoId == dto.Id, cancellationToken);
        var created = video is null;
        video ??= new Video { Id = Guid.NewGuid(), YouTubeVideoId = dto.Id, CollectedAt = now };

        video.Club = club;
        video.ChannelId = dto.ChannelId;
        video.ChannelTitle = dto.ChannelTitle;
        video.Title = dto.Title;
        video.Description = dto.Description;
        video.PublishedAt = dto.PublishedAt;
        video.CategoryId = dto.CategoryId;
        video.DefaultLanguage = dto.DefaultLanguage;
        video.DefaultAudioLanguage = dto.DefaultAudioLanguage;
        video.LiveBroadcastContent = dto.LiveBroadcastContent;
        video.Duration = dto.Duration;
        video.Definition = dto.Definition;
        video.Dimension = dto.Dimension;
        video.HasCaption = dto.HasCaption;
        video.LicensedContent = dto.LicensedContent;
        video.Projection = dto.Projection;
        video.ThumbnailUrl = dto.ThumbnailUrl;
        video.ViewCount = dto.ViewCount;
        video.LikeCount = dto.LikeCount;
        video.CommentCount = dto.CommentCount;
        video.LastUpdatedAt = now;

        if (created)
        {
            dbContext.Videos.Add(video);
        }

        SynchronizeTags(video, dto.Tags);
        dbContext.VideoMetricSnapshots.Add(new VideoMetricSnapshot
        {
            Id = Guid.NewGuid(),
            Video = video,
            ViewCount = dto.ViewCount,
            LikeCount = dto.LikeCount,
            CommentCount = dto.CommentCount,
            CapturedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return (video, created, !created);
    }

    private static void SynchronizeTags(Video video, IReadOnlyCollection<string> tags)
    {
        var desired = tags.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in video.Tags.Where(x => !desired.Contains(x.Value)).ToList())
        {
            video.Tags.Remove(tag);
        }

        foreach (var tag in desired.Where(tag => video.Tags.All(existing => !string.Equals(existing.Value, tag, StringComparison.OrdinalIgnoreCase))))
        {
            video.Tags.Add(new VideoTag { Id = Guid.NewGuid(), Value = tag });
        }
    }

    private static Comment MapComment(YouTubeCommentDto dto, Guid videoId)
    {
        return new Comment
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            YouTubeCommentId = dto.Id,
            ParentYouTubeCommentId = dto.ParentCommentId,
            Text = dto.Text,
            AuthorChannelId = dto.AuthorChannelId,
            AuthorDisplayName = dto.AuthorDisplayName,
            AuthorProfileImageUrl = dto.AuthorProfileImageUrl,
            PublishedAt = dto.PublishedAt,
            UpdatedAt = dto.UpdatedAt,
            CollectedAt = DateTimeOffset.UtcNow,
            LikeCount = dto.LikeCount,
            IsReply = dto.IsReply
        };
    }
}
