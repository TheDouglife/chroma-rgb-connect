// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Shell.Components;

/// <summary>
/// The menu bar component.
/// </summary>
public partial class MenuBar
{
    /// <summary>
    /// The content to be rendered.
    /// </summary>
    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// If a menu is active.
    /// </summary>
    public bool MenuActive { get; private set; }

    /// <summary>
    /// Occurs when the active menu changes.
    /// </summary>
    public event EventHandler<MenuBarDropdown?>? ActiveMenuChanged;

    /// <summary>
    /// Changes the active menu.
    /// </summary>
    /// <param name="newItem">The new active item.</param>
    public void ChangeActiveMenu(MenuBarDropdown? newItem)
    {
        if (newItem == null)
        {
            MenuActive = false;
        }
        else
        {
            MenuActive = true;
        }

        ActiveMenuChanged?.Invoke(this, newItem);
    }
}
