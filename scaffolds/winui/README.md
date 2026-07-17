WinUI Migration Scaffold

This scaffold contains guidance and placeholder files to help migrate the desktop UI to WinUI 3 (Windows App SDK) + WebView2 for a modern Windows 11 experience.

Recommended steps:

1. Create a new WinUI 3 desktop project (Project Reunion / Windows App SDK) and add it to the solution as `ChromaControl.WinUI`.
2. Move or embed existing Blazor UI into a WebView2 control or use BlazorWebView for .NET WinUI if available.
3. Implement windowing features: Mica, rounded corners, titlebar customization, and per-monitor DPI.
4. Update project to target `net10.0-windows10.0.22621` and add the `Microsoft.WindowsAppSDK` NuGet package.
5. Test input, accessibility, and high DPI on x64 and ARM64.

Files below are placeholders and should not be included in the main solution until validated.
