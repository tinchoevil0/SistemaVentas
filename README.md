# Sistema Web de Gestión de Ventas

ASP.NET Core MVC + Razor + Entity Framework Core 9 + Turso/libSQL.

## Estructura

```
SistemaVentas/
├── SistemaVentas.csproj
├── Program.cs                  ← configuración de Turso, .env, DbContext, rutas
├── Dockerfile                  ← para el deploy (Railway)
├── .gitignore  .env.example    ← el .env real NO se sube
├── Models/                     ← Categoria, Cliente, Producto, Venta, DetalleVenta
├── Data/AppDbContext.cs        ← DbSets + datos iniciales
├── Controllers/                ← Home, Categorias, Productos (luego Clientes, Ventas)
├── Views/
│   ├── _ViewImports.cshtml  _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml   ← menú + Bootstrap
│   ├── Home/Index.cshtml
│   ├── Categorias/             ← Index, Create, Edit, Delete, _Form
│   └── Productos/              ← Index, Create, Edit, Delete, _Form
├── Migrations/                 ← la crea dotnet ef (se sube a GitHub)
└── wwwroot/
```

## Puesta en marcha

```bash
dotnet --version                       # SDK 10.x
dotnet tool install --global dotnet-ef --version '9.*'
dotnet build
```

## Conexión a Turso

La aplicación usa directamente una base remota Turso; no crea ni usa `ventas.db`.
Al arrancar, `Database.Migrate()` crea las tablas pendientes y aplica los datos iniciales
de `AppDbContext` (categorías y productos).

1. En Windows, abrir WSL e instalar la CLI de Turso si aún no está instalada:

   ```bash
   curl -sSfL https://get.tur.so/install.sh | bash
   export PATH="$HOME/.turso:$PATH"
   ```

   Iniciar sesión. En WSL, usar el modo `--headless` y abrir manualmente en el navegador
   el enlace que muestra la CLI:

   ```bash
   turso auth login --headless
   turso auth whoami
   ```

2. Esta app usa la base existente `ventas-db` de la cuenta `tinchoevil0`, en
   `aws-us-east-1`. **No crear otra base**. Consultar su URL y generar un token de base
   nuevo para esta computadora:

   ```bash
   turso db show ventas-db
   turso db tokens create ventas-db
   ```

3. En PowerShell, desde la carpeta `SistemaVentas`, copiar el ejemplo y editar `.env`:

   ```powershell
   Copy-Item .env.example .env
   notepad .env
   ```

   Reemplazar `TURSO_DATABASE_URL` por
   `libsql://ventas-db-tinchoevil0.aws-us-east-1.turso.io` y `TURSO_AUTH_TOKEN`
   por el token recién generado. No publicar ni compartir ese token. `.env` está
   excluido de Git y hay que crear uno por computadora.

4. Instalar el SDK de .NET 10 y ejecutar desde `SistemaVentas`:

   ```bash
   dotnet tool install --global dotnet-ef --version 9.*
   dotnet build
   dotnet run
   ```

   Si `dotnet-ef` ya está instalado, no hace falta volver a instalarlo.
   La base ya tiene la migración inicial y los datos de demostración; al iniciar,
   `Database.Migrate()` solo aplica migraciones pendientes.
5. Verificar la base, si se desea:

   ```bash
   turso db shell ventas-db
   ```

   Dentro del shell SQL:

   ```sql
   SELECT name FROM sqlite_master WHERE type='table';
   SELECT Nombre FROM Categorias;
   SELECT Nombre FROM Productos;
   ```

   La primera ejecución en otra computadora no crea otra base; conecta a la misma
   base remota usando su `.env`.

Para continuar trabajando localmente, ejecutar `dotnet run` desde la carpeta del proyecto.
Para cada cambio de modelos, crear una migración con `dotnet ef migrations add NombreCambio`
y desplegar la aplicación para que `Database.Migrate()` la aplique. Las variables
`TURSO_DATABASE_URL` y `TURSO_AUTH_TOKEN` también deben configurarse como secretos/
variables de entorno en el hosting; no subir `.env` ni publicar el token.

## Acceso de demostración

La aplicación solicita iniciar sesión antes de acceder a sus páginas. Para pruebas locales:

- Usuario: `admin`
- Contraseña: `admin123!`

Estas credenciales son únicamente para demostración; no deben utilizarse en producción. Se pueden reemplazar mediante las variables de entorno `DemoLogin__Username` y `DemoLogin__Password`. El botón de Google es solo una muestra visual y no realiza autenticación.

## Deploy

El workflow `.github/workflows/deploy.yml` publica a MonsterASP mediante FTPS. Antes
del despliegue, configurar `TURSO_DATABASE_URL` y `TURSO_AUTH_TOKEN` en la configuración
de entorno de la aplicación en el hosting; el archivo `.env` local no se publica.
Confirmar también que el plan de hosting admite la versión de .NET requerida por el proyecto.

El `Dockerfile` queda disponible para despliegues en plataformas como Railway. En ese
caso, configurar las mismas variables de entorno en el panel de la plataforma.

## Git

Commits pequeños y frecuentes, una rama por historia (`feature/crud-productos`) y Pull Request hacia `main`.
