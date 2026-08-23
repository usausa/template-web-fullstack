namespace Template.WebApp.Host.Infrastructure.Mvc;

public static class HttpRequestExtensions
{
    public static bool IsInitialRequest(this HttpRequest request) => request.Query.Count == 0;
}
