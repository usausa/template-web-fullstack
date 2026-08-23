namespace Template.WebApp.Host.Areas.Default.Controllers;

public sealed class DashboardController : BaseDefaultController
{
    [DefaultRoute]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
