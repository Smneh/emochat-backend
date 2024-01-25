namespace Contract.Events;

public class ContentDeletedEvent
{
    public string GroupId { get; set; }
    public List<string> Members { get; set; }
    public long MessageId { get; set; }

}