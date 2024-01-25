using Contract.Commands.IAM;
using Contract.DTOs.IAM;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class AuthenticationController : BaseController
{
    private readonly ISender _sender;

    public AuthenticationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand loginRequest)
    {
        return Ok(await _sender.Send(loginRequest));
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] NewUserCommand command)
    {
        return Ok(await _sender.Send(command));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        return Ok(
            await _sender.Send(new LogoutCommand())
        );
    }

    [HttpPut]
    [ProducesResponseType(typeof(List<UpdatePasswordResult>), 200)]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordCommand request)
    {
        return Ok(await _sender.Send(request));
    }
}