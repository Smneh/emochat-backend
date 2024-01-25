namespace Contract.Events;

public class EndCallEvent
{
    public string Receiver { get; set; } = default!;
    public string Sender { get; set; } = default!;
}