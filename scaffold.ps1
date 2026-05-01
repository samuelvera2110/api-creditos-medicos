# ============================================================
#  EF Core Scaffold — HealthCare (PostgreSQL 18)
#  Uso: ./scaffold.ps1
#  Desde la raiz de la solucion: HeathCare/
# ============================================================

# ─── CONNECTION STRING ───────────────────────────────────────
$Server   = "dpg-d7qc1n4m0tmc73d12db0-a.oregon-postgres.render.com"
$Port     = "5432"
$Database = "healthcare_3aof"
$User     = "jchango1"
$Password = "VTiwx7ZMdgDqUviDLpWhuwMmwKzqifW0"

$ConnectionString = "Host=$Server;Port=$Port;Database=$Database;Username=$User;Password=$Password;"

# ─── PROYECTOS ───────────────────────────────────────────────
$Project        = "HealthCare.Infrastructure/HealthCare.Infrastructure.csproj"
$StartupProject = "HeathCare.Api/HeathCare.Api.csproj"

# ─── SALIDA ──────────────────────────────────────────────────
$OutputDir   = "Persistence/Entities"
$ContextDir  = "Persistence/Context"
$ContextName = "HeathCareDbContext"

# ─── SCAFFOLD ────────────────────────────────────────────────
Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  EF Core Scaffold — HealthCare (PostgreSQL)" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Server   : $Server`:$Port" -ForegroundColor Gray
Write-Host "  Database : $Database" -ForegroundColor Gray
Write-Host "  Proyecto : $Project" -ForegroundColor Gray
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

dotnet ef dbcontext scaffold $ConnectionString `
    Npgsql.EntityFrameworkCore.PostgreSQL `
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
    Write-Host "  Entidades -> HealthCare.Infrastructure/$OutputDir" -ForegroundColor Green
    Write-Host "  DbContext -> HealthCare.Infrastructure/$ContextDir/$ContextName.cs" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "  Error en el scaffold. Revisa la conexion y los proyectos." -ForegroundColor Red
    Write-Host ""
}