using MediatR;

namespace Contract.Commands.Profile;

public class UpdateProfilePictureCommand : IRequest<string>
{
    public string NewId { get; set; }
    public string Field { get; set; }
    public string Username { get; set; }
}