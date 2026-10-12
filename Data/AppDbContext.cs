using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;

namespace SistemaVentas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Datos iniciales (se insertan con la migración)
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Bebidas" },
            new Categoria { Id = 2, Nombre = "Abarrotes" },
            new Categoria { Id = 3, Nombre = "Limpieza" }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Agua 2L", Precio = 8m, Stock = 50, StockMinimo = 10, CategoriaId = 1 },
            new Producto { Id = 2, Nombre = "Arroz 1kg", Precio = 12m, Stock = 30, StockMinimo = 5, CategoriaId = 2 },
            new Producto { Id = 3, Nombre = "Detergente 1kg", Precio = 25m, Stock = 20, StockMinimo = 5, CategoriaId = 3 }
        );
    }
}
