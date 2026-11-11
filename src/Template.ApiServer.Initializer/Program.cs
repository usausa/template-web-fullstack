using Npgsql;

using Serilog;

using Smart.Data;

using Template.ApiServer.Accessors;
using Template.ApiServer.Initializer;
using Template.ApiServer.Services;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options =>
{
    options.ReadFrom.Configuration(builder.Configuration);
});

// Data
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddSingleton<IDbProvider>(new DelegateDbProvider(() => new NpgsqlConnection(connectionString)));
builder.Services.AddDataAccessors(typeof(GenericAccessor).Assembly);
builder.Services.AddSingleton<DatabaseService>();

// Build
using var host = builder.Build();

var log = host.Services.GetRequiredService<ILogger<Program>>();

// Initialize
log.InfoInitializeStart();
await host.Services.GetRequiredService<DatabaseService>().InitializeAsync("Assets/Data/Schema.sql", CancellationToken.None);
log.InfoInitializeEnd();
