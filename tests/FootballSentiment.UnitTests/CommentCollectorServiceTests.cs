using FootballSentiment.Application.DTOs;
using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Requests;
using FootballSentiment.Application.Results;
using FootballSentiment.Domain.Enums;
using FootballSentiment.Infrastructure.Persistence;
using FootballSentiment.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FootballSentiment.UnitTests;

public class CommentCollectorServiceTests
{
    [Fact]
    public async Task CollectFromVideoAsync_WhenRunTwice_DoesNotDuplicateCommentsOrTags()
    {
        var dbContext = CreateDbContext();
        var youtube = new FakeYouTubeService();
        var quota = new FakeQuotaTracker();
        var service = new CommentCollectorService(dbContext, youtube, quota);

        await service.CollectFromVideoAsync("video-1", Club.Palmeiras, 100, true, CancellationToken.None);
        var secondRun = await service.CollectFromVideoAsync("video-1", Club.Palmeiras, 100, true, CancellationToken.None);

        Assert.False(secondRun.VideoCreated);
        Assert.True(secondRun.VideoUpdated);
        Assert.Equal(0, secondRun.NewComments);
        Assert.Equal(2, secondRun.ExistingComments);
        Assert.Equal(1, await dbContext.Videos.CountAsync());
        Assert.Equal(2, await dbContext.Comments.CountAsync());
        Assert.Equal(2, await dbContext.VideoTags.CountAsync());
    }

    private static FootballSentimentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FootballSentimentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FootballSentimentDbContext(options);
    }

    private sealed class FakeYouTubeService : IYouTubeService
    {
        public Task<YouTubeChannelDto?> GetChannelAsync(string channelId, CancellationToken cancellationToken)
        {
            return Task.FromResult<YouTubeChannelDto?>(new YouTubeChannelDto(channelId, "Canal", "uploads"));
        }

        public Task<IReadOnlyCollection<YouTubeVideoDto>> GetChannelVideosAsync(string channelId, int maxVideos, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<YouTubeVideoDto> videos = [CreateVideo()];
            return Task.FromResult(videos);
        }

        public Task<YouTubeVideoDto?> GetVideoAsync(string videoId, CancellationToken cancellationToken)
        {
            return Task.FromResult<YouTubeVideoDto?>(CreateVideo());
        }

        public Task<IReadOnlyCollection<YouTubeCommentDto>> GetVideoCommentsAsync(string videoId, int? maxComments, bool includeReplies, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<YouTubeCommentDto> comments =
            [
                new("comment-1", "bom jogo", "author-1", "Autor 1", null, DateTimeOffset.UtcNow, null, 1, false, null),
                new("comment-2", "resposta", "author-2", "Autor 2", null, DateTimeOffset.UtcNow, null, 0, true, "comment-1")
            ];
            return Task.FromResult(comments);
        }

        private static YouTubeVideoDto CreateVideo()
        {
            return new YouTubeVideoDto(
                "video-1",
                "channel-1",
                "Canal Oficial",
                "PALMEIRAS 1 X 0 INTERNACIONAL",
                "Descricao",
                DateTimeOffset.UtcNow,
                ["palmeiras", "internacional"],
                "17",
                "pt-BR",
                "pt-BR",
                "none",
                TimeSpan.FromMinutes(3),
                "hd",
                "2d",
                false,
                true,
                "rectangular",
                "https://example.com/thumb.jpg",
                100,
                10,
                2);
        }
    }

    private sealed class FakeQuotaTracker : IYouTubeQuotaTracker
    {
        public void RegisterRequest(string endpoint, int units)
        {
        }

        public YouTubeQuotaUsage GetCurrentUsage()
        {
            return new YouTubeQuotaUsage(DateOnly.FromDateTime(DateTime.UtcNow), 0, new Dictionary<string, int>());
        }
    }
}
