#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Models;
#pragma warning restore CA1716

public sealed class AccountLoginForm
{
    [Required(ErrorMessage = Messages.Required)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = Messages.Required)]
    public string Password { get; set; } = default!;
}
