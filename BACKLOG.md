# Product Backlog, Cronograma y Sprint Backlogs

Programación Web II — Fecha de inicio: sáb 10/oct/2026 — Fecha de entrega: **mié 28/oct/2026**

## 1. Cronograma de sprints

| Sprint | Fechas | Objetivo | Entregable |
|---|---|---|---|
| Sprint 1 | (completado) | Diagnóstico, clases y modelos | Modelos iniciales |
| **Sprint 2** | sáb 10 – mié 14 oct | Proyecto compilando + CRUD Categorías y Productos + primer deploy de prueba | App con CRUD en GitHub y desplegada |
| **Sprint 3** | jue 15 – mar 20 oct | Clientes y Ventas (núcleo del sistema) · grupos: CRUD base propio | Venta completa con detalle y stock |
| **Sprint 4** | mié 21 – sáb 24 oct | Validaciones, búsquedas, funciones propias de cada grupo · **cierre de desarrollo (sáb 24)** | Funcionalidades terminadas |
| **Sprint 5** | dom 25 – mié 28 oct | Solo pruebas, corrección de errores, deploy final y presentación | Sistema en línea + demo |

Reglas: desde el sáb 24 no se agregan funcionalidades nuevas, solo correcciones. El primer deploy se hace en el Sprint 2 (no al final) para descubrir problemas de plataforma y base de datos con tiempo.

Ceremonias por sprint: Planning (inicio, 15 min) · Daily corta (inicio de cada clase) · Review (demo del sprint) · Retrospectiva (3 preguntas: qué salió bien, qué no, qué cambiamos).

## 2. Product Backlog (Sistema de Ventas)

Prioridad: A = alta, M = media, B = baja.

| ID | Historia de usuario | Prio. | Sprint |
|---|---|---|---|
| PB-01 | Como equipo, quiero el proyecto organizado (Models, Data, Controllers, Views) y compilando | A | 2 |
| PB-02 | Como equipo, quiero el código en GitHub con .gitignore y ramas | A | 2 |
| PB-03 | Como administrador, quiero crear las tablas con migraciones | A | 2 |
| PB-04 | Como administrador, quiero gestionar categorías (CRUD) | A | 2 |
| PB-05 | Como administrador, quiero gestionar productos (CRUD) con su categoría | A | 2 |
| PB-06 | Como equipo, quiero un primer despliegue de prueba en línea | A | 2 |
| PB-07 | Como administrador, quiero gestionar clientes (CRUD) | A | 3 |
| PB-08 | Como vendedor, quiero registrar una venta eligiendo cliente y productos | A | 3 |
| PB-09 | Como vendedor, quiero que el subtotal, total y cambio se calculen solos | A | 3 |
| PB-10 | Como vendedor, quiero que el stock baje al confirmar la venta | A | 3 |
| PB-11 | Como vendedor, quiero ver el listado y detalle de ventas | M | 3 |
| PB-12 | Como usuario, quiero mensajes de validación claros en los formularios | M | 4 |
| PB-13 | Como administrador, quiero buscar productos por nombre/categoría | M | 4 |
| PB-14 | Como administrador, quiero ver alertas de stock bajo | M | 4 |
| PB-15 | Como vendedor, quiero impedir ventas mayores al stock disponible | A | 4 |
| PB-16 | Como administrador, quiero un reporte simple de ventas por fecha | B | 4 |
| PB-17 | Como equipo, quiero el sistema final desplegado y probado | A | 5 |
| PB-18 | Como equipo, quiero una presentación/demo del sistema | A | 5 |

## 3. Sprint Backlogs (Sistema de Ventas, a cargo del docente)

### Sprint 2 — sáb 10 – mié 14 oct

| ID | Tarea | Estado |
|---|---|---|
| S2-01 | Verificar entorno de cada estudiante (`dotnet --version`, `dotnet ef`, `turso`) | Pendiente |
| S2-02 | Crear repo en GitHub, subir proyecto base, verificar que `.env` y `*.db` no suben | Pendiente |
| S2-03 | Ordenar carpetas y namespaces (`SistemaVentas.*`); `dotnet build` sin errores | Pendiente |
| S2-04 | Copiar Models y AppDbContext; `dotnet ef migrations add Inicial` | Pendiente |
| S2-05 | Generar `migracion.sql` y aplicarlo en Turso; verificar tablas | Pendiente |
| S2-06 | CategoriasController + vistas (Index, Create, Edit, Delete) | Pendiente |
| S2-07 | Probar conexión de la app a Turso (spike). Si falla: plan B SQLite con volumen | Pendiente |
| S2-08 | ProductosController + vistas con lista desplegable de categorías | Pendiente |
| S2-09 | Primer deploy de prueba en Railway (Dockerfile + variables) | Pendiente |
| S2-10 | Review + retrospectiva | Pendiente |

### Sprint 3 — jue 15 – mar 20 oct

| ID | Tarea | Estado |
|---|---|---|
| S3-01 | ClientesController + vistas CRUD | Pendiente |
| S3-02 | Pantalla "Nueva venta": elegir cliente y fecha | Pendiente |
| S3-03 | Agregar productos al detalle (cantidad, precio, `CalcularSubtotal`) | Pendiente |
| S3-04 | `CalcularTotal` y `CalcularCambio` al guardar | Pendiente |
| S3-05 | `ActualizarStock` al confirmar (en una transacción) | Pendiente |
| S3-06 | Listado de ventas y vista de detalle | Pendiente |
| S3-07 | Subir a GitHub al cerrar cada tarea; redeploy | Pendiente |
| S3-08 | Review + retrospectiva | Pendiente |

### Sprint 4 — mié 21 – sáb 24 oct

| ID | Tarea | Estado |
|---|---|---|
| S4-01 | Validaciones en modelos y formularios (campos obligatorios, rangos) | Pendiente |
| S4-02 | Búsqueda/filtro de productos | Pendiente |
| S4-03 | Alerta de stock bajo (`Stock <= StockMinimo`) | Pendiente |
| S4-04 | Impedir cantidad mayor al stock en la venta | Pendiente |
| S4-05 | Reporte simple de ventas por fecha (opcional) | Pendiente |
| S4-06 | Cierre de desarrollo: sáb 24, merge a `main` | Pendiente |

### Sprint 5 — dom 25 – mié 28 oct

| ID | Tarea | Estado |
|---|---|---|
| S5-01 | Lista de pruebas manuales (crear, editar, eliminar, vender) en el sitio desplegado | Pendiente |
| S5-02 | Corregir solo errores (sin funcionalidades nuevas) | Pendiente |
| S5-03 | Deploy final y verificar variables de entorno | Pendiente |
| S5-04 | Ensayo y presentación/demo (mié 28) | Pendiente |

## 4. Equivalencia para los proyectos de los grupos

Cada grupo repite el patrón del docente con sus propias entidades, sin agregar atributos de más.

| Sprint | Sistema de Ventas (docente) | Grupo 1 — Biblioteca de Proyectos de Grado | Grupo 2 — Mascotas Extraviadas |
|---|---|---|---|
| 2 | Categoría + Producto (CRUD) | Carrera/Área + Proyecto (CRUD) | Especie + Mascota (CRUD) |
| 3 | Cliente + Venta + Detalle | Autor + relación Proyecto–Autor | Reporte (extraviada/encontrada) + contacto |
| 4 | Búsqueda, validaciones, stock | Búsqueda por título, carrera, gestión | Filtro por estado y especie, seguimiento del reporte |
| 5 | Pruebas y deploy | Pruebas y deploy | Pruebas y deploy |

Cada grupo debe tener su propio repositorio, su Product Backlog (mínimo 10 historias) y su Sprint Backlog por sprint.

## 5. Riesgos

| Riesgo | Mitigación |
|---|---|
| Entornos distintos (WSL/Linux, SDK 9 vs 10) | S2-01 el primer día; unificar versión de .NET |
| Conexión app→Turso no funciona con el proveedor elegido | Spike S2-07; plan B SQLite con volumen |
| Secretos subidos a GitHub | `.gitignore` desde el primer commit; regenerar token si pasa |
| Límites del plan gratuito de hosting | Probar deploy en Sprint 2; tener una plataforma alternativa |
| Falta de tiempo al final | Congelar funcionalidades el sáb 24 |
