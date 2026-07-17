// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Service.Data.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text.Json;

namespace ChromaControl.Service.Tests;

public sealed class ServiceHealthFailureIntegrationTests
{
    [Fact]
    public async Task HealthReadyResponseIsUnhealthyAndIncludesWarningsWhenStartupFails()
    {
        await using var factory = new FailedStartupServiceWebApplicationFactory();
        var httpClient = factory.CreateDefaultClient();

        HttpResponseMessage? response = null;
        JsonDocument? document = null;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            response?.Dispose();
            document?.Dispose();

            response = await httpClient.GetAsync("/health/ready");
            var content = await response.Content.ReadAsStringAsync();
            document = JsonDocument.Parse(content);

            var startupEntry = document.RootElement.GetProperty("entries").GetProperty("startup");
            var startupStatus = startupEntry.GetProperty("status").GetString();

            if (string.Equals(startupStatus, HealthStatus.Unhealthy.ToString(), StringComparison.Ordinal))
            {
                break;
            }

            await Task.Delay(100);
        }

        Assert.NotNull(response);
        Assert.NotNull(document);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response!.StatusCode);

        var root = document!.RootElement;
        Assert.Equal(HealthStatus.Unhealthy.ToString(), root.GetProperty("status").GetString());

        var startup = root.GetProperty("entries").GetProperty("startup");
        var startupData = startup.GetProperty("data");

        Assert.Equal(HealthStatus.Unhealthy.ToString(), startup.GetProperty("status").GetString());
        Assert.Equal("Configuration database migration failed and retry threshold was exceeded.", startup.GetProperty("description").GetString());
        Assert.True(startupData.GetProperty("stopRequested").GetBoolean());

        Assert.True(startupData.TryGetProperty("transportWarningCount", out var warningCount));
        Assert.Equal(1, warningCount.GetInt32());

        var warnings = startupData.GetProperty("transportWarnings").EnumerateArray().Select(item => item.GetString());
        Assert.Contains(
            warnings,
            warning => string.Equals(
                warning,
                "Service:Transport:Port is ignored when Service:Transport:UseNamedPipes=true.",
                StringComparison.Ordinal));
    }
}

public sealed class FailedStartupServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _singleInstanceName = $"ChromaControl.Service.Tests.FailingFactory.{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("CHROMACONTROL_SINGLE_INSTANCE_NAME", _singleInstanceName);
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var testDatabasePath = Path.Combine(Path.GetTempPath(), $"chroma-control-service-tests-{Guid.NewGuid():N}.db");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ChromaControl:SingleInstanceName"] = $"ChromaControl.Service.Tests.Failing.{Guid.NewGuid():N}",
                ["Service:Transport:UseNamedPipes"] = "true",
                ["Service:Transport:Port"] = "50123",
                ["ConnectionStrings:Database"] = $"Data Source={testDatabasePath}",
                ["Database:Migration:MaxAttempts"] = "1",
                ["Database:Migration:RetryDelay"] = "00:00:00",
                ["Database:Migration:SuppressHostStop"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            var nonMigrationHostedServices = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService) && descriptor.ImplementationType != typeof(MigrationService))
                .ToList();

            foreach (var descriptor in nonMigrationHostedServices)
            {
                services.Remove(descriptor);
            }

            var migrationRunnerDescriptors = services
                .Where(descriptor => descriptor.ServiceType == typeof(IDatabaseMigrationRunner))
                .ToList();

            foreach (var descriptor in migrationRunnerDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IDatabaseMigrationRunner, AlwaysFailingDatabaseMigrationRunner>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable("CHROMACONTROL_SINGLE_INSTANCE_NAME", null);
        base.Dispose(disposing);
    }
}

internal sealed class AlwaysFailingDatabaseMigrationRunner : IDatabaseMigrationRunner
{
    public Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("forced startup failure");
    }
}
