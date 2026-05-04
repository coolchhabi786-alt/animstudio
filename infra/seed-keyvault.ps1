#Requires -Version 7.1
<#
.SYNOPSIS
    Interactively seeds secrets into AnimStudio Key Vault.

.DESCRIPTION
    Prompts for each required secret. Skips secrets that already have a real
    (non-placeholder) value. Use -DryRun to preview what would be set.

.EXAMPLE
    .\seed-keyvault.ps1 -Environment dev
    .\seed-keyvault.ps1 -Environment prod -DryRun
    .\seed-keyvault.ps1 -Environment dev -KeyVaultName my-override-kv
#>
param (
    [Parameter(Mandatory)]
    [ValidateSet('dev', 'prod')]
    [string] $Environment,

    [string] $KeyVaultName,

    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'

$kvName = $KeyVaultName ? $KeyVaultName : "animstudio-$Environment-kv"

$secrets = @(
    'SqlConnectionString'
    'HangfireSqlConnectionString'
    'StripeSecretKey'
    'StripeWebhookSecret'
    'ServiceBusConnectionString'
    'SignalRConnectionString'
    'AzureOpenAIKey'
    'AzureOpenAIEndpoint'
    'FalApiKey'
    'ElevenLabsApiKey'
    'AzureCommunicationServicesConnectionString'
)

# ── Preflight ──────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Key Vault : $kvName" -ForegroundColor Cyan
Write-Host "Mode      : $(if ($DryRun) { 'DRY RUN (no changes)' } else { 'LIVE' })" -ForegroundColor Cyan
Write-Host ""

# Verify az login
$account = az account show --query user.name -o tsv 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in. Run: az login"
}
Write-Host "Logged in as: $account" -ForegroundColor DarkGray
Write-Host ""

# Verify Key Vault is accessible
az keyvault show --name $kvName --query id -o tsv | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Cannot access Key Vault '$kvName'. Ensure it exists and you have Key Vault Secrets Officer role."
}

# ── Per-secret loop ────────────────────────────────────────────────────────────

$results = [ordered]@{}

foreach ($secretName in $secrets) {
    # Check current value
    $existing = az keyvault secret show --vault-name $kvName --name $secretName --query value -o tsv 2>$null
    $hasRealValue = ($LASTEXITCODE -eq 0) -and ($existing -notlike 'PLACEHOLDER_*')

    if ($hasRealValue) {
        Write-Host "[SKIP] $secretName — already set" -ForegroundColor Yellow
        $results[$secretName] = 'skipped'
        continue
    }

    if ($DryRun) {
        $status = if ($LASTEXITCODE -eq 0) { 'has placeholder (would prompt)' } else { 'missing (would prompt)' }
        Write-Host "[DRY ] $secretName — $status" -ForegroundColor DarkCyan
        $results[$secretName] = 'dry-run'
        continue
    }

    # Prompt — input is masked, value is NOT printed
    $value = Read-Host -MaskInput "Enter $secretName (blank to skip)"

    if ([string]::IsNullOrWhiteSpace($value)) {
        Write-Host "       $secretName — left blank, skipping" -ForegroundColor DarkGray
        $results[$secretName] = 'blank'
        continue
    }

    az keyvault secret set --vault-name $kvName --name $secretName --value $value --output none
    Write-Host "[SET ] $secretName" -ForegroundColor Green
    $results[$secretName] = 'set'
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── Summary ──────────────────────────────────────────────────────" -ForegroundColor Cyan
$results.GetEnumerator() | Group-Object Value | ForEach-Object {
    $colour = switch ($_.Name) {
        'set'     { 'Green' }
        'skipped' { 'Yellow' }
        'blank'   { 'DarkGray' }
        'dry-run' { 'DarkCyan' }
    }
    Write-Host ("  {0,-8} {1}" -f $_.Name.ToUpper(), ($_.Group.Name -join ', ')) -ForegroundColor $colour
}
Write-Host ""

if (-not $DryRun -and ($results.Values -contains 'blank')) {
    Write-Host "Secrets left blank will use PLACEHOLDER values until you re-run this script." -ForegroundColor DarkYellow
}
