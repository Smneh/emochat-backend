namespace Contract.Events;

public class AcceptCallEvent
{
    public string Receiver { get; set; } = default!;
    public string Sender { get; set; } = default!;
    public string UniqueId { get; set; } = default!;
    public string ConnectionId { get; set; } = default!;
}