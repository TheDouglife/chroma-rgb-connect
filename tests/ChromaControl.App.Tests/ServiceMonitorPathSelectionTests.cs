// Licensed to the Douglife (Doug Montgomery) under one or more agreements.
// The Douglife (Doug Montgomery) licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ChromaControl.App.Core.Services;

namespace ChromaControl.App.Tests;

public class ServiceMonitorPathSelectionTests
{
    [Fact]
    public void SelectMatchingCandidateIndexReturnsNullWhenNoCandidates()
    {
        var result = ServiceMonitor.SelectMatchingCandidateIndex("C:/Apps/ChromaControl.Service.exe", []);

        Assert.Null(result);
    }

    [Fact]
    public void SelectMatchingCandidateIndexReturnsMatchingIndexCaseInsensitive()
    {
        var candidates = new[]
        {
            new ProcessPathCandidate(0, "C:/Other/Service.exe"),
            new ProcessPathCandidate(1, "c:/apps/chromacontrol.service.exe")
        };

        var result = ServiceMonitor.SelectMatchingCandidateIndex("C:/Apps/ChromaControl.Service.exe", candidates);

        Assert.Equal(1, result);
    }

    [Fact]
    public void SelectMatchingCandidateIndexSkipsEmptyAndInvalidPaths()
    {
        var candidates = new[]
        {
            new ProcessPathCandidate(0, null),
            new ProcessPathCandidate(1, ""),
            new ProcessPathCandidate(2, "::invalid::path::"),
            new ProcessPathCandidate(3, "C:/Apps/ChromaControl.Service.exe")
        };

        var result = ServiceMonitor.SelectMatchingCandidateIndex("C:/Apps/ChromaControl.Service.exe", candidates);

        Assert.Equal(3, result);
    }

    [Fact]
    public void SelectMatchingCandidateIndexReturnsNullWhenAllCandidatesDiffer()
    {
        var candidates = new[]
        {
            new ProcessPathCandidate(0, "C:/Foo/Bar.exe"),
            new ProcessPathCandidate(1, "C:/Baz/Qux.exe")
        };

        var result = ServiceMonitor.SelectMatchingCandidateIndex("C:/Apps/ChromaControl.Service.exe", candidates);

        Assert.Null(result);
    }
}
