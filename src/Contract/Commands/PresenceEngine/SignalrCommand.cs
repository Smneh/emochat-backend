using Aerospike.Client;
using MediatR;

namespace Contract.Commands.PresenceEngine;

public class SignalrCommand : INotification
{
    public string Method { get; set; }
    public object Message { get; set; }
    public List<Record> Receivers { get; set; }
    public List<Dictionary<string, object>> ActiveSessions { get; set; }
}