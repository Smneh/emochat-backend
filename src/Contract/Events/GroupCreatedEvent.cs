namespace Contract.Events;

public class GroupCreatedEvent
{
    public string Type { get; set; }
    public string UniqueId { get; set; }
    public string GroupId { get; set; }
    public string Description { get; set; }
    public string Creator { get; set; }
    public string Content { get; set; }
    public int LastMessageId { get; set; }
    public long FollowerCount { get; set; }
    public int OldId { get; set; }
    public DateTime RegDateTime { get; set; }
    public bool IsObsolete { get; set; }
    public bool IsSeen { get; set; }
    public string Attachments { get; set; }
    public string RegUser { get; set; }
    public string UnreadMessageIds { get; set; }
    public string UnreadMessages { get; set; }
    public long FirstUnreadMessageId { get; set; }
    public long MembersSetId { get; set; }
    public int FirstMessageId { get; set; }
    public string ReceiverId { get; set; }
    public string ProfilePictureId { get; set; }
    public string TypeTitle { get; set; }
    public string LastActionTime { get; set; }
    public List<string> Members { get; set; }
    public int MembersCount { get; set; }
}