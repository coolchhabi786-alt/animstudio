#Requires -Version 7.1
<#
.SYNOPSIS
    Stops local Docker containers (SQL Server + Redis) used for local dev testing.
    Optionally downgrades SignalR back to Free tier if -FullStack was used.

.DESCRIPTION
    Pairs with start-local.ps1.
    Does NOT touch Container Apps, Static Web App, CDN, or Service Bus.

.EXAMPLE
    .\stop-local.ps1                     # Stop Docker containers
    .\stop-local.ps1 -FullStack          # Also downgrade SignalR back to Free
    .\stop-local.ps1 -Remove             # Stop AND remove Docker containers (full reset)
    .\stop-local.ps1 -WhatIf             # Preview without making changes
#>
param (
    [string] $ResourceGroup = 'animstudio-dev-rg',
    [string] $Environment   = 'dev',

    # Also downgrade SignalR back to Free_F1 (reverses FullStack upgrade)
    [switch] $FullStack,

    # Remove containers entirely instead of just stopping them
    [switch] $Remove,

    [switch] $WhatIf
)

$ErrorActionPreference = 'Stop'

$signalRName = "animstudio-$Environment-signalr"

function Write-Section($title) { Write-Host ""; Write-Host "[ $title ]" -ForegroundColor White }
function Write-OK($msg)        { Write-Host "  [OK  ] $msg" -ForegroundColor Green }
function Write-Skip($msg)      { Write-Host "  [SKIP] $msg" -ForegroundColor Yellow }
function Write-Action($msg) {
    if ($WhatIf) { Write-Host "  [WHAT] $msg" -ForegroundColor DarkCyan }
    else         { Write-Host "  [  >> ] $msg" -ForegroundColor Cyan }
}

Write-Host ""
Write-Host "AnimStudio — STOP LOCAL" -ForegroundColor Red
Write-Host "  Mode: $(if ($Remove) { 'Remove containers' } else { 'Stop containers (data preserved)' })" -ForegroundColor DarkGray
if ($WhatIf) { Write-Host "  ** WhatIf — no changes will be made **" -ForegroundColor Yellow }

# ── Docker: SQL Server ─────────────────────────────────────────────────────────

Write-Section "SQL Server (Docker)"

$sqlRunning = docker ps --filter "name=animstudio-sql" --format "{{.Names}}" 2>$null

if ($sqlRunning -eq 'animstudio-sql') {
    if ($Remove) {
        Write-Action "Stopping and removing animstudio-sql"
        if (-not $WhatIf) {
            docker stop animstudio-sql | Out-Null
            docker rm   animstudio-sql | Out-Null
            Write-OK "animstudio-sql stopped and removed"
        }
    } else {
        Write-Action "Stopping animstudio-sql (data preserved in container)"
        if (-not $WhatIf) {
            docker stop animstudio-sql | Out-Null
            Write-OK "animstudio-sql stopped"
        }
    }
} else {
    $sqlExists = docker ps -a --filter "name=animstudio-sql" --format "{{.Names}}" 2>$null
    if ($Remove -and $sqlExists -eq 'animstudio-sql') {
        Write-Action "Removing stopped animstudio-sql container"
        if (-not $WhatIf) { docker rm animstudio-sql | Out-Null; Write-OK "animstudio-sql removed" }
    } else {
        Write-Skip "animstudio-sql is not running"
    }
}

# ── Docker: Redis ──────────────────────────────────────────────────────────────

Write-Section "Redis (Docker)"

$redisRunning = docker ps --filter "name=animstudio-redis" --format "{{.Names}}" 2>$null

if ($redisRunning -eq 'animstudio-redis') {
    if ($Remove) {
        Write-Action "Stopping and removing animstudio-redis"
        if (-not $WhatIf) {
            docker stop animstudio-redis | Out-Null
            docker rm   animstudio-redis | Out-Null
            Write-OK "animstudio-redis stopped and removed"
        }
    } else {
        Write-Action "Stopping animstudio-redis"
        if (-not $WhatIf) {
            docker stop animstudio-redis | Out-Null
            Write-OK "animstudio-redis stopped"
        }
    }
} else {
    $redisExists = docker ps -a --filter "name=animstudio-redis" --format "{{.Names}}" 2>$null
    if ($Remove -and $redisExists -eq 'animstudio-redis') {
        Write-Action "Removing stopped animstudio-redis container"
        if (-not $WhatIf) { docker rm animstudio-redis | Out-Null; Write-OK "animstudio-redis removed" }
    } else {
        Write-Skip "animstudio-redis is not running"
    }
}

# ── SignalR — downgrade to Free (FullStack mode) ───────────────────────────────

if ($FullStack) {
    Write-Section "SignalR (Azure)"

    $account = az account show --query user.name -o tsv 2>$null
    if ($LASTEXITCODE -ne 0) { Write-Host "  [SKIP] Not logged in to Azure — skipping SignalR downgrade" -ForegroundColor Yellow }
    else {
        $currentSku = az signalr show -g $ResourceGroup -n $signalRName `
                        --query 'sku.name' -o tsv 2>$null

        if ($currentSku -eq 'Free_F1') {
            Write-Skip "$signalRName — already on Free_F1"
        } else {
            Write-Action "$signalRName : $currentSku → Free_F1  (saves ~`$49/month)"
            if (-not $WhatIf) {
                az signalr update -g $ResourceGroup -n $signalRName `
                    --sku Free_F1 --unit-count 1 --output none
                Write-OK "$signalRName downgraded to Free_F1"
            }
        }
    }
}

# ── Summary ────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── Local dev stopped ────────────────────────────────────────────────" -ForegroundColor Cyan
if (-not $Remove) {
    Write-Host "  Database data is preserved in Docker volumes." -ForegroundColor DarkGray
    Write-Host "  Run .\start-local.ps1 to bring everything back up." -ForegroundColor DarkGray
    Write-Host "  Run .\stop-local.ps1 -Remove to wipe containers and start fresh." -ForegroundColor DarkGray
} else {
    Write-Host "  Containers removed. Next start-local.ps1 will recreate them from scratch." -ForegroundColor DarkGray
    Write-Host "  Note: All local database data has been deleted." -ForegroundColor Yellow
}
Write-Host ""
