namespace Template.WebApp.Host.Areas.Default.Models;

public sealed class ErrorViewModel
{
    public int StatusCode { get; set; }

    public string RequestId { get; set; } = default!;

    public bool ShowRequestId => !String.IsNullOrEmpty(RequestId);
}
