using System;
using System.IO;
using HavilaKystruten.Maritime.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HavilaKystruten.Maritime.Tests.Integration;

public class MaritimeWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var contentRoot = ResolveContentRoot();

        builder.UseEnvironment("Development");
        builder.UseContentRoot(contentRoot);
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.AddJsonFile(Path.Combine("config", "appsettings.json"), optional: true);
            configurationBuilder.AddJsonFile(Path.Combine("config", "appsettings.Production.json"), optional: true);
            configurationBuilder.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(new KeyVaultConfiguration
            {
                VaultUri = "https://placeholder.vault.azure.net/",
                TenantId = "11111111-1111-1111-1111-111111111111",
                ClientId = "22222222-2222-2222-2222-222222222222",
                ClientSecret = "fake-secret"
            });

            services.AddSingleton(new PowerBIConfiguration
            {
                ClientId = "33333333-3333-3333-3333-333333333333",
                ClientSecret = "fake-powerbi-secret",
                TenantId = "44444444-4444-4444-4444-444444444444",
                WorkspaceId = "55555555-5555-5555-5555-555555555555",
                Username = "test@havila.no",
                Password = "fake-password"
            });
        });
    }

    private static string ResolveContentRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Program.cs")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate project content root containing Program.cs.");
    }
}
