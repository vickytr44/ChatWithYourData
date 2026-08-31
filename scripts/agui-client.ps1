param(
    [string]$Prompt = "Show me the top products and stock levels from the inventory"
)

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "                AG-UI PROTOCOL CLIENT (SSE Streaming)                " -ForegroundColor Yellow
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "Target Endpoint: http://localhost:5005/ag-ui" -ForegroundColor Gray
Write-Host "User Prompt:     $Prompt`n" -ForegroundColor Green

Add-Type -AssemblyName System.Net.Http

# Prepare AG-UI payload
$payload = @{
    messages = @(
        @{
            role = "user"
            content = $Prompt
        }
    )
} | ConvertTo-Json -Depth 5

$handler = [System.Net.Http.HttpClientHandler]::new()
$client = [System.Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(60)

$request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post, "http://localhost:5005/ag-ui")
$request.Headers.Add("Accept", "text/event-stream")
$request.Content = [System.Net.Http.StringContent]::new($payload, [System.Text.Encoding]::UTF8, "application/json")

try {
    $response = $client.SendAsync($request, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
    Write-Host "Connected! HTTP Status: $($response.StatusCode)" -ForegroundColor Cyan
    Write-Host "Content-Type:   $($response.Content.Headers.ContentType)" -ForegroundColor Gray
    Write-Host "----------------------------------------------------------------------`n" -ForegroundColor Gray

    $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
    $reader = [System.IO.StreamReader]::new($stream)

    $currentEvent = ""
    while (-not $reader.EndOfStream) {
        $line = $reader.ReadLine()
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line.StartsWith("event:")) {
            $currentEvent = $line.Substring(6).Trim()
        }
        elseif ($line.StartsWith("data:")) {
            $dataStr = $line.Substring(5).Trim()
            
            # Print raw event details for protocol transparency
            if ($currentEvent -ne "response.output_text.delta" -and $currentEvent -ne "message.delta" -and $dataStr -ne "[DONE]") {
                Write-Host "`n[SSE Event: $currentEvent] " -ForegroundColor DarkYellow -NoNewline
                try {
                    $jsonObj = $dataStr | ConvertFrom-Json
                    Write-Host "($($jsonObj.type))" -ForegroundColor DarkGray
                } catch {
                    Write-Host $dataStr -ForegroundColor DarkGray
                }
            }

            # If it's a streaming delta token, render directly
            try {
                $jsonObj = $dataStr | ConvertFrom-Json
                if ($jsonObj.delta) {
                    Write-Host $jsonObj.delta -NoNewline -ForegroundColor White
                } elseif ($jsonObj.content) {
                    Write-Host $jsonObj.content -NoNewline -ForegroundColor White
                } elseif ($jsonObj.text) {
                    Write-Host $jsonObj.text -NoNewline -ForegroundColor White
                }
            } catch {
                if ($dataStr -ne "[DONE]") {
                    Write-Host $dataStr -ForegroundColor White
                }
            }
        }
        else {
            Write-Host $line -ForegroundColor DarkGray
        }
    }

    Write-Host "`n`n======================================================================" -ForegroundColor Cyan
    Write-Host "AG-UI Stream Completed." -ForegroundColor Green
    Write-Host "======================================================================" -ForegroundColor Cyan
}
catch {
    Write-Host "Error during AG-UI client request: $_" -ForegroundColor Red
}
finally {
    $client.Dispose()
    $handler.Dispose()
}
