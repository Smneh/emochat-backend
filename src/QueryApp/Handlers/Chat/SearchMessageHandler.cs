using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Contract.Queries.Profile;
using Core.Services;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class SearchMessageHandler : IRequestHandler<SearchMessageQuery, List<SearchMessageResponseDto>>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;
    private readonly ISender _sender;

    public SearchMessageHandler(GroupRepository groupRepository, IdentityService identityService, ISender sender)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
        _sender = sender;
    }

    public async Task<List<SearchMessageResponseDto>> Handle(SearchMessageQuery request, CancellationToken cancellationToken)
    {
        var result = await _groupRepository.FullTextSearchInChat( request.SearchStr, request.Receiver, request.StartRow, request.RowCount);

        // Get RegUsers Profiles
        var regUsers = result.Select(x => x.RegUser).ToList().Distinct().ToList();
        var profiles = await _getProfileInfo(regUsers);
        
        result.ForEach(message =>
        {
            message.FullName = profiles.FirstOrDefault(x => x.Username == message.RegUser)!.Fullname;
            message.ProfilePictureId = profiles.FirstOrDefault(x => x.Username == message.RegUser)!.ProfilePictureId;
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