namespace Contract.DTOs.Chat;

public class SendMessageResponseDto
{
    public long MessageId { get; set; }
    public string UniqueId { get; set; }
    public string Content { get; set; }
    public string ReceiverId { get; set; }
    public string Type { get; set; }
}