using MediatR;

namespace Contract.Commands.PresenceEngine;

public class SendUserStatusCommand : IRequest
{
    public string Username { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string LastActionTime { get; set; } = default!;
}