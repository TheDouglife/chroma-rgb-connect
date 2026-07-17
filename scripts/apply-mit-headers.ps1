# Prepends short MIT header to all .cs files that don't already contain it
$header = @"
// MIT License
// Copyright (c) Douglife (Doug Montgomery)
// See LICENSE in repository root for full license text.
"@

Get-ChildItem -Path . -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch "[\\/]obj[\\/]" -and $_.FullName -notmatch "[\\/]bin[\\/]" -and $_.FullName -notmatch "\\.git" } | ForEach-Object {
    try {
        $text = Get-Content -Raw -LiteralPath $_.FullName -ErrorAction Stop
    } catch {
        Write-Warning "Failed to read $($_.FullName): $_"
        return
    }

    if ($text -notmatch 'MIT License' -and $text -notmatch 'Licensed to the Douglife') {
        Write-Host "Adding header to $($_.FullName)"
        $new = $header + "`r`n" + $text
        Set-Content -LiteralPath $_.FullName -Value $new -Encoding UTF8
    }
}

Write-Host "Done. Review changes and commit them."