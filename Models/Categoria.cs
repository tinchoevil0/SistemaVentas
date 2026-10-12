namespace SistemaVentas.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // Relación: una categoría tiene muchos productos
    public List<Producto> Productos { get; set; } = new();
}
