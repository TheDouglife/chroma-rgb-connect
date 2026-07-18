// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Service.Data.Services;
using ChromaControl.Service.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChromaControl.Service.Tests;

public class MigrationServiceTests
{
    [Fact]
    public async Task StartAsyncWhenMigrationSucceedsDoesNotStopApplication()
    {
        var hostApplicationLifetime = new FakeHostApplicationLifetime();
        var migrationRunner = new FakeDatabaseMigrationRunner([null]);
        var startupState = new ServiceStartupState();
        var migrationService = CreateService(hostApplicationLifetime, migrationRunner, startupState, maxAttempts: 3);

        await migrationService.StartAsync(CancellationToken.None);

        Assert.Equal(1, migrationRunner.AttemptCount);
        Assert.False(hostApplicationLifetime.StopCalled);
        Assert.Equal(StartupStatus.Ready, startupState.Status);
    }

    [Fact]
    public async Task StartAsyncWhenMigrationAlwaysFailsStopsApplicationAfterThreshold()
    {
        var hostApplicationLifetime = new FakeHostApplicationLifetime();
        var migrationRunner = new FakeDatabaseMigrationRunner([new InvalidOperationException("fail"), new InvalidOperationException("fail"), new InvalidOperationException("fail")]);
        var startupState = new ServiceStartupState();
        var migrationService = CreateService(hostApplicationLifetime, migrationRunner, startupState, maxAttempts: 3);

        await migrationService.StartAsync(CancellationToken.None);

        Assert.Equal(3, migrationRunner.AttemptCount);
        Assert.True(hostApplicationLifetime.StopCalled);
        Assert.Equal(StartupStatus.Failed, startupState.Status);
        Assert.True(startupState.StopRequested);
    }

    [Fact]
    public async Task StartAsyncWhenFailureIsTransientRetriesAndContinues()
    {
        var hostApplicationLifetime = new FakeHostApplicationLifetime();
        var migrationRunner = new FakeDatabaseMigrationRunner([new InvalidOperationException("transient"), null]);
        var startupState = new ServiceStartupState();
        var migrationService = CreateService(hostApplicationLifetime, migrationRunner, startupState, maxAttempts: 3);

        await migrationService.StartAsync(CancellationToken.None);

        Assert.Equal(2, migrationRunner.AttemptCount);
        Assert.False(hostApplicationLifetime.StopCalled);
        Assert.Equal(StartupStatus.Ready, startupState.Status);
        Assert.False(startupState.StopRequested);
    }

    private static MigrationService CreateService(
        FakeHostApplicationLifetime hostApplicationLifetime,
        FakeDatabaseMigrationRunner migrationRunner,
        ServiceStartupState startupState,
        int maxAttempts)
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var options = Options.Create(new MigrationServiceOptions
        {
            MaxAttempts = maxAttempts,
            RetryDelay = TimeSpan.Zero
        });

        return new MigrationService(
            serviceProvider,
            migrationRunner,
            hostApplicationLifetime,
                startupState,
            options,
            NullLogger<MigrationService>.Instance);
    }

    private sealed class FakeDatabaseMigrationRunner : IDatabaseMigrationRunner
    {
        private readonly Queue<Exception?> _outcomes;

        public FakeDatabaseMigrationRunner(IEnumerable<Exception?> outcomes)
        {
            _outcomes = new(outcomes);
        }

        public int AttemptCount { get; private set; }

        public Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            AttemptCount++;

            var next = _outcomes.Count > 0 ? _outcomes.Dequeue() : null;

            if (next != null)
            {
                throw next;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeHostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping => CancellationToken.None;

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public bool StopCalled { get; private set; }

        public void StopApplication()
        {
            StopCalled = true;
        }
    }
}