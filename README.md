# 🏥 HealthCare API

Proyecto basado en **Clean Architecture**, diseñado para mantener una estructura escalable, mantenible y desacoplada.

---

# 🧠 ¿Qué es Clean Architecture?

Clean Architecture es un enfoque de diseño que separa el sistema en capas independientes, donde:

- Las reglas de negocio no dependen de frameworks
- Las dependencias siempre apuntan hacia el núcleo (Domain)
- Se facilita el mantenimiento, testing y escalabilidad

---

# 🧱 Estructura del Proyecto

## 📦 HealthCare.Api
👉 **Capa de presentación (entry point)**
- Contiene los **controllers**
- Configuración de la aplicación (JWT, CORS, middlewares)
- Maneja las peticiones HTTP

---

## 📦 HealthCare.Application
👉 **Capa de lógica de aplicación**
- Casos de uso (Use Cases)
- Servicios de aplicación
- Validaciones
- Orquestación de operaciones

💡 No contiene lógica de acceso a datos ni dependencias externas

---

## 📦 HealthCare.Domain
👉 **Capa central (núcleo del sistema)**
- Entidades del dominio
- Interfaces (contratos)
- Reglas de negocio

💥 Esta capa NO depende de ninguna otra

---

## 📦 HealthCare.Infrastructure
👉 **Capa de acceso a datos y servicios externos**
- Implementación de repositorios
- Conexión a base de datos (Entity Framework Core)
- Entidades generadas por Scaffold en `Persistence/Entities/`
- DbContext en `Persistence/Context/`
- Integraciones externas

💡 Depende de Domain, pero Domain no depende de ella

---

## 📦 HealthCare.Shared
👉 **Capa de utilidades compartidas**
- Clases comunes
- Helpers
- Constantes
- Modelos reutilizables

---

# 🔄 Flujo de Dependencias

```text
Api → Application → Domain
          ↓
   Infrastructure
```

---

# 🐳 Base de Datos con Docker

El proyecto incluye un `docker-compose.yml` en la raíz para levantar **SQL Server 2022** de forma rápida sin necesidad de instalarlo manualmente.

## ✅ Requisitos
- Tener **Docker Desktop** instalado y corriendo
- Imagen usada: `mcr.microsoft.com/mssql/server:2022-latest`

## 🚀 Levantar SQL Server

```bash
docker compose up -d
```

Esto levanta el contenedor en segundo plano con:

| Parámetro | Valor |
|---|---|
| Puerto | `1433` |
| Usuario | `sa` |
| Edición | Developer (gratuita) |
| Volumen | `healthcare_sqlserver_data` (datos persistentes) |

## 🛑 Comandos útiles

```bash
# Ver estado del contenedor
docker compose ps

# Apagar sin borrar datos
docker compose down

# Apagar y borrar todos los datos
docker compose down -v

# Ver logs del contenedor
docker compose logs sqlserver
```

> ⚠️ **Nota:** Los datos del volumen persisten aunque apagues el contenedor. Solo se borran si usas `docker compose down -v`.

---

# 🔧 Entity Framework Core — Scaffold

El proyecto usa **Database First** con Entity Framework Core 9.x. Las entidades y el `DbContext` se generan automáticamente desde la base de datos usando el script `scaffold.ps1`.

## ✅ Requisitos previos

Tener instalada la herramienta global de EF:

```bash
dotnet tool install --global dotnet-ef
```

Verificar instalación:

```bash
dotnet ef --version
```

## 📁 ¿Dónde se generan los archivos?

```
HealthCare.Infrastructure/
└── Persistence/
    ├── Context/
    │   └── HeathCareDbContext.cs       ← DbContext principal
    └── Entities/
        ├── Paciente.cs                 ← Entidades generadas
        ├── Doctor.cs
        └── ...
```

## ⚙️ Usar el script de Scaffold

En la raíz del repositorio existe el archivo `scaffold.ps1`. Antes de ejecutarlo, abre el archivo y configura tus credenciales:

```powershell
$Server   = "localhost"
$Database = "TU_BASE_DE_DATOS"
$User     = "sa"
$Password = "TU_PASSWORD"
```

Luego ejecuta desde la raíz de la solución:

```bash
# Mac / Linux (requiere PowerShell instalado)
pwsh scaffold.ps1

# Windows
./scaffold.ps1
```

> 💡 El script usa `--force`, lo que significa que **sobreescribe** los archivos existentes. Úsalo cada vez que hagas cambios en la base de datos y quieras regenerar las entidades.

### Instalar PowerShell en Mac (si no lo tienes)

```bash
brew install powershell
```

---

# 🗄️ Scripts de Base de Datos

En la carpeta `Database/scripts/` se encuentran los scripts SQL del proyecto. Deben ejecutarse **después de levantar el contenedor de Docker** para crear las tablas y estructura inicial.

```
HeathCare/
└── Database/
    └── scripts/
        └── 00_Init_Auth_Module.sql     ← Tablas del módulo de autenticación
```

## ▶️ ¿Cómo ejecutar el script?

1. Levanta el contenedor:
```bash
docker compose up -d
```

2. Conéctate con tu cliente SQL (SSMS, Azure Data Studio, DBeaver, Rider):

| Campo | Valor |
|---|---|
| Server | `localhost,1433` |
| Usuario | `sa` |
| Password | la que configuraste en `docker-compose.yml` |

3. Abre `Database/scripts/00_Init_Auth_Module.sql`, copia el contenido y ejecútalo.

> 💡 Los scripts están numerados (`00_`, `01_`, etc.) para indicar el orden de ejecución. Respeta ese orden si hay más de uno en el futuro.

---

# 🗂️ Archivos en la raíz del repositorio

| Archivo / Carpeta | Descripción |
|---|---|
| `docker-compose.yml` | Levanta SQL Server 2022 con Docker |
| `scaffold.ps1` | Genera entidades y DbContext desde la BD |
| `Database/scripts/` | Scripts SQL para crear tablas y estructura |
| `HealthCare.sln` | Solución principal del proyecto |
| `README.md` | Este archivo |

---

# 🛠️ Stack Tecnológico

| Tecnología | Versión |
|---|---|
| .NET | 9.0 |
| Entity Framework Core | 9.0.x |
| SQL Server | 2022 |
| Docker | Desktop |

---

# 🚀 Guía de inicio rápido — Primera vez

Sigue estos pasos en orden para levantar el proyecto desde cero:

### Paso 1 — Clonar el repositorio
```bash
git clone <url-del-repo>
cd HeathCare
```

### Paso 2 — Levantar SQL Server con Docker
```bash
docker compose up -d
```
Verifica que esté corriendo:
```bash
docker compose ps
```
Debes ver el contenedor `healthcare_sqlserver` en estado `running`.

### Paso 3 — Ejecutar el script de base de datos
Conéctate a `localhost,1433` con usuario `sa` desde tu cliente SQL favorito (SSMS, Azure Data Studio, DBeaver, Rider), luego abre y ejecuta:
```
Database/scripts/00_Init_Auth_Module.sql
```
Esto crea todas las tablas necesarias para el proyecto.

### Paso 4 — Generar las entidades con Scaffold
```bash
pwsh scaffold.ps1
```
Esto genera automáticamente las clases C# en `HealthCare.Infrastructure/Persistence/Entities/` y el `DbContext` en `Persistence/Context/`.

> ⚠️ Asegúrate de que el `scaffold.ps1` tenga configuradas las credenciales correctas antes de ejecutarlo.

### Paso 5 — Restaurar dependencias
```bash
dotnet restore
```

### Paso 6 — Correr la API
```bash
dotnet run --project HeathCare.Api/HeathCare.Api.csproj
```

La API estará disponible en `https://localhost:5001` o `http://localhost:5000`.

---

> 🔁 **Próximas veces** solo necesitas `docker compose up -d` y `dotnet run`. El scaffold solo se vuelve a ejecutar si hay cambios en la base de datos.
