using Contract.Queries.Chat;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using MediatR;
using Repository.Group;
using Repository.Profile;

namespace QueryApp.Handlers.Chat;

public class GetUsersWithExcludeFilterHandler : IRequestHandler<GetUsersWithExcludeFilterQuery, List<Entities.Models.Profile.Profile>>
{
    private readonly GroupRepository _groupRepository;
    private readonly ProfileRepository _profileRepository;
    private readonly IdentityService _identityService;

    public GetUsersWithExcludeFilterHandler(GroupRepository groupRepository, ProfileRepository profileRepository, IdentityService identityService)
    {
        _groupRepository = groupRepository;
        _profileRepository = profileRepository;
        _identityService = identityService;
    }

    public async Task<List<Entities.Models.Profile.Profile>> Handle(GetUsersWithExcludeFilterQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(request.GroupId, cancellationToken);

        if (group == null)
            throw new AppException(Messages.NotFound);
        
        return await _profileRepository.GetAllUsersExceptPriority(request.SearchText, group.Members, _identityService.Username, request.Limit, request.Offset);
    }
}