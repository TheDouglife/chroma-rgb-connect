// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;

namespace ChromaConnect.App.Tests;

public class ChromaConnectSingleInstanceTests
{
    [Fact]
    public void AcquireSingleInstanceMutexReturnsMutexForUniqueName()
    {
        var applicationName = $"ChromaConnect.Tests.{Guid.NewGuid():N}";

        using var mutex = ChromaConnectExtensions.AcquireSingleInstanceMutex(applicationName);

        Assert.NotNull(mutex);
    }

    [Fact]
    public void AcquireSingleInstanceMutexThrowsWhenNameIsAlreadyHeld()
    {
        var applicationName = $"ChromaConnect.Tests.{Guid.NewGuid():N}";
        using var mutex = ChromaConnectExtensions.AcquireSingleInstanceMutex(applicationName);

        var exception = Assert.Throws<InvalidOperationException>(() => ChromaConnectExtensions.AcquireSingleInstanceMutex(applicationName));

        Assert.Contains(applicationName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AcquireSingleInstanceMutexFallsBackForBlankName()
    {
        using var mutex = ChromaConnectExtensions.AcquireSingleInstanceMutex("   ");

        Assert.NotNull(mutex);
    }
}