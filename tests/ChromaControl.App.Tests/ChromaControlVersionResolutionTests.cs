// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Extensions;
using System.Diagnostics;
using System.Reflection;

namespace ChromaControl.App.Tests;

public class ChromaControlVersionResolutionTests
{
    [Fact]
    public void ResolveProductVersionReturnsAssemblyFallbackWhenExecutableIsMissing()
    {
        var assembly = typeof(ChromaControlVersionResolutionTests).Assembly;

        var version = ChromaControlExtensions.ResolveProductVersion("C:/missing/ChromaControl.exe", assembly);

        Assert.Equal(GetExpectedFallbackVersion(assembly), version);
    }

    [Fact]
    public void ResolveProductVersionReturnsAssemblyFallbackWhenMetadataIsUnavailable()
    {
        var assembly = typeof(ChromaControlVersionResolutionTests).Assembly;
        var tempFile = Path.GetTempFileName();

        try
        {
            var version = ChromaControlExtensions.ResolveProductVersion(tempFile, assembly);

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
        var version = ChromaControlExtensions.ResolveProductVersion("C:/missing/ChromaControl.exe", null);

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