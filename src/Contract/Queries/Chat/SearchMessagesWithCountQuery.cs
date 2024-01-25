using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class SearchMessagesWithCountQuery : IRequest<List<GetMessagesResponseDto>>
{
    public string GroupId { get; set; } = default!;
    public long MessageId { get; set; }
    public int Count { get; set; }
}