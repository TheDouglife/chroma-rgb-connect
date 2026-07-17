Compatibility scan - quick notes

Scanned PackageReferences in csproj files. Potential items to verify/upgrade:

- `BlazorDesktop` (src/App): likely deprecated. Recommend migrating to WinUI 3 + WebView2 or `BlazorWebView` in WinUI/.NET MAUI.
- `Aspire.Hosting.AppHost` (src/AppHost): verify compatibility with .NET 10; consider replacing with `GenericHost` or supported host if unmaintained.
- `NetSparkleUpdater.SparkleUpdater` (src/App): evaluate support and code-signing expectations; consider MSIX/AppInstaller updates.
- `NReco.Logging.File` (src/Common): check maintainer status; replace with `Serilog`/sinks if necessary.
- `ChromaControl.SDK.*` packages: likely internal; ensure native interop bindings are rebuilt for new runtimes and for ARM64.
- `Grpc.*` packages: generally keep up-to-date; ensure `Grpc.Tools` and `Protobuf` usages are compatible with `net10.0` and codegen updated.
- `Microsoft.EntityFrameworkCore.Sqlite` + EF tools: ensure migrations and SQLite native binaries support ARM64 and the new runtime.
- `OpenTelemetry.*`: ensure exporter packages support the new SDK and update configuration if API changes.

Action items

- Run `dotnet list package --outdated` on CI for all projects to get a concrete list.
- Add Dependabot config to automate minor/patch updates.
- Test native dependencies (SDK packages) on ARM64 and Windows 11 VMs.

Local build note

- Local `dotnet build` failed because the local machine does not have .NET 10 SDK installed. CI will use `actions/setup-dotnet` to install `10.0.x` and perform builds.
