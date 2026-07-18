// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Service.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace ChromaConnect.Service.Data;

/// <summary>
/// Data extension methods.
/// </summary>
public static class DataExtensions
{
    /// <summary>
    /// Adds data configuration to a <see cref="IHostApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to add configuration to.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> to continue adding configuration to.</returns>
    public static IHostApplicationBuilder ConfigureData(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }

            options.UseSqlite(builder.Configuration.GetConnectionString("Database"), config =>
            {
                config.MigrationsHistoryTable("Migrations");
            });
        }, contextLifetime: ServiceLifetime.Transient);

        builder.Services.Configure<MigrationServiceOptions>(builder.Configuration.GetSection(MigrationServiceOptions.SectionName));
        builder.Services.AddSingleton<IDatabaseMigrationRunner, DatabaseMigrationRunner>();
        builder.Services.AddHostedService<MigrationService>();

        return builder;
    }
}
