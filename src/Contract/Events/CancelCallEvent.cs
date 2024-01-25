namespace Contract.Events;

public class CancelCallEvent
{
    public string Receiver { get; set; } = default!;
    public string Sender { get; set; } = default!;
}