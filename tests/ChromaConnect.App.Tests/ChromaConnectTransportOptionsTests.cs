// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using Microsoft.Extensions.Configuration;

namespace ChromaConnect.App.Tests;

public class ChromaConnectTransportOptionsTests
{
    [Fact]
    public void ResolveTransportOptionsReturnsNamedPipeDefaults()
    {
        var configuration = new ConfigurationManager();

        var options = ChromaConnectExtensions.ResolveTransportOptions(configuration);

        Assert.True(options.UseNamedPipes);
        Assert.Equal("ChromaConnect", options.PipeName);
        Assert.Equal(50071, options.Port);
        Assert.True(options.AllowAllAuthenticatedUsers);
        Assert.Equal("named-pipe", ChromaConnectExtensions.GetTransportModeLabel(options));
        Assert.Equal("pipe:ChromaConnect", ChromaConnectExtensions.GetTransportEndpointLabel(options, forServer: true));
        Assert.Equal(new Uri("http://localhost"), ChromaConnectExtensions.ResolveGrpcClientAddress(options));
    }

    [Fact]
    public void ResolveTransportOptionsPreservesCustomPipeName()
    {
        var configuration = new ConfigurationManager();
        configuration["Service:Transport:PipeName"] = " ChromaConnect.CustomPipe ";

        var options = ChromaConnectExtensions.ResolveTransportOptions(configuration);

        Assert.True(options.UseNamedPipes);
        Assert.Equal("ChromaConnect.CustomPipe", options.PipeName);
    }

    [Fact]
    public void ResolveGrpcClientAddressReturnsTcpAddressWhenNamedPipesDisabled()
    {
        var configuration = new ConfigurationManager();
        configuration["Service:Transport:UseNamedPipes"] = "false";
        configuration["Service:Transport:Port"] = "50123";

        var options = ChromaConnectExtensions.ResolveTransportOptions(configuration);

        Assert.False(options.UseNamedPipes);
        Assert.Equal("tcp", ChromaConnectExtensions.GetTransportModeLabel(options));
        Assert.Equal("tcp:127.0.0.1:50123", ChromaConnectExtensions.GetTransportEndpointLabel(options, forServer: true));
        Assert.Equal(new Uri("http://127.0.0.1:50123"), ChromaConnectExtensions.ResolveGrpcClientAddress(options));
    }

    [Fact]
    public void ResolveGrpcClientAddressThrowsForNonPositiveTcpPort()
    {
        var options = new ChromaConnectTransportOptions(false, "ignored", 0, false);

        var exception = Assert.Throws<InvalidOperationException>(() => ChromaConnectExtensions.ResolveGrpcClientAddress(options));

        Assert.Equal("tcp:127.0.0.1:auto", ChromaConnectExtensions.GetTransportEndpointLabel(options, forServer: true));
        Assert.Equal("tcp:invalid-port", ChromaConnectExtensions.GetTransportEndpointLabel(options, forServer: false));
        Assert.Contains("Service:Transport:Port", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetTransportConfigurationWarningsDetectsIgnoredTcpPortWhenNamedPipeEnabled()
    {
        var configuration = new ConfigurationManager();
        configuration["Service:Transport:UseNamedPipes"] = "true";
        configuration["Service:Transport:Port"] = "50123";
        var options = ChromaConnectExtensions.ResolveTransportOptions(configuration);

        var warnings = ChromaConnectExtensions.GetTransportConfigurationWarnings(configuration, options, forServer: true);

        Assert.Contains("Service:Transport:Port is ignored when Service:Transport:UseNamedPipes=true.", warnings);
    }

    [Fact]
    public void GetTransportConfigurationWarningsDetectsIgnoredPipeSettingsAndDynamicPort()
    {
        var configuration = new ConfigurationManager();
        configuration["Service:Transport:UseNamedPipes"] = "false";
        configuration["Service:Transport:PipeName"] = "CustomPipe";
        configuration["Service:Transport:AllowAllAuthenticatedUsers"] = "true";
        configuration["Service:Transport:Port"] = "0";
        var options = ChromaConnectExtensions.ResolveTransportOptions(configuration);

        var warnings = ChromaConnectExtensions.GetTransportConfigurationWarnings(configuration, options, forServer: true);

        Assert.Contains("Service:Transport:PipeName is ignored when Service:Transport:UseNamedPipes=false.", warnings);
        Assert.Contains("Service:Transport:AllowAllAuthenticatedUsers is ignored when Service:Transport:UseNamedPipes=false.", warnings);
        Assert.Contains("Service:Transport:Port<=0 enables dynamic server TCP port selection, which cannot be auto-discovered by TCP clients.", warnings);
    }
}