namespace Contract.Events;

public class RejectCallEvent
{
    public string Receiver { get; set; } = default!;
    public string Sender { get; set; } = default!;
    public string ConnectionId { get; set; } = default!;
}