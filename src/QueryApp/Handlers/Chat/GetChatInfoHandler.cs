using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Contract.Queries.Profile;
using Core.Services;
using Entities.Models.Chat;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class GetChatInfoHandler : IRequestHandler<GetChatInfoQuery, GetChatInfoResponseDto>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly ISender _sender;

    public GetChatInfoHandler(GroupRepository groupRepository, IdentityService identityService, ISender sender)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _sender = sender;
    }

    public async Task<GetChatInfoResponseDto> Handle(GetChatInfoQuery request, CancellationToken cancellationToken)
    {
        // Read GroupInfo From Sql
        string groupId = null!;
        if(request.Type == "User")
        {
            var chatGroup = await _groupRepository.GetChatGroup(request.ReceiverId, _identityService.Username, cancellationToken);
            if (chatGroup == null)
            {
                var guid = Guid.NewGuid().ToString();
                var regDate = DateTime.Now;
                // Create  Group In ElasticSearch
                await CreateGroup(guid, request.ReceiverId, regDate, cancellationToken);

                // Create  Group User In ElasticSearch
                await CreateGroupUser(guid, request.ReceiverId,regDate, cancellationToken);
                groupId = guid;
            }
            else
            {
                groupId = chatGroup.GroupId;
            }
        }
        else if(request.Type == "Group")
        {
            groupId = request.ReceiverId;
            var group = await _groupRepository.GetGroupByGroupId(groupId, cancellationToken);
            if (group == null)
            {
                return null;
            }
            groupId = group.GroupId;
        }

        return await GetGroupUser(groupId, cancellationToken);
    }
    
    private async Task<GetChatInfoResponseDto> GetGroupUser(string groupId,
        CancellationToken cancellationToken)
    {
        var groupUser = await _groupRepository.GetGroupUser(_identityService.Username,
            groupId, cancellationToken);

        if (groupUser == null)
            return null;
        
        if (groupUser is not { Type: "User" }) return groupUser;
        
        var profile = await _getProfileInfo(groupUser.ReceiverId);
        groupUser.ProfilePictureId = profile.ProfilePictureId;
        groupUser.Title = profile.Fullname;

        return groupUser;
    }

    private async Task<Entities.Models.Profile.Profile> _getProfileInfo(string username)
    {
        var getProfilesByUsernamesRequest = new GetProfilesByUsernamesQuery
        {
            Usernames = new List<string>
            {
                username
            }
        };
        var userProfiles = await _sender.Send(getProfilesByUsernamesRequest);
        return userProfiles.FirstOrDefault()!;
    }

    private async Task CreateGroup(string guid,string receiverId,DateTime regDate, CancellationToken cancellationToken)
    {
        var group = new Group
        {
            GroupId = guid,
            Creator = _identityService.Username,
            RegDateTime = regDate,
            Type = "User",
            Description = "Chat",
            Members = new List<string> { _identityService.Username, receiverId },
            Admins = new List<string> { _identityService.Username, receiverId },
            MembersCount = 2,
        };
        await _groupRepository.CreateGroup(group, cancellationToken);
    }

    private async Task CreateGroupUser(string guid, string receiverId, DateTime regDate, CancellationToken cancellationToken)
    {
        var groupUser = new GroupUser
        {
            GroupId = guid,
            ReceiverId = receiverId,
            FirstUnreadMessageId = 0,//Todo
            UnReadMessages = new List<long>(),
            LastActionTime = regDate.ToString(),
            Type = "User",
            Content = string.Empty,
            RegUser = _identityService.Username,
            IsSeen = false,
            RegDateTime = regDate,
            Username = _identityService.Username
        };

        // Create  Group User For current User
        await _groupRepository.CreateGroupUser(groupUser, cancellationToken);

        // Create  Group User For Receiver
        await CreateGroupUserForReceiver(groupUser,receiverId, cancellationToken);
    }

    private async Task CreateGroupUserForReceiver(GroupUser groupUser,string receiverId, CancellationToken cancellationToken)
    {
        groupUser.ReceiverId = _identityService.Username;
        groupUser.Username = receiverId;
        await _groupRepository.CreateGroupUserForReceiver(groupUser, cancellationToken);
    }
}