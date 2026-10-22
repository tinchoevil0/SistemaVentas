namespace SistemaVentas.ViewModels;

public class DashboardViewModel
{
    // Tarjetas superiores
    public decimal VentasHoy { get; set; }
    public int VentasHoyCantidad { get; set; }
    public decimal VentasMes { get; set; }
    public int VentasMesCantidad { get; set; }
    public decimal? VariacionMes { get; set; }   // % frente al mes anterior (null si no hay base)
    public decimal VentasAnio { get; set; }
    public decimal TicketPromedio { get; set; }
    public int TotalVentas { get; set; }
    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public int ProductosStockBajo { get; set; }

    // Paneles
    public int AlertasTotal { get; set; }
    public List<AlertaDashboard> Alertas { get; set; } = new();
    public List<TopProductoDashboard> TopProductos { get; set; } = new();
    public List<CategoriaDashboard> VentasPorCategoria { get; set; } = new();
    public List<TopClienteDashboard> TopClientes { get; set; } = new();
    public List<UltimaVentaDashboard> UltimasVentas { get; set; } = new();
}

// Tipo: "agotado", "bajo" o "rotacion" (define el color y el icono en la vista)
public record AlertaDashboard(string Tipo, string Titulo, string Detalle);
public record TopProductoDashboard(string Nombre, int Unidades, decimal Monto);
public record TopClienteDashboard(int Id, string Nombre, int Ventas, decimal Monto);
public record CategoriaDashboard(string Nombre, decimal Monto);
public record UltimaVentaDashboard(int Id, DateTime Fecha, string Cliente, decimal Total);