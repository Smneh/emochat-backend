using MediatR;

namespace Contract.Commands.Group;

public class AddGroupMemberCommand  :IRequest
{
    public string GroupId { get; set; }
    public string member { get; set; }
}