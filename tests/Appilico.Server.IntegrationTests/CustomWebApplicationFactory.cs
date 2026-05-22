using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Appilico.Server.DataAccess.Data;

namespace Appilico.Server.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var databaseName = "IntegrationTestDb_" + Guid.NewGuid();

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=integration;Username=test;Password=test-only-password",
                ["JWT:Secret"] = "IntegrationTestJwtSecretForHardening12345",
                ["JWT:Issuer"] = "Appilico.Server",
                ["JWT:Audience"] = "Appilico.Client",
                ["Swagger:Enabled"] = "true",
                ["Stripe:Enabled"] = "false",
                ["AzureStorage:Enabled"] = "false",
                ["Email:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove production DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add InMemory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });
        });

        builder.UseEnvironment("Development");
    }
}
