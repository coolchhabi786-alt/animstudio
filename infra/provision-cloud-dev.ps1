#Requires -Version 7.1
<#
.SYNOPSIS
    Re-provisions the full AnimStudio dev environment after a teardown-cloud-dev.ps1 run.

.DESCRIPTION
    Orchestrates three steps in sequence:
      1. Full Bicep deployment (deploy.ps1) — recreates all deleted resources
      2. Key Vault secret seeding (seed-keyvault-dev.ps1) — re-populates KV from .env.secrets
      3. Local connectivity check (start-local.ps1 -FullStack) — verifies SB + Storage
         and prints the startup commands for each project

    Use teardown-cloud-dev.ps1 to delete the expensive resources and save ~$128/month.

.EXAMPLE
    .\provision-cloud-dev.ps1
    .\provision-cloud-dev.ps1 -SkipSeed          # Skip KV seeding (secrets already present)
    .\provision-cloud-dev.ps1 -SkipLocalCheck    # Skip start-local.ps1 step
#>
param (
    [string] $ResourceGroup = 'animstudio-dev-rg',
    [string] $Environment   = 'dev',
    [string] $Location      = 'eastus',

    # Skip Key Vault secret seeding (use if secrets are still in KV from a previous run)
    [switch] $SkipSeed,

    # Skip the start-local.ps1 -FullStack connectivity check at the end
    [switch] $SkipLocalCheck
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Write-Step($n, $msg) {
    Write-Host ""
    Write-Host "━━━ Step $n — $msg" -ForegroundColor Cyan
    Write-Host ""
}

# ── Banner ────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "AnimStudio Dev — PROVISION FULL ENVIRONMENT" -ForegroundColor Green
Write-Host "  Resource group : $ResourceGroup" -ForegroundColor DarkGray
Write-Host "  Environment    : $Environment" -ForegroundColor DarkGray
Write-Host "  Location       : $Location" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  This will create:" -ForegroundColor White
Write-Host "    Container Apps (API + Worker), Container App Environment, VNet," -ForegroundColor DarkGray
Write-Host "    Log Analytics, Static Web App, ACR, CDN, SignalR, SQL, Key Vault" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Estimated time: 10–15 minutes" -ForegroundColor DarkGray
Write-Host ""

# ── Preflight: Azure login ────────────────────────────────────────────────────

$account = az account show --query user.name -o tsv 2>$null
if ($LASTEXITCODE -ne 0) { Write-Error "Not logged in. Run: az login" }
Write-Host "  Logged in as: $account" -ForegroundColor DarkGray

# ── Step 1: Full Bicep deploy ─────────────────────────────────────────────────

Write-Step 1 "Bicep deployment (deploy.ps1)"

& "$scriptDir\deploy.ps1" `
    -Environment   $Environment `
    -ResourceGroup $ResourceGroup `
    -Location      $Location

if ($LASTEXITCODE -ne 0) {
    Write-Error "deploy.ps1 failed — see errors above. Aborting provision."
}

Write-Host ""
Write-Host "  [OK  ] Bicep deployment complete" -ForegroundColor Green

# ── Step 2: Key Vault seeding ─────────────────────────────────────────────────

Write-Step 2 "Key Vault secret seeding (seed-keyvault-dev.ps1)"

if ($SkipSeed) {
    Write-Host "  [SKIP] -SkipSeed flag set — skipping Key Vault seeding" -ForegroundColor Yellow
    Write-Host "         If secrets are missing, run: .\seed-keyvault-dev.ps1" -ForegroundColor DarkGray
} else {
    $seedScript = "$scriptDir\seed-keyvault-dev.ps1"
    if (Test-Path $seedScript) {
        & $seedScript -Environment $Environment -ResourceGroup $ResourceGroup
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  [WARN] seed-keyvault-dev.ps1 returned errors — secrets may be incomplete." -ForegroundColor Yellow
            Write-Host "         Check $scriptDir\.env.secrets exists and is populated." -ForegroundColor DarkGray
        } else {
            Write-Host "  [OK  ] Key Vault secrets seeded" -ForegroundColor Green
        }
    } else {
        Write-Host "  [WARN] seed-keyvault-dev.ps1 not found at $seedScript" -ForegroundColor Yellow
        Write-Host "         Seed manually: .\seed-keyvault-dev.ps1 -Environment $Environment" -ForegroundColor DarkGray
    }
}

# ── Step 3: Local connectivity check ─────────────────────────────────────────

Write-Step 3 "Local connectivity check + startup commands (start-local.ps1 -FullStack)"

if ($SkipLocalCheck) {
    Write-Host "  [SKIP] -SkipLocalCheck flag set" -ForegroundColor Yellow
} else {
    & "$scriptDir\start-local.ps1" `
        -Environment   $Environment `
        -ResourceGroup $ResourceGroup `
        -FullStack

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [WARN] start-local.ps1 returned errors — check SB/Storage connectivity" -ForegroundColor Yellow
    }
}

# ── Final summary ─────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "  Full dev environment is provisioned." -ForegroundColor Green
Write-Host ""
Write-Host "  Switch to full cloud mode in your .NET app:" -ForegroundColor White
Write-Host '    appsettings.Development.json → "AnimStudio": { "LocalDevMode": false }' -ForegroundColor DarkYellow
Write-Host '    appsettings.Development.json → FileStorage:Provider = "AzureBlob"' -ForegroundColor DarkYellow
Write-Host ""
Write-Host "  When done developing, save costs with:" -ForegroundColor White
Write-Host "    .\stop-dev.ps1 -Full       # scale to 0 + downgrade SignalR" -ForegroundColor DarkGray
Write-Host "    .\teardown-cloud-dev.ps1   # full teardown to minimal mode" -ForegroundColor DarkGray
Write-Host ""
