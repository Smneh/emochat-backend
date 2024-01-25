namespace Entities.Models.Chat;

public class GroupUser
{
    public string GroupId { get; set; }
    public string Type { get; set; }
    public string Creator { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Attachments { get; set; }
    public string RegUser { get; set; }
    public string ReceiverId { get; set; }
    public string ProfilePictureId { get; set; }
    public long LastMessageId { get; set; }
    public bool IsSeen { get; set; }
    public DateTime RegDateTime { get; set; }
    public long FirstUnreadMessageId { get; set; }
    public List<long> UnReadMessages { get; set; }
    public string LastActionTime { get; set; }
    public string Username { get; set; }
}