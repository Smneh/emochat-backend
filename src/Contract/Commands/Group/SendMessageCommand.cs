using Contract.DTOs.Chat;
using Core.Utilities.Assistant;
using MediatR;

namespace Contract.Commands.Group;

public class SendMessageCommand : IRequest<SendMessageResponseDto>
{
    public string Content { get; set; }
    public string ReceiverId { get; set; }
    public string ReceiverType { get; set; }
    public string RegDate { get; set; } = DateHelper.TodayDateInt().ToString();
    public string RegTime { get; set; } = DateHelper.ToTimeInt(DateTime.Now).ToString();
    public string Attachments { get; set; }
    public bool Self { get; set; } = true;
    public long? ParentId { get; set; }
    public long MessageTypeId { get; set; }
    public ReplyMessageDto? ReplyMessage { get; set; }
    public bool IsFirst { get; set; } = false;
}