namespace Template.WebApp.Host.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc.Filters;

using Template.WebApp.Infrastructure.Storage;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class StorageExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is StorageException)
        {
            context.Result = new BadRequestResult();
            context.ExceptionHandled = true;
        }
    }
}
