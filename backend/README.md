# CommerceApp — Prueba técnica

Aplicación para importar archivos de comercios, procesarlos por fecha y consultar los registros rechazados.

## Tecnologías

- Backend: ASP.NET Core Web API / C# (.NET 10)
- Frontend: Angular / TypeScript (Angular 22)
- Base de datos: Microsoft SQL Server (desarrollado con SQL Server 2022)
- Bibliotecas: Microsoft.Data.SqlClient, CsvHelper y Papa Parse

## Estructura

```text
backend/    Proyecto ASP.NET Core Web API
frontend/   Proyecto Angular
 database/  Script de la base de datos
samples/    Archivo CSV para pruebas
```

## Requisitos

- .NET SDK compatible con el proyecto (recomendado: .NET 10)
- Node.js compatible con Angular 22 (recomendado: Node.js 24 LTS)
- SQL Server y SQL Server Management Studio (SSMS)

## 1. Base de datos

1. Conectarse a una instancia local de SQL Server desde SSMS.
2. Abrir `database/CommerceDB.sql` y ejecutarlo en una instancia de prueba. Si el script no incluye `CREATE DATABASE`, crear antes una base llamada `CommerceDB` y ejecutar el script sobre ella.
3. Verificar que existan las tablas `commerce`, `commerce_quarantine` y `commerce_imports`; los procedimientos `sp_create_commerce` y `sp_process_commerce`; y el tipo de tabla `dbo.CommerceTableType`.
4. Configurar la cadena `ConnectionStrings:CommerceDb` en el archivo `appsettings.json` del backend según el servidor donde se ejecute.

Ejemplo para SQL Server local con autenticación de Windows:

```json
"ConnectionStrings": {
  "CommerceDb": "Server=.;Database=CommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## 2. Backend

1. Abrir el proyecto `.csproj` o la solución de Visual Studio incluida en `backend/`.
2. Restaurar los paquetes NuGet y compilar.
3. Ejecutar la API con Visual Studio o mediante `dotnet run` desde la carpeta donde está el `.csproj`.
4. Identificar su URL HTTPS. Durante el desarrollo se utilizó `https://localhost:7204`.

La URL y el puerto pueden variar dependiendo de la configuración de ejecución.

## 3. Frontend

Desde una terminal en `frontend/`:

```powershell
npm ci
npm run start -- --port 4200
```

Abrir `http://localhost:4200`.

Si cambia el puerto HTTPS del backend, actualizar `apiUrl` en `src/app/services/commerce.service.ts`. Comprobar que el backend permita `http://localhost:4200` mediante su política CORS.

## Endpoints

| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/commerce/upload` | Recibe `multipart/form-data` con campo `file` y carga el CSV |
| POST | `/api/commerce/process` | Recibe JSON con `processDate` y ejecuta el procesamiento |
| GET | `/api/commerce/quarantine` | Lista registros rechazados y motivos |
| GET | `/api/commerce` | Lista comercios existentes; permite filtro `?fecha=YYYY-MM-DD` |
| GET | `/api/commerce/ping` | Comprueba que la API responde |
| GET | `/api/commerce/database` | Comprueba la conexión con la base |

Ejemplo de solicitud de procesamiento:

```json
{"processDate":"2026-10-08"}
```

## Prueba de funcionamiento

1. Abrir **Importar CSV** y seleccionar `samples/commerce_08102026.csv`.
2. Revisar la vista previa e importar. El archivo contiene cuatro registros.
3. Consultar **Comercios**: antes del procesamiento deben aparecer los cuatro registros si se parte de una base vacía.
4. Abrir **Procesar**, elegir `2026-10-08` y ejecutar la validación. Deben trasladarse dos registros a cuarentena.
5. Consultar **Cuarentena** y verificar los motivos del rechazo.
6. Volver a **Comercios** para confirmar que permanecen los dos registros válidos.
7. Intentar importar nuevamente el mismo archivo: la API debe responder HTTP 409 por duplicidad de contenido.

## Decisiones técnicas

- Arquitectura con `Controller`, `Service` y `Repository`.
- Inserción por lotes mediante un parámetro de tipo tabla (TVP) en `sp_create_commerce`.
- Procedimiento transaccional para trasladar registros inválidos a `commerce_quarantine`.
- Detección de importaciones repetidas mediante SHA-256 del contenido.
- Comunicación Angular–API mediante `HttpClient` y navegación por rutas.

## Alcance

Esta versión se construyó utilizando las tres columnas indicadas explícitamente en el enunciado: `pc_processdate`, `pc_nomcomred` y `pc_numdoc`. Si el archivo de evaluación original contiene columnas adicionales, deben incorporarse al modelo, al tipo de tabla SQL, a las tablas y a la importación para cumplir la estructura completa.

La autenticación de usuarios era opcional y no está incluida. Spring Boot aparece en la sección «Entorno» del enunciado, pero la implementación requerida para el backend especifica .NET.
