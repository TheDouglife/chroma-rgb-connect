// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.App.Shell.Services;
using ChromaControl.App.Updater.Services;
using Microsoft.AspNetCore.Components;

namespace ChromaControl.App.Shell.Components;

/// <summary>
/// The about dialog.
/// </summary>
public partial class AboutDialog
{
    /// <summary>
    /// The <see cref="DialogService"/>.
    /// </summary>
    [Inject]
    public required DialogService DialogService { get; set; }

    /// <summary>
    /// The <see cref="UpdateService"/>.
    /// </summary>
    [Inject]
    public required UpdateService UpdateService { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        UpdateService.StateChanged += UpdateServiceStateChanged;
    }

    private void UpdateServiceStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OpenLicenseInfo()
    {
        DialogService.Open<LicenseDialog>();
    }

    private async Task CheckForUpdates()
    {
        await UpdateService.CheckForUpdates();

        await Task.Delay(500);
    }
}
