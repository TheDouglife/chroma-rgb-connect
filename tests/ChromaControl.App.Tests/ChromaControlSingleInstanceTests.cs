// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Extensions;

namespace ChromaControl.App.Tests;

public class ChromaControlSingleInstanceTests
{
    [Fact]
    public void AcquireSingleInstanceMutexReturnsMutexForUniqueName()
    {
        var applicationName = $"ChromaControl.Tests.{Guid.NewGuid():N}";

        using var mutex = ChromaControlExtensions.AcquireSingleInstanceMutex(applicationName);

        Assert.NotNull(mutex);
    }

    [Fact]
    public void AcquireSingleInstanceMutexThrowsWhenNameIsAlreadyHeld()
    {
        var applicationName = $"ChromaControl.Tests.{Guid.NewGuid():N}";
        using var mutex = ChromaControlExtensions.AcquireSingleInstanceMutex(applicationName);

        var exception = Assert.Throws<InvalidOperationException>(() => ChromaControlExtensions.AcquireSingleInstanceMutex(applicationName));

        Assert.Contains(applicationName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AcquireSingleInstanceMutexFallsBackForBlankName()
    {
        using var mutex = ChromaControlExtensions.AcquireSingleInstanceMutex("   ");

        Assert.NotNull(mutex);
    }
}