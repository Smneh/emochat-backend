using MediatR;

namespace Contract.Commands.Profile;

public class UpdateUserCommand : IRequest
{
    public string Fullname { get; set; } = default!;
}