# WinMate bootstrap — downloads the latest release and launches it elevated.
# Usage:  irm https://raw.githubusercontent.com/Kha1idQ/winmate/main/scripts/install.ps1 | iex

$repo = "Kha1idQ/winmate"

Write-Host ""
Write-Host "  WinMate installer" -ForegroundColor Yellow
Write-Host "  -----------------" -ForegroundColor DarkYellow

try {
    Write-Host "  Fetching latest release info..." -ForegroundColor Gray
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/releases/latest" -UseBasicParsing
    $asset = $release.assets | Where-Object { $_.name -eq "WinMate.exe" } | Select-Object -First 1
    if (-not $asset) { throw "WinMate.exe not found in the latest release." }

    $target = Join-Path $env:TEMP "WinMate.exe"
    Write-Host "  Downloading WinMate $($release.tag_name) ($([math]::Round($asset.size / 1MB)) MB)..." -ForegroundColor Gray
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $target -UseBasicParsing

    Write-Host "  Launching (a UAC prompt will appear)..." -ForegroundColor Gray
    Start-Process -FilePath $target -Verb RunAs
    Write-Host "  Done! Enjoy WinMate." -ForegroundColor Yellow
}
catch {
    Write-Host "  Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  You can download WinMate manually: https://github.com/$repo/releases" -ForegroundColor Gray
}
