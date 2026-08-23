namespace Template.WebApp.Host.Areas.Default.Models;

public sealed class DataEditForm
{
    [Required(ErrorMessage = Messages.Required)]
    [StringLength(Length.Name, ErrorMessage = Messages.MaxLength)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = Messages.Required)]
    [Range(0, 999_999_999, ErrorMessage = Messages.Range)]
    public int? Value { get; set; }
}
