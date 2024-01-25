using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version}/[controller]/[action]")]
public class BaseController : ControllerBase
{
}