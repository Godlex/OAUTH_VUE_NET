<#
.SYNOPSIS
    Starts all InventoryApp services: PostgreSQL (Docker), IdentityServer, WebAPI, and Vue frontend.

.DESCRIPTION
    Run this script from the project root:
        .\start.ps1

    Optional flags:
        -SkipDocker     Don't try to start the PostgreSQL Docker container
                        (use this if PostgreSQL is already running locally)
        -NoBrowser      Don't open the browser automatically after startup

.EXAMPLE
    .\start.ps1
    .\start.ps1 -SkipDocker
    .\start.ps1 -SkipDocker -NoBrowser
#>

param(
    [switch]$SkipDocker,
    [switch]$NoBrowser
)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Write-Header { param([string]$Text)
    Write-Host ""
    Write-Host "  == $Text ==" -ForegroundColor Cyan
}

function Write-Step { param([string]$Text)
    Write-Host "  >> $Text" -ForegroundColor White
}

function Write-Ok { param([string]$Text)
    Write-Host "  [OK] $Text" -ForegroundColor Green
}

function Write-Warn { param([string]$Text)
    Write-Host "  [!!] $Text" -ForegroundColor Yellow
}

function Write-Fail { param([string]$Text)
    Write-Host "  [ERROR] $Text" -ForegroundColor Red
}

function Test-CommandExists {
    param([string]$Name)
    return ($null -ne (Get-Command $Name -ErrorAction SilentlyContinue))
}

function Wait-ForUrl {
    param(
        [string]$Url,
        [string]$ServiceName,
        [int]$TimeoutSeconds = 90
    )
    $elapsed  = 0
    $interval = 2
    Write-Step "Waiting for $ServiceName to respond at $Url ..."
    while ($elapsed -lt $TimeoutSeconds) {
        try {
            $resp = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 3 -SkipCertificateCheck -ErrorAction Stop
            if ($resp.StatusCode -lt 500) {
                Write-Ok "$ServiceName is ready!"
                return $true
            }
        } catch { }
        Start-Sleep -Seconds $interval
        $elapsed += $interval
        Write-Host "    ... $elapsed s / ${TimeoutSeconds}s" -ForegroundColor DarkGray
    }
    Write-Fail "$ServiceName did not respond after ${TimeoutSeconds}s. Check the terminal window for errors."
    return $false
}

function Open-ServiceWindow {
    param(
        [string]$Title,
        [string]$WorkingDir,
        [string]$Command
    )
    if (Test-CommandExists "wt") {
        # Windows Terminal: open as a new tab
        Start-Process "wt" -ArgumentList "new-tab", "--title", $Title, "--", "powershell", "-NoExit", "-Command",
            "cd '$WorkingDir'; $Command"
    } else {
        # Plain PowerShell window
        Start-Process "powershell" -ArgumentList "-NoExit", "-Command",
            "cd '$WorkingDir'; `$host.UI.RawUI.WindowTitle = '$Title'; $Command"
    }
}

# ---------------------------------------------------------------------------
# Banner
# ---------------------------------------------------------------------------

Clear-Host
Write-Host ""
Write-Host "  +--------------------------------------------------+" -ForegroundColor Cyan
Write-Host "  |       InventoryApp -- Full Stack Launcher        |" -ForegroundColor Cyan
Write-Host "  |  .NET 10  Vue 3  Duende IdentityServer  PG       |" -ForegroundColor Cyan
Write-Host "  +--------------------------------------------------+" -ForegroundColor Cyan
Write-Host ""

$root = $PSScriptRoot

# ---------------------------------------------------------------------------
# Step 1 -- Prerequisites check
# ---------------------------------------------------------------------------

Write-Header "Checking prerequisites"

if (-not (Test-CommandExists "dotnet")) {
    Write-Fail ".NET SDK not found. Install it from https://dotnet.microsoft.com/download"
    exit 1
}
Write-Ok ".NET SDK: $((dotnet --version 2>&1))"

if (-not (Test-CommandExists "node")) {
    Write-Fail "Node.js not found. Install it from https://nodejs.org/"
    exit 1
}
Write-Ok "Node.js: $((node --version 2>&1))"

$nodeModules = Join-Path $root "frontend\node_modules"
if (-not (Test-Path $nodeModules)) {
    Write-Step "node_modules not found -- running npm install..."
    Push-Location (Join-Path $root "frontend")
    npm install --silent
    Pop-Location
    Write-Ok "npm install complete"
} else {
    Write-Ok "node_modules already present"
}

# ---------------------------------------------------------------------------
# Step 2 -- PostgreSQL via Docker
# ---------------------------------------------------------------------------

Write-Header "PostgreSQL"

if ($SkipDocker) {
    Write-Warn "-SkipDocker flag set. Make sure PostgreSQL is running on localhost:5432"
    Write-Warn "Credentials expected: username=postgres  password=postgres"
}
elseif (Test-CommandExists "docker") {
    $daemonRunning = $false
    try {
        docker info 2>&1 | Out-Null
        $daemonRunning = ($LASTEXITCODE -eq 0)
    } catch { }

    if ($daemonRunning) {
        Write-Step "Starting PostgreSQL container (docker compose up -d)..."
        Push-Location $root
        docker compose up -d 2>&1 | Out-Null
        Pop-Location

        $pgReady = $false
        for ($i = 0; $i -lt 15; $i++) {
            $check = docker compose -f "$root\docker-compose.yml" exec -T postgres pg_isready -U postgres 2>&1
            if ($check -match "accepting connections") {
                $pgReady = $true
                break
            }
            Start-Sleep -Seconds 2
        }

        if ($pgReady) {
            Write-Ok "PostgreSQL ready on localhost:5432"
            Write-Ok "pgAdmin available at http://localhost:5050  (admin@admin.com / admin)"
        } else {
            Write-Warn "Container started but health check timed out. Services will still try to connect."
        }
    } else {
        Write-Fail "Docker is installed but the Docker daemon is not running."
        Write-Fail "Start Docker Desktop, then re-run this script."
        Write-Fail "Or use: .\start.ps1 -SkipDocker   if PostgreSQL is already running."
        exit 1
    }
} else {
    Write-Warn "Docker not found. Assuming PostgreSQL is already running on localhost:5432."
    Write-Warn "Install Docker Desktop to use the bundled container: https://www.docker.com"
}

# ---------------------------------------------------------------------------
# Step 3 -- IdentityServer
# ---------------------------------------------------------------------------

Write-Header "Starting IdentityServer  -->  https://localhost:5001"

$isDir = Join-Path $root "backend\src\IdentityServer\OAUTH_VUE_NET.IdentityServer"
Open-ServiceWindow -Title "IdentityServer :5001" -WorkingDir $isDir -Command "dotnet run"

$isUp = Wait-ForUrl -Url "https://localhost:5001/.well-known/openid-configuration" `
                    -ServiceName "IdentityServer" -TimeoutSeconds 90
if (-not $isUp) { exit 1 }

# ---------------------------------------------------------------------------
# Step 4 -- WebAPI
# ---------------------------------------------------------------------------

Write-Header "Starting WebAPI  -->  https://localhost:5002"

$apiDir = Join-Path $root "backend\src\WebAPI\OAUTH_VUE_NET.WebAPI"
Open-ServiceWindow -Title "WebAPI :5002" -WorkingDir $apiDir -Command "dotnet run"

$apiUp = Wait-ForUrl -Url "https://localhost:5002/swagger/index.html" `
                     -ServiceName "WebAPI" -TimeoutSeconds 60
if (-not $apiUp) { exit 1 }

# ---------------------------------------------------------------------------
# Step 5 -- Vue Frontend
# ---------------------------------------------------------------------------

Write-Header "Starting Vue Frontend  -->  http://localhost:5173"

$frontendDir = Join-Path $root "frontend"
Open-ServiceWindow -Title "Vue Frontend :5173" -WorkingDir $frontendDir -Command "npm run dev"

$null = Wait-ForUrl -Url "http://localhost:5173" -ServiceName "Vue Frontend" -TimeoutSeconds 30

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "  +--------------------------------------------------+" -ForegroundColor Green
Write-Host "  |          All services are running!               |" -ForegroundColor Green
Write-Host "  +--------------------------------------------------+" -ForegroundColor Green
Write-Host "  |  Frontend       http://localhost:5173            |" -ForegroundColor Green
Write-Host "  |  WebAPI         https://localhost:5002/swagger   |" -ForegroundColor Green
Write-Host "  |  IdentityServer https://localhost:5001           |" -ForegroundColor Green
Write-Host "  +--------------------------------------------------+" -ForegroundColor Green
Write-Host "  |  Test users:  alice / Alice123!                  |" -ForegroundColor Green
Write-Host "  |               bob   / Bob123!                    |" -ForegroundColor Green
Write-Host "  +--------------------------------------------------+" -ForegroundColor Green
Write-Host ""
Write-Host "  To stop everything:  .\stop.ps1" -ForegroundColor DarkGray
Write-Host ""

if (-not $NoBrowser) {
    Write-Step "Opening browser at http://localhost:5173 ..."
    Start-Sleep -Seconds 1
    Start-Process "http://localhost:5173"
}
