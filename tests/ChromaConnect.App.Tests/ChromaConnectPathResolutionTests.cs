// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Extensions;
using Microsoft.Extensions.Configuration;

namespace ChromaControl.App.Tests;

public class ChromaControlPathResolutionTests
{
    [Fact]
    public void ResolvePathsReturnsDefaultsWhenOverridesAreMissing()
    {
        var configuration = new ConfigurationManager();
        var expectedDataPath = Path.Combine("C:/Users/Test/AppData/Local", "ChromaControl");

        var paths = ChromaControlExtensions.ResolvePaths(configuration, "C:/Apps/ChromaControl/", "C:/Users/Test/AppData/Local");

        Assert.Equal("C:/Apps/ChromaControl/", paths.AppPath);
        Assert.Equal(expectedDataPath, paths.DataPath);
        Assert.Equal(Path.Combine(expectedDataPath, "logs"), paths.LogsPath);
        Assert.Equal(Path.Combine(expectedDataPath, "config"), paths.ConfigPath);
        Assert.Equal($"Data Source={Path.Combine(expectedDataPath, "ChromaControl.db")}", paths.DatabaseConnectionString);
    }

    [Fact]
    public void ResolvePathsPreservesExplicitOverridesAndDerivedDefaults()
    {
        var configuration = new ConfigurationManager();
        configuration["ChromaControl:Path:APP"] = "D:/Portable/ChromaControl";
        configuration["ChromaControl:Path:DATA"] = "E:/ChromaData";

        var paths = ChromaControlExtensions.ResolvePaths(configuration, "C:/Ignored", "C:/Users/Test/AppData/Local");

        Assert.Equal("D:/Portable/ChromaControl", paths.AppPath);
        Assert.Equal("E:/ChromaData", paths.DataPath);
        Assert.Equal(Path.Combine("E:/ChromaData", "logs"), paths.LogsPath);
        Assert.Equal(Path.Combine("E:/ChromaData", "config"), paths.ConfigPath);
        Assert.Equal($"Data Source={Path.Combine("E:/ChromaData", "ChromaControl.db")}", paths.DatabaseConnectionString);
    }

    [Fact]
    public void ResolvePathsPreservesExplicitLogConfigAndDatabaseOverrides()
    {
        var configuration = new ConfigurationManager();
        configuration["ChromaControl:Path:DATA"] = "E:/ChromaData";
        configuration["ChromaControl:Path:LOGS"] = "F:/SharedLogs";
        configuration["ChromaControl:Path:CONFIG"] = "G:/SharedConfig";
        configuration["ConnectionStrings:Database"] = "Data Source=H:/Custom/Chroma.db";

        var paths = ChromaControlExtensions.ResolvePaths(configuration, "C:/Ignored", "C:/Users/Test/AppData/Local");

        Assert.Equal("E:/ChromaData", paths.DataPath);
        Assert.Equal("F:/SharedLogs", paths.LogsPath);
        Assert.Equal("G:/SharedConfig", paths.ConfigPath);
        Assert.Equal("Data Source=H:/Custom/Chroma.db", paths.DatabaseConnectionString);
    }
}