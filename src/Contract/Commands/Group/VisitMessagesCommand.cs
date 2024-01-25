using MediatR;

namespace Contract.Commands.Group;

public class VisitMessagesCommand : IRequest
{
    public string GroupId { get; set; } = default!;
    public long MessageId { get; set; }
}