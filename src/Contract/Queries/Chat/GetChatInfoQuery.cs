using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class GetChatInfoQuery : IRequest<GetChatInfoResponseDto>
{
    public string ReceiverId { get; set; } = default!;
    public string Type { get; set; } = default!;
}