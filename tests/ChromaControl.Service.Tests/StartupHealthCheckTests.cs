// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Service.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChromaControl.Service.Tests;

public class StartupHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsyncIncludesTransportWarningsWhenSettingsAreContradictory()
    {
        var startupState = new ServiceStartupState();
        startupState.MarkReady("ready");

        var configuration = new ConfigurationManager();
        configuration["Service:Transport:UseNamedPipes"] = "true";
        configuration["Service:Transport:Port"] = "50123";

        var healthCheck = new StartupHealthCheck(startupState, configuration);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.True(result.Data.ContainsKey("transportWarningCount"));
        Assert.Equal(1, Assert.IsType<int>(result.Data["transportWarningCount"]));

        var warnings = Assert.IsAssignableFrom<IReadOnlyList<string>>(result.Data["transportWarnings"]);
        Assert.Contains("Service:Transport:Port is ignored when Service:Transport:UseNamedPipes=true.", warnings);
    }

    [Fact]
    public async Task CheckHealthAsyncOmitsTransportWarningsWhenSettingsAreConsistent()
    {
        var startupState = new ServiceStartupState();
        startupState.MarkReady("ready");

        var configuration = new ConfigurationManager();
        configuration["Service:Transport:UseNamedPipes"] = "true";
        configuration["Service:Transport:PipeName"] = "ChromaControl";

        var healthCheck = new StartupHealthCheck(startupState, configuration);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.False(result.Data.ContainsKey("transportWarningCount"));
        Assert.False(result.Data.ContainsKey("transportWarnings"));
    }
}