// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;

namespace ChromaControl.Service.Data.Services;

/// <summary>
/// Runs startup database initialization for the configuration database.
/// </summary>
public interface IDatabaseMigrationRunner
{
    /// <summary>
    /// Runs the migration workflow.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RunAsync(IServiceProvider services, CancellationToken cancellationToken);
}

/// <summary>
/// Default implementation of <see cref="IDatabaseMigrationRunner"/>.
/// </summary>
public sealed class DatabaseMigrationRunner : IDatabaseMigrationRunner
{
    /// <inheritdoc/>
    public async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = delete;", cancellationToken);
    }
}
