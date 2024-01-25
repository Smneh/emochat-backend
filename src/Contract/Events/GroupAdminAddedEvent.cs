namespace Contract.Events;

public class GroupAdminAddedEvent
{
    public string GroupId { get; set; }
    public string Member { get; set; }
}