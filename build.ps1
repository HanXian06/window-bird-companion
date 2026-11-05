$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
dotnet restore tests/Bird.Tests --configfile NuGet.Config
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
dotnet run --project tests/Bird.Tests --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed' }
dotnet restore tests/Bird.UiTests --configfile NuGet.Config
dotnet run --project tests/Bird.UiTests --no-restore
if ($LASTEXITCODE -ne 0) { throw 'UI tests failed' }
dotnet publish app/Bird.App -c Release -r win-x64 --self-contained true --configfile NuGet.Config -o runtime
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
