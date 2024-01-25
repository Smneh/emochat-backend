using Contract.DTOs.Chat;

namespace Contract.Events;

public class VisitorAddedEvent
{
    public GroupVisitorDto Visitor { get; set; }
    public string GroupId { get; set; }
    public List<long> MessageIds { get; set; }
    public List<string> Members { get; set; }
}