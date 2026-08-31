#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Controllers;
#pragma warning restore CA1716

using Microsoft.AspNetCore.Authentication.Cookies;

using Template.WebApp.Host.Areas.Default.Models;

#pragma warning disable CA1054
[AllowAnonymous]
public sealed class AccountController : BaseDefaultController
{
    private AccountService AccountService { get; }

    public AccountController(AccountService accountService)
    {
        AccountService = accountService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Login([FromForm] AccountLoginForm form, string? returnUrl)
    {
        if (ModelState.IsValid)
        {
            var account = await AccountService.AuthenticateAsync(form.Name, form.Password);
            if (account is not null)
            {
                var identity = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, account.Name),
                        new Claim(ClaimTypes.Name, account.Name),
                        new Claim(ClaimTypes.Role, account.Role)
                    ],
                    CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return !String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("~/");
            }

            ModelState.AddModelError(nameof(form.Password), Messages.LoginFailed);
        }

        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }
}
#pragma warning restore CA1054
