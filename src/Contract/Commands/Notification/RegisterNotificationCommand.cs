using MediatR;

namespace Contract.Commands.Notification;

public class RegisterNotificationCommand :  IRequest
{
    public string Username { get; set; }
    public string Content { get; set; } = default!;
    public string Sender { get; set; } = default!;
    public List<string>  Receivers { get; set; } = default!;
    public string MetaInfo { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Category { get; set; } = default!;
    public object Data { get; set; } = default!;
}