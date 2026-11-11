namespace Template.ApiServer.Initializer;

internal static partial class Log
{
    // Initialize

    [LoggerMessage(Level = LogLevel.Information, Message = "Initialize start.")]
    public static partial void InfoInitializeStart(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Initialize end.")]
    public static partial void InfoInitializeEnd(this ILogger logger);
}
