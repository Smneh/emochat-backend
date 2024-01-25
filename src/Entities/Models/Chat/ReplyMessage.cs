namespace Entities.Models.Chat;

public class ReplyMessage
{
    public string Content { get; set; }
    public string Attachments { get; set; }
    public int GroupTypeId { get; set; }
    public string ReceiverId { get; set; }
    public long? MessageId { get; set; }
    public int MessageTypeId { get; set; }
    public string UniqueId { get; set; }
    public string RegUser { get; set; }
}