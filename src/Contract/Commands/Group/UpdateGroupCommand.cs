using MediatR;

namespace Contract.Commands.Group;

public class UpdateGroupCommand : IRequest
{
    public string ReceiverId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ProfilePictureId { get; set; }
}