// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BlazorDesktop.Hosting;
using ChromaConnect.App.Shell.Services;
using Microsoft.AspNetCore.Components.Web;

namespace ChromaConnect.App.Shell;

/// <summary>
/// Shell extension methods.
/// </summary>
public static class ShellExtensions
{
    /// <summary>
    /// Adds shell configuration to a <see cref="BlazorDesktopHostBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="BlazorDesktopHostBuilder"/> to add configuration to.</param>
    /// <returns>The <see cref="BlazorDesktopHostBuilder"/> to continue adding configuration to.</returns>
    public static BlazorDesktopHostBuilder ConfigureShell(this BlazorDesktopHostBuilder builder)
    {
        builder.ConfigureServices()
            .ConfigureWindow()
            .ConfigureRootComponents();

        return builder;
    }

    private static BlazorDesktopHostBuilder ConfigureServices(this BlazorDesktopHostBuilder builder)
    {
        builder.Services.AddSingleton<DialogService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<ThemeService>();

        return builder;
    }

    private static BlazorDesktopHostBuilder ConfigureWindow(this BlazorDesktopHostBuilder builder)
    {
        builder.Window.UseTitle("Chroma RGB Connect");
        builder.Window.UseResizable(true);

        builder.Window.UseFrame(false);

        builder.Window.UseHeight(720);
        builder.Window.UseWidth(1280);

        builder.Window.UseMinHeight(660);
        builder.Window.UseMinWidth(960);

        return builder;
    }

    private static BlazorDesktopHostBuilder ConfigureRootComponents(this BlazorDesktopHostBuilder builder)
    {
        builder.RootComponents.Add<Components.Routes>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        return builder;
    }
}
