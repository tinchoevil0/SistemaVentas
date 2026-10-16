namespace SistemaVentas.ViewModels;

// Un ViewModel NO es una tabla: es una clase auxiliar con los datos del formulario "Nueva venta".
// Los campos son opcionales (int?, decimal?) para poder mostrar nuestros propios mensajes de error.
public class NuevaVentaViewModel
{
    public int? ClienteId { get; set; }
    public decimal? Recibido { get; set; }
    public List<LineaVenta> Lineas { get; set; } = new();
}

// Una fila del formulario: un producto y su cantidad
public class LineaVenta
{
    public int? ProductoId { get; set; }
    public int? Cantidad { get; set; }
}
