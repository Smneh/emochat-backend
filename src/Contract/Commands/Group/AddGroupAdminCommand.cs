using MediatR;

namespace Contract.Commands.Group;

public class AddGroupAdminCommand : IRequest
{
    public string GroupId { get; set; }
    public string member { get; set; }
}