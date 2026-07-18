// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using System.Diagnostics;
using System.IO;

namespace ChromaConnect.App.Core.Services;

/// <summary>
/// A <see cref="BackgroundService"/> that monitors the lighting service.
/// </summary>
public partial class ServiceMonitor : IHostedService
{
    private const int MaxRestartsPerWindow = 5;
    private static readonly TimeSpan s_restartWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan s_restartDelay = TimeSpan.FromSeconds(3);

    private readonly object _syncRoot = new();
    private Process? _serviceProcess;
    private readonly ILogger _logger;
    private readonly string _servicePath;
    private CancellationTokenSource? _shutdownTokenSource;
    private DateTimeOffset _restartWindowStartedAt = DateTimeOffset.MinValue;
    private int _restartAttempts;

    [LoggerMessage(0, LogLevel.Warning, "Unable to find the lighting service executable.", EventName = "ServiceExecutableNotFound")]
    private static partial void LogServiceExecutableNotFound(ILogger logger);

    [LoggerMessage(1, LogLevel.Information, "Lighting service exited. Attempting restart in {DelaySeconds} seconds.", EventName = "ServiceRestartScheduled")]
    private static partial void LogServiceRestartScheduled(ILogger logger, double delaySeconds);

    [LoggerMessage(2, LogLevel.Error, "Lighting service restart threshold exceeded. Maximum {MaxRestarts} restarts in {WindowSeconds} seconds.", EventName = "ServiceRestartThresholdExceeded")]
    private static partial void LogServiceRestartThresholdExceeded(ILogger logger, int maxRestarts, double windowSeconds);

    [LoggerMessage(3, LogLevel.Information, "Starting lighting service from path {ServicePath}.", EventName = "ServiceStarting")]
    private static partial void LogServiceStarting(ILogger logger, string servicePath);

    [LoggerMessage(4, LogLevel.Error, "Failed to start or monitor the lighting service.", EventName = "ServiceStartFailed")]
    private static partial void LogServiceStartFailed(ILogger logger, Exception ex);

    [LoggerMessage(5, LogLevel.Warning, "Found existing service process '{ProcessPath}' that does not match expected executable path '{ExpectedPath}'. Ignoring.", EventName = "ServiceProcessPathMismatch")]
    private static partial void LogServiceProcessPathMismatch(ILogger logger, string processPath, string expectedPath);

    /// <summary>
    /// Creates a <see cref="ServiceMonitor"/> instance.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    public ServiceMonitor(IConfiguration configuration, ILogger<ServiceMonitor> logger)
    {
        _servicePath = Path.Combine(configuration.GetChromaConnectPath("app"), "ChromaRGBConnect.Service.exe");
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _shutdownTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        StartProcess();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdownTokenSource?.Cancel();
        _shutdownTokenSource?.Dispose();
        _shutdownTokenSource = null;

        lock (_syncRoot)
        {
            if (_serviceProcess != null)
            {
                _serviceProcess.Exited -= ServiceProcessExited;
                _serviceProcess.Dispose();
                _serviceProcess = null;
            }
        }

        return Task.CompletedTask;
    }

    private void StartProcess()
    {
        lock (_syncRoot)
        {
            if (_serviceProcess != null)
            {
                _serviceProcess.Exited -= ServiceProcessExited;
                _serviceProcess.Dispose();
                _serviceProcess = null;
            }

            try
            {
                _serviceProcess = TryFindMatchingServiceProcess();

                if (_serviceProcess == null)
                {
                    if (File.Exists(_servicePath))
                    {
                        LogServiceStarting(_logger, _servicePath);
                        _serviceProcess = Process.Start(_servicePath);
                    }
                    else
                    {
                        LogServiceExecutableNotFound(_logger);
                    }
                }

                if (_serviceProcess != null)
                {
                    _serviceProcess.Exited -= ServiceProcessExited;
                    _serviceProcess.EnableRaisingEvents = true;
                    _serviceProcess.Exited += ServiceProcessExited;
                }
            }
            catch (Exception ex)
            {
                _serviceProcess = null;
                LogServiceStartFailed(_logger, ex);
            }
        }
    }

    private Process? TryFindMatchingServiceProcess()
    {
        var expectedPath = Path.GetFullPath(_servicePath);
        var processes = Process.GetProcessesByName("ChromaRGBConnect.Service");
        var candidates = new List<(Process Process, string? ProcessPath)>();

        foreach (var process in processes)
        {
            string? processPath = null;

            try
            {
                processPath = process.MainModule?.FileName;
            }
            catch
            {
                // Some system APIs can deny process metadata access; ignore and continue.
            }

            candidates.Add((process, processPath));
        }

        var selectedIndex = SelectMatchingCandidateIndex(expectedPath, candidates.Select((candidate, index) => new ProcessPathCandidate(index, candidate.ProcessPath)));

        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];

            if (selectedIndex == i)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(candidate.ProcessPath))
            {
                LogServiceProcessPathMismatch(_logger, candidate.ProcessPath, expectedPath);
            }

            candidate.Process.Dispose();
        }

        if (selectedIndex.HasValue)
        {
            return candidates[selectedIndex.Value].Process;
        }

        return null;
    }

    internal static int? SelectMatchingCandidateIndex(string expectedPath, IEnumerable<ProcessPathCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(expectedPath);

        var normalizedExpectedPath = Path.GetFullPath(expectedPath);

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate.ExecutablePath))
            {
                continue;
            }

            try
            {
                if (string.Equals(Path.GetFullPath(candidate.ExecutablePath), normalizedExpectedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate.Index;
                }
            }
            catch
            {
                // Ignore malformed paths and continue evaluating candidates.
            }
        }

        return null;
    }

    private void ServiceProcessExited(object? sender, EventArgs e)
    {
        _ = RestartProcessAsync();
    }

    private async Task RestartProcessAsync()
    {
        var shutdownTokenSource = _shutdownTokenSource;

        if (shutdownTokenSource?.IsCancellationRequested != false)
        {
            return;
        }

        if (!CanRestart())
        {
            LogServiceRestartThresholdExceeded(_logger, MaxRestartsPerWindow, s_restartWindow.TotalSeconds);
            return;
        }

        LogServiceRestartScheduled(_logger, s_restartDelay.TotalSeconds);

        try
        {
            await Task.Delay(s_restartDelay, shutdownTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (shutdownTokenSource.IsCancellationRequested)
        {
            return;
        }

        StartProcess();
    }

    private bool CanRestart()
    {
        lock (_syncRoot)
        {
            var now = DateTimeOffset.UtcNow;

            if (now - _restartWindowStartedAt >= s_restartWindow)
            {
                _restartWindowStartedAt = now;
                _restartAttempts = 0;
            }

            _restartAttempts++;

            return _restartAttempts <= MaxRestartsPerWindow;
        }
    }
}

internal readonly record struct ProcessPathCandidate(int Index, string? ExecutablePath);
