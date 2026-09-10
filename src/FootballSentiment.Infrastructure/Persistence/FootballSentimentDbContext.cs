using FootballSentiment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballSentiment.Infrastructure.Persistence;

public class FootballSentimentDbContext(DbContextOptions<FootballSentimentDbContext> options) : DbContext(options)
{
    public DbSet<ClubChannel> ClubChannels => Set<ClubChannel>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<VideoTag> VideoTags => Set<VideoTag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ApiQuotaUsage> ApiQuotaUsages => Set<ApiQuotaUsage>();
    public DbSet<VideoMetricSnapshot> VideoMetricSnapshots => Set<VideoMetricSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClubChannel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.YouTubeChannelId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChannelName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.UploadsPlaylistId).HasMaxLength(128);
            entity.HasIndex(x => x.Club).IsUnique();
            entity.HasIndex(x => x.YouTubeChannelId).IsUnique();
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.YouTubeVideoId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ChannelId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChannelTitle).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CategoryId).HasMaxLength(32);
            entity.Property(x => x.DefaultLanguage).HasMaxLength(32);
            entity.Property(x => x.DefaultAudioLanguage).HasMaxLength(32);
            entity.Property(x => x.LiveBroadcastContent).HasMaxLength(64);
            entity.Property(x => x.Definition).HasMaxLength(32);
            entity.Property(x => x.Dimension).HasMaxLength(32);
            entity.Property(x => x.Projection).HasMaxLength(64);
            entity.Property(x => x.ThumbnailUrl).HasMaxLength(1024);
            entity.HasIndex(x => x.YouTubeVideoId).IsUnique();
            entity.HasIndex(x => new { x.Club, x.PublishedAt });
        });

        modelBuilder.Entity<VideoTag>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Value).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.VideoId, x.Value }).IsUnique();
            entity.HasOne(x => x.Video).WithMany(x => x.Tags).HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.YouTubeCommentId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ParentYouTubeCommentId).HasMaxLength(128);
            entity.Property(x => x.Text).IsRequired();
            entity.Property(x => x.AuthorChannelId).HasMaxLength(128);
            entity.Property(x => x.AuthorDisplayName).HasMaxLength(256);
            entity.Property(x => x.AuthorProfileImageUrl).HasMaxLength(1024);
            entity.HasIndex(x => x.YouTubeCommentId).IsUnique();
            entity.HasIndex(x => new { x.VideoId, x.PublishedAt });
            entity.HasIndex(x => x.ParentYouTubeCommentId);
            entity.HasOne(x => x.Video).WithMany(x => x.Comments).HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiQuotaUsage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Endpoint).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.Date, x.Endpoint }).IsUnique();
        });

        modelBuilder.Entity<VideoMetricSnapshot>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.VideoId, x.CapturedAt });
            entity.HasOne(x => x.Video).WithMany(x => x.MetricSnapshots).HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
