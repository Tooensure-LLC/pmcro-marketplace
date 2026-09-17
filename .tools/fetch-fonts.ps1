<#
.SYNOPSIS
    Downloads the PMCR-O UI fonts into Resources/Fonts.

.DESCRIPTION
    Inter (UI) and JetBrains Mono (code/trail) - both SIL Open Font License, so they
    can ship inside the app, including store builds.

    Static instances, not variable fonts: MAUI resolves a FontFamily to a single face,
    so a variable .ttf renders at its default weight and the Bold/SemiBold styles in
    PmcroComponents.xaml would silently do nothing.
#>
[CmdletBinding()]
param(
    [string] $FontsDir = 'T:\pmcro-marketplace\ProjectName.App\Resources\Fonts',
    [string] $Work = "$env:TEMP\pmcro-fonts"
)

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

New-Item -ItemType Directory -Force -Path $Work, $FontsDir | Out-Null

$sources = @(
    @{
        Name = 'Inter'
        Url  = 'https://github.com/rsms/inter/releases/download/v4.1/Inter-4.1.zip'
        Want = @('Inter-Regular.ttf', 'Inter-SemiBold.ttf', 'Inter-Bold.ttf')
    },
    @{
        Name = 'JetBrainsMono'
        Url  = 'https://github.com/JetBrains/JetBrainsMono/releases/download/v2.304/JetBrainsMono-2.304.zip'
        Want = @('JetBrainsMono-Regular.ttf', 'JetBrainsMono-Bold.ttf')
    }
)

foreach ($s in $sources) {
    $zip = Join-Path $Work "$($s.Name).zip"
    Write-Host "== $($s.Name): downloading"
    Invoke-WebRequest -Uri $s.Url -OutFile $zip -UseBasicParsing -TimeoutSec 180

    $dest = Join-Path $Work $s.Name
    if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
    Expand-Archive -Path $zip -DestinationPath $dest -Force

    foreach ($want in $s.Want) {
        $hit = Get-ChildItem $dest -Recurse -File -Filter $want | Select-Object -First 1
        if ($hit) {
            Copy-Item $hit.FullName (Join-Path $FontsDir $want) -Force
            Write-Host "   copied $want  ($([math]::Round($hit.Length/1KB,0)) KB)"
        }
        else {
            Write-Host "   MISSING $want"
        }
    }
}

Write-Host "== fonts now in $FontsDir"
Get-ChildItem $FontsDir -Filter *.ttf | ForEach-Object { "   $($_.Name)" }
