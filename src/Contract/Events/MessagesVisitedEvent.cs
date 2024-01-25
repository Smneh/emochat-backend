namespace Contract.Events;

public class MessagesVisitedEvent
{
    public string GroupId { get; set; }
    public long MessageId { get; set; }
}