namespace Entities.Models.Notification;

public class Notification
{
    public string Id { get; set; } = default!;
    public long ExternalId { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Sender { get; set; } = default!;
    public List<string> Receivers { get; set; } = default!;
    public string MetaInfo { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string TypeTitle { get; set; } = default!;
    public string Category { get; set; } = default!;
    public bool IsSeen { get; set; } = default!;
    public DateTime RegDateTime { get; set; } = default!;
    public DateTime SeenDate { get; set; } = default!;
    public bool IsObsolete { get; set; }
    public string UniqueId { get; set; } = default!;
    public string ParameterValues { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public long ContentId { get; set; } = default!;
    public string SenderProfile { get; set; } = default!;
    public string SenderFullname { get; set; } = default!;
    public string SenderUsername { get; set; } = default!;
    public string Username { get; set; } = default!;
}