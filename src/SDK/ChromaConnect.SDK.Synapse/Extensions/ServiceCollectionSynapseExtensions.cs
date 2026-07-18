// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.Synapse.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ChromaConnect.SDK.Synapse.Extensions;

/// <summary>
/// Extension methods for adding the Synapse SDK to an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionSynapseExtensions
{
    /// <summary>
    /// Registers the Synapse SDK in an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddSynapseSDK(this IServiceCollection services)
    {
        services.AddSingleton<ISynapseService, SynapseService>();

        services.AddHostedService<SynapseHostService>();

        return services;
    }
}
