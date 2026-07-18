// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ChromaConnect.SDK.OpenRGB.Internal.Enums;

internal enum PacketId : uint
{
    RequestControllerCount = 0,
    RequestControllerData = 1,
    RequestProtocolVersion = 40,
    SetClientName = 50,
    DeviceListUpdated = 100,
    ResizeZone = 1000,
    UpdateLeds = 1050,
    UpdateZoneLeds = 1051,
    UpdateSingleLed = 1052
}
