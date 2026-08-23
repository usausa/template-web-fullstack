namespace Template.WebApp.Host.Areas.Default.Controllers;

using Microsoft.FeatureManagement;

public sealed class DashboardController : BaseDefaultController
{
    private IFeatureManager FeatureManager { get; }

    public DashboardController(IFeatureManager featureManager)
    {
        FeatureManager = featureManager;
    }

    [DefaultRoute]
    [HttpGet]
    public async ValueTask<IActionResult> Index()
    {
        // Feature flag example
        ViewData["FeatureCustomOption"] = await FeatureManager.IsEnabledAsync(FeatureFlags.CustomOption);

        return View();
    }
}
