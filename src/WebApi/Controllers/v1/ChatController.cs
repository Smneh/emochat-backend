using Contract.Commands.Group;
using Contract.DTOs.Chat;
using Contract.Queries.Chat;
using Entities.Models.Chat;
using Entities.Models.Profile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class ChatController : BaseController
{
    private readonly ISender _sender;

    public ChatController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetChatInfoResponseDto), 200)]
    public async Task<IActionResult> GetChatInfo([FromQuery] GetChatInfoQuery getChatInfoQuery, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(getChatInfoQuery, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetChatsListResponseDto>), 200)]
    public async Task<IActionResult> GetChatsList([FromQuery] GetChatsListQuery getChatsListQuery,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(getChatsListQuery, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetMessagesResponseDto>), 200)]
    public async Task<IActionResult> GetMessages([FromQuery] GetMessagesQuery getMessagesQuery,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(getMessagesQuery, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SearchMessageResponseDto>), 200)]
    public async Task<IActionResult> SearchMessage([FromQuery] SearchMessageQuery searchMessage,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(searchMessage, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(List<Message>), 200)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand MessageCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(MessageCommand, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateGroupCommand createGroupCommand, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(createGroupCommand, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> AddGroupMember([FromBody] AddGroupMemberCommand groupMemberCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(groupMemberCommand, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> RemoveGroupMember([FromBody] RemoveGroupMemberCommand removeGroupMemberCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(removeGroupMemberCommand, cancellationToken));
    }

    [HttpDelete]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> DeleteGroupById([FromQuery] DeleteGroupCommand deleteGroupCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(deleteGroupCommand, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> VisitMessages([FromBody] VisitMessagesCommand visitMessagesCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(visitMessagesCommand, cancellationToken));
    }

    [HttpDelete("{groupId}/{messageId}")]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> DeleteMessage(long messageId, string groupId, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new DeleteMessageCommand
        {
            MessageId = messageId,
            GroupId = groupId
        }, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> AddGroupAdmin([FromBody] AddGroupAdminCommand groupAdminCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(groupAdminCommand, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> RemoveGroupAdmin([FromBody] RemoveGroupAdminCommand removeGroupAdminCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(removeGroupAdminCommand, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(IResult), 200)]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupCommand updateGroupCommand,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(updateGroupCommand, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetGroupByIdResponseDto), 200)]
    public async Task<IActionResult> GetGroupById(
        [FromQuery] GetGroupByIdQuery groupByIdQuery, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(groupByIdQuery, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetMessagesResponseDto>), 200)]
    public async Task<IActionResult> SearchMessagesWithCount([FromQuery] SearchMessagesWithCountQuery searchMessagesWithCountQuery,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(searchMessagesWithCountQuery, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetUnReadMessagesCountResponseDto), 200)]
    public async Task<IActionResult> GetUnReadMessagesCount([FromQuery] GetUnReadMessagesCountQuery unReadMessagesCountQuery,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(unReadMessagesCountQuery, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Profile>), 200)]
    public async Task<IActionResult> GetUsersWithExcludeFilter([FromQuery] GetUsersWithExcludeFilterQuery unReadsCountQuery,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(unReadsCountQuery, cancellationToken));
    }
}