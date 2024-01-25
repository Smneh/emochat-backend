namespace Contract.DTOs.Chat;

public class GetMessagesResponseDto
{
    public long MessageId { get; set; }
    public string Content { get; set; } = default!;
    public string ReceiverId { get; set; } = default!;
    public string UniqueId { get; set; } = default!;
    public long GroupTypeId { get; set; }
    public long? ParentId { get; set; }
    public string? FullName { get; set; } = default!;
    public string ProfileAddress { get; set; } = default!;
    public bool IsObsolete { get; set; }
    public bool CopyStatus { get; set; }
    public string RegUser { get; set; } = default!;
    public long MessageTypeId { get; set; }
    public string Attachments { get; set; } = default!;
    public DateTime RegDateTime { get; set; }
    public DateTime RegDate { get; set; }

    public List<GroupVisitorDto> Visitors { get; set; }
    public bool IsSelf { get; set; } = false;
    public bool IsDelivered { get; set; } = false;
    public string Meta { get; set; }
    public ReplyMessageDto? ReplyMessage { get; set; }
    public bool IsFirst { get; set; }

}