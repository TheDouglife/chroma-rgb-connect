// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.App.Lighting.Commands;
using ChromaConnect.App.Shell.Services;
using ChromaConnect.App.Tutorials.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Tutorials.Pages;

/// <summary>
/// The vendors page.
/// </summary>
public partial class Vendors
{
    /// <summary>
    /// The <see cref="NavigationManager"/>.
    /// </summary>
    [Inject]
    public required NavigationManager Navigation { get; set; }

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

    private void NavigateBackward()
    {
        Navigation.NavigateTo("/tutorials/welcome");
    }

    private async Task NavigateForward()
    {
        Navigation.NavigateTo("/");

        var result = await Mediator.Send(new RestartService.Command());

        if (result.IsFailure(out var error))
        {
            DialogService.ShowError(error);
        }

        await Mediator.Send(new FinishTutorials.Command());
    }
}
