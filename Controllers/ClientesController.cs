using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Data;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly ContextoSistema _contexto;

    public ClientesController(ContextoSistema contexto) => _contexto = contexto;

    public async Task<IActionResult> Index()
    {
        return View(await _contexto.Clientes.OrderBy(c => c.Nombre).ToListAsync());
    }

    [Authorize(Roles = "Admin,Editor,Vendedor")]
    [HttpGet]
    public IActionResult Crear() => View(new Cliente());

    [Authorize(Roles = "Admin,Editor,Vendedor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cliente modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        _contexto.Clientes.Add(modelo);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
