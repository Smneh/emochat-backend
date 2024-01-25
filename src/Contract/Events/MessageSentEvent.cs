using Contract.DTOs.Chat;

namespace Contract.Events;

public class MessageSentEvent
{
    public string Content { get; set; }
    public long MessageId { get; set; }
    public long GroupTypeId { get; set; }
    public string UniqueId { get; set; }
    public string? FullName { get; set; }
    public string? ProfileAddress { get; set; }
    public string ReceiverId { get; set; }
    public long MessageTypeId { get; set; }
    public string Type { get; set; }
    public string RegUser { get; set; }
    public string Attachments { get; set; }
    public DateTime RegDateTime { get; set; }
    public DateTime RegDate { get; set; }
    public List<string> Members { get; set; }
    public ReplyMessageDto? ReplyMessage { get; set; }
    public List<GroupVisitorDto> Visitors { get; set; }
    public bool IsFirst { get; set; }
}