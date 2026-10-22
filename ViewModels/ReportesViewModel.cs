namespace SistemaVentas.ViewModels;

public class ReportesViewModel
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteSeleccionado { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TicketPromedio { get; set; }
    public List<ReporteVenta> Ventas { get; set; } = new();
    public List<ReporteProductoVendido> ProductosVendidos { get; set; } = new();
    public List<ReporteStock> Inventario { get; set; } = new();
    public List<ReporteCompraCliente> ComprasCliente { get; set; } = new();
    public List<ReporteCliente> Clientes { get; set; } = new();
}

public record ReporteVenta(int Id, DateTime Fecha, string Cliente, decimal Total);
public record ReporteProductoVendido(string Nombre, string Categoria, int Unidades, decimal Monto);
public record ReporteStock(int Id, string Nombre, string Categoria, int Stock, int StockMinimo, decimal Precio);
public record ReporteCompraCliente(int Id, DateTime Fecha, decimal Total);
public record ReporteCliente(int Id, string Nombre, string? Celular, string? Email);
