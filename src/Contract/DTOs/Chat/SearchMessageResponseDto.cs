namespace Contract.DTOs.Chat;

public class SearchMessageResponseDto
{
    public string ReceiverId { get; set; } = default!;
    public string RegDateTime { get; set; } = default!;
    public string RegUser { get; set; } = default!;
    public string AccessType { get; set; } = default!;
    public string ProfilePictureId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Content { get; set; } = default!;
    public long MessageId { get; set; }
    public long ContentId { get; set; }
    public long TypeId { get; set; }
    public long MatchCount { get; set; }
}