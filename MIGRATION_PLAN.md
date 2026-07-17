Migration plan: Windows 11 / 2026 upgrades

1. Create migration branch
   - `scripts/start-migration-branch.ps1` will create `migration/windows11-2026` locally.

2. Update target frameworks
   - Decide on target .NET version (2026 stable). Update `TargetFramework` in all `.csproj` files accordingly.
   - Update `TargetPlatformVersion` / Windows SDK if present to a Windows 11 baseline.

3. UI migration
   - Replace `BlazorDesktop` with WinUI 3 + WebView2 or .NET MAUI + WebView2 depending on requirements.
   - Rework desktop host to use Windows App SDK for Mica, rounded corners, DPI-awareness, and Fluent styles.

4. Packaging & distribution
   - Create MSIX package, sign artifacts, and add ARM64 build matrix.
   - Deprecate legacy installer or keep as optional offline installer.

5. CI/CD
   - Add ARM64 and x64 build matrix (GitHub Actions already added). Add packaging/installer steps.

6. Tests & telemetry
   - Run full test suite, ensure EF Core migrations work, and validate OpenTelemetry span correlation.

7. Rollout
   - Ship behind feature flag, monitor telemetry, and plan rollback strategy.

Notes:
- This plan is intentionally concise. I can start automating csproj updates and UI migration scaffolding on the migration branch when you confirm the target .NET version to upgrade to.
