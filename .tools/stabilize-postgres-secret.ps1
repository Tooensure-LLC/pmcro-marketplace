$secret = (& aspire secret get Parameters:postgres-password --apphost 'T:\pmcro-marketplace\AppHost\AppHost.csproj' --non-interactive | Select-Object -Last 1).ToString().Trim()
[Environment]::SetEnvironmentVariable('Parameters__postgres_password', $secret, 'User')
Write-Output 'postgres parameter environment synchronized'
