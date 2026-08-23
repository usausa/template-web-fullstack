namespace Template.WebApp.Host.Areas.Default.Controllers;

using System.Diagnostics;

using Template.WebApp.Host.Areas.Default.Models;

[AllowAnonymous]
public sealed class ErrorController : BaseDefaultController
{
    [HttpGet("~/error")]
    [HttpPost("~/error")]
    [HttpGet("~/error/{statusCode:int}")]
    [HttpPost("~/error/{statusCode:int}")]
#pragma warning disable CA5391
    // 表示専用のエラーページ。ReExecuteは元リクエストのメソッドを維持するためPOSTも受けるが、偽造防止トークン検証は不要
    public IActionResult Index(int? statusCode)
    {
        return View(new ErrorViewModel
        {
            StatusCode = statusCode ?? StatusCodes.Status500InternalServerError,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
#pragma warning restore CA5391
}
