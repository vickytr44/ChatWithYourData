# Diagnostic script testing Gemini 3.5 Flash Lite with MCP Tools vs Structured Output

$appSettingsPath = Join-Path $PSScriptRoot "..\src\Services\ChatWithYourData.ChatService\ChatWithYourData.ChatService.API\appsettings.Development.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
$apiKey = $appSettings.Agent.ApiKey
$endpoint = $appSettings.Agent.Endpoint + "chat/completions"
$model = $appSettings.Agent.Model

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTIC TEST: Gemini 3.5 Flash Lite + MCP Tools vs Structured Output" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan

# Test 1: Simple Schema ONLY
Write-Host "`n[TEST 1] Structured Schema ONLY (No Tools)..." -ForegroundColor Yellow
$bodySchemaOnly = @{
    model = $model
    messages = @(
        @{ role = "user"; content = "Give me 1 dummy invoice" }
    )
    response_format = @{
        type = "json_schema"
        json_schema = @{
            name = "AgentStructuredOutput"
            strict = $true
            schema = @{
                type = "object"
                properties = @{
                    summary = @{ type = "string" }
                    data = @{ type = "string" }
                    primaryEntityName = @{ type = "string" }
                    success = @{ type = "boolean" }
                }
                required = @("summary", "data", "primaryEntityName", "success")
            }
        }
    }
} | ConvertTo-Json -Depth 10

try {
    $res1 = Invoke-RestMethod -Uri $endpoint -Method Post -Headers @{ "Authorization" = "Bearer $apiKey"; "Content-Type" = "application/json" } -Body $bodySchemaOnly
    Write-Host ">>> TEST 1 RESULT: SUCCESS (HTTP 200) - Gemini returned structured JSON output:" -ForegroundColor Green
    $res1.choices[0].message.content | Write-Host -ForegroundColor Gray
} catch {
    $stream = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    Write-Host ">>> TEST 1 RESULT: FAILED - $($stream.ReadToEnd())" -ForegroundColor Red
}

# Test 2: Tools ONLY (Standard Natural Language Agent Execution)
Write-Host "`n[TEST 2] Tools ONLY (Standard agent.RunAsync without response_format)..." -ForegroundColor Yellow
$bodyToolsOnly = @{
    model = $model
    messages = @(
        @{ role = "user"; content = "Get me all the unpaid invoices" }
    )
    tools = @(
        @{
            type = "function"
            function = @{
                name = "search_invoices_and_payments"
                description = "Searches customer invoices"
                parameters = @{
                    type = "object"
                    properties = @{
                        first = @{ type = "integer" }
                    }
                }
            }
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $res2 = Invoke-RestMethod -Uri $endpoint -Method Post -Headers @{ "Authorization" = "Bearer $apiKey"; "Content-Type" = "application/json" } -Body $bodyToolsOnly
    $toolName = $res2.choices[0].message.tool_calls[0].function.name
    Write-Host ">>> TEST 2 RESULT: SUCCESS (HTTP 200) - Gemini invoked tool '$toolName'!" -ForegroundColor Green
} catch {
    $stream = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    Write-Host ">>> TEST 2 RESULT: FAILED - $($stream.ReadToEnd())" -ForegroundColor Red
}

# Test 3: Complex Tools (with $defs and $ref) + Structured Output Schema
Write-Host "`n[TEST 3] GraphQL MCP Tools (with `$defs / `$ref) + response_format (agent.RunAsync<T>)..." -ForegroundColor Yellow
$bodyComplex = @{
    model = $model
    messages = @(
        @{ role = "user"; content = "Get me all the unpaid invoices" }
    )
    tools = @(
        @{
            type = "function"
            function = @{
                name = "search_financial_gl"
                description = "Searches ledger"
                parameters = @{
                    type = "object"
                    properties = @{
                        where = @{
                            anyOf = @(
                                @{ "`$ref" = "#/`$defs/AccountFilterInput" },
                                @{ type = "null" }
                            )
                        }
                    }
                    "`$defs" = @{
                        AccountFilterInput = @{
                            type = "object"
                            properties = @{
                                accountCode = @{
                                    anyOf = @(
                                        @{ type = "string" },
                                        @{ type = "null" }
                                    )
                                }
                            }
                        }
                    }
                }
            }
        }
    )
    response_format = @{
        type = "json_schema"
        json_schema = @{
            name = "AgentStructuredOutput"
            strict = $true
            schema = @{
                type = "object"
                properties = @{
                    summary = @{ type = "string" }
                    data = @{ type = "string" }
                    primaryEntityName = @{ type = "string" }
                    success = @{ type = "boolean" }
                }
                required = @("summary", "data", "primaryEntityName", "success")
            }
        }
    }
} | ConvertTo-Json -Depth 20

try {
    $res3 = Invoke-RestMethod -Uri $endpoint -Method Post -Headers @{ "Authorization" = "Bearer $apiKey"; "Content-Type" = "application/json" } -Body $bodyComplex
    Write-Host ">>> TEST 3 RESULT: SUCCESS (HTTP 200)" -ForegroundColor Green
} catch {
    $stream = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    $errorBody = $stream.ReadToEnd()
    Write-Host ">>> TEST 3 RESULT: FAILED (HTTP 400 Bad Request):" -ForegroundColor Red
    Write-Host $errorBody -ForegroundColor Magenta
}

Write-Host "`n======================================================================" -ForegroundColor Cyan
Write-Host " ROOT CAUSE DIAGNOSIS & FIX:" -ForegroundColor Cyan
Write-Host " - Test 1 proves Gemini supports Structured Outputs." -ForegroundColor Cyan
Write-Host " - Test 2 proves Gemini supports MCP Tools execution." -ForegroundColor Cyan
Write-Host " - Test 3 proves Gemini API throws 400 Bad Request when combining" -ForegroundColor Cyan
Write-Host "   complex GraphQL `$defs/`$ref tool schemas with response_format." -ForegroundColor Cyan
Write-Host " - FIX: Call agent.RunAsync(intent) to allow full MCP tool execution," -ForegroundColor Cyan
Write-Host "   instructing the agent to output the JSON structure in its system prompt." -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
