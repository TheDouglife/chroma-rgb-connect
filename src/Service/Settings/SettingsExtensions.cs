// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Service.Settings.Services;

namespace ChromaConnect.Service.Settings;

/// <summary>
/// Settings extension methods.
/// </summary>
public static class SettingsExtensions
{
    /// <summary>
    /// Adds settings middleware to a <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add middleware to.</param>
    /// <returns>The <see cref="WebApplication"/> to continue adding middleware to.</returns>
    public static WebApplication UseSettings(this WebApplication app)
    {
        app.MapGrpcService<SettingsService>();

        return app;
    }
}
