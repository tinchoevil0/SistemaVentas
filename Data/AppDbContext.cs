using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;

namespace SistemaVentas.Data;

public class AppDbContext : DbContext
{
    // Constructor que recibe las opciones de configuración (como la conexión a la base de datos)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Define los DbSet, que se traducirán en las tablas de la base de datos
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    // Método para configurar restricciones, relaciones y datos iniciales (Seed Data)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Inserción de datos iniciales para la tabla Categorias (se ejecutan con la migración)
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Bebidas" },
            new Categoria { Id = 2, Nombre = "Abarrotes" },
            new Categoria { Id = 3, Nombre = "Limpieza" }
        );

        // 2. Inserción de datos iniciales para la tabla Productos
        // Nota: Asegúrate de que los 'CategoriaId' coincidan con los IDs de las categorías creadas arriba
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Agua 2L", Precio = 8m, Stock = 50, StockMinimo = 10, CategoriaId = 1 },
            new Producto { Id = 2, Nombre = "Arroz 1kg", Precio = 12m, Stock = 30, StockMinimo = 5, CategoriaId = 2 },
            new Producto { Id = 3, Nombre = "Detergente 1kg", Precio = 25m, Stock = 20, StockMinimo = 5, CategoriaId = 3 }
        );
    }
}