using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class GetGroupByIdQuery : IRequest<GetGroupByIdResponseDto>
{
    public string GroupId { get; set; } = default!;
}