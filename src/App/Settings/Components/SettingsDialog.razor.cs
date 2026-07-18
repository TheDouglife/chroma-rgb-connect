// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.App.Lighting.Commands;
using ChromaConnect.App.Shell.Services;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Settings.Components;

/// <summary>
/// The settings dialog.
/// </summary>
public partial class SettingsDialog
{
    /// <summary>
    /// The <see cref="IMediator"/>.
    /// </summary>
    [Inject]
    public required IMediator Mediator { get; set; }

    /// <summary>
    /// The <see cref="DialogService"/>.
    /// </summary>
    [Inject]
    public required DialogService DialogService { get; set; }

    private async Task RestartService()
    {
        DialogService.ShowInfo("Lighting service has been restarted, it may take a moment for effects to resume.");

        var result = await Mediator.Send(new RestartService.Command());

        if (result.IsFailure(out var error))
        {
            DialogService.ShowError(error);
        }
    }
}
