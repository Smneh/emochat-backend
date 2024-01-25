using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class SearchMessageQuery : IRequest<List<SearchMessageResponseDto>>
{
    public string SearchStr { get; set; } = default!;
    public string Receiver { get; set; } = default!;
    public int StartRow { get; set; }
    public int RowCount { get; set; }
}