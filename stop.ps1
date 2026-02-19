<#
.SYNOPSIS
    Stops all InventoryApp services.

.DESCRIPTION
    Kills .NET processes (IdentityServer, WebAPI), Node.js (Vue dev server),
    and optionally stops the PostgreSQL Docker container.

.EXAMPLE
    .\stop.ps1
    .\stop.ps1 -KeepDocker    # leave PostgreSQL running
#>

param(
    [switch]$KeepDocker
)

function Write-Ok   { param([string]$T) Write-Host "  [OK] $T" -ForegroundColor Green  }
function Write-Step { param([string]$T) Write-Host "  >>   $T" -ForegroundColor White  }
function Write-Warn { param([string]$T) Write-Host "  [!!] $T" -ForegroundColor Yellow }
function Write-Fail { param([string]$T) Write-Host "  [ERROR] $T" -ForegroundColor Red }
function Test-CommandExists { param([string]$N) return ($null -ne (Get-Command $N -ErrorAction SilentlyContinue)) }

Clear-Host
Write-Host ""
Write-Host "  +----------------------------------------------+" -ForegroundColor Red
Write-Host "  |       InventoryApp -- Stopping Services      |" -ForegroundColor Red
Write-Host "  +----------------------------------------------+" -ForegroundColor Red
Write-Host ""

$root = $PSScriptRoot

# ---- Stop .NET processes -------------------------------------------------
Write-Step "Stopping .NET services (IdentityServer + WebAPI)..."

$killed = 0
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | ForEach-Object {
    $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
    if ($cmdLine -match "OAUTH_VUE_NET") {
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        $killed++
    }
}

# Fallback: if none matched by project name, kill all dotnet processes
if ($killed -eq 0) {
    $procs = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    if ($procs) {
        Write-Warn "Could not identify specific dotnet processes -- stopping all dotnet processes."
        $procs | Stop-Process -Force -ErrorAction SilentlyContinue
        $killed = $procs.Count
    }
}

if ($killed -gt 0) {
    Write-Ok "Stopped $killed dotnet process(es)"
} else {
    Write-Warn "No dotnet processes were running"
}

# ---- Stop Node / Vite (Vue frontend) -------------------------------------
Write-Step "Stopping Vue frontend (node)..."

$nodeKilled = 0
Get-Process -Name "node" -ErrorAction SilentlyContinue | ForEach-Object {
    $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
    if ($cmdLine -match "vite" -or $cmdLine -match "frontend") {
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        $nodeKilled++
    }
}

if ($nodeKilled -gt 0) {
    Write-Ok "Stopped $nodeKilled Node.js process(es)"
} else {
    Write-Warn "No matching Node.js (Vite) processes were running"
}

# ---- Stop PostgreSQL Docker container ------------------------------------
if ($KeepDocker) {
    Write-Warn "Skipping Docker shutdown (-KeepDocker flag set)"
}
elseif (Test-CommandExists "docker") {
    $composeFile = Join-Path $root "docker-compose.yml"
    if (Test-Path $composeFile) {
        Write-Step "Stopping PostgreSQL Docker container..."
        Push-Location $root
        docker compose stop 2>&1 | Out-Null
        Pop-Location
        Write-Ok "PostgreSQL container stopped (data is preserved)"
        Write-Host "  Tip: to delete ALL data run:  docker compose down -v" -ForegroundColor DarkGray
    }
} else {
    Write-Warn "Docker not found -- skipping container shutdown"
}

Write-Host ""
Write-Host "  All services stopped." -ForegroundColor Green
Write-Host "  To start again:  .\start.ps1" -ForegroundColor DarkGray
Write-Host ""
