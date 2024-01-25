using MediatR;

namespace Contract.Commands.Group;

public class RemoveGroupAdminCommand : IRequest
{
    public string GroupId { get; set; }
    public string member { get; set; }
}