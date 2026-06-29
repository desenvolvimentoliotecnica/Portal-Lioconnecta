param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "",
    [string]$ApiProjectPath = "",
    [string]$FrontendRootPath = ""
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..")).Path

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts\deploy\lioconnecta-package"
}

if ([string]::IsNullOrWhiteSpace($ApiProjectPath)) {
    $ApiProjectPath = Join-Path $repoRoot "src\PortalLioConnecta.Api\PortalLioConnecta.Api.csproj"
}

if ([string]::IsNullOrWhiteSpace($FrontendRootPath)) {
    $FrontendRootPath = Join-Path $repoRoot "LioConnecta"
}

$apiPublishDirectory = Join-Path $OutputRoot "api"
$frontendPackageDirectory = Join-Path $OutputRoot "frontend"

if (Test-Path $OutputRoot) {
    Remove-Item -Path $OutputRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $apiPublishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $frontendPackageDirectory -Force | Out-Null

Write-Host ""
Write-Host "==> Publicando API PortalLioConnecta.Api ($Configuration)"
dotnet publish $ApiProjectPath -c $Configuration -o $apiPublishDirectory --nologo

$frontendItems = @(
    "admin",
    "assets",
    "docs",
    "local-api",
    "login",
    "index.html",
    "manifest.webmanifest",
    "package.json",
    "service-worker.js",
    "dev-static-server.js"
)

Write-Host ""
Write-Host "==> Empacotando frontend LioConnecta"
foreach ($item in $frontendItems) {
    $sourcePath = Join-Path $FrontendRootPath $item
    if (-not (Test-Path $sourcePath)) {
        throw "Item esperado do frontend não encontrado: $sourcePath"
    }

    $destinationPath = Join-Path $frontendPackageDirectory $item
    Copy-Item -Path $sourcePath -Destination $destinationPath -Recurse -Force
}

$commitHash = ""
try {
    $commitHash = (git -C $repoRoot rev-parse HEAD).Trim()
}
catch {
    $commitHash = $env:GITHUB_SHA
}

$branchName = if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_REF_NAME)) {
    $env:GITHUB_REF_NAME
}
else {
    try {
        (git -C $repoRoot branch --show-current).Trim()
    }
    catch {
        "desconhecida"
    }
}

$manifest = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    branch         = $branchName
    commit         = $commitHash
    configuration  = $Configuration
    package        = @{
        api      = "api"
        frontend = "frontend"
    }
}

$manifestPath = Join-Path $OutputRoot "deploy-manifest.json"
$manifest | ConvertTo-Json -Depth 5 | Set-Content -Path $manifestPath -Encoding UTF8

Write-Host ""
Write-Host "Pacote gerado em: $OutputRoot"
Write-Host "Manifesto: $manifestPath"
