using Contract.DTOs.IAM;
using MediatR;

namespace Contract.Commands.IAM;

public class UpdatePasswordCommand : IRequest<UpdatePasswordResult>
{
    public string Password { get; set; }
    public string NewPassword { get; set; }
}