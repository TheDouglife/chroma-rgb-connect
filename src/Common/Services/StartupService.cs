// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChromaConnect.Common.Services;

/// <summary>
/// The Chroma Control startup service.
/// </summary>
internal sealed partial class StartupService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    [LoggerMessage(0, LogLevel.Information, "Chroma Control {property}: {value}", EventName = "Startup")]
    private static partial void LogStartup(ILogger logger, string property, string? value);

    [LoggerMessage(1, LogLevel.Warning, "Chroma Control transport configuration warning: {Message}", EventName = "TransportConfigurationWarning")]
    private static partial void LogTransportConfigurationWarning(ILogger logger, string message);

    /// <summary>
    /// Creates a <see cref="StartupService"/> instance.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="hostEnvironment">The <see cref="IHostEnvironment"/>.</param>
    public StartupService(ILogger<StartupService> logger, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var transportOptions = ChromaConnectExtensions.ResolveTransportOptions(_configuration);
        var transportWarnings = ChromaConnectExtensions.GetTransportConfigurationWarnings(_configuration, transportOptions, forServer: true);

        LogStartup(_logger, "version", _configuration.GetSection("ChromaConnect")["VERSION"]);
        LogStartup(_logger, "environment", _hostEnvironment.EnvironmentName);
        LogStartup(_logger, "app path", _configuration.GetChromaConnectPath("app").Trim('\\'));
        LogStartup(_logger, "data path", _configuration.GetChromaConnectPath("data"));
        LogStartup(_logger, "transport mode", ChromaConnectExtensions.GetTransportModeLabel(transportOptions));
        LogStartup(_logger, "transport endpoint", ChromaConnectExtensions.GetTransportEndpointLabel(transportOptions, forServer: true));

        if (transportOptions.UseNamedPipes)
        {
            LogStartup(_logger, "transport allow all authenticated users", transportOptions.AllowAllAuthenticatedUsers.ToString());
        }

        foreach (var warning in transportWarnings)
        {
            LogTransportConfigurationWarning(_logger, warning);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
