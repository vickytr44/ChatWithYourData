$invProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Services/ChatWithYourData.InventoryService/ChatWithYourData.InventoryService.API/ChatWithYourData.InventoryService.API.csproj" -PassThru
$salesProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Services/ChatWithYourData.SalesService/ChatWithYourData.SalesService.API/ChatWithYourData.SalesService.API.csproj" -PassThru
$procProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Services/ChatWithYourData.ProcurementService/ChatWithYourData.ProcurementService.API/ChatWithYourData.ProcurementService.API.csproj" -PassThru
$finProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Services/ChatWithYourData.FinanceService/ChatWithYourData.FinanceService.API/ChatWithYourData.FinanceService.API.csproj" -PassThru

Write-Host "Waiting for services to spin up..."
Start-Sleep -Seconds 10

# Retry loop to fetch SDLs
$services = @(
    @{ Name = "inventory"; Url = "http://localhost:5001/graphql?sdl" },
    @{ Name = "sales"; Url = "http://localhost:5002/graphql?sdl" },
    @{ Name = "procurement"; Url = "http://localhost:5003/graphql?sdl" },
    @{ Name = "finance"; Url = "http://localhost:5004/graphql?sdl" }
)

foreach ($svc in $services) {
    $fetched = $false
    for ($i = 0; $i -lt 15; $i++) {
        try {
            $sdl = Invoke-RestMethod -Uri $svc.Url -TimeoutSec 3 -ErrorAction Stop
            New-Item -ItemType Directory -Force -Path "subgraphs/$($svc.Name)" | Out-Null
            [System.IO.File]::WriteAllText("subgraphs/$($svc.Name)/schema.graphqls", $sdl)
            Write-Host "Exported $($svc.Name) schema successfully."
            $fetched = $true
            break
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }
    if (-not $fetched) {
        Write-Error "Failed to export $($svc.Name) schema."
    }
}

Stop-Process -Id $invProc.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $salesProc.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $procProc.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $finProc.Id -Force -ErrorAction SilentlyContinue
Get-Process -Name "ChatWithYourData*" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
