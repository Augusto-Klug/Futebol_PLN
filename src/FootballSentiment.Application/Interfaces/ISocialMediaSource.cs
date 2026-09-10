using FootballSentiment.Application.DTOs;
using FootballSentiment.Application.Requests;

namespace FootballSentiment.Application.Interfaces;

public interface ISocialMediaSource
{
    Task<IReadOnlyCollection<SocialCommentDto>> GetCommentsAsync(SocialCollectionRequest request, CancellationToken cancellationToken);
}
