// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChromaConnect.Service.Core.Services;

/// <summary>
/// Exposes startup readiness and degraded state via health checks.
/// </summary>
public sealed class StartupHealthCheck : IHealthCheck
{
    private readonly ServiceStartupState _startupState;
    private readonly ChromaConnectTransportOptions _transportOptions;
    private readonly IReadOnlyList<string> _transportWarnings;

    /// <summary>
    /// Creates a <see cref="StartupHealthCheck"/> instance.
    /// </summary>
    /// <param name="startupState">The startup state.</param>
    /// <param name="configuration">The host configuration.</param>
    public StartupHealthCheck(ServiceStartupState startupState, IConfiguration configuration)
    {
        _startupState = startupState;
        _transportOptions = ChromaConnectExtensions.ResolveTransportOptions(configuration);
        _transportWarnings = ChromaConnectExtensions.GetTransportConfigurationWarnings(configuration, _transportOptions, forServer: true);
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var snapshot = _startupState.GetSnapshot();

        var data = new Dictionary<string, object>
        {
            ["status"] = snapshot.Status.ToString(),
            ["updatedAtUtc"] = snapshot.UpdatedAtUtc.ToString("O"),
            ["stopRequested"] = snapshot.StopRequested,
            ["transportMode"] = ChromaConnectExtensions.GetTransportModeLabel(_transportOptions),
            ["transportEndpoint"] = ChromaConnectExtensions.GetTransportEndpointLabel(_transportOptions, forServer: true)
        };

        if (_transportOptions.UseNamedPipes)
        {
            data["transportAllowAllAuthenticatedUsers"] = _transportOptions.AllowAllAuthenticatedUsers;
        }

        if (_transportWarnings.Count > 0)
        {
            data["transportWarningCount"] = _transportWarnings.Count;
            data["transportWarnings"] = _transportWarnings;
        }

        return snapshot.Status switch
        {
            StartupStatus.Ready => Task.FromResult(HealthCheckResult.Healthy(snapshot.Message, data)),
            StartupStatus.Degraded => Task.FromResult(HealthCheckResult.Degraded(snapshot.Message, data: data)),
            StartupStatus.Failed => Task.FromResult(HealthCheckResult.Unhealthy(snapshot.Message, data: data)),
            StartupStatus.Starting => Task.FromResult(HealthCheckResult.Degraded(snapshot.Message, data: data)),
            _ => throw new ArgumentOutOfRangeException(nameof(context), $"Unknown startup status value '{snapshot.Status}'.")
        };
    }
}
