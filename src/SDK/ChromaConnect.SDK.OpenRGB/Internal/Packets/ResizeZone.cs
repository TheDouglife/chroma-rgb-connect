// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.OpenRGB.Internal.Enums;
using ChromaConnect.SDK.OpenRGB.Internal.Extensions;
using System.Buffers;

namespace ChromaConnect.SDK.OpenRGB.Internal.Packets;

internal struct ResizeZone : IOpenRGBPacket
{
    public readonly PacketId Id => PacketId.ResizeZone;

    public uint DeviceIndex { get; private set; }

    public readonly uint Length => 8;

    public uint ZoneIndex { get; private set; }

    public uint Size { get; private set; }

    public ResizeZone(uint deviceIndex, uint zoneIndex, uint size)
    {
        DeviceIndex = deviceIndex;
        ZoneIndex = zoneIndex;
        Size = size;
    }

    public bool TryParse(ref SequenceReader<byte> input, uint deviceIndex)
    {
        DeviceIndex = deviceIndex;

        return true;
    }

    public readonly void WriteToBuffer(IBufferWriter<byte> output)
    {
        output.Write(ZoneIndex);
        output.Write(Size);
    }
}
