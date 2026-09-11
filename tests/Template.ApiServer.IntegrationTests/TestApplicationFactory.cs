namespace Template.ApiServer;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Testcontainers.PostgreSql;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // テストクラス単位でPostgreSQLコンテナを起動し、本番相当のDBで検証する
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("http_ports", string.Empty);
        builder.UseSetting("ConnectionStrings:Default", container.GetConnectionString());
        builder.UseSetting("Prometheus:Uri", string.Empty);
        builder.UseSetting("Profiler:SqlLog:Enable", "false");
        builder.UseSetting("Profiler:SqlTelemetry:Enable", "false");
        builder.UseSetting("Log:HttpLog", "false");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await container.DisposeAsync();
    }
}
