// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Options;
using ChromaControl.Service.Core.Services;

namespace ChromaControl.Service.Data.Services;

/// <summary>
/// Service that performs database migrations.
/// </summary>
public partial class MigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabaseMigrationRunner _migrationRunner;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly MigrationServiceOptions _options;
    private readonly ServiceStartupState _startupState;
    private readonly ILogger<MigrationService> _logger;

    [LoggerMessage(0, LogLevel.Information, "Starting configuration database migration...", EventName = "DatabaseMigrationStarted")]
    private static partial void LogStartingMigration(ILogger logger);

    [LoggerMessage(1, LogLevel.Error, "Configuration database migration attempt {Attempt} of {MaxAttempts} failed, retrying in {RetryDelaySeconds} seconds...", EventName = "DatabaseMigrationFailed")]
    private static partial void LogMigrationFailed(ILogger logger, Exception ex, int attempt, int maxAttempts, double retryDelaySeconds);

    [LoggerMessage(2, LogLevel.Error, "Configuration database migration aborted after {MaxAttempts} failed attempts.", EventName = "DatabaseMigrationAborted")]
    private static partial void LogMigrationAborted(ILogger logger, Exception ex, int maxAttempts);

    [LoggerMessage(3, LogLevel.Information, "Configuration database migration completed successfully.", EventName = "DatabaseMigrationFinished")]
    private static partial void LogMigrationComplete(ILogger logger);

    /// <summary>
    /// Creates a <see cref="MigrationService"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="migrationRunner">The <see cref="IDatabaseMigrationRunner"/>.</param>
    /// <param name="hostApplicationLifetime">The <see cref="IHostApplicationLifetime"/>.</param>
    /// <param name="startupState">The <see cref="ServiceStartupState"/>.</param>
    /// <param name="options">The <see cref="MigrationServiceOptions"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public MigrationService(
        IServiceProvider serviceProvider,
        IDatabaseMigrationRunner migrationRunner,
        IHostApplicationLifetime hostApplicationLifetime,
        ServiceStartupState startupState,
        IOptions<MigrationServiceOptions> options,
        ILogger<MigrationService> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(migrationRunner);
        ArgumentNullException.ThrowIfNull(hostApplicationLifetime);
        ArgumentNullException.ThrowIfNull(startupState);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        if (options.Value.MaxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxAttempts must be greater than zero.");
        }

        if (options.Value.RetryDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "RetryDelay cannot be negative.");
        }

        _serviceProvider = serviceProvider;
        _migrationRunner = migrationRunner;
        _hostApplicationLifetime = hostApplicationLifetime;
        _startupState = startupState;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogStartingMigration(_logger);

        var attempt = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            attempt++;

            try
            {
                await _migrationRunner.RunAsync(_serviceProvider, cancellationToken);
                _startupState.MarkReady("Configuration database migration completed successfully.");
                LogMigrationComplete(_logger);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                if (attempt < _options.MaxAttempts)
                {
                    _startupState.MarkDegraded($"Configuration database migration attempt {attempt} failed. Retrying.");
                    LogMigrationFailed(_logger, ex, attempt, _options.MaxAttempts, _options.RetryDelay.TotalSeconds);

                    try
                    {
                        await Task.Delay(_options.RetryDelay, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    _startupState.MarkFailed("Configuration database migration failed and retry threshold was exceeded.");
                    _startupState.MarkStopRequested();
                    LogMigrationAborted(_logger, ex, _options.MaxAttempts);

                    if (!_options.SuppressHostStop)
                    {
                        _hostApplicationLifetime.StopApplication();
                    }

                    return;
                }
            }
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
