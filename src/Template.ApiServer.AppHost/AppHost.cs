// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQLはコンテナとして起動する。データはボリュームへ残し、再起動しても消えないようにする
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var database = postgres.AddDatabase("template");

// 接続文字列はConnectionStrings__Defaultとして注入され、appsettingsの既定値を上書きする
builder.AddProject<Projects.Template_ApiServer_Host>("apiserver")
    .WithReference(database, connectionName: "Default")
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
