using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Core.Services;
using MediatR;
using Repository.Group;

namespace QueryApp.Handlers.Chat;

public class GetUnReadMessagesCountHandler : IRequestHandler<GetUnReadMessagesCountQuery, GetUnReadMessagesCountResponseDto>
{
    private readonly GroupRepository _groupRepository;
    private readonly IdentityService _identityService;


    public GetUnReadMessagesCountHandler(GroupRepository groupRepository, IdentityService identityService)
    {
        _groupRepository = groupRepository;
        _identityService = identityService;
    }

    public async Task<GetUnReadMessagesCountResponseDto> Handle(GetUnReadMessagesCountQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetUnReadsCount(_identityService.Username,  cancellationToken);

        var unReadsCountDto = new GetUnReadMessagesCountResponseDto
        {
            ChatCount = GetUnReadsCountByType(groups , "User"),
            GroupCount = GetUnReadsCountByType(groups , "Group"),
            ProjectGroupCount = GetUnReadsCountByType(groups , "ProjectGroup"),
            SpecialGroupCount = GetUnReadsCountByType(groups , "SpecialGroup"),
        };

        return unReadsCountDto;
    }


    private long GetUnReadsCountByType(List<GetChatsListResponseDto> groups, string type)
    {
        var filterGroups = groups.Where(group => group.Type == type).ToList();
        var unReadsCount = filterGroups.Sum(group => group.UnReadMessages.Count);
        return  unReadsCount;
    }
}