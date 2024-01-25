namespace Contract.DTOs.PresenceEngine;

public class PeMessage
{
    public string Method { get; set; } = default!;
    public object Message { get; set; } = default!;
    public string Sender { get; set; } = default!;
    public List<string> Receivers { get; set; } = default!;
}