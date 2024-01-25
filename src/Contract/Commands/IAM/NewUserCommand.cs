using Contract.DTOs.IAM;
using MediatR;

namespace Contract.Commands.IAM;

public class NewUserCommand : IRequest<UserData>
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Fullname { get; set; } = default!;
    public string Email { get; set; } = default!;
}