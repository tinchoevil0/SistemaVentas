using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class ProductosController : Controller
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    private void CargarCategorias(int? seleccionada = null)
    {
        ViewBag.Categorias = new SelectList(
            _context.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", seleccionada);
    }

    // GET: /Productos
    public async Task<IActionResult> Index()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
        return View(productos);
    }

    // GET: /Productos/Create
    public IActionResult Create()
    {
        CargarCategorias();
        return View(new Producto());
    }

    // POST: /Productos/Create
    [HttpPost]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (!ModelState.IsValid)
        {
            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();
        CargarCategorias(producto.CategoriaId);
        return View(producto);
    }

    // POST: /Productos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
