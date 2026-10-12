namespace SistemaVentas.Models;

public class Producto
{
    // Atributos
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }

    // Relación: cada producto pertenece a una categoría
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // Métodos
    public bool TieneStock()
    {
        return Stock > 0;
    }
}
