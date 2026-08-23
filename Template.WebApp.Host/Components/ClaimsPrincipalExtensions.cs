namespace Template.WebApp.Host.Components;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAuthenticated(this ClaimsPrincipal principal) => principal.Identity?.IsAuthenticated ?? false;

    public static bool IsAdministrator(this ClaimsPrincipal principal) => principal.IsInRole(Roles.Administrator);
}
