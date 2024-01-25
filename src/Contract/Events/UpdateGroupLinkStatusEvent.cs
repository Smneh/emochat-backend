namespace Contract.Events;

public class UpdateGroupLinkStatusEvent
{
    public string GroupId { get; set; }
    public bool Status { get; set; }
}