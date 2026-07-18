// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Protos.Settings;
using ChromaControl.Service.Data.Services;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace ChromaControl.Service.Tests;

public sealed class ServiceGrpcIntegrationTests : IClassFixture<ServiceWebApplicationFactory>
{
    private readonly ServiceWebApplicationFactory _factory;

    public ServiceGrpcIntegrationTests(ServiceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SettingsGrpcGetStringReturnsDefaultWhenMissing()
    {
        var httpClient = _factory.CreateDefaultClient();

        using var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        var client = new SettingsGrpc.SettingsGrpcClient(channel);

        var response = await client.GetStringAsync(new GetSettingRequest
        {
            Module = "integration",
            Name = "missing-setting"
        });

        Assert.Equal(string.Empty, response.Value);
    }

    [Fact]
    public async Task HealthReadyResponseIncludesTransportWarningsData()
    {
        var httpClient = _factory.CreateDefaultClient();

        using var response = await httpClient.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var startupEntry = root.GetProperty("entries").GetProperty("startup");
        var startupData = startupEntry.GetProperty("data");

        Assert.True(startupData.TryGetProperty("transportWarningCount", out var warningCount));
        Assert.Equal(1, warningCount.GetInt32());

        Assert.True(startupData.TryGetProperty("transportWarnings", out var warnings));
        Assert.Contains(
            warnings.EnumerateArray().Select(item => item.GetString()),
            warning => string.Equals(
                warning,
                "Service:Transport:Port<=0 enables dynamic server TCP port selection, which cannot be auto-discovered by TCP clients.",
                StringComparison.Ordinal));
    }
}

public sealed class ServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _singleInstanceName = $"ChromaControl.Service.Tests.Factory.{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("CHROMACONTROL_SINGLE_INSTANCE_NAME", _singleInstanceName);
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var testDatabasePath = Path.Combine(Path.GetTempPath(), $"chroma-control-service-tests-{Guid.NewGuid():N}.db");

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ChromaControl:SingleInstanceName"] = $"ChromaControl.Service.Tests.{Guid.NewGuid():N}",
                ["Service:Transport:UseNamedPipes"] = "false",
                ["Service:Transport:Port"] = "0",
                ["ConnectionStrings:Database"] = $"Data Source={testDatabasePath}",
                ["Database:Migration:MaxAttempts"] = "2",
                ["Database:Migration:RetryDelay"] = "00:00:00"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Keep migration startup hosted service, but remove all other background hosted services.
            var nonMigrationHostedServices = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService) && descriptor.ImplementationType != typeof(MigrationService))
                .ToList();

            foreach (var descriptor in nonMigrationHostedServices)
            {
                services.Remove(descriptor);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable("CHROMACONTROL_SINGLE_INSTANCE_NAME", null);
        base.Dispose(disposing);
    }
}
