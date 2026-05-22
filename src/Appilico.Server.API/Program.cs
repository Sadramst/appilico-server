using Appilico.Server.API.Data;
using Appilico.Server.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// TODO: Remove after verifying all persisted PostgreSQL timestamps are UTC-compatible.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Host.UseAppilicoLogging(builder.Configuration);
builder.Services.AddAppilicoApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseAppilicoPipeline();
app.MapAppilicoEndpoints();

await DatabaseSeeder.SeedAsync(app.Services);

app.Run();

/// <summary>Entry point class for WebApplicationFactory.</summary>
public partial class Program { }
