namespace Entities.Models.Chat;

public class Message
{
    public string Content { get; set; }
    public long MessageId { get; set; }
    public long GroupTypeId { get; set; }
    public string UniqueId { get; set; }
    public string ReceiverId { get; set; }
    public bool IsObsolete { get; set; }
    public long MessageTypeId { get; set; }
    public string Type { get; set; }
    public string RegUser { get; set; }
    public string Attachments { get; set; }
    public List<GroupVisitor> Visitors { get; set; }
    public DateTime RegDateTime { get; set; }
    public DateTime RegDate { get; set; }
    public long? ParentId { get; set; }
    public ReplyMessage? ReplyMessage { get; set; }
    public bool IsFirst { get; set; }
}