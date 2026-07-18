// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.OpenRGB.Internal.Enums;
using System.Buffers;

namespace ChromaConnect.SDK.OpenRGB.Internal.Packets;

internal struct DeviceListUpdated : IOpenRGBPacket
{
    public readonly PacketId Id => PacketId.DeviceListUpdated;

    public uint DeviceIndex { get; private set; }

    public readonly uint Length => 0;

    public DeviceListUpdated()
    {
        DeviceIndex = 0;
    }

    public bool TryParse(ref SequenceReader<byte> input, uint deviceIndex)
    {
        DeviceIndex = deviceIndex;

        return true;
    }

    public readonly void WriteToBuffer(IBufferWriter<byte> output) { }
}
