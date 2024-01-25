using Contract.Queries.Profile;
using Core.Enums;
using Core.Exceptions;
using MediatR;
using Repository.Profile;

namespace QueryApp.Handlers.Profile;

public class GetUserInfoHandler : IRequestHandler<GetUserInfoQuery, Entities.Models.Profile.Profile>
{
    private readonly ProfileRepository _profileRepository;

    public GetUserInfoHandler(ProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<Entities.Models.Profile.Profile> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetProfile(request.Username);

        if (profile == null)
            throw new AppException(Messages.UserNotFound);

        return profile;
    }
}