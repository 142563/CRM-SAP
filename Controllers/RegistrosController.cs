using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Data;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

[Authorize]
public class RegistrosController : Controller
{
    private readonly ContextoSistema _contexto;
    private readonly UserManager<UsuarioAplicacion> _users;

    public RegistrosController(ContextoSistema contexto, UserManager<UsuarioAplicacion> users)
    {
        _contexto = contexto;
        _users = users;
    }

    public async Task<IActionResult> Index(int moduloId)
    {
        var modulo = await _contexto.ModulosDinamicos
            .Include(m => m.Registros)
            .FirstOrDefaultAsync(m => m.Id == moduloId);
        if (modulo == null) return NotFound();
        ViewBag.Modulo = modulo;
        return View(modulo.Registros.OrderByDescending(r => r.FechaCreacion).ToList());
    }

    [Authorize(Roles = "Admin,Editor,Vendedor")]
    [HttpGet]
    public async Task<IActionResult> Crear(int moduloId)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(moduloId);
        if (modulo == null) return NotFound();
        ViewBag.Modulo = modulo;
        return View(new RegistroDinamico { ModuloDinamicoId = moduloId, Datos = new Dictionary<string, string>() });
    }

    [Authorize(Roles = "Admin,Editor,Vendedor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int moduloId, IFormCollection form)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(moduloId);
        if (modulo == null) return NotFound();

        var datos = new Dictionary<string, string>();
        foreach (var campo in modulo.EsquemaCampos)
        {
            datos[campo.Key] = form[campo.Key].ToString();
        }

        var registro = new RegistroDinamico
        {
            ModuloDinamicoId = moduloId,
            Datos = datos,
            UsuarioCreacionId = _users.GetUserId(User)
        };

        _contexto.RegistrosDinamicos.Add(registro);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { moduloId });
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var registro = await _contexto.RegistrosDinamicos
            .Include(r => r.Modulo)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (registro == null) return NotFound();
        ViewBag.Modulo = registro.Modulo;
        return View(registro);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, IFormCollection form)
    {
        var registro = await _contexto.RegistrosDinamicos
            .Include(r => r.Modulo)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (registro == null) return NotFound();

        var datos = new Dictionary<string, string>();
        foreach (var campo in registro.Modulo!.EsquemaCampos)
        {
            datos[campo.Key] = form[campo.Key].ToString();
        }
        registro.Datos = datos;
        _contexto.Update(registro);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { moduloId = registro.ModuloDinamicoId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var registro = await _contexto.RegistrosDinamicos.FindAsync(id);
        if (registro != null)
        {
            int moduloId = registro.ModuloDinamicoId;
            _contexto.RegistrosDinamicos.Remove(registro);
            await _contexto.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { moduloId });
        }
        return RedirectToAction("Index", "Modulos");
    }
}
