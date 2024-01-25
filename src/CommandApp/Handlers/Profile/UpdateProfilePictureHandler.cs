using Contract.Commands.Profile;
using Core.Constants;
using MediatR;
using Repository.Profile;
using StackExchange.Redis;

namespace CommandApp.Handlers.Profile;

public class UpdateProfilePictureHandler : IRequestHandler<UpdateProfilePictureCommand, string>
{
    private readonly ProfileRepository _profileRepository;
    private readonly IDatabase _redis;

    public UpdateProfilePictureHandler(ProfileRepository profileRepository, IConnectionMultiplexer redis)
    {
        _profileRepository = profileRepository;
        _redis = redis.GetDatabase();
    }

    public async Task<string> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        await _profileRepository.UpdatePictureAsync(request.NewId, request.Username, request.Field);
        var redisKey = GlobalConstants.KeyPrefix + request.Username;
        await _redis.KeyDeleteAsync(redisKey);

        return "Success";
    }
}