namespace Contract.DTOs.Chat;

public class GetChatsListResponseDto
{
    public string Type { get; set; }
    public string UniqueId { get; set; }
    public string GroupId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Creator { get; set; }
    public int LastMessageId { get; set; }
    public DateTime RegDateTime { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsSeen { get; set; }
    public string Attachments { get; set; }
    public string RegUser { get; set; }
    public string UnreadMessageIds { get; set; }
    public int FirstMessageId { get; set; }
    public string ReceiverId { get; set; }
    public string ProfilePictureId { get; set; }
    public int TypeId { get; set; }
    public string TypeTitle { get; set; }
    public List<long> UnReadMessages { get; set; }

}