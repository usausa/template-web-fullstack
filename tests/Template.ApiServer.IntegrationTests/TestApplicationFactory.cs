namespace Template.ApiServer;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Testcontainers.PostgreSql;
using Testcontainers.Redis;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? container;

    private RedisContainer? cacheContainer;

    public async ValueTask InitializeAsync()
    {
        if (ContainerRuntime.IsAvailable)
        {
            container = new PostgreSqlBuilder("postgres:18-alpine").Build();
            cacheContainer = new RedisBuilder("valkey/valkey:9.1-alpine").Build();
            await Task.WhenAll(container.StartAsync(), cacheContainer.StartAsync());
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("http_ports", string.Empty);
        builder.UseSetting("ConnectionStrings:Default", container?.GetConnectionString() ?? throw new InvalidOperationException(ContainerRuntime.Reason));
        builder.UseSetting("ConnectionStrings:Cache", cacheContainer!.GetConnectionString());
        builder.UseSetting("Prometheus:Uri", string.Empty);
        builder.UseSetting("Profiler:SqlLog:Enable", "false");
        builder.UseSetting("Profiler:SqlTelemetry:Enable", "false");
        builder.UseSetting("Log:HttpLog", "false");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (container is not null)
        {
            await container.DisposeAsync();
        }

        if (cacheContainer is not null)
        {
            await cacheContainer.DisposeAsync();
        }
    }
}
