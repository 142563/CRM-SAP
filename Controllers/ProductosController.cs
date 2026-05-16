using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Data;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly ContextoSistema _contexto;

    public ProductosController(ContextoSistema contexto) => _contexto = contexto;

    public async Task<IActionResult> Index()
    {
        return View(await _contexto.Productos.OrderBy(p => p.Nombre).ToListAsync());
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpGet]
    public IActionResult Crear() => View(new Producto());

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Producto modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        _contexto.Productos.Add(modelo);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _contexto.Productos.FindAsync(id);
        if (producto == null) return NotFound();
        return View(producto);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Producto modelo)
    {
        if (id != modelo.Id) return BadRequest();
        if (!ModelState.IsValid) return View(modelo);
        _contexto.Update(modelo);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
