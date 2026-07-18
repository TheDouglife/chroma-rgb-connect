// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BlazorDesktop.Hosting;
using ChromaConnect.App.Core;
using ChromaConnect.App.Lighting;
using ChromaConnect.App.Settings;
using ChromaConnect.App.Shell;
using ChromaConnect.App.Tutorials;
using ChromaConnect.App.Updater;
using System.IO;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = BlazorDesktopHostBuilder.CreateDefault(args);

builder.ConfigureCore()
    .ConfigureUpdater()
    .ConfigureSettings()
    .ConfigureShell()
    .ConfigureLighting()
    .ConfigureTutorials();

await builder.Build().RunAsync();
