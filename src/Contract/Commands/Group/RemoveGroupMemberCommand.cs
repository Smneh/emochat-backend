using MediatR;

namespace Contract.Commands.Group;

public class RemoveGroupMemberCommand : IRequest
{
    public string GroupId { get; set; }
    public string member { get; set; }
}