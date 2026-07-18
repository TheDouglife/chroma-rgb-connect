// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.App.Settings.Components;
using ChromaConnect.App.Shell.Services;
using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Lighting.Components;

/// <summary>
/// The blank slate component.
/// </summary>
public partial class BlankSlate
{
    /// <summary>
    /// The dialog service.
    /// </summary>
    [Inject]
    public required DialogService DialogService { get; set; }

    private void ShowSettings()
    {
        DialogService.Open<SettingsDialog>();
    }
}
