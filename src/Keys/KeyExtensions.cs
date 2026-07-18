// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;

namespace ChromaConnect.Keys;

/// <summary>
/// Extension methods for ChromaConnect configuration keys.
/// </summary>
public static class KeyExtensions
{
    private const string SynapseKey = "0e881c6f-1194-4c62-813d-a609beafa8b5";

    /// <summary>
    /// Adds ChromaConnect keys to an <see cref="IConfigurationManager"/>.
    /// </summary>
    /// <param name="configurationManager">The configuration manager to update.</param>
    /// <returns>The configuration manager.</returns>
    public static IConfigurationManager AddChromaConnectKeys(this IConfigurationManager configurationManager)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        configurationManager["CHROMACONNECT_KEY_SYNAPSE"] = SynapseKey;

        return configurationManager;
    }
}
