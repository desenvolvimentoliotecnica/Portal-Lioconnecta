#Requires -Version 5.1
<#
.SYNOPSIS
  Cria environments e secrets do deploy LIOCONNECTA no GitHub.

.DESCRIPTION
  Requer GitHub CLI com permissao de admin no repo (secrets + environments).
  Autenticacao (escolha uma):
    1) gh auth login --web --scopes repo,admin:repo_hook
    2) $env:GH_TOKEN = '<PAT com repo + secrets>'

  Repo alvo: desenvolvimentoliotecnica/Portal-Lioconnecta

.EXAMPLE
  .\scripts\setup-lioconnecta-github-secrets.ps1 -LoginWeb
.EXAMPLE
  .\scripts\setup-lioconnecta-github-secrets.ps1 -Environment dev -SshKeyPath $env:USERPROFILE\.ssh\lioconnecta_deploy
.EXAMPLE
  .\scripts\setup-lioconnecta-github-secrets.ps1 -Environment all -SshKeyPath $env:USERPROFILE\.ssh\lioconnecta_deploy
#>
[CmdletBinding()]
param(
    [ValidateSet("dev", "hml", "prd", "all")]
    [string]$Environment,

    [string]$SshKeyPath = "$env:USERPROFILE\.ssh\lioconnecta_deploy",

    [string]$Repo = "desenvolvimentoliotecnica/Portal-Lioconnecta",

    [switch]$SkipKnownHosts,

    [switch]$LoginWeb,

    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

function Test-GhAuth {
    if ($env:GH_TOKEN) {
        Write-Host "Usando GH_TOKEN do ambiente."
        return
    }

    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "SilentlyContinue"
    & gh auth status 2>$null | Out-Null
    $authenticated = $LASTEXITCODE -eq 0
    $ErrorActionPreference = $previousErrorAction

    if (-not $authenticated) {
        throw @"
GitHub CLI nao autenticado.

Rode (use o .cmd, nao o .ps1):
  cd D:\Leonardo\PortalLioConnecta-2.0
  .\setup-github-secrets.cmd -LoginWeb
"@
    }
}

function Start-GhWebLogin {
    Write-Host "Abrindo login do GitHub no navegador..." -ForegroundColor Yellow
    gh auth login --web --scopes repo,admin:repo_hook
    if ($LASTEXITCODE -ne 0) {
        throw "Login falhou. Tente: gh auth login --web"
    }
    Write-Host "Login OK." -ForegroundColor Green
}

function Show-GhEnvSecrets {
    param([string]$EnvName)
    if ($WhatIf) { return }
    Write-Host "Secrets em $EnvName :"
    gh secret list --env $EnvName --repo $Repo
}

function Ensure-GhEnvironment {
    param([string]$EnvName)
    if ($WhatIf) {
        Write-Host "[WhatIf] Criar environment: $EnvName"
        return
    }
    gh api --method PUT "/repos/$Repo/environments/$EnvName" | Out-Null
    Write-Host "Environment OK: $EnvName"
}

function Set-GhEnvSecret {
    param(
        [string]$EnvName,
        [string]$Name,
        [string]$Value
    )
    if ($WhatIf) {
        Write-Host "[WhatIf] $EnvName :: $Name = $(if ($Name -match 'KEY|PASSWORD') { '***' } else { $Value })"
        return
    }
    $Value | gh secret set $Name --env $EnvName --repo $Repo
    Write-Host "Secret OK: $EnvName / $Name"
}

function Set-GhEnvSecretFromFile {
    param(
        [string]$EnvName,
        [string]$Name,
        [string]$FilePath
    )
    if ($WhatIf) {
        Write-Host "[WhatIf] $EnvName :: $Name <= $FilePath"
        return
    }
    Get-Content -LiteralPath $FilePath -Raw | gh secret set $Name --env $EnvName --repo $Repo
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao gravar secret $Name a partir de $FilePath"
    }
    Write-Host "Secret OK: $EnvName / $Name (arquivo)"
}

function Get-KnownHostsForHost {
    param([string]$HostAddress)
    if ($SkipKnownHosts) { return $null }
    try {
        $lines = ssh-keyscan -H $HostAddress 2>$null
        if (-not $lines) { return $null }
        return ($lines -join "`n")
    }
    catch {
        Write-Warning "ssh-keyscan falhou para $HostAddress. Configure LIOCONNECTA_DEPLOY_KNOWN_HOSTS manualmente."
        return $null
    }
}

$profiles = @{
    dev = @{
        GhEnvironment  = "lioconnecta-dev"
        Host           = "10.0.0.79"
        DeployPath     = "/home/administrator/lioconnecta/dev"
        ApiService     = "lioconnecta-api-dev"
        FrontendPath   = "/home/administrator/lioconnecta-dev"
        PostCommand    = "rsync -a --delete /home/administrator/lioconnecta/dev/current/frontend/ /home/administrator/lioconnecta-dev/ && sudo systemctl restart lioconnecta-frontend-dev"
        Note           = "DEV usa python http.server em lioconnecta-dev (nao nginx)."
    }
    hml = @{
        GhEnvironment = "lioconnecta-hml"
        Host          = "10.0.0.80"
        DeployPath    = "/home/administrator/lioconnecta/hml"
        ApiService    = "lioconnecta-api-hml"
        FrontendPath  = "/var/www/lioconnecta-hml"
        Note          = "Valores de LioConnecta/docs/hml-server-provisioning.md"
    }
    prd = @{
        GhEnvironment = "lioconnecta-prd"
        Host          = "10.0.0.88"
        DeployPath    = "/home/administrator/lioconnecta/prd"
        ApiService    = "lioconnecta-api-prd"
        FrontendPath  = "/var/www/lioconnecta-prd"
        Note          = "Valores de LioConnecta/docs/prd-server-provisioning.md"
    }
}

$targets = if ($Environment -eq "all") { @("dev", "hml", "prd") } else { @($Environment) }

if ($LoginWeb) {
    Start-GhWebLogin
    if (-not $Environment) {
        Write-Host "Login concluido. Proximo comando:"
        Write-Host "  .\scripts\setup-lioconnecta-github-secrets.ps1 -Environment dev" -ForegroundColor Cyan
        return
    }
}

if (-not $Environment) {
    throw "Informe -Environment dev|hml|prd|all (ou use -LoginWeb so para autenticar)."
}

if (-not (Test-Path -LiteralPath $SshKeyPath)) {
    throw "Chave privada nao encontrada: $SshKeyPath`nGere com: ssh-keygen -t ed25519 -f `"$SshKeyPath`" -N '""'"
}

Test-GhAuth

Write-Host ""
Write-Host "Repo: $Repo"
Write-Host "Chave SSH: $SshKeyPath"
Write-Host ""

foreach ($key in $targets) {
    $p = $profiles[$key]
    Write-Host "=== $($p.GhEnvironment) ($key) ===" -ForegroundColor Cyan
    Write-Host $p.Note

    Ensure-GhEnvironment -EnvName $p.GhEnvironment

    Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_HOST" -Value $p.Host
    Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_PORT" -Value "22"
    Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_USER" -Value "administrator"
    Set-GhEnvSecretFromFile -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_SSH_KEY" -FilePath $SshKeyPath
    Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_PATH" -Value $p.DeployPath
    Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_API_SERVICE" -Value $p.ApiService

    $postCommand = if ($p.PostCommand) {
        $p.PostCommand
    } else {
        "sudo rsync -a --delete $($p.DeployPath)/current/frontend/ $($p.FrontendPath)/ && sudo nginx -s reload"
    }
    Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_POST_COMMAND" -Value $postCommand

    $knownHosts = Get-KnownHostsForHost -HostAddress $p.Host
    if ($knownHosts) {
        Set-GhEnvSecret -EnvName $p.GhEnvironment -Name "LIOCONNECTA_DEPLOY_KNOWN_HOSTS" -Value $knownHosts
    }

    Show-GhEnvSecrets -EnvName $p.GhEnvironment
    Write-Host ""
}

Write-Host "Concluido." -ForegroundColor Green
Write-Host "Proximo passo: push em Lioconnecta_DEV / Lioconnecta_HML / Lioconnecta_PRD e acompanhar Actions."
Write-Host "PRD: considere adicionar 'Required reviewers' em Settings > Environments > lioconnecta-prd."
