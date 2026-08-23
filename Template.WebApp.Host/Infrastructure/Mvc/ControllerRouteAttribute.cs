namespace Template.WebApp.Host.Infrastructure.Mvc;

public sealed class ControllerRouteAttribute : RouteAttribute
{
    public ControllerRouteAttribute()
        : base("~/[controller]")
    {
    }
}
