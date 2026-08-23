namespace Template.WebApp.Host.Infrastructure.Mvc;

public sealed class DefaultRouteAttribute : RouteAttribute
{
    public DefaultRouteAttribute()
        : base("~/")
    {
    }
}
