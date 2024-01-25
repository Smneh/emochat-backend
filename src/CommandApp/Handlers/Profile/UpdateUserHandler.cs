using Contract.Commands.Profile;
using Core.Constants;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using MediatR;
using Repository.Profile;
using StackExchange.Redis;

namespace CommandApp.Handlers.Profile;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly ProfileRepository _profileRepository;
    private readonly IdentityService _identityService;
    private readonly IDatabase _redis;

    public UpdateUserHandler(ProfileRepository profileRepository, IdentityService identityService, IConnectionMultiplexer redis)
    {
        _profileRepository = profileRepository;
        _identityService = identityService;
        _redis = redis.GetDatabase();
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var oldProfile = await _profileRepository.GetProfile(_identityService.Username);

        if (oldProfile == null)
            throw new AppException(Messages.UserNotFound);

        await _profileRepository.UpdateProfile(_identityService.Username, request.Fullname);
        
        var redisKey = GlobalConstants.KeyPrefix + _identityService.Username;
        await _redis.KeyDeleteAsync(redisKey);
        
        return Unit.Value;
    }
}