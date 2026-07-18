// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ChromaConnect.SDK.OpenRGB.Internal;

internal sealed class OpenRGBConstants
{
    public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChromaConnect");
    public static readonly string ConfigPath = Path.Combine(DataPath, "config");
    public static readonly int PortNumber = 22742;
    public static readonly uint ProtocolVersion = 4;
}
