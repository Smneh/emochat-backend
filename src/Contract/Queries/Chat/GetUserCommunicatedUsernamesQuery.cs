using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class GetUserCommunicatedUsernamesQuery : IRequest<List<GetUserCommunicatedUsernamesResponseDto>>
{
    public string Username { get; set; } = default!;
}