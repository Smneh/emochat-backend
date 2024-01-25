using Contract.DTOs.IAM;
using MediatR;

namespace Contract.Commands.IAM;

public class LoginCommand : IRequest<UserData>
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}