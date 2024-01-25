using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class GetUserCommunicationUsernamesHandler: IRequestHandler<GetUserCommunicatedUsernamesQuery, List<GetUserCommunicatedUsernamesResponseDto>>
{
    
    private readonly GroupRepository _groupRepository;

    public GetUserCommunicationUsernamesHandler(GroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }
    public async Task<List<GetUserCommunicatedUsernamesResponseDto>> Handle(GetUserCommunicatedUsernamesQuery request, CancellationToken cancellationToken)
    {
        var result = await _groupRepository.GetUserCommunicationUsernames( 
            request.Username, cancellationToken);

        return result;
    }
}