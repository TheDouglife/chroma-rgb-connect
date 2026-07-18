// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.OpenRGB.Internal.Enums;
using System.Buffers;
using System.Text;

namespace ChromaConnect.SDK.OpenRGB.Internal.Packets;

internal struct SetClientName : IOpenRGBPacket
{
    public readonly PacketId Id => PacketId.SetClientName;

    public uint DeviceIndex { get; private set; }

    public readonly uint Length => (uint)Encoding.ASCII.GetByteCount(Name) + 1;

    public string Name { get; private set; }

    public SetClientName(string name)
    {
        Name = name;
    }

    public bool TryParse(ref SequenceReader<byte> input, uint deviceIndex)
    {
        DeviceIndex = deviceIndex;

        return true;
    }

    public readonly void WriteToBuffer(IBufferWriter<byte> output)
    {
        output.Write(Encoding.ASCII.GetBytes($"{Name}\0"));
    }
}
