// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var database = postgres.AddDatabase("template");

var cache = builder.AddValkey("cache")
    .WithDataVolume();

// 接続文字列はConnectionStrings__Default / ConnectionStrings__cacheとして注入され、appsettingsの既定値を上書きする
builder.AddProject<Projects.Template_ApiServer_Host>("apiserver")
    .WithReference(database, connectionName: "Default")
    .WithReference(cache)
    .WaitFor(database)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
