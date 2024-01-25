using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Contract.Queries.Profile;
using Core.Enums;
using Core.Exceptions;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class GetGroupByIdHandler : IRequestHandler<GetGroupByIdQuery, GetGroupByIdResponseDto>
{
    private readonly GroupRepository _groupRepository;
    private readonly ISender _sender;

    public GetGroupByIdHandler(GroupRepository groupRepository, ISender sender)
    {
        _groupRepository = groupRepository;
        _sender = sender;
    }

    public async Task<GetGroupByIdResponseDto> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupByGroupId(request.GroupId,  cancellationToken);

        if (group == null)
            throw new AppException(Messages.NotFound);

        var profiles = await _getProfileInfo(group.Members);

        var groupMembers = new List<GroupMemberDto>();
        
        foreach (var member in group.Members)
        {
            var profile = profiles.FirstOrDefault(u => u.Username == member);
            groupMembers.Add(new GroupMemberDto
            {
                Username = profile?.Username ?? "Deleted Account",
                FullName = profile?.Fullname ?? "Deleted Account",
                ProfilePictureId = profile?.ProfilePictureId ?? "-",
                IsCreator = member == group.Creator,
                IsAdmin = group.Admins.Contains(member)
            });    
        }

        group.GroupMembers = groupMembers;
        return group;
    }
    
    private async Task<List<Entities.Models.Profile.Profile>> _getProfileInfo(List<string> usernames)
    {
        var getProfilesByUsernamesRequest = new GetProfilesByUsernamesQuery
        {
            Usernames = usernames
        };
        var userProfiles = await _sender.Send(getProfilesByUsernamesRequest);
        return userProfiles;
    }
}