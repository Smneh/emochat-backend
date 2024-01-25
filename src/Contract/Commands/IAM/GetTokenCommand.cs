using Contract.DTOs.IAM;
using MediatR;

namespace Contract.Commands.IAM;

public class GetTokenCommand : IRequest<UserToken>
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;    
}