namespace Template.ApiServer;

using System.Runtime.CompilerServices;

// コンテナランタイムが無いときはスキップするTheory
[AttributeUsage(AttributeTargets.Method)]
public sealed class ContainerTheoryAttribute : TheoryAttribute
{
    public ContainerTheoryAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        Skip = ContainerRuntime.Reason;
        SkipUnless = nameof(ContainerRuntime.IsAvailable);
        SkipType = typeof(ContainerRuntime);
    }
}
