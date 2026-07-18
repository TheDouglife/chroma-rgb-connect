// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Connections;
using System.Net;

namespace ChromaConnect.SDK.OpenRGB.Internal.Sockets;

internal sealed class SocketConnectionFactory : IConnectionFactory
{
    public ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
    {
        return new SocketConnection(endpoint).StartAsync();
    }
}
