// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Tutorials.Pages;

/// <summary>
/// The welcome page.
/// </summary>
public partial class Welcome
{
    /// <summary>
    /// The <see cref="NavigationManager"/>.
    /// </summary>
    [Inject]
    public required NavigationManager Navigation { get; set; }

    private void NavigateForward()
    {
        Navigation.NavigateTo("/tutorials/vendors");
    }
}
