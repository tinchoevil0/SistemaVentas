using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.ViewModels;

namespace SistemaVentas.Controllers;

public class ReportesController : Controller
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, int? clienteId)
    {
        var fechaHasta = (hasta ?? DateTime.Today).Date;
        var fechaDesde = (desde ?? new DateTime(fechaHasta.Year, fechaHasta.Month, 1)).Date;

        if (fechaDesde > fechaHasta)
            return BadRequest("La fecha inicial no puede ser posterior a la fecha final.");

        var clientes = await _context.Clientes.AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new ReporteCliente(c.Id, c.Nombre, c.Celular, c.Email))
            .ToListAsync();

        string? nombreCliente = null;
        if (clienteId.HasValue)
        {
            nombreCliente = clientes.FirstOrDefault(c => c.Id == clienteId.Value)?.Nombre;
            if (nombreCliente == null)
                return NotFound();
        }

        var finExclusivo = fechaHasta.AddDays(1);
        var ventasPeriodo = await _context.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= fechaDesde && v.Fecha < finExclusivo)
            .OrderByDescending(v => v.Fecha)
            .Select(v => new ReporteVenta(v.Id, v.Fecha, v.Cliente!.Nombre, v.Total))
            .ToListAsync();

        var detallesPeriodo = await _context.DetallesVenta.AsNoTracking()
            .Where(d => d.Venta!.Fecha >= fechaDesde && d.Venta.Fecha < finExclusivo)
            .Select(d => new
            {
                Producto = d.Producto!.Nombre,
                Categoria = d.Producto.Categoria!.Nombre,
                d.Cantidad,
                d.Subtotal
            })
            .ToListAsync();

        var productosVendidos = detallesPeriodo
            .GroupBy(d => new { d.Producto, d.Categoria })
            .Select(g => new ReporteProductoVendido(g.Key.Producto, g.Key.Categoria, g.Sum(d => d.Cantidad), g.Sum(d => d.Subtotal)))
            .OrderByDescending(p => p.Unidades)
            .ToList();

        var inventario = await _context.Productos.AsNoTracking()
            .OrderBy(p => p.Categoria!.Nombre)
            .ThenBy(p => p.Nombre)
            .Select(p => new ReporteStock(p.Id, p.Nombre, p.Categoria!.Nombre, p.Stock, p.StockMinimo, p.Precio))
            .ToListAsync();

        var comprasCliente = new List<ReporteCompraCliente>();
        if (clienteId.HasValue)
        {
            comprasCliente = await _context.Ventas.AsNoTracking()
                .Where(v => v.ClienteId == clienteId.Value)
                .OrderByDescending(v => v.Fecha)
                .Select(v => new ReporteCompraCliente(v.Id, v.Fecha, v.Total))
                .ToListAsync();
        }

        var vm = new ReportesViewModel
        {
            Desde = fechaDesde,
            Hasta = fechaHasta,
            ClienteId = clienteId,
            ClienteSeleccionado = nombreCliente,
            TotalVentas = ventasPeriodo.Sum(v => v.Total),
            TicketPromedio = ventasPeriodo.Count == 0 ? 0 : ventasPeriodo.Sum(v => v.Total) / ventasPeriodo.Count,
            Ventas = ventasPeriodo,
            ProductosVendidos = productosVendidos,
            Inventario = inventario,
            ComprasCliente = comprasCliente,
            Clientes = clientes
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarCsv(DateTime? desde, DateTime? hasta)
    {
        var fechaHasta = (hasta ?? DateTime.Today).Date;
        var fechaDesde = (desde ?? new DateTime(fechaHasta.Year, fechaHasta.Month, 1)).Date;

        if (fechaDesde > fechaHasta)
            return BadRequest("La fecha inicial no puede ser posterior a la fecha final.");

        var finExclusivo = fechaHasta.AddDays(1);
        var ventas = await _context.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= fechaDesde && v.Fecha < finExclusivo)
            .OrderBy(v => v.Fecha)
            .Select(v => new { v.Id, v.Fecha, Cliente = v.Cliente!.Nombre, v.Total })
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Venta,Fecha,Cliente,Total");
        foreach (var venta in ventas)
        {
            csv.Append(venta.Id).Append(',')
                .Append(venta.Fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(',')
                .Append(EscaparCsv(venta.Cliente)).Append(',')
                .Append(venta.Total.ToString(CultureInfo.InvariantCulture)).AppendLine();
        }

        var nombreArchivo = $"ventas_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv; charset=utf-8", nombreArchivo);
    }

    private static string EscaparCsv(string valor)
    {
        if (valor.Length > 0 && "=+-@\t\r".Contains(valor[0]))
            valor = "'" + valor;
        return $"\"{valor.Replace("\"", "\"\"")}\"";
    }
}
