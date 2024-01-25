namespace Contract.DTOs.Chat;

public class ReplyMessageDto
{
    public string Content { get; set; }
    public string Attachments { get; set; }
    public string FullName { get; set; }
    public int GroupTypeId { get; set; }
    public string ReceiverId { get; set; }
    public long? MessageId { get; set; }
    public int MessageTypeId { get; set; }
    public string ProfileAddress { get; set; }
    public string UniqueId { get; set; }
    public string RegUser { get; set; }
}