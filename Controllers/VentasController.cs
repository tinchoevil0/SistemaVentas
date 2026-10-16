using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Controllers;

public class VentasController : Controller
{
    private readonly AppDbContext _context;

    public VentasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Ventas  -> listado
    public async Task<IActionResult> Index()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
        return View(ventas);
    }

    // GET: /Ventas/Details/5  -> venta con sus detalles
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();
        return View(venta);
    }

    // GET: /Ventas/Create  -> pantalla de venta vacía
    public async Task<IActionResult> Create()
    {
        return await MostrarFormulario(new NuevaVentaViewModel());
    }

    // POST: /Ventas/Create  -> registrar la venta
    // El navegador envía: ClienteId, Recibido y una lista Lineas[i].ProductoId / Lineas[i].Cantidad
    [HttpPost]
    public async Task<IActionResult> Create(NuevaVentaViewModel vm)
    {
        // 1) Validar lo que llegó del formulario (aunque la pantalla ya valida, el servidor decide)
        if (vm.ClienteId == null)
            ModelState.AddModelError("", "Seleccione un cliente.");

        if (vm.Recibido == null)
            ModelState.AddModelError("", "Ingrese el monto recibido.");

        var pedidos = new List<LineaVenta>();
        var filaInvalida = false;

        foreach (var linea in vm.Lineas)
        {
            if (linea.ProductoId == null || linea.Cantidad == null || linea.Cantidad <= 0)
            {
                filaInvalida = true;
                break;
            }
            pedidos.Add(linea);
        }

        if (filaInvalida)
            ModelState.AddModelError("", "Cada producto debe tener una cantidad mayor a 0.");
        else if (pedidos.Count == 0)
            ModelState.AddModelError("", "Agregue al menos un producto.");

        if (!ModelState.IsValid) return await MostrarFormulario(vm);

        // 2) Si el mismo producto llegó dos veces, se suman las cantidades
        var grupos = pedidos
            .GroupBy(p => p.ProductoId!.Value)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(p => p.Cantidad!.Value) })
            .ToList();

        // 3) Traer de la base los productos reales (el precio y el stock salen de la BD, no del navegador)
        var ids = grupos.Select(g => g.ProductoId).ToList();
        var productos = await _context.Productos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (!await _context.Clientes.AnyAsync(c => c.Id == vm.ClienteId!.Value))
        {
            ModelState.AddModelError("", "El cliente seleccionado no existe.");
            return await MostrarFormulario(vm);
        }

        // 4) Armar la venta con sus detalles
        var venta = new Venta
        {
            Fecha = DateTime.Now,
            ClienteId = vm.ClienteId!.Value,
            Recibido = vm.Recibido!.Value
        };

        foreach (var g in grupos)
        {
            if (!productos.TryGetValue(g.ProductoId, out var producto))
            {
                ModelState.AddModelError("", "Uno de los productos ya no existe.");
                continue;
            }

            if (g.Cantidad > producto.Stock)
            {
                ModelState.AddModelError("", $"Stock insuficiente de {producto.Nombre}: disponible {producto.Stock}.");
                continue;
            }

            var detalle = new DetalleVenta
            {
                Producto = producto,
                ProductoId = producto.Id,
                Cantidad = g.Cantidad,
                Precio = producto.Precio
            };
            detalle.CalcularSubtotal();
            venta.Detalles.Add(detalle);
        }

        if (!ModelState.IsValid) return await MostrarFormulario(vm);

        // 5) Métodos de negocio de la clase Venta
        venta.CalcularTotal();

        if (venta.Recibido < venta.Total)
        {
            ModelState.AddModelError("", $"El monto recibido ({venta.Recibido:N2}) es menor al total ({venta.Total:N2}). Falta {venta.Total - venta.Recibido:N2}.");
            return await MostrarFormulario(vm);
        }

        venta.CalcularCambio();
        venta.ActualizarStock();

        // 6) Guardar. SaveChangesAsync guarda venta, detalles y stock en una sola transacción:
        //    o se guarda todo, o no se guarda nada.
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = venta.Id });
    }

    // ---------- Acciones de apoyo para la pantalla de venta (devuelven JSON) ----------

    // GET: /Ventas/BuscarClientes?q=juan   (por nombre o celular)
    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string? q)
    {
        q = (q ?? "").Trim();
        if (q.Length == 0) return Json(new List<object>());

        var clientes = await _context.Clientes
            .Where(c => EF.Functions.Like(c.Nombre, $"%{q}%")
                     || (c.Celular != null && EF.Functions.Like(c.Celular, $"%{q}%")))
            .OrderBy(c => c.Nombre)
            .Take(8)
            .Select(c => new { id = c.Id, nombre = c.Nombre, celular = c.Celular })
            .ToListAsync();

        return Json(clientes);
    }

    // POST: /Ventas/RegistrarCliente   (formulario de la ventana modal)
    [HttpPost]
    public async Task<IActionResult> RegistrarCliente(string? nombre, string? celular, string? email)
    {
        nombre = nombre?.Trim();
        celular = string.IsNullOrWhiteSpace(celular) ? null : celular.Trim();
        email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        if (string.IsNullOrEmpty(nombre))
            return BadRequest(new { error = "El nombre es obligatorio." });

        if (celular != null && await _context.Clientes.AnyAsync(c => c.Celular == celular))
            return BadRequest(new { error = "Ya existe un cliente con ese celular." });

        var cliente = new Cliente { Nombre = nombre, Celular = celular, Email = email };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return Json(new { id = cliente.Id, nombre = cliente.Nombre, celular = cliente.Celular });
    }

    // GET: /Ventas/BuscarProductos?q=agua   (solo productos con stock)
    [HttpGet]
    public async Task<IActionResult> BuscarProductos(string? q)
    {
        q = (q ?? "").Trim();

        var consulta = _context.Productos.Where(p => p.Stock > 0);
        if (q.Length > 0)
            consulta = consulta.Where(p => EF.Functions.Like(p.Nombre, $"%{q}%"));

        var productos = await consulta
            .OrderBy(p => p.Nombre)
            .Take(10)
            .Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.Precio, stock = p.Stock })
            .ToListAsync();

        return Json(productos);
    }

    // ---------- Método auxiliar ----------

    // Muestra la pantalla de venta. Si hubo un error, reconstruye el cliente y la grilla
    // con lo que el usuario ya había elegido, para que no tenga que empezar de nuevo.
    private async Task<IActionResult> MostrarFormulario(NuevaVentaViewModel vm)
    {
        object? cliente = null;
        if (vm.ClienteId != null)
        {
            var clienteId = vm.ClienteId.Value;
            cliente = await _context.Clientes
                .Where(c => c.Id == clienteId)
                .Select(c => new { id = c.Id, nombre = c.Nombre, celular = c.Celular })
                .FirstOrDefaultAsync();
        }

        var ids = vm.Lineas
            .Where(l => l.ProductoId != null)
            .Select(l => l.ProductoId!.Value)
            .Distinct()
            .ToList();

        var productos = await _context.Productos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var lineas = new List<object>();
        foreach (var l in vm.Lineas)
        {
            if (l.ProductoId == null || l.Cantidad == null) continue;
            if (!productos.TryGetValue(l.ProductoId.Value, out var p)) continue;

            lineas.Add(new
            {
                productoId = p.Id,
                nombre = p.Nombre,
                precio = p.Precio,
                stock = p.Stock,
                cantidad = l.Cantidad.Value
            });
        }

        // La vista los convierte en variables de JavaScript
        ViewBag.ClienteJson = JsonSerializer.Serialize(cliente);   // "null" si no hay cliente
        ViewBag.LineasJson = JsonSerializer.Serialize(lineas);

        return View("Create", vm);
    }
}
