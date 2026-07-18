// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.SDK.OpenRGB.Internal.Extensions;
using System.Buffers;

namespace ChromaConnect.SDK.OpenRGB.Structs;

/// <summary>
/// Represents an OpenRGB LED.
/// </summary>
public struct OpenRGBLed
{
    /// <summary>
    /// The LEDs name.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The LEDs index.
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Converts this <see cref="OpenRGBLed"/> into a string representation.
    /// </summary>
    /// <returns>A string representation of this <see cref="OpenRGBLed"/>.</returns>
    public override readonly string ToString()
    {
        return Name;
    }

    internal static OpenRGBLed Parse(ref SequenceReader<byte> input, uint index)
    {
        var name = input.ReadString();

        input.Advance(4); // led_value

        return new()
        {
            Name = name,
            Index = (int)index
        };
    }
}
