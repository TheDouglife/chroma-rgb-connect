# Removes the short MIT header from .cs files under any obj directories
$headerPattern = "// MIT License[\r\n]+// Copyright \(c\) Douglife \(Doug Montgomery\)[\s\S]*?See LICENSE in repository root for full license text\.[\r\n]*"

Get-ChildItem -Path . -Recurse -Filter *.cs | Where-Object { $_.FullName -match "[\\/]obj[\\/]" } | ForEach-Object {
    try {
        $path = $_.FullName
        $text = Get-Content -Raw -LiteralPath $path -ErrorAction Stop
        if ($text -match $headerPattern) {
            Write-Host "Removing header from $path"
            $new = $text -replace $headerPattern, ""
            Set-Content -LiteralPath $path -Value $new -Encoding UTF8
        }
    } catch {
        Write-Warning "Failed to process $($_.FullName): $_"
    }
}

Write-Host "Done. Review changes and commit them."