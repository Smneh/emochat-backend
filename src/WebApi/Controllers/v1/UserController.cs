using Contract.Commands.Profile;
using Contract.Queries.Profile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class UserController : BaseController
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetUsersQuery query)
    {
        return Ok(await _sender.Send(query));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserInfo([FromQuery] GetUserInfoQuery query)
    {
        return Ok(await _sender.Send(query));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserCommand command)
    {
        return Ok(await _sender.Send(command));
    } 
}