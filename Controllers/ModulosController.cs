using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Data;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

[Authorize]
public class ModulosController : Controller
{
    private readonly ContextoSistema _contexto;

    public ModulosController(ContextoSistema contexto) => _contexto = contexto;

    public async Task<IActionResult> Index()
    {
        var modulos = await _contexto.ModulosDinamicos
            .Include(m => m.Registros)
            .Include(m => m.Operaciones)
            .ToListAsync();
        return View(modulos);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpGet]
    public IActionResult Crear() => View(new ModuloDinamico());

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ModuloDinamico modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        modelo.EsquemaCampos = new Dictionary<string, CampoDinamico>();
        _contexto.ModulosDinamicos.Add(modelo);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Detalle), new { id = modelo.Id });
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var modulo = await _contexto.ModulosDinamicos
            .Include(m => m.Registros)
            .Include(m => m.Operaciones)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (modulo == null) return NotFound();
        return View(modulo);
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarCampo(int id, string etiqueta, string tipoCampo, bool requerido)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(id);
        if (modulo == null) return NotFound();

        var clave = NormalizarClave(etiqueta);
        if (!modulo.EsquemaCampos.ContainsKey(clave))
        {
            modulo.EsquemaCampos[clave] = new CampoDinamico
            {
                NombreCampo = clave,
                Etiqueta = etiqueta,
                TipoCampo = tipoCampo,
                Requerido = requerido
            };
            _contexto.Update(modulo);
            await _contexto.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCampo(int id, string clave)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(id);
        if (modulo == null) return NotFound();
        modulo.EsquemaCampos.Remove(clave);
        _contexto.Update(modulo);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarModulo(int id)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(id);
        if (modulo != null)
        {
            _contexto.ModulosDinamicos.Remove(modulo);
            await _contexto.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private static string NormalizarClave(string texto)
    {
        return new string(texto.ToLower()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => c < 128 && (char.IsLetterOrDigit(c) || c == '_'))
            .ToArray())
            .Replace(' ', '_');
    }
}
