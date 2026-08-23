namespace Template.WebApp.Host.Areas.Default.Models;

public sealed class AccountLoginForm
{
    [Required(ErrorMessage = Messages.Required)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = Messages.Required)]
    public string Password { get; set; } = default!;
}
