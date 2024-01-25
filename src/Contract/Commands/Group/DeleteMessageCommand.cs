using MediatR;

namespace Contract.Commands.Group;

public class DeleteMessageCommand : IRequest
{
    public long MessageId { get; set; }
    public string GroupId { get; set; }
}