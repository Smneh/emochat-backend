using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Queries.Chat;

public class GetUnReadMessagesCountQuery : IRequest<GetUnReadMessagesCountResponseDto>
{
  
}