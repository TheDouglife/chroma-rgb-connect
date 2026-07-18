// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.OpenRGB.Internal.Enums;
using ChromaConnect.SDK.OpenRGB.Internal.Extensions;
using ChromaConnect.SDK.OpenRGB.Structs;
using System.Buffers;

namespace ChromaConnect.SDK.OpenRGB.Internal.Packets;

internal struct RequestControllerData : IOpenRGBPacket
{
    public readonly PacketId Id => PacketId.RequestControllerData;

    public uint DeviceIndex { get; private set; }

    public readonly uint Length => 4;

    public uint ProtocolVersion { get; private set; }

    public OpenRGBDevice Device { get; private set; }

    public RequestControllerData(uint deviceIndex, uint protocolVersion)
    {
        DeviceIndex = deviceIndex;
        ProtocolVersion = protocolVersion;
    }

    public bool TryParse(ref SequenceReader<byte> input, uint deviceIndex)
    {
        DeviceIndex = deviceIndex;

        input.Advance(4);

        Device = OpenRGBDevice.Parse(ref input, deviceIndex);

        return true;
    }

    public readonly void WriteToBuffer(IBufferWriter<byte> output)
    {
        output.Write(ProtocolVersion);
    }
}
