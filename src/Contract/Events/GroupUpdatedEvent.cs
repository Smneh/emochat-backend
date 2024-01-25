namespace Contract.Events;

public class GroupUpdatedEvent
{
    public string ReceiverId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ProfilePictureId { get; set; }
}