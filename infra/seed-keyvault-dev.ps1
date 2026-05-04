#Requires -Version 7.1
<#
.SYNOPSIS
    Non-interactively seeds Key Vault from a local .env.secrets file.

.DESCRIPTION
    Reads SECRET_NAME=value pairs from infra/.env.secrets and uploads them to
    Key Vault. Used for local dev setup and CI pipelines. Skips secrets that
    already have a real (non-placeholder) value unless -Force is passed.

.EXAMPLE
    .\seed-keyvault-dev.ps1 -Environment dev
    .\seed-keyvault-dev.ps1 -Environment dev -Force
    .\seed-keyvault-dev.ps1 -Environment dev -SecretsFile C:\path\to\other.env
#>
param (
    [Parameter(Mandatory)]
    [ValidateSet('dev', 'prod')]
    [string] $Environment,

    [string] $KeyVaultName,

    [string] $SecretsFile = (Join-Path $PSScriptRoot '.env.secrets'),

    # Overwrite secrets that already have a real value
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

$kvName = $KeyVaultName ? $KeyVaultName : "animstudio-$Environment-kv"

# ── Preflight ──────────────────────────────────────────────────────────────────

if (-not (Test-Path $SecretsFile)) {
    Write-Error "Secrets file not found: $SecretsFile`nCopy .env.secrets.template to .env.secrets and fill in values."
}

$account = az account show --query user.name -o tsv 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in. Run: az login"
}

az keyvault show --name $kvName --query id -o tsv | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Cannot access Key Vault '$kvName'. Ensure it exists and you have Key Vault Secrets Officer role."
}

Write-Host ""
Write-Host "Key Vault   : $kvName" -ForegroundColor Cyan
Write-Host "Secrets file: $SecretsFile" -ForegroundColor Cyan
Write-Host ""

# ── Parse .env.secrets ────────────────────────────────────────────────────────

$parsed = [ordered]@{}
Get-Content $SecretsFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -eq '' -or $line.StartsWith('#')) { return }
    $idx = $line.IndexOf('=')
    if ($idx -lt 1) { return }
    $key   = $line.Substring(0, $idx).Trim()
    $value = $line.Substring($idx + 1).Trim()
    $parsed[$key] = $value
}

if ($parsed.Count -eq 0) {
    Write-Error "No valid KEY=value pairs found in $SecretsFile"
}

Write-Host "Parsed $($parsed.Count) secret(s) from file." -ForegroundColor DarkGray
Write-Host ""

# ── Upload ────────────────────────────────────────────────────────────────────

$set = 0; $skipped = 0; $blank = 0

foreach ($secretName in $parsed.Keys) {
    $value = $parsed[$secretName]

    if ([string]::IsNullOrWhiteSpace($value)) {
        Write-Host "[BLANK] $secretName — no value in file, skipping" -ForegroundColor DarkGray
        $blank++
        continue
    }

    if (-not $Force) {
        $existing = az keyvault secret show --vault-name $kvName --name $secretName --query value -o tsv 2>$null
        if ($LASTEXITCODE -eq 0 -and $existing -notlike 'PLACEHOLDER_*') {
            Write-Host "[SKIP ] $secretName — already set (use -Force to overwrite)" -ForegroundColor Yellow
            $skipped++
            continue
        }
    }

    az keyvault secret set --vault-name $kvName --name $secretName --value $value --output none
    Write-Host "[SET  ] $secretName" -ForegroundColor Green
    $set++
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "── Done ─────────────────────────────────────────────────────────" -ForegroundColor Cyan
Write-Host "  Set    : $set" -ForegroundColor Green
Write-Host "  Skipped: $skipped" -ForegroundColor Yellow
Write-Host "  Blank  : $blank" -ForegroundColor DarkGray
Write-Host ""
