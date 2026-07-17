// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.Common.Extensions;

namespace ChromaControl.App.Tests;

public class ChromaControlBootstrapDirectoryTests
{
    [Fact]
    public void EnsureBootstrapDirectoriesExistCreatesDataLogsAndConfigDirectories()
    {
        var createdPaths = new List<string>();
        var basePath = $"C:/Bootstrap/{Guid.NewGuid():N}";
        var paths = new ChromaControlPaths(
            AppPath: "C:/Apps/ChromaControl",
            DataPath: Path.Combine(basePath, "data"),
            LogsPath: Path.Combine(basePath, "logs"),
            ConfigPath: Path.Combine(basePath, "config"),
            DatabaseConnectionString: $"Data Source={Path.Combine(basePath, "ChromaControl.db")}");

        ChromaControlExtensions.EnsureBootstrapDirectoriesExist(paths, path =>
        {
            createdPaths.Add(path);
            return new DirectoryInfo(path);
        });

        Assert.Equal([paths.DataPath, paths.LogsPath, paths.ConfigPath], createdPaths);
    }

    [Fact]
    public void EnsureBootstrapDirectoriesExistWrapsDirectoryCreationErrorsWithPurposeAndPath()
    {
        var paths = new ChromaControlPaths(
            AppPath: "C:/Apps/ChromaControl",
            DataPath: $"C:/Bootstrap/{Guid.NewGuid():N}/data",
            LogsPath: $"C:/Bootstrap/{Guid.NewGuid():N}/logs",
            ConfigPath: $"C:/Bootstrap/{Guid.NewGuid():N}/config",
            DatabaseConnectionString: "Data Source=C:/Bootstrap/ChromaControl.db");

        var exception = Assert.Throws<InvalidOperationException>(() => ChromaControlExtensions.EnsureBootstrapDirectoriesExist(paths, _ => throw new UnauthorizedAccessException("denied")));

        Assert.Contains("data directory", exception.Message, StringComparison.Ordinal);
        Assert.Contains(paths.DataPath, exception.Message, StringComparison.Ordinal);
        Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
    }
}