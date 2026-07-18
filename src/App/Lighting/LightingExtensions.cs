// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BlazorDesktop.Hosting;
using ChromaConnect.App.Lighting.Services;
using ChromaConnect.Common.Extensions;
using ChromaConnect.Common.Protos.Lighting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChromaConnect.App.Lighting;

/// <summary>
/// Lighting extension methods.
/// </summary>
public static class LightingExtensions
{
    /// <summary>
    /// Adds lighting configuration to a <see cref="BlazorDesktopHostBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="BlazorDesktopHostBuilder"/> to add configuration to.</param>
    /// <returns>The <see cref="BlazorDesktopHostBuilder"/> to continue adding configuration to.</returns>
    public static BlazorDesktopHostBuilder ConfigureLighting(this BlazorDesktopHostBuilder builder)
    {
        builder.Services.AddChromaConnectGrpcClient<LightingGrpc.LightingGrpcClient>(builder.Configuration);
        builder.Services.TryAddSingleton<EventService>();
        builder.Services.AddHostedService<EventMonitor>();

        return builder;
    }
}
