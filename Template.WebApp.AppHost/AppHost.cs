// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Template_WebApp_Host>("webapp")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
