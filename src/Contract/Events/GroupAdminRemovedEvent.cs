namespace Contract.Events;

public class GroupAdminRemovedEvent
{
    public string GroupId { get; set; }
    public string Member { get; set; }
}