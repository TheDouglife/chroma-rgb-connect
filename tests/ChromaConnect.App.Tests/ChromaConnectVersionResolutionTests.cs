// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using System.Diagnostics;
using System.Reflection;

namespace ChromaConnect.App.Tests;

public class ChromaConnectVersionResolutionTests
{
    [Fact]
    public void ResolveProductVersionReturnsAssemblyFallbackWhenExecutableIsMissing()
    {
        var assembly = typeof(ChromaConnectVersionResolutionTests).Assembly;

        var version = ChromaConnectExtensions.ResolveProductVersion("C:/missing/ChromaConnect.exe", assembly);

        Assert.Equal(GetExpectedFallbackVersion(assembly), version);
    }

    [Fact]
    public void ResolveProductVersionReturnsAssemblyFallbackWhenMetadataIsUnavailable()
    {
        var assembly = typeof(ChromaConnectVersionResolutionTests).Assembly;
        var tempFile = Path.GetTempFileName();

        try
        {
            var version = ChromaConnectExtensions.ResolveProductVersion(tempFile, assembly);

            Assert.Equal(GetExpectedFallbackVersion(assembly), version);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolveProductVersionReturnsUnknownWhenNoFileOrAssemblyMetadataExists()
    {
        var version = ChromaConnectExtensions.ResolveProductVersion("C:/missing/ChromaConnect.exe", null);

        Assert.Equal("Unknown", version);
    }

    private static string GetExpectedFallbackVersion(Assembly assembly)
    {
        var productVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

        if (!string.IsNullOrWhiteSpace(productVersion))
        {
            return productVersion;
        }

        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion;
        }

        return assembly.GetName().Version?.ToString() ?? "Unknown";
    }
}