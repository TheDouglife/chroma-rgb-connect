// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.OpenRGB.Internal.Enums;
using ChromaConnect.SDK.OpenRGB.Internal.Extensions;
using System.Buffers;

namespace ChromaConnect.SDK.OpenRGB.Internal.Packets;

internal struct RequestProtocolVersion : IOpenRGBPacket
{
    public readonly PacketId Id => PacketId.RequestProtocolVersion;

    public uint DeviceIndex { get; private set; }

    public readonly uint Length => 4;

    public uint ClientVersion { get; private set; }

    public uint ServerVersion { get; private set; }

    public RequestProtocolVersion(uint clientVersion)
    {
        DeviceIndex = 0;
        ClientVersion = clientVersion;
    }

    public bool TryParse(ref SequenceReader<byte> input, uint deviceIndex)
    {
        DeviceIndex = deviceIndex;

        ServerVersion = input.ReadUInt32();

        return true;
    }

    public readonly void WriteToBuffer(IBufferWriter<byte> output)
    {
        output.Write(ClientVersion);
    }
}
