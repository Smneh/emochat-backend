namespace Contract.Events;

public class OnlineStatusEvent
{
    public string Username { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string LastActionTime { get; set; } = default!;
}