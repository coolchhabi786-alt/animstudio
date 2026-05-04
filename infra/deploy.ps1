#Requires -Version 7
<#
.SYNOPSIS
    Deploys AnimStudio infrastructure to Azure via Bicep.

.PARAMETER Environment
    Target environment: dev or prod.

.PARAMETER ResourceGroup
    Azure resource group name (created if it does not exist).

.PARAMETER Location
    Azure region for the resource group. Defaults to eastus.

.EXAMPLE
    # Dry-run only (what-if — no changes):
    .\deploy.ps1 -Environment dev -ResourceGroup animstudio-dev-rg -WhatIfOnly

    # Full deployment:
    .\deploy.ps1 -Environment dev  -ResourceGroup animstudio-dev-rg
    .\deploy.ps1 -Environment prod -ResourceGroup animstudio-prod-rg
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('dev', 'prod')]
    [string]$Environment,

    [Parameter(Mandatory)]
    [string]$ResourceGroup,

    [string]$Location = 'eastus',

    # Pass -WhatIfOnly to run the what-if dry-run without proceeding to deploy.
    [switch]$WhatIfOnly
)

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$mainBicep = Join-Path $scriptDir 'main.bicep'
$paramFile = Join-Path $scriptDir "parameters/$Environment.json"

if (-not (Test-Path $mainBicep))  { throw "main.bicep not found at: $mainBicep" }
if (-not (Test-Path $paramFile))  { throw "Parameter file not found at: $paramFile" }

Write-Host ''
Write-Host '=== AnimStudio Deployment ===' -ForegroundColor Cyan
Write-Host "  Environment    : $Environment"
Write-Host "  Resource Group : $ResourceGroup"
Write-Host "  Location       : $Location"
Write-Host "  Parameter file : $paramFile"
Write-Host ''

# ─── 1. Verify Azure login ────────────────────────────────────────────────────
Write-Host '[1/4] Checking Azure login...' -ForegroundColor Yellow
az account show --output none 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host '      Not logged in — launching az login...' -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) { throw 'az login failed. Aborting.' }
}
$account = az account show --output json | ConvertFrom-Json
Write-Host "      Subscription: $($account.name) ($($account.id))"

# ─── 2. Ensure resource group exists ─────────────────────────────────────────
Write-Host "[2/4] Ensuring resource group '$ResourceGroup' exists in $Location..." -ForegroundColor Yellow
$rgExists = az group exists --name $ResourceGroup
if ($rgExists -eq 'false') {
    Write-Host '      Creating resource group...'
    az group create --name $ResourceGroup --location $Location --output none
    if ($LASTEXITCODE -ne 0) { throw "Failed to create resource group '$ResourceGroup'." }
    Write-Host '      Created.'
} else {
    Write-Host '      Already exists.'
}

# ─── 3. What-if (dry run) ────────────────────────────────────────────────────
Write-Host '[3/4] Running what-if (dry run)...' -ForegroundColor Yellow
az deployment group what-if `
    --resource-group $ResourceGroup `
    --template-file  $mainBicep `
    --parameters     "@$paramFile"

if ($LASTEXITCODE -ne 0) { throw 'What-if validation failed. Fix errors above before deploying.' }

if ($WhatIfOnly) {
    Write-Host ''
    Write-Host 'WhatIfOnly flag set — stopping before real deployment.' -ForegroundColor Cyan
    exit 0
}

Write-Host ''
$confirm = Read-Host 'Proceed with actual deployment? Type "yes" to continue'
if ($confirm -ne 'yes') {
    Write-Host 'Deployment cancelled.' -ForegroundColor Red
    exit 0
}

# ─── 4. Deploy ───────────────────────────────────────────────────────────────
Write-Host "[4/4] Deploying to '$ResourceGroup'..." -ForegroundColor Yellow
$deployJson = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file  $mainBicep `
    --parameters     "@$paramFile" `
    --output json

if ($LASTEXITCODE -ne 0) { throw 'Deployment failed. See errors above.' }

$deploy  = $deployJson | ConvertFrom-Json
$outputs = $deploy.properties.outputs

Write-Host ''
Write-Host '=== Deployment Complete ===' -ForegroundColor Green
Write-Host "  API URL  : $($outputs.apiUrl.value)"
Write-Host "  SWA URL  : $($outputs.swaUrl.value)"
Write-Host "  SWA Name : $($outputs.swaName.value)"
if ($outputs.cdnUrl -and $outputs.cdnUrl.value) {
    Write-Host "  CDN URL  : $($outputs.cdnUrl.value)"
}
Write-Host ''
Write-Host 'Next steps after first deploy:'
Write-Host "  1. Retrieve SWA deploy token:  az staticwebapp secrets list --name $($outputs.swaName.value) --query 'properties.apiKey' -o tsv"
Write-Host '  2. Add the token to GitHub Secrets as AZURE_STATIC_WEB_APPS_API_TOKEN'
Write-Host '  3. Seed Key Vault secrets:     .\seed-keyvault.ps1 -Environment $Environment -ResourceGroup $ResourceGroup'
