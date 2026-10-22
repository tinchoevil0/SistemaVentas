# Sistema Web de Gestión de Ventas

Proyecto de ejemplo — Programación Web II. ASP.NET Core MVC + Razor + Entity Framework Core + Turso (SQLite/libSQL).

## Estructura

```
SistemaVentas/
├── SistemaVentas.csproj
├── Program.cs                  ← configuración, .env, DbContext, rutas
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

## Puesta en marcha (cada estudiante)

```bash
dotnet --version                       # debe ser 10.x (el .csproj usa net10.0)
dotnet tool install --global dotnet-ef # en Linux/WSL: export PATH="$PATH:$HOME/.dotnet/tools"
dotnet build                           # debe compilar sin errores
dotnet ef migrations add Inicial       # crea la carpeta Migrations
dotnet ef database update              # crea ventas.db local con datos iniciales
dotnet run                             # abrir la URL que muestra la terminal
```

## Acceso de demostración

La aplicación solicita iniciar sesión antes de acceder a sus páginas. Para pruebas locales:

- Usuario: `admin`
- Contraseña: `admin123!`

Estas credenciales son únicamente para demostración; no deben utilizarse en producción. Se pueden reemplazar mediante las variables de entorno `DemoLogin__Username` y `DemoLogin__Password`. El botón de Google es solo una muestra visual y no realiza autenticación.

## Flujo con Turso (el de la clase)

```bash
dotnet ef migrations script -o migracion.sql
turso db shell ventas-db < migracion.sql
turso db shell ventas-db "SELECT name FROM sqlite_master WHERE type='table';"
```

Cada cambio de modelo: nueva migración (`dotnet ef migrations add NombreCambio`) y repetir el script.
Para un script solo de lo nuevo: `dotnet ef migrations script NombreAnterior NombreNuevo -o cambio.sql`.

## Conexión a Turso desde la aplicación (decisión pendiente)

`Program.cs` usa hoy SQLite local (`UseSqlite`) para que todos puedan compilar y probar ya.
Con el proveedor oficial `Microsoft.EntityFrameworkCore.Sqlite` se generan las migraciones y el script SQL
que se aplica a Turso con la CLI. Conectar la app en ejecución directamente a la base remota requiere un
proveedor/driver de Turso compatible con EF Core que **debe probarse antes de depender de él**
(tarea S2-07 del backlog). Todo el cambio vive en una sola línea de `Program.cs`.

Plan B para el deploy si ese proveedor no funciona: SQLite en un volumen persistente de la plataforma
(`DB_PATH=/data/ventas.db`).

## Deploy (Railway u otro)

1. Subir el repositorio a GitHub (sin `.env`, sin `*.db`).
2. En Railway: *New Project → Deploy from GitHub repo*. Detecta el `Dockerfile`.
3. Variables de entorno en el panel: `DB_PATH` (y `TURSO_DATABASE_URL` / `TURSO_AUTH_TOKEN` si se usa Turso).
4. Railway define `PORT`; `Program.cs` ya lo lee.
5. Confirmar las condiciones vigentes del plan gratuito de la plataforma antes de la fecha de entrega.

## Git

Commits pequeños y frecuentes, una rama por historia (`feature/crud-productos`) y Pull Request hacia `main`.
