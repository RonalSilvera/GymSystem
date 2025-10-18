param(
    [string]$MigrationName = $(Get-Date -Format "yyyyMMddHHmmss")
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$solutionDir = Resolve-Path (Join-Path $scriptDir "..")

$appSettingsPath = Join-Path $solutionDir '02.Service/API/appsettings.json'
$projectPath = Join-Path $solutionDir '00.Persistence/Infrastructure/Infrastructure.csproj'
$startupProject = $projectPath

Write-Host "Creando migracion '$MigrationName'..."
dotnet ef migrations add $MigrationName `
    --project $projectPath `
    --startup-project $startupProject `
    --context AppDbContext

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creando migracion." -ForegroundColor Red
    exit $LASTEXITCODE
} else {
    Write-Host "Migracion creada correctamente." -ForegroundColor Green
}

$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
$tenantIds = $appSettings.ConnectionStrings.PSObject.Properties.Name

foreach ($tenantId in $tenantIds) {
    Write-Host "Aplicando migraciones para tenant: $tenantId"

    dotnet ef database update `
        --project $projectPath `
        --startup-project $startupProject `
        --context AppDbContext `
        --connection $appSettings.ConnectionStrings.$tenantId

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error aplicando migracion para tenant $tenantId" -ForegroundColor Red
        exit $LASTEXITCODE
    } else {
        Write-Host "Migracion aplicada correctamente para $tenantId" -ForegroundColor Green
    }
}

Write-Host "Proceso de migracion completado para todos los tenants"
