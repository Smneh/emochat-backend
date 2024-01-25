using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Contract.Queries.Profile;
using Core.Services;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class GetChatsListHandler : IRequestHandler<GetChatsListQuery, List<GetChatsListResponseDto>>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly ISender _sender;

    public GetChatsListHandler(GroupRepository groupRepository, IdentityService identityService, ISender sender)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _sender = sender;
    }

    public async Task<List<GetChatsListResponseDto>> Handle(GetChatsListQuery request, CancellationToken cancellationToken)
    {
        var result = await _groupRepository.GetAllGroupsList(_identityService.Username,
            request.Type, request.StartRow, request.RowCount, cancellationToken);

        result.RemoveAll(chat => chat is { Type: "User", LastMessageId: 0 });

        foreach (var group in result)
        {
            group.UniqueId = group.GroupId;
            if (group.Type == "User")
            {
                var profile = await GetUserProfile(group.ReceiverId);
                group.ProfilePictureId = profile?.ProfilePictureId ?? "-";
                group.Title = profile != null ? profile.Fullname : "Deleted Account" ;
            }
            else
            {
                group.ReceiverId = group.GroupId;
            }
        }

        return result;
    }
    
    private async Task<Entities.Models.Profile.Profile?> GetUserProfile(string username)
    {
        var getProfilesByUsernamesRequest = new GetProfilesByUsernamesQuery
        {
            Usernames = new List<string>
            {
                username
            }
        };
        var profiles = await _sender.Send(getProfilesByUsernamesRequest);
        return profiles.FirstOrDefault();
    }
}