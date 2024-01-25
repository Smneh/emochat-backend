namespace Contract.Events;

public class UpdateGroupSendFileStatusEvent
{
    public string GroupId { get; set; }
    public bool Status { get; set; }
}