namespace Contract.Events;

public class IncomingCallEvent
{
    public string ReceiverId { get; set; }
    public string Receiver { get; set; }
    public string Sender { get; set; } = default!;
    public string Fullname { get; set; }
    public string ProfilePictureId { get; set; }
    public string CallType { get; set; }
}