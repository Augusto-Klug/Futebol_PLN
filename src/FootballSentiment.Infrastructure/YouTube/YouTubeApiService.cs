using System.Xml;
using FootballSentiment.Application.DTOs;
using FootballSentiment.Application.Interfaces;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Options;

namespace FootballSentiment.Infrastructure.YouTube;

public class YouTubeApiService : IYouTubeService
{
    private readonly YouTubeService _client;
    private readonly IYouTubeQuotaTracker _quotaTracker;

    public YouTubeApiService(IOptions<YouTubeOptions> options, IYouTubeQuotaTracker quotaTracker)
    {
        _quotaTracker = quotaTracker;
        _client = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = options.Value.ApiKey,
            ApplicationName = options.Value.ApplicationName
        });
    }

    public async Task<YouTubeChannelDto?> GetChannelAsync(string channelId, CancellationToken cancellationToken)
    {
        var request = _client.Channels.List("snippet,contentDetails");
        request.Id = channelId;
        _quotaTracker.RegisterRequest("channels.list", 1);
        var response = await request.ExecuteAsync(cancellationToken);
        var channel = response.Items.FirstOrDefault();
        return channel is null
            ? null
            : new YouTubeChannelDto(channel.Id, channel.Snippet.Title, channel.ContentDetails?.RelatedPlaylists?.Uploads);
    }

    public async Task<IReadOnlyCollection<YouTubeVideoDto>> GetChannelVideosAsync(string channelId, int maxVideos, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(channelId, cancellationToken);
        if (channel?.UploadsPlaylistId is null)
        {
            return [];
        }

        var videoIds = new List<string>();
        string? pageToken = null;
        while (videoIds.Count < maxVideos)
        {
            var request = _client.PlaylistItems.List("snippet");
            request.PlaylistId = channel.UploadsPlaylistId;
            request.MaxResults = Math.Min(50, maxVideos - videoIds.Count);
            request.PageToken = pageToken;
            _quotaTracker.RegisterRequest("playlistItems.list", 1);

            var response = await request.ExecuteAsync(cancellationToken);
            videoIds.AddRange(response.Items
                .Select(x => x.Snippet.ResourceId.VideoId)
                .Where(x => !string.IsNullOrWhiteSpace(x))!);

            pageToken = response.NextPageToken;
            if (string.IsNullOrWhiteSpace(pageToken))
            {
                break;
            }
        }

        var videos = new List<YouTubeVideoDto>();
        foreach (var id in videoIds.Take(maxVideos))
        {
            var video = await GetVideoAsync(id, cancellationToken);
            if (video is not null)
            {
                videos.Add(video);
            }
        }

        return videos;
    }

    public async Task<YouTubeVideoDto?> GetVideoAsync(string videoId, CancellationToken cancellationToken)
    {
        var request = _client.Videos.List("snippet,statistics,contentDetails");
        request.Id = videoId;
        _quotaTracker.RegisterRequest("videos.list", 1);
        var response = await request.ExecuteAsync(cancellationToken);
        var video = response.Items.FirstOrDefault();
        return video is null ? null : MapVideo(video);
    }

    public async Task<IReadOnlyCollection<YouTubeCommentDto>> GetVideoCommentsAsync(string videoId, int? maxComments, bool includeReplies, CancellationToken cancellationToken)
    {
        var comments = new List<YouTubeCommentDto>();
        string? pageToken = null;

        while (maxComments is null || comments.Count < maxComments)
        {
            var request = _client.CommentThreads.List("snippet,replies");
            request.VideoId = videoId;
            request.MaxResults = 100;
            request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
            request.Order = CommentThreadsResource.ListRequest.OrderEnum.Time;
            request.PageToken = pageToken;
            _quotaTracker.RegisterRequest("commentThreads.list", 1);

            var response = await request.ExecuteAsync(cancellationToken);
            foreach (var thread in response.Items)
            {
                var topLevel = thread.Snippet.TopLevelComment;
                comments.Add(MapComment(topLevel, false, null));
                if (maxComments is not null && comments.Count >= maxComments)
                {
                    break;
                }

                if (includeReplies)
                {
                    var replies = await GetRepliesAsync(thread, cancellationToken);
                    foreach (var reply in replies)
                    {
                        comments.Add(reply);
                        if (maxComments is not null && comments.Count >= maxComments)
                        {
                            break;
                        }
                    }
                }
            }

            pageToken = response.NextPageToken;
            if (string.IsNullOrWhiteSpace(pageToken) || (maxComments is not null && comments.Count >= maxComments))
            {
                break;
            }
        }

        return comments;
    }

    private async Task<IReadOnlyCollection<YouTubeCommentDto>> GetRepliesAsync(CommentThread thread, CancellationToken cancellationToken)
    {
        var parentId = thread.Snippet.TopLevelComment.Id;
        var embeddedReplies = thread.Replies?.Comments ?? [];
        if ((thread.Snippet.TotalReplyCount ?? 0) <= embeddedReplies.Count)
        {
            return embeddedReplies.Select(x => MapComment(x, true, parentId)).ToList();
        }

        var replies = new List<YouTubeCommentDto>();
        string? pageToken = null;
        do
        {
            var request = _client.Comments.List("snippet");
            request.ParentId = parentId;
            request.MaxResults = 100;
            request.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;
            request.PageToken = pageToken;
            _quotaTracker.RegisterRequest("comments.list", 1);
            var response = await request.ExecuteAsync(cancellationToken);
            replies.AddRange(response.Items.Select(x => MapComment(x, true, parentId)));
            pageToken = response.NextPageToken;
        } while (!string.IsNullOrWhiteSpace(pageToken));

        return replies;
    }

    private static YouTubeVideoDto MapVideo(Video video)
    {
        var snippet = video.Snippet;
        var content = video.ContentDetails;
        var stats = video.Statistics;

        return new YouTubeVideoDto(
            video.Id,
            snippet.ChannelId,
            snippet.ChannelTitle,
            snippet.Title,
            snippet.Description,
            snippet.PublishedAtDateTimeOffset ?? DateTimeOffset.MinValue,
            snippet.Tags?.ToList() ?? [],
            snippet.CategoryId,
            snippet.DefaultLanguage,
            snippet.DefaultAudioLanguage,
            snippet.LiveBroadcastContent,
            ParseDuration(content?.Duration),
            content?.Definition,
            content?.Dimension,
            ParseBooleanString(content?.Caption),
            content?.LicensedContent,
            content?.Projection,
            SelectThumbnail(snippet.Thumbnails),
            ToLong(stats?.ViewCount),
            ToLong(stats?.LikeCount),
            ToLong(stats?.CommentCount));
    }

    private static YouTubeCommentDto MapComment(Comment comment, bool isReply, string? parentId)
    {
        var snippet = comment.Snippet;
        return new YouTubeCommentDto(
            comment.Id,
            snippet.TextDisplay ?? string.Empty,
            snippet.AuthorChannelId?.Value,
            snippet.AuthorDisplayName,
            snippet.AuthorProfileImageUrl,
            snippet.PublishedAtDateTimeOffset ?? DateTimeOffset.MinValue,
            snippet.UpdatedAtDateTimeOffset,
            snippet.LikeCount ?? 0,
            isReply,
            parentId);
    }

    private static TimeSpan? ParseDuration(string? duration)
    {
        return string.IsNullOrWhiteSpace(duration) ? null : XmlConvert.ToTimeSpan(duration);
    }

    private static bool? ParseBooleanString(string? value)
    {
        return value?.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private static string? SelectThumbnail(ThumbnailDetails? thumbnails)
    {
        return thumbnails?.Maxres?.Url
            ?? thumbnails?.Standard?.Url
            ?? thumbnails?.High?.Url
            ?? thumbnails?.Medium?.Url
            ?? thumbnails?.Default__?.Url;
    }

    private static long? ToLong(ulong? value)
    {
        return value is null ? null : checked((long)value.Value);
    }
}
