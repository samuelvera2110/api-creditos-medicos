# ============================================================
#  EF Core Scaffold — HeathCare
#  Uso: ./scaffold.ps1
#  Desde la raiz de la solucion: HeathCare/
# ============================================================

# ─── CONNECTION STRING ───────────────────────────────────────
$Server   = "localhost"       # Ej: localhost  |  192.168.1.10  |  miserver.database.windows.net
$Database = "HealthCare"             # Ej: HeathCareDB
$User     = "sa"        # Ej: sa
$Password = "Dragoncity5*"       # Ej: Admin1234!

$ConnectionString = "Server=$Server;Database=$Database;User Id=$User;Password=$Password;TrustServerCertificate=True;"

# ─── PROYECTOS ───────────────────────────────────────────────
$Project = "HealthCare.Infrastructure/HealthCare.Infrastructure.csproj"
$StartupProject = "HeathCare.Api/HeathCare.Api.csproj"

# ─── SALIDA ──────────────────────────────────────────────────
$OutputDir  = "Persistence/Entities"
$ContextDir = "Persistence/Context"
$ContextName = "HeathCareDbContext"

# ─── SCAFFOLD ────────────────────────────────────────────────
Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  EF Core Scaffold — HeathCare" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Server   : $Server" -ForegroundColor Gray
Write-Host "  Database : $Database" -ForegroundColor Gray
Write-Host "  Proyecto : $Project" -ForegroundColor Gray
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

dotnet ef dbcontext scaffold $ConnectionString `
    Microsoft.EntityFrameworkCore.SqlServer `
    --project $Project `
    --startup-project $StartupProject `
    --output-dir $OutputDir `
    --context-dir $ContextDir `
    --context $ContextName `
    --data-annotations `
    --no-onconfiguring `
    --force

# ─── RESULTADO ───────────────────────────────────────────────
if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "  Scaffold completado exitosamente!" -ForegroundColor Green
    Write-Host "  Entidades  -> HeathCare.Infrastructure/$OutputDir" -ForegroundColor Green
    Write-Host "  DbContext  -> HeathCare.Infrastructure/$ContextDir/$ContextName.cs" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "  Error en el scaffold. Revisa la conexion y los proyectos." -ForegroundColor Red
    Write-Host ""
}