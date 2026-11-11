// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgWeb();
var database = postgres.AddDatabase("template");

var cache = builder.AddValkey("cache")
    .WithDataVolume();

var initializer = builder.AddProject<Projects.Template_ApiServer_Initializer>("initializer")
    .WithReference(database, connectionName: "Default")
    .WaitFor(database);

// 接続文字列はConnectionStrings__Default / ConnectionStrings__cacheとして注入され、appsettingsの既定値を上書きする
var apiserver = builder.AddProject<Projects.Template_ApiServer_Host>("apiserver")
    .WithReference(database, connectionName: "Default")
    .WithReference(cache)
    .WithEnvironment("Database__Initialize", "false")
    .WaitForCompletion(initializer)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

// 前段のリバースプロキシ。/api を API へ振り分ける(別サービスを足す場合は経路を追加する)
builder.AddYarp("gateway")
    .WithConfiguration(yarp => yarp.AddRoute("/api/{**catch-all}", apiserver))
    .WaitFor(apiserver);

builder.Build().Run();
