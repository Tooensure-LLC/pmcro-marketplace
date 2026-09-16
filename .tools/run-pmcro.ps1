$ErrorActionPreference = 'Stop'
$secret = (& aspire secret get Parameters:postgres-password --apphost 'T:\pmcro-marketplace\AppHost\AppHost.csproj' --non-interactive | Select-Object -Last 1).ToString().Trim()
$env:Parameters__postgres_password = $secret
Set-Location 'T:\pmcro-marketplace'
& aspire run --non-interactive --detach --format Json
