using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Data;
using CRM_ERP_UMG.Models;
using CRM_ERP_UMG.Services;

namespace CRM_ERP_UMG.Controllers;

[Authorize]
public class OperacionesController : Controller
{
    private readonly ContextoSistema _contexto;
    private readonly ServicioFormulas _formulas;

    public OperacionesController(ContextoSistema contexto, ServicioFormulas formulas)
    {
        _contexto = contexto;
        _formulas = formulas;
    }

    public async Task<IActionResult> Index(int moduloId)
    {
        var modulo = await _contexto.ModulosDinamicos
            .Include(m => m.Operaciones)
            .FirstOrDefaultAsync(m => m.Id == moduloId);
        if (modulo == null) return NotFound();
        ViewBag.Modulo = modulo;
        return View(modulo.Operaciones.ToList());
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpGet]
    public async Task<IActionResult> Crear(int moduloId)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(moduloId);
        if (modulo == null) return NotFound();
        ViewBag.Modulo = modulo;
        return View(new OperacionDinamica { ModuloDinamicoId = moduloId });
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int moduloId, string nombreOperacion,
        List<string> columnasVisibles, List<string> nombreResultado, List<string> expresion)
    {
        var modulo = await _contexto.ModulosDinamicos.FindAsync(moduloId);
        if (modulo == null) return NotFound();

        var formulas = new List<FormulaDinamica>();
        for (int i = 0; i < nombreResultado.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(nombreResultado[i]) && !string.IsNullOrWhiteSpace(expresion[i]))
            {
                formulas.Add(new FormulaDinamica
                {
                    NombreResultado = nombreResultado[i],
                    Expresion = expresion[i]
                });
            }
        }

        var operacion = new OperacionDinamica
        {
            ModuloDinamicoId = moduloId,
            NombreOperacion = nombreOperacion,
            ColumnasVisibles = columnasVisibles ?? new List<string>(),
            Formulas = formulas
        };

        _contexto.OperacionesDinamicas.Add(operacion);
        await _contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { moduloId });
    }

    public async Task<IActionResult> Resultado(int id)
    {
        var operacion = await _contexto.OperacionesDinamicas
            .Include(o => o.Modulo)
            .ThenInclude(m => m!.Registros)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (operacion == null) return NotFound();

        var resultados = new List<Dictionary<string, string>>();
        foreach (var registro in operacion.Modulo!.Registros)
        {
            var fila = new Dictionary<string, string>();
            foreach (var col in operacion.ColumnasVisibles)
            {
                fila[col] = registro.Datos.TryGetValue(col, out var v) ? v : "";
            }
            foreach (var formula in operacion.Formulas)
            {
                var vars = new Dictionary<string, decimal>();
                foreach (var d in registro.Datos)
                {
                    if (decimal.TryParse(d.Value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal num))
                        vars[d.Key] = num;
                }
                fila[formula.NombreResultado] = _formulas.Evaluar(formula.Expresion, vars).ToString("F2");
            }
            resultados.Add(fila);
        }

        ViewBag.Operacion = operacion;
        return View(resultados);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var operacion = await _contexto.OperacionesDinamicas.FindAsync(id);
        if (operacion != null)
        {
            int moduloId = operacion.ModuloDinamicoId;
            _contexto.OperacionesDinamicas.Remove(operacion);
            await _contexto.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { moduloId });
        }
        return RedirectToAction("Index", "Modulos");
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
