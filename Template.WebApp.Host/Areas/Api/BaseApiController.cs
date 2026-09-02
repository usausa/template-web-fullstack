namespace Template.WebApp.Host.Areas.Api;

[Area("api")]
[Route("[area]/[controller]/[action]")]
[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase;
