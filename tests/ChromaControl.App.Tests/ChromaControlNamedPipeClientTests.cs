// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Extensions;
using System.IO.Pipes;

namespace ChromaControl.App.Tests;

public class ChromaControlNamedPipeClientTests
{
    [Fact]
    public async Task ConnectNamedPipeAsyncReturnsConnectedStreamWhenServerIsAvailable()
    {
        var pipeName = $"ChromaControl.Tests.{Guid.NewGuid():N}";
        using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        using var serverCancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var waitForConnectionTask = server.WaitForConnectionAsync(serverCancellationSource.Token);
        using var clientCancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await using var clientStream = await ChromaControlExtensions.ConnectNamedPipeAsync(pipeName, clientCancellationSource.Token);
        await waitForConnectionTask;

        Assert.True(clientStream.CanRead);
        Assert.True(clientStream.CanWrite);
    }

    [Fact]
    public async Task ConnectNamedPipeAsyncPreservesCancellation()
    {
        var pipeName = $"ChromaControl.Tests.{Guid.NewGuid():N}";
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ChromaControlExtensions.ConnectNamedPipeAsync(pipeName, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ConnectNamedPipeAsyncWrapsPipeConnectionFailuresWithContext()
    {
        var pipeName = $"ChromaControl.Tests.{Guid.NewGuid():N}";

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => ChromaControlExtensions.ConnectNamedPipeAsync(
            pipeName,
            static name => new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous),
            static (_, _) => Task.FromException(new System.IO.IOException("pipe unavailable")),
            CancellationToken.None));

        Assert.Contains(pipeName, exception.Message, StringComparison.Ordinal);
        Assert.IsType<System.IO.IOException>(exception.InnerException);
    }
}