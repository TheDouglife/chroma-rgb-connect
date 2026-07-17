Param(
  [string]$Branch = "migration/windows11-2026"
)

Write-Host "Creating branch $Branch (local git required)"

git checkout -b $Branch

Write-Host "Branch created. Next steps: edit csproj targets, migrate UI to WinUI/Win11, test, commit, and push. See MIGRATION_PLAN.md for details."

git status
