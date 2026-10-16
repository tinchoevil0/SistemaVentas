using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/productos")]
public class ProductosApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosApiController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/productos
    [HttpGet]
    public async Task<ActionResult<List<Producto>>> GetAll()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    // GET api/productos/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null) return NotFound();
        return producto;
    }

    // POST api/productos
    // Body: { "nombre": "Leche 1L", "precio": 9.5, "stock": 40, "stockMinimo": 8, "categoriaId": 1 }
    [HttpPost]
    public async Task<ActionResult<Producto>> Create(Producto producto)
    {
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
            return BadRequest("La categoría indicada no existe.");

        producto.Categoria = null; // solo usamos CategoriaId; evita insertar una categoría por error
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    // PUT api/productos/4   (mismo cuerpo que POST, incluyendo "id")
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Producto producto)
    {
        if (id != producto.Id) return BadRequest("El id de la URL no coincide con el del cuerpo.");
        if (!await _context.Productos.AnyAsync(p => p.Id == id)) return NotFound();
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
            return BadRequest("La categoría indicada no existe.");

        producto.Categoria = null;
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/productos/4
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        if (await _context.DetallesVenta.AnyAsync(d => d.ProductoId == id))
            return Conflict("No se puede eliminar: el producto tiene ventas registradas.");

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
