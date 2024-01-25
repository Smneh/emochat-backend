using Contract.Queries.Profile;
using Core.Services;
using MediatR;
using Repository.Profile;

namespace QueryApp.Handlers.Profile;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<Entities.Models.Profile.Profile>>
{
    private readonly ProfileRepository _profileRepository;
    private readonly IdentityService _identityService;

    public GetUsersHandler(ProfileRepository profileRepository, IdentityService identityService)
    {
        _profileRepository = profileRepository;
        _identityService = identityService;
    }

    public async Task<List<Entities.Models.Profile.Profile>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _profileRepository.GetAllUsers(_identityService.Username, request.SearchText ,request.Limit, request.Offset);
    }

}