using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /  -> Dashboard
    // Nota: SQLite no suma columnas decimal en SQL, por eso se traen solo las columnas
    // necesarias (Fecha, Total...) y se suma en memoria. Con pocos miles de ventas es instantáneo.
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);
        var inicioAnio = new DateTime(hoy.Year, 1, 1);
        var desde30 = hoy.AddDays(-29); // últimos 30 días incluyendo hoy

        var vm = new DashboardViewModel();

        // ── Ventas del año (de aquí salen hoy, mes y año) ──
        var ventasAnio = await _context.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= inicioAnio)
            .Select(v => new { v.Fecha, v.Total })
            .ToListAsync();

        var ventasHoy = ventasAnio.Where(v => v.Fecha >= hoy).ToList();
        var ventasMes = ventasAnio.Where(v => v.Fecha >= inicioMes).ToList();

        vm.VentasHoy = ventasHoy.Sum(v => v.Total);
        vm.VentasHoyCantidad = ventasHoy.Count;
        vm.VentasMes = ventasMes.Sum(v => v.Total);
        vm.VentasMesCantidad = ventasMes.Count;
        vm.VentasAnio = ventasAnio.Sum(v => v.Total);
        vm.TicketPromedio = ventasMes.Count > 0 ? vm.VentasMes / ventasMes.Count : 0;
        vm.TotalVentas = await _context.Ventas.CountAsync();
        vm.TotalProductos = await _context.Productos.CountAsync();

        // ── Variación frente al mes anterior ──
        var totalMesAnterior = (await _context.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= inicioMesAnterior && v.Fecha < inicioMes)
            .Select(v => v.Total)
            .ToListAsync()).Sum();

        if (totalMesAnterior > 0)
            vm.VariacionMes = decimal.Round((vm.VentasMes - totalMesAnterior) / totalMesAnterior * 100, 1);

        vm.TotalClientes = await _context.Clientes.CountAsync();

        // ── Alertas: stock bajo / agotado ──
        var stockBajoQuery = _context.Productos.AsNoTracking()
            .Where(p => p.Stock <= p.StockMinimo);

        vm.ProductosStockBajo = await stockBajoQuery.CountAsync();

        var stockBajo = await stockBajoQuery
            .OrderBy(p => p.Stock)
            .Take(5)
            .Select(p => new { p.Nombre, p.Stock, p.StockMinimo })
            .ToListAsync();

        foreach (var p in stockBajo)
        {
            vm.Alertas.Add(p.Stock <= 0
                ? new AlertaDashboard("agotado", "Producto agotado", $"{p.Nombre} — sin stock (mínimo {p.StockMinimo})")
                : new AlertaDashboard("bajo", "Stock bajo", $"{p.Nombre} — quedan {p.Stock} (mínimo {p.StockMinimo})"));
        }

        // ── Alertas: productos sin ventas en 30 días ──
        var sinRotacionQuery = _context.Productos.AsNoTracking()
            .Where(p => p.Stock > p.StockMinimo
                     && !_context.DetallesVenta.Any(d => d.ProductoId == p.Id && d.Venta!.Fecha >= desde30));

        var sinRotacionTotal = await sinRotacionQuery.CountAsync();

        var sinRotacion = await sinRotacionQuery
            .OrderByDescending(p => p.Stock)
            .Take(3)
            .Select(p => new { p.Nombre, p.Stock })
            .ToListAsync();

        foreach (var p in sinRotacion)
            vm.Alertas.Add(new AlertaDashboard("rotacion", "Sin rotación", $"{p.Nombre} — stock {p.Stock}, sin ventas en 30 días"));

        vm.AlertasTotal = vm.ProductosStockBajo + sinRotacionTotal;

        // ── Top productos y ventas por categoría (últimos 30 días) ──
        var detalles = await _context.DetallesVenta.AsNoTracking()
            .Where(d => d.Venta!.Fecha >= desde30)
            .Select(d => new
            {
                Producto = d.Producto!.Nombre,
                Categoria = d.Producto!.Categoria!.Nombre,
                d.Cantidad,
                d.Subtotal
            })
            .ToListAsync();

        vm.TopProductos = detalles
            .GroupBy(d => d.Producto)
            .Select(g => new TopProductoDashboard(g.Key, g.Sum(x => x.Cantidad), g.Sum(x => x.Subtotal)))
            .OrderByDescending(t => t.Unidades)
            .Take(5)
            .ToList();

        vm.VentasPorCategoria = detalles
            .GroupBy(d => d.Categoria)
            .Select(g => new CategoriaDashboard(g.Key, g.Sum(x => x.Subtotal)))
            .OrderByDescending(c => c.Monto)
            .ToList();

        var clientesConVentas = await _context.Ventas.AsNoTracking()
            .Select(v => new { v.ClienteId, Cliente = v.Cliente!.Nombre, v.Total })
            .ToListAsync();
        vm.TopClientes = clientesConVentas
            .GroupBy(v => new { v.ClienteId, v.Cliente })
            .Select(g => new TopClienteDashboard(g.Key.ClienteId, g.Key.Cliente, g.Count(), g.Sum(v => v.Total)))
            .OrderByDescending(c => c.Monto)
            .Take(5)
            .ToList();

        // ── Últimas ventas ──
        var ultimas = await _context.Ventas.AsNoTracking()
            .OrderByDescending(v => v.Fecha)
            .Take(6)
            .Select(v => new { v.Id, v.Fecha, Cliente = v.Cliente!.Nombre, v.Total })
            .ToListAsync();

        vm.UltimasVentas = ultimas
            .Select(v => new UltimaVentaDashboard(v.Id, v.Fecha, v.Cliente, v.Total))
            .ToList();

        return View(vm);
    }

    // GET: /Home/VentasPorDia?dias=7  -> datos para el gráfico (JSON)
    [HttpGet]
    public async Task<IActionResult> VentasPorDia(int dias = 7)
    {
        if (dias != 7 && dias != 15 && dias != 30) dias = 7;

        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-(dias - 1));

        var ventas = await _context.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= desde)
            .Select(v => new { v.Fecha, v.Total })
            .ToListAsync();

        var labels = new List<string>();
        var totales = new List<decimal>();
        var transacciones = new List<int>();

        for (var dia = desde; dia <= hoy; dia = dia.AddDays(1))
        {
            var delDia = ventas.Where(v => v.Fecha.Date == dia).ToList();
            labels.Add(dia.ToString("dd/MM"));
            totales.Add(delDia.Sum(v => v.Total));
            transacciones.Add(delDia.Count);
        }

        return Json(new { labels, totales, transacciones });
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return Content("Ocurrió un error. Intenta nuevamente.");
    }
}