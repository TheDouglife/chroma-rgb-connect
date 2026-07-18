// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaConnect.Common.Extensions;
using Microsoft.Extensions.Configuration;

namespace ChromaConnect.App.Tests;

public class ChromaConnectPathResolutionTests
{
    [Fact]
    public void ResolvePathsReturnsDefaultsWhenOverridesAreMissing()
    {
        var configuration = new ConfigurationManager();
        var expectedDataPath = Path.Combine("C:/Users/Test/AppData/Local", "ChromaConnect");

        var paths = ChromaConnectExtensions.ResolvePaths(configuration, "C:/Apps/ChromaConnect/", "C:/Users/Test/AppData/Local");

        Assert.Equal("C:/Apps/ChromaConnect/", paths.AppPath);
        Assert.Equal(expectedDataPath, paths.DataPath);
        Assert.Equal(Path.Combine(expectedDataPath, "logs"), paths.LogsPath);
        Assert.Equal(Path.Combine(expectedDataPath, "config"), paths.ConfigPath);
        Assert.Equal($"Data Source={Path.Combine(expectedDataPath, "ChromaConnect.db")}", paths.DatabaseConnectionString);
    }

    [Fact]
    public void ResolvePathsPreservesExplicitOverridesAndDerivedDefaults()
    {
        var configuration = new ConfigurationManager();
        configuration["ChromaConnect:Path:APP"] = "D:/Portable/ChromaConnect";
        configuration["ChromaConnect:Path:DATA"] = "E:/ChromaData";

        var paths = ChromaConnectExtensions.ResolvePaths(configuration, "C:/Ignored", "C:/Users/Test/AppData/Local");

        Assert.Equal("D:/Portable/ChromaConnect", paths.AppPath);
        Assert.Equal("E:/ChromaData", paths.DataPath);
        Assert.Equal(Path.Combine("E:/ChromaData", "logs"), paths.LogsPath);
        Assert.Equal(Path.Combine("E:/ChromaData", "config"), paths.ConfigPath);
        Assert.Equal($"Data Source={Path.Combine("E:/ChromaData", "ChromaConnect.db")}", paths.DatabaseConnectionString);
    }

    [Fact]
    public void ResolvePathsPreservesExplicitLogConfigAndDatabaseOverrides()
    {
        var configuration = new ConfigurationManager();
        configuration["ChromaConnect:Path:DATA"] = "E:/ChromaData";
        configuration["ChromaConnect:Path:LOGS"] = "F:/SharedLogs";
        configuration["ChromaConnect:Path:CONFIG"] = "G:/SharedConfig";
        configuration["ConnectionStrings:Database"] = "Data Source=H:/Custom/Chroma.db";

        var paths = ChromaConnectExtensions.ResolvePaths(configuration, "C:/Ignored", "C:/Users/Test/AppData/Local");

        Assert.Equal("E:/ChromaData", paths.DataPath);
        Assert.Equal("F:/SharedLogs", paths.LogsPath);
        Assert.Equal("G:/SharedConfig", paths.ConfigPath);
        Assert.Equal("Data Source=H:/Custom/Chroma.db", paths.DatabaseConnectionString);
    }
}