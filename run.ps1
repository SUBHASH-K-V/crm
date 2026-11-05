# Add dotnet to current session PATH if missing
if ($env:PATH -notlike "*C:\Program Files\dotnet*") {
    $env:PATH = "C:\Program Files\dotnet;" + $env:PATH
}

# Stop any stale running instances of AcxiomCRM to prevent file-locking issues
Get-Process -Name "AcxiomCRM" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

Set-Location "$PSScriptRoot\AcxiomCRM"
Write-Host "Starting AcxiomCRM on http://localhost:5001..." -ForegroundColor Cyan

# Use dotnet.exe directly or from PATH
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    dotnet run
} else {
    & "C:\Program Files\dotnet\dotnet.exe" run
}
