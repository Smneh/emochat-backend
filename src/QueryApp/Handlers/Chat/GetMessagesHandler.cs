using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Contract.Queries.Profile;
using Core.Services;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class GetMessagesHandler : IRequestHandler<GetMessagesQuery, List<GetMessagesResponseDto>>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly ISender _sender;

    public GetMessagesHandler(GroupRepository groupRepository, IdentityService identityService, ISender sender)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _sender = sender;
    }

    public async Task<List<GetMessagesResponseDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var result = await _groupRepository.GetGroupMessages(request.GroupId, request.MessageId, request.Dir, request.Count);

        // Handle IsSelf
        result.ForEach(message =>
            {
                message.IsSelf = _identityService.Username == message.RegUser;
                message.ParentId = message.ReplyMessage?.MessageId ?? 0;
            }
        );

        // Handle Visitors
        var users = new List<string>();
        result.ForEach(x =>
        {
            users.Add(x.RegUser);
            users.AddRange(x.Visitors.Select(visitor => visitor.Username).ToList());
        });

        users = users.Distinct().ToList();

        var profiles = await _getProfileInfo(users);

        result.ForEach(message =>
        {
            message.FullName = profiles.FirstOrDefault(x => x.Username == message.RegUser)!.Fullname;
            message.ProfileAddress = profiles.FirstOrDefault(x => x.Username == message.RegUser)!.ProfilePictureId;

            message.Visitors.ForEach(visitor =>
            {
                visitor.FullName = profiles.FirstOrDefault(x => x.Username == visitor.Username)!.Fullname;
                visitor.ProfileAddress = profiles.FirstOrDefault(x => x.Username == visitor.Username)!.ProfilePictureId;
            });
        });

        // Handle Seen
        result.ForEach(message => { message.IsDelivered = message.Visitors.Any(visitor => visitor.Username != _identityService.Username); });

        // Handle ReplyMessage User Info
        result.ForEach(message =>
        {
            if (message.ReplyMessage != null)
            {
                message.ReplyMessage.ProfileAddress = profiles.FirstOrDefault(x => x.Username == message.ReplyMessage.RegUser)!.ProfilePictureId;
                message.ReplyMessage.FullName = profiles.FirstOrDefault(x => x.Username == message.ReplyMessage.RegUser)!.Fullname;
            }
        });


        return result;
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