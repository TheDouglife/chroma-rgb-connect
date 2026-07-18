// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ChromaConnect.Service.Core.Services;

/// <summary>
/// The current startup state of the service.
/// </summary>
public sealed class ServiceStartupState
{
    private readonly object _syncRoot = new();

    /// <summary>
    /// Gets the current startup status.
    /// </summary>
    public StartupStatus Status { get; private set; } = StartupStatus.Starting;

    /// <summary>
    /// Gets the latest status message.
    /// </summary>
    public string Message { get; private set; } = "Service startup in progress.";

    /// <summary>
    /// Gets the timestamp when the current state was recorded.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a value indicating whether host stop was requested due to startup failure.
    /// </summary>
    public bool StopRequested { get; private set; }

    /// <summary>
    /// Gets an atomic snapshot of the current startup state.
    /// </summary>
    /// <returns>The startup state snapshot.</returns>
    public StartupStateSnapshot GetSnapshot()
    {
        lock (_syncRoot)
        {
            return new StartupStateSnapshot(Status, Message, UpdatedAtUtc, StopRequested);
        }
    }

    /// <summary>
    /// Marks startup as ready.
    /// </summary>
    /// <param name="message">The status message.</param>
    public void MarkReady(string message)
    {
        Update(StartupStatus.Ready, message);
    }

    /// <summary>
    /// Marks startup as degraded.
    /// </summary>
    /// <param name="message">The status message.</param>
    public void MarkDegraded(string message)
    {
        Update(StartupStatus.Degraded, message);
    }

    /// <summary>
    /// Marks startup as failed.
    /// </summary>
    /// <param name="message">The status message.</param>
    public void MarkFailed(string message)
    {
        Update(StartupStatus.Failed, message);
    }

    /// <summary>
    /// Marks that a host stop was requested.
    /// </summary>
    public void MarkStopRequested()
    {
        lock (_syncRoot)
        {
            StopRequested = true;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    private void Update(StartupStatus status, string message)
    {
        lock (_syncRoot)
        {
            Status = status;
            Message = message;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}

/// <summary>
/// Immutable snapshot of startup state values.
/// </summary>
/// <param name="Status">The startup status value.</param>
/// <param name="Message">The startup status message.</param>
/// <param name="UpdatedAtUtc">The timestamp of the last state update.</param>
/// <param name="StopRequested">Whether host stop was requested.</param>
public readonly record struct StartupStateSnapshot(
    StartupStatus Status,
    string Message,
    DateTimeOffset UpdatedAtUtc,
    bool StopRequested);

/// <summary>
/// Startup status values.
/// </summary>
public enum StartupStatus
{
    /// <summary>
    /// Startup work is still in progress.
    /// </summary>
    Starting,

    /// <summary>
    /// Startup completed and service is ready.
    /// </summary>
    Ready,

    /// <summary>
    /// Startup is still progressing but with recoverable failures.
    /// </summary>
    Degraded,

    /// <summary>
    /// Startup failed and the service is not ready.
    /// </summary>
    Failed
}
