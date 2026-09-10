using System;
using FootballSentiment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballSentiment.Infrastructure.Migrations;

[DbContext(typeof(FootballSentimentDbContext))]
[Migration("20260828000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApiQuotaUsages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Endpoint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                RequestCount = table.Column<int>(type: "integer", nullable: false),
                EstimatedUnits = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ApiQuotaUsages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ClubChannels",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Club = table.Column<int>(type: "integer", nullable: false),
                YouTubeChannelId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ChannelName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                UploadsPlaylistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                Active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ClubChannels", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Videos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Club = table.Column<int>(type: "integer", nullable: false),
                YouTubeVideoId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ChannelId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ChannelTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CategoryId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                DefaultLanguage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                DefaultAudioLanguage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                LiveBroadcastContent = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                Definition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                Dimension = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                HasCaption = table.Column<bool>(type: "boolean", nullable: true),
                LicensedContent = table.Column<bool>(type: "boolean", nullable: true),
                Projection = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ThumbnailUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                ViewCount = table.Column<long>(type: "bigint", nullable: true),
                LikeCount = table.Column<long>(type: "bigint", nullable: true),
                CommentCount = table.Column<long>(type: "bigint", nullable: true),
                CollectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Videos", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                YouTubeCommentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ParentYouTubeCommentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                Text = table.Column<string>(type: "text", nullable: false),
                AuthorChannelId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                AuthorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                AuthorProfileImageUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CollectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LikeCount = table.Column<long>(type: "bigint", nullable: false),
                IsReply = table.Column<bool>(type: "boolean", nullable: false),
                Sentiment = table.Column<int>(type: "integer", nullable: true),
                SentimentScore = table.Column<decimal>(type: "numeric", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comments", x => x.Id);
                table.ForeignKey("FK_Comments_Videos_VideoId", x => x.VideoId, "Videos", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VideoMetricSnapshots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                ViewCount = table.Column<long>(type: "bigint", nullable: true),
                LikeCount = table.Column<long>(type: "bigint", nullable: true),
                CommentCount = table.Column<long>(type: "bigint", nullable: true),
                CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VideoMetricSnapshots", x => x.Id);
                table.ForeignKey("FK_VideoMetricSnapshots_Videos_VideoId", x => x.VideoId, "Videos", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VideoTags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                VideoId = table.Column<Guid>(type: "uuid", nullable: false),
                Value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VideoTags", x => x.Id);
                table.ForeignKey("FK_VideoTags_Videos_VideoId", x => x.VideoId, "Videos", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_ApiQuotaUsages_Date_Endpoint", "ApiQuotaUsages", new[] { "Date", "Endpoint" }, unique: true);
        migrationBuilder.CreateIndex("IX_ClubChannels_Club", "ClubChannels", "Club", unique: true);
        migrationBuilder.CreateIndex("IX_ClubChannels_YouTubeChannelId", "ClubChannels", "YouTubeChannelId", unique: true);
        migrationBuilder.CreateIndex("IX_Comments_ParentYouTubeCommentId", "Comments", "ParentYouTubeCommentId");
        migrationBuilder.CreateIndex("IX_Comments_VideoId_PublishedAt", "Comments", new[] { "VideoId", "PublishedAt" });
        migrationBuilder.CreateIndex("IX_Comments_YouTubeCommentId", "Comments", "YouTubeCommentId", unique: true);
        migrationBuilder.CreateIndex("IX_VideoMetricSnapshots_VideoId_CapturedAt", "VideoMetricSnapshots", new[] { "VideoId", "CapturedAt" });
        migrationBuilder.CreateIndex("IX_Videos_Club_PublishedAt", "Videos", new[] { "Club", "PublishedAt" });
        migrationBuilder.CreateIndex("IX_Videos_YouTubeVideoId", "Videos", "YouTubeVideoId", unique: true);
        migrationBuilder.CreateIndex("IX_VideoTags_VideoId_Value", "VideoTags", new[] { "VideoId", "Value" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ApiQuotaUsages");
        migrationBuilder.DropTable("ClubChannels");
        migrationBuilder.DropTable("Comments");
        migrationBuilder.DropTable("VideoMetricSnapshots");
        migrationBuilder.DropTable("VideoTags");
        migrationBuilder.DropTable("Videos");
    }
}
