// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace ChromaControl.App.Shell.Components;

/// <summary>
/// The error dialog.
/// </summary>
public partial class ErrorDialog
{
    /// <summary>
    /// The error message.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Message { get; set; }
}
