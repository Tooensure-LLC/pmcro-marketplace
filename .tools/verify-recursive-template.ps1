<#
.SYNOPSIS
    Proves the app template reproduces itself, by executing it - not by assertion.

.DESCRIPTION
    Recursion here means: generate an app from the template, and the GENERATED app is
    itself an installable template that can generate another app. Three things have to
    hold for that, and all three are easy to get wrong:

      1. .template.config must survive generation. The engine's default `exclude`
         strips it, so template.json overrides `exclude` to keep it.
      2. The child must not collide with the parent. identity/shortName carry the
         sourceName token, so substitution gives the child its own identity.
      3. The child must still be a valid template after substitution.

    This installs the root, generates Alpha, installs Alpha, generates Beta, and
    reports depth reached. It uninstalls both templates on the way out.
#>
[CmdletBinding()]
param(
    [string] $TemplateRoot = 'T:\pmcro-marketplace\ProjectName.App',
    [string] $Work = "$env:TEMP\pmcro-template-verify"
)

$ErrorActionPreference = 'Continue'

function Step($msg) { Write-Host "== $msg" }

if (Test-Path $Work) { Remove-Item $Work -Recurse -Force -ErrorAction SilentlyContinue }
New-Item -ItemType Directory -Force -Path $Work | Out-Null

$installed = @()
$depth = 0

try {
    Step "install root template from $TemplateRoot"
    dotnet new install $TemplateRoot --force 2>&1 | Select-Object -Last 4
    $installed += $TemplateRoot

    Step "generate Alpha from 'ProjectName-app'"
    dotnet new ProjectName-app -n Alpha -o "$Work\Alpha" 2>&1 | Select-Object -Last 4

    $alphaCfg = "$Work\Alpha\.template.config\template.json"
    if (-not (Test-Path $alphaCfg)) {
        Write-Host "RESULT depth=1 - generated app exists but carries NO .template.config"
        Write-Host "       the engine stripped it; recursion does not hold"
        return
    }

    $depth = 1
    $alpha = Get-Content $alphaCfg -Raw -Encoding UTF8 | ConvertFrom-Json
    Step "Alpha identity   = $($alpha.identity)"
    Step "Alpha shortName  = $($alpha.shortName)"

    if ($alpha.identity -eq 'Tooensure.ProjectName.App') {
        Write-Host "RESULT depth=1 - identity was NOT substituted, child collides with parent"
        return
    }

    Step "install Alpha as its own template"
    dotnet new install "$Work\Alpha" --force 2>&1 | Select-Object -Last 4
    $installed += "$Work\Alpha"

    Step "generate Beta from '$($alpha.shortName)'"
    dotnet new $alpha.shortName -n Beta -o "$Work\Beta" 2>&1 | Select-Object -Last 4

    # The project is "<Name>.App.csproj", not "<Name>.csproj" - checking the wrong
    # name reports a false failure on a template that actually worked.
    if (Get-ChildItem "$Work\Beta" -Filter '*.csproj' -ErrorAction SilentlyContinue) {
        $depth = 2
        $betaCfg = "$Work\Beta\.template.config\template.json"
        $betaRecursive = Test-Path $betaCfg
        Write-Host "RESULT depth=2 - Beta generated from Alpha; Beta is itself a template: $betaRecursive"
    }
    else {
        Write-Host "RESULT depth=1 - Alpha installed but produced no Beta project"
        Get-ChildItem "$Work\Beta" -ErrorAction SilentlyContinue | Select-Object -First 5 | ForEach-Object { "   $($_.Name)" }
    }
}
finally {
    Step "cleanup"
    foreach ($i in $installed) { dotnet new uninstall $i 2>&1 | Out-Null }
    Write-Host "uninstalled $($installed.Count) template package(s); work dir left at $Work"
    Write-Host "DEPTH=$depth"
}
