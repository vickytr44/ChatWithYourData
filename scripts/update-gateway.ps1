# ==============================================================================
# Update-Gateway.ps1
# Automates the Subgraph Schema Export and Fusion Gateway Composition (.far)
# ==============================================================================

param (
    [switch]$SkipExport = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  ChatWithYourData - Fusion Gateway Composition Tool " -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# 1. Ensure dotnet tool dependencies (Nitro CLI) are restored
Write-Host "[1/3] Restoring local .NET tools (Nitro CLI)..." -ForegroundColor Yellow
dotnet tool restore | Out-Null

# 2. Export fresh schemas if not skipped
if (-not $SkipExport) {
    Write-Host "[2/3] Exporting GraphQL schemas from subgraphs..." -ForegroundColor Yellow
    
    # Check if services are already running on ports 5001-5004
    $subgraphs = @(
        @{ Name = "inventory"; Url = "http://localhost:5001/graphql?sdl" },
        @{ Name = "sales"; Url = "http://localhost:5002/graphql?sdl" },
        @{ Name = "procurement"; Url = "http://localhost:5003/graphql?sdl" },
        @{ Name = "finance"; Url = "http://localhost:5004/graphql?sdl" }
    )

    $needsSpinUp = $false
    foreach ($sg in $subgraphs) {
        try {
            $resp = Invoke-RestMethod -Uri $sg.Url -TimeoutSec 2 -ErrorAction Stop
            New-Item -ItemType Directory -Force -Path "subgraphs/$($sg.Name)" | Out-Null
            [System.IO.File]::WriteAllText("subgraphs/$($sg.Name)/schema.graphqls", $resp)
            Write-Host "  ✓ Exported $($sg.Name) schema from live service." -ForegroundColor Green
        }
        catch {
            $needsSpinUp = $true
            break
        }
    }

    if ($needsSpinUp) {
        Write-Host "  -> Running ./scripts/export-schemas.ps1 to start services and extract schemas..." -ForegroundColor Gray
        & "./scripts/export-schemas.ps1"
    }
}
else {
    Write-Host "[2/3] Skipping schema export (using existing subgraphs/ schema files)..." -ForegroundColor DarkGray
}

# 3. Compose subgraphs into gateway.far
Write-Host "[3/3] Composing federated schema into gateway.far..." -ForegroundColor Yellow
$farPath = "./src/Gateway/ChatWithYourData.Gateway/gateway.far"

dotnet nitro fusion compose `
    -f ./subgraphs/inventory `
    -f ./subgraphs/sales `
    -f ./subgraphs/procurement `
    -f ./subgraphs/finance `
    -a $farPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "=====================================================" -ForegroundColor Green
    Write-Host "  ✓ Gateway successfully composed: $farPath" -ForegroundColor Green
    Write-Host "=====================================================" -ForegroundColor Green
}
else {
    Write-Host "❌ Failed to compose Fusion Gateway archive." -ForegroundColor Red
    exit 1
}
