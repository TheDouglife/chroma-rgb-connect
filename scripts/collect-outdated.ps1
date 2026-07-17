$out = "outdated-packages.txt"
if (Test-Path $out) { Remove-Item $out -Force }

Get-ChildItem -Path src,tests -Recurse -Filter *.csproj | ForEach-Object {
    $p = $_.FullName
    "`n== $p ==`n" | Out-File -FilePath $out -Append -Encoding utf8
    dotnet list "$p" package --outdated 2>&1 | Out-File -FilePath $out -Append -Encoding utf8
}

Write-Host "Wrote $out"