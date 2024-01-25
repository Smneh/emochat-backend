using MediatR;

namespace Contract.Commands.Group;

public class CreateGroupCommand : IRequest<Unit>
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ProfilePictureId { get; set; } = default!;
    public List<string> Members { get; set; } = new List<string>();
}
