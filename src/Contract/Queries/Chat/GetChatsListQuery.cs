using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class GetChatsListQuery : IRequest<List<GetChatsListResponseDto>>
{
    public string Type { get; set; } = default!;
    public int StartRow { get; set; } = 0;
    public int RowCount { get; set; } = 15;
}