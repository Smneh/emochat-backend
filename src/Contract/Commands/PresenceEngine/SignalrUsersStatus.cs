using Aerospike.Client;
using MediatR;

namespace Contract.Commands.PresenceEngine;

public class SignalrUsersStatus: IRequest
{
    public string Method { get; set; } = default!;
    public object Message { get; set; } = default!;
    public string Sender { get; set; } = default!;
    public List<Record> UsersInfo { get; set; } = default!;
    
    
}