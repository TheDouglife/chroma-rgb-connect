// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ChromaConnect.Service.Data.Services;

/// <summary>
/// Configuration options for <see cref="MigrationService"/> startup behavior.
/// </summary>
public sealed class MigrationServiceOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Database:Migration";

    /// <summary>
    /// Maximum number of migration attempts before aborting startup.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Delay between migration retries.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Suppresses the call to stop the host when migration fails.
    /// </summary>
    public bool SuppressHostStop { get; set; }
}
