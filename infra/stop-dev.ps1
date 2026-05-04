#Requires -Version 7.1
<#
.SYNOPSIS
    Stops AnimStudio dev resources to minimise cost when not in use.

.DESCRIPTION
    Quick mode (default): scales the API Container App to 0 replicas.
    Full mode  (-Full)  : also downgrades SignalR to Free SKU and disables the
                          Front Door CDN endpoint, saving an extra ~$85/month.

    Use start-dev.ps1 to bring everything back up.

.EXAMPLE
    .\stop-dev.ps1                    # quick stop — Container Apps to 0
    .\stop-dev.ps1 -Full              # deep stop — + SignalR Free + CDN off
    .\stop-dev.ps1 -WhatIf            # preview without making changes
    .\stop-dev.ps1 -ResourceGroup animstudio-prod-rg -Environment prod
#>
param (
    [string] $ResourceGroup = 'animstudio-dev-rg',
    [string] $Environment   = 'dev',

    # Also downgrade SignalR to Free SKU and disable the Front Door endpoint
    [switch] $Full,

    # Preview mode — print what would happen, make no changes
    [switch] $WhatIf
)

$ErrorActionPreference = 'Stop'

$apiApp      = "animstudio-$Environment-api"
$workerApp   = "animstudio-$Environment-worker"
$signalRName = "animstudio-$Environment-signalr"
$cdnProfile  = "animstudio-$Environment-cdn"
$cdnEndpoint = "animstudio-$Environment-assets"

function Write-Action($msg) {
    if ($WhatIf) { Write-Host "[WHATIF] $msg" -ForegroundColor DarkCyan }
    else         { Write-Host "  >>  $msg"    -ForegroundColor Cyan }
}

# ── Preflight ──────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "AnimStudio Dev — STOP" -ForegroundColor Red
Write-Host "Resource group : $ResourceGroup" -ForegroundColor DarkGray
Write-Host "Mode           : $(if ($Full) { 'Full (apps + SignalR + CDN)' } else { 'Quick (apps only)' })" -ForegroundColor DarkGray
if ($WhatIf) { Write-Host "** WhatIf — no changes will be made **" -ForegroundColor Yellow }
Write-Host ""

$account = az account show --query user.name -o tsv 2>$null
if ($LASTEXITCODE -ne 0) { Write-Error "Not logged in. Run: az login" }

# ── 1. API Container App → 0 replicas ─────────────────────────────────────────

Write-Host "[ Container Apps ]" -ForegroundColor White

$apiMin = az containerapp show -g $ResourceGroup -n $apiApp `
            --query 'properties.template.scale.minReplicas' -o tsv 2>$null

if ($apiMin -eq '0') {
    Write-Host "  [SKIP] $apiApp — already at 0 replicas" -ForegroundColor Yellow
} else {
    Write-Action "$apiApp : min-replicas $apiMin → 0  (scales to 0 when idle, wakes on HTTP)"
    if (-not $WhatIf) {
        az containerapp update -g $ResourceGroup -n $apiApp `
            --min-replicas 0 --max-replicas 3 --output none
        Write-Host "  [OK  ] $apiApp scaled to 0" -ForegroundColor Green
    }
}

# Worker is KEDA-driven (already 0 when queues empty) — just report status
$workerReplicas = az containerapp show -g $ResourceGroup -n $workerApp `
                    --query 'properties.template.scale.minReplicas' -o tsv 2>$null
Write-Host "  [INFO] $workerApp — min replicas = $workerReplicas (KEDA-driven, already idles to 0)" -ForegroundColor DarkGray

# ── 2. SignalR — downgrade to Free (saves ~$49/month) ─────────────────────────

if ($Full) {
    Write-Host ""
    Write-Host "[ SignalR ]" -ForegroundColor White

    $currentSku = az signalr show -g $ResourceGroup -n $signalRName `
                    --query 'sku.name' -o tsv 2>$null

    if ($currentSku -eq 'Free_F1') {
        Write-Host "  [SKIP] $signalRName — already on Free SKU" -ForegroundColor Yellow
    } else {
        Write-Action "$signalRName : SKU $currentSku → Free_F1  (limit: 20 concurrent connections)"
        if (-not $WhatIf) {
            az signalr update -g $ResourceGroup -n $signalRName `
                --sku Free_F1 --unit-count 1 --output none
            Write-Host "  [OK  ] $signalRName downgraded to Free_F1" -ForegroundColor Green
        }
    }

    # ── 3. Front Door CDN endpoint — disable (stops traffic charges) ───────────

    Write-Host ""
    Write-Host "[ Front Door CDN ]" -ForegroundColor White

    $endpointState = az afd endpoint show -g $ResourceGroup `
                        --profile-name $cdnProfile --endpoint-name $cdnEndpoint `
                        --query 'enabledState' -o tsv 2>$null

    if ($endpointState -eq 'Disabled') {
        Write-Host "  [SKIP] $cdnEndpoint — already disabled" -ForegroundColor Yellow
    } else {
        Write-Action "$cdnEndpoint : Enabled → Disabled  (stops per-GB charges; $35/month base still applies)"
        if (-not $WhatIf) {
            az afd endpoint update -g $ResourceGroup `
                --profile-name $cdnProfile --endpoint-name $cdnEndpoint `
                --enabled-state Disabled --output none
            Write-Host "  [OK  ] $cdnEndpoint disabled" -ForegroundColor Green
        }
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── Estimated savings ────────────────────────────────────────────" -ForegroundColor Cyan

if ($Full) {
    Write-Host "  Container App compute  ~$55–70/month" -ForegroundColor Green
    Write-Host "  SignalR Standard_S1    ~$49/month"    -ForegroundColor Green
    Write-Host "  CDN traffic charges    variable"       -ForegroundColor Green
    Write-Host ""
    Write-Host "  Still running (fixed costs):"          -ForegroundColor DarkGray
    Write-Host "    Service Bus Standard  ~$10/month"    -ForegroundColor DarkGray
    Write-Host "    SQL Basic ×2          ~$10/month"    -ForegroundColor DarkGray
    Write-Host "    ACR Basic             ~$5/month"     -ForegroundColor DarkGray
    Write-Host "    Front Door base       ~$35/month"    -ForegroundColor DarkGray
    Write-Host "    Storage / KV / LAW    ~$3/month"     -ForegroundColor DarkGray
    Write-Host "  ─────────────────────────────────────────────────────────"  -ForegroundColor DarkGray
    Write-Host "  Idle cost               ~$63/month"    -ForegroundColor Yellow
} else {
    Write-Host "  Container App compute  ~$55–70/month" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Tip: run with -Full to also save ~$49/month on SignalR" -ForegroundColor DarkYellow
    Write-Host "  Idle cost (quick stop)  ~$112/month"  -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Run .\start-dev.ps1 to bring everything back up." -ForegroundColor Cyan
Write-Host ""
