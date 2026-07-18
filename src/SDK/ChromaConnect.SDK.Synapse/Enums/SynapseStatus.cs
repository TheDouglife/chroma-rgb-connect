// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ChromaConnect.SDK.Synapse.Enums;

/// <summary>
/// Synapse status.
/// </summary>
public enum SynapseStatus
{
    /// <summary>
    /// <see cref="Live"/> occurs when all expected conditions are met.
    /// </summary>
    Live = 1,

    /// <summary>
    /// <see cref="NotLive"/> occurs when one of the expected conditions is not fulfilled.
    /// </summary>
    NotLive = 2
}
