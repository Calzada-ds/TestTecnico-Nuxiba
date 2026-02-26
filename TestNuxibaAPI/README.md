# Nuxiba Practice API — Prueba Técnica TestDevBackJr

Solución integral para la prueba técnica de **Nuxiba**, desarrollada por **Daniel Sebastian Calzada Guerrero**.  
La API gestiona el control de accesos, realiza cálculos de tiempos de sesión y automatiza la persistencia de datos mediante migraciones de EF Core.

---

## Tecnologías y Herramientas

| Categoría | Tecnología |
|---|---|
| Framework | .NET 8 (Web API) |
| ORM | Entity Framework Core (Code First) |
| Base de Datos | SQL Server (Docker) |
| Ingesta de Datos | MiniExcel (carga desde `.xlsx`) |
| Pruebas Unitarias | xUnit |
| Documentación | Swagger / OpenAPI |

---

## Configuración y Requisitos Previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y corriendo
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Cualquier cliente SQL: SQL Server Management Studio o Azure Data Studio

---

## Paso 1 — Levantar SQL Server en Docker

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Credenciales de conexión:

| Campo | Valor |
|---|---|
| Servidor | `localhost,1433` |
| Usuario | `sa` |
| Contraseña | `YourStrong!Passw0rd` |

---

## Paso 2 — Configurar la Cadena de Conexión

En el archivo `appsettings.json`, asegúrate de que el bloque `ConnectionStrings` quede así:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=NuxibaDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  }
}
```

---

## ▶️ Paso 3 — Ejecutar el Proyecto

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar (aplica migraciones y pobla la BD automáticamente)
dotnet run
```

La API quedará disponible en:
- `https://localhost:7194`
- `http://localhost:5194`

---

## 🔄 Flujo de Automatización al dar Play

El proyecto es **autoconfigurable e idempotente**. Al iniciar, ejecuta automáticamente:

### ✅ 1. Migraciones Automáticas
```csharp
context.Database.Migrate();
```
Crea las siguientes tablas si no existen:
- `ccUsers`
- `ccloglogin`
- `ccRIACat_Areas`

### ✅ 2. Poblado desde Excel
Usa `CCenterRIA.xlsx` (incluido en el proyecto) para llenar las tablas automáticamente mediante MiniExcel.

> ⚠️ El archivo está configurado con `CopyToOutputDirectory = Always` en el `.csproj` para que siempre esté disponible al compilar.

### ✅ 3. Protección contra Duplicados
Valida con `.Any()` antes de insertar. Si la BD ya tiene datos, no los vuelve a cargar.

---

## 📁 Estructura del Proyecto

```
NuxibaPracticeAPI/
├── Controllers/
│   └── LoginsController.cs       ← Endpoints CRUD + export CSV
├── Data/
│   ├── AppDbContext.cs            ← Contexto EF Core
│   └── DbInitializer.cs          ← Carga inicial desde Excel
├── DTOs/
│   ├── LoginCreateDTO.cs          ← Datos de entrada
│   └── LoginResponseDTO.cs        ← Datos de salida
├── Models/
│   ├── Area.cs
│   ├── Login.cs
│   └── User.cs
├── Migrations/                    ← Migraciones EF Core (commiteadas)
├── Tests/                         ← Pruebas unitarias xUnit
├── CCenterRIA.xlsx                ← Datos iniciales
├── appsettings.json
└── Program.cs
```

---

## 🏁 Ejercicio 1 — Endpoints de la API

### Base URL
```
https://localhost:7194/api
```

### `GET /api/logins`
Devuelve todos los registros de logins y logouts ordenados por fecha descendente.

**Respuesta exitosa `200 OK`:**
```json
[
  {
    "id": 1,
    "user_id": 92,
    "extension": 1001,
    "tipoMov": 1,
    "fecha": "2023-03-15T08:30:00"
  }
]
```

---

### `POST /api/logins`
Registra un nuevo movimiento de login o logout.

**Body:**
```json
{
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 1,
  "fecha": "2023-03-15T08:30:00"
}
```

**Respuesta exitosa `201 Created`:**
```json
{
  "id": 150,
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 1,
  "fecha": "2023-03-15T08:30:00"
}
```

**Errores posibles:**

| Código | Motivo |
|---|---|
| `400` | Fecha inválida o futura |
| `400` | `TipoMov` distinto de 0 o 1 |
| `400` | `User_id` no existe en `ccUsers` |
| `400` | Error de secuencia (dos logins o dos logouts seguidos) |

---

### `PUT /api/logins/{id}`
Actualiza un registro existente.

**URL de ejemplo:** `PUT /api/logins/150`

**Body:**
```json
{
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 0,
  "fecha": "2023-03-15T17:45:00"
}
```

**Respuesta exitosa:** `204 No Content`

---

### `DELETE /api/logins/{id}`
Elimina un registro por ID.

**URL de ejemplo:** `DELETE /api/logins/150`

**Respuesta exitosa:** `204 No Content`

---

### `GET /api/logins/export-csv`
Descarga un archivo CSV con el resumen de horas trabajadas por usuario.

**Respuesta exitosa `200 OK`:** archivo `.csv` descargable.

**Contenido del CSV:**

| Login | Nombre Completo | Área | Total Horas Trabajadas |
|---|---|---|---|
| jperez | Juan Pérez López | Soporte | 361 días, 12 horas, 51 minutos |

---

## 📊 Ejercicio 2 — Consultas SQL Server

### Query 1 — Usuario con MÁS tiempo logueado

```sql
WITH Sesiones AS (
    SELECT 
        User_id,
        fecha                                                    AS Inicio,
        LEAD(fecha)   OVER (PARTITION BY User_id ORDER BY fecha) AS Fin,
        TipoMov,
        LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha) AS SigMov
    FROM ccloglogin
),
CalculoSegundos AS (
    SELECT 
        User_id,
        SUM(DATEDIFF(SECOND, Inicio, Fin)) AS TotalSegundos
    FROM Sesiones
    WHERE TipoMov = 1 AND SigMov = 0  -- Pares Login → Logout válidos
    GROUP BY User_id
)
SELECT TOP 1
    User_id,
    COALESCE(CONCAT_WS(', ',
        NULLIF(CAST(TotalSegundos / 86400          AS VARCHAR) + ' días',     '0 días'),
        NULLIF(CAST((TotalSegundos % 86400) / 3600 AS VARCHAR) + ' horas',    '0 horas'),
        NULLIF(CAST((TotalSegundos % 3600)  / 60   AS VARCHAR) + ' minutos',  '0 minutos'),
        NULLIF(CAST(TotalSegundos % 60             AS VARCHAR) + ' segundos', '0 segundos')
    ), '0 segundos') AS [Tiempo total]
FROM CalculoSegundos
ORDER BY TotalSegundos DESC;
```

**Resultado esperado:**
```
User_id: 92 | Tiempo total: 361 días, 12 horas, 51 minutos, 8 segundos
```

---

### Query 2 — Usuario con MENOS tiempo logueado

```sql
WITH Sesiones AS (
    SELECT 
        User_id,
        fecha                                                    AS Inicio,
        LEAD(fecha)   OVER (PARTITION BY User_id ORDER BY fecha) AS Fin,
        TipoMov,
        LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha) AS SigMov
    FROM ccloglogin
),
CalculoSegundos AS (
    SELECT 
        User_id,
        SUM(DATEDIFF(SECOND, Inicio, Fin)) AS TotalSegundos
    FROM Sesiones
    WHERE TipoMov = 1 AND SigMov = 0
    GROUP BY User_id
)
SELECT TOP 1
    User_id,
    COALESCE(CONCAT_WS(', ',
        NULLIF(CAST(TotalSegundos / 86400          AS VARCHAR) + ' días',     '0 días'),
        NULLIF(CAST((TotalSegundos % 86400) / 3600 AS VARCHAR) + ' horas',    '0 horas'),
        NULLIF(CAST((TotalSegundos % 3600)  / 60   AS VARCHAR) + ' minutos',  '0 minutos'),
        NULLIF(CAST(TotalSegundos % 60             AS VARCHAR) + ' segundos', '0 segundos')
    ), '0 segundos') AS [Tiempo total]
FROM CalculoSegundos
ORDER BY TotalSegundos ASC;
```

**Resultado esperado:**
```
User_id: 90 | Tiempo total: 244 días, 43 minutos, 15 segundos
```

---

### Query 3 — Promedio de logueo por mes

```sql
SET LANGUAGE Spanish;
WITH Sesiones AS (
    SELECT 
        User_id,
        fecha                                                    AS Inicio,
        LEAD(fecha)   OVER (PARTITION BY User_id ORDER BY fecha) AS Fin,
        TipoMov,
        LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha) AS SigMov
    FROM ccloglogin
),
Promedios AS (
    SELECT 
        User_id,
        YEAR(Inicio)            AS Anio,
        MONTH(Inicio)           AS MesNum,
        DATENAME(MONTH, Inicio) AS MesNombre,
        AVG(CAST(DATEDIFF(SECOND, Inicio, Fin) AS BIGINT)) AS TotalSegundos
    FROM Sesiones
    WHERE TipoMov = 1 AND SigMov = 0
    GROUP BY User_id, YEAR(Inicio), MONTH(Inicio), DATENAME(MONTH, Inicio)
)
SELECT
    CONCAT('Usuario ', User_id, ' en ', MesNombre, ' ', Anio, ':') AS Detalle,
    COALESCE(NULLIF(CONCAT_WS(', ',
        NULLIF(CAST(TotalSegundos / 86400          AS VARCHAR) + ' días',     '0 días'),
        NULLIF(CAST((TotalSegundos % 86400) / 3600 AS VARCHAR) + ' horas',    '0 horas'),
        NULLIF(CAST((TotalSegundos % 3600)  / 60   AS VARCHAR) + ' minutos',  '0 minutos'),
        NULLIF(CAST(TotalSegundos % 60             AS VARCHAR) + ' segundos', '0 segundos')
    ), ''), '0 segundos') AS [Promedio de logueo]
FROM Promedios
ORDER BY Anio, MesNum, User_id;
```

**Resultado esperado:**
```
Usuario 70 en enero 2023: 3 días, 14 horas, 1 minuto, 16 segundos
```

---

## 📥 Ejercicio 3 — Descarga del CSV

### Desde el navegador
```
https://localhost:7194/api/logins/export-csv
```

### Desde curl
```bash
curl -k -o reporte.csv https://localhost:7194/api/logins/export-csv
```

### Desde Postman

1. Abre Postman y crea una nueva request
2. Selecciona método `GET`
3. URL: `https://localhost:7194/api/logins/export-csv`
4. En la pestaña **Settings**, desactiva *SSL certificate verification* (para entornos locales)
5. Haz clic en **Send**
6. En la respuesta, haz clic en **Save Response → Save to a file** y guárdalo como `reporte.csv`

**Encabezados del CSV generado:**
```
Login,Nombre Completo,Área,Total Horas Trabajadas
jperez,Juan Pérez López,Soporte,361 días 12 horas 51 minutos
```

---

## 🧪 Pruebas Unitarias

Las pruebas cubren los siguientes escenarios del `LoginsController`:

| Prueba | Descripción |
|---|---|
| `GetLogins_ReturnsOk` | Verifica que el GET devuelve `200 OK` con lista |
| `PostLogin_ValidData_ReturnsCreated` | POST con datos válidos devuelve `201 Created` |
| `PostLogin_InvalidDate_ReturnsBadRequest` | Fecha vacía devuelve `400` |
| `PostLogin_UserNotFound_ReturnsBadRequest` | `User_id` inexistente devuelve `400` |
| `PostLogin_DuplicateSequence_ReturnsBadRequest` | Dos logins seguidos devuelve `400` |
| `PutLogin_NotFound_ReturnsNotFound` | PUT con ID inexistente devuelve `404` |
| `DeleteLogin_NotFound_ReturnsNotFound` | DELETE con ID inexistente devuelve `404` |

### Ejecutar las pruebas

**Desde Visual Studio:**
```
Menú → Test → Run All Tests
```
O usa el Explorador de Pruebas (`Ctrl + E, T`).

**Desde consola:**
```bash
dotnet test
```

**Con reporte de resultados:**
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 🌐 Colección de Postman — Guía Completa

### Configuración inicial

1. Abre Postman
2. Crea un nuevo **Environment** llamado `NuxibaLocal`
3. Agrega la variable:
   - `base_url` = `https://localhost:7194`
4. En **Settings → General**, desactiva *SSL certificate verification*

---

### Request 1 — Obtener todos los registros

| Campo | Valor |
|---|---|
| Método | `GET` |
| URL | `{{base_url}}/api/logins` |
| Body | ninguno |

**Respuesta esperada:** `200 OK` con array JSON

---

### Request 2 — Registrar un Login

| Campo | Valor |
|---|---|
| Método | `POST` |
| URL | `{{base_url}}/api/logins` |
| Headers | `Content-Type: application/json` |

**Body (raw JSON):**
```json
{
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 1,
  "fecha": "2024-01-15T08:00:00"
}
```

**Respuesta esperada:** `201 Created`

---

### Request 3 — Registrar un Logout (continuación del anterior)

**Body (raw JSON):**
```json
{
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 0,
  "fecha": "2024-01-15T17:00:00"
}
```

**Respuesta esperada:** `201 Created`

---

### Request 4 — Actualizar un registro

| Campo | Valor |
|---|---|
| Método | `PUT` |
| URL | `{{base_url}}/api/logins/150` |
| Headers | `Content-Type: application/json` |

**Body (raw JSON):**
```json
{
  "user_id": 92,
  "extension": 1002,
  "tipoMov": 0,
  "fecha": "2024-01-15T18:00:00"
}
```

**Respuesta esperada:** `204 No Content`

---

### Request 5 — Eliminar un registro

| Campo | Valor |
|---|---|
| Método | `DELETE` |
| URL | `{{base_url}}/api/logins/150` |
| Body | ninguno |

**Respuesta esperada:** `204 No Content`

---

### Request 6 — Descargar CSV

| Campo | Valor |
|---|---|
| Método | `GET` |
| URL | `{{base_url}}/api/logins/export-csv` |
| Body | ninguno |

**Respuesta esperada:** `200 OK` con archivo CSV descargable

> En Postman: una vez recibida la respuesta, clic en **Save Response → Save to a file**

---

### Casos de error para probar

**Usuario inexistente:**
```json
{
  "user_id": 99999,
  "extension": 1001,
  "tipoMov": 1,
  "fecha": "2024-01-15T08:00:00"
}
```
Respuesta esperada: `400 Bad Request` — *"El User_id 99999 no existe..."*

**Fecha inválida:**
```json
{
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 1,
  "fecha": "0001-01-01T00:00:00"
}
```
Respuesta esperada: `400 Bad Request` — *"Debe proporcionar una fecha y hora válida."*

**TipoMov inválido:**
```json
{
  "user_id": 92,
  "extension": 1001,
  "tipoMov": 5,
  "fecha": "2024-01-15T08:00:00"
}
```
Respuesta esperada: `400 Bad Request` — *"El TipoMov debe ser 0 (Logout) o 1 (Login)."*

---

## 👤 Información del Candidato

**Daniel Sebastian Calzada Guerrero**  
Fecha de entrega: 2026-02-25

---

🚀 Proyecto listo para evaluación técnica.