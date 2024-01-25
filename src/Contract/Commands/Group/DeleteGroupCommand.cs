using Contract.DTOs.Chat;
using MediatR;

namespace Contract.Commands.Group;

public class DeleteGroupCommand : IRequest<DeleteGroupResponseDto>
{
    public string GroupId { get; set; }
}