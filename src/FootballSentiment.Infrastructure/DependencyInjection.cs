using FootballSentiment.Application.Interfaces;
using FootballSentiment.Application.Services;
using FootballSentiment.Infrastructure.Persistence;
using FootballSentiment.Infrastructure.Services;
using FootballSentiment.Infrastructure.YouTube;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FootballSentiment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YouTubeOptions>(options =>
            configuration.GetSection(YouTubeOptions.SectionName).Bind(options));
        services.AddDbContext<FootballSentimentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddScoped<IYouTubeQuotaTracker, YouTubeQuotaTracker>();
        services.AddScoped<IYouTubeService, YouTubeApiService>();
        services.AddScoped<ICommentCollectorService, CommentCollectorService>();
        services.AddScoped<IQueryService, QueryService>();
        services.AddScoped<ISentimentAnalyzer, NullSentimentAnalyzer>();
        services.AddScoped<IVideoTranscriptProvider, NullVideoTranscriptProvider>();

        return services;
    }
}
