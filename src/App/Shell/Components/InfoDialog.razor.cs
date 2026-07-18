// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace ChromaConnect.App.Shell.Components;

/// <summary>
/// The info dialog.
/// </summary>
public partial class InfoDialog
{
    /// <summary>
    /// The info message.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Message { get; set; }
}
