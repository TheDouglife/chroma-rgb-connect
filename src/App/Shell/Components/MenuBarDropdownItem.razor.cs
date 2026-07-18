// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Shell.Components;

/// <summary>
/// The menu bar dropdown item component.
/// </summary>
public partial class MenuBarDropdownItem
{
    /// <summary>
    /// The label of the item.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Label { get; set; }

    /// <summary>
    /// The shortcut of the item.
    /// </summary>
    [Parameter]
    public string? Shortcut { get; set; }

    /// <summary>
    /// Any additional values.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];
}
