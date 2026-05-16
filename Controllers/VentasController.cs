using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Data;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

[Authorize]
public class VentasController : Controller
{
    private readonly ContextoSistema _contexto;
    private readonly UserManager<UsuarioAplicacion> _users;

    public VentasController(ContextoSistema contexto, UserManager<UsuarioAplicacion> users)
    {
        _contexto = contexto;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var ventas = await _contexto.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
        return View(ventas);
    }

    [Authorize(Roles = "Admin,Editor,Vendedor")]
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        ViewBag.Clientes = await _contexto.Clientes.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Productos = await _contexto.Productos.Where(p => p.Activo && p.Existencia > 0).OrderBy(p => p.Nombre).ToListAsync();
        return View();
    }

    [Authorize(Roles = "Admin,Editor,Vendedor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int clienteId, decimal descuento,
        List<int> productoId, List<decimal> cantidad, List<decimal> precioUnitario)
    {
        if (productoId == null || productoId.Count == 0)
        {
            ModelState.AddModelError("", "Debe agregar al menos un producto.");
            ViewBag.Clientes = await _contexto.Clientes.Where(c => c.Activo).ToListAsync();
            ViewBag.Productos = await _contexto.Productos.Where(p => p.Activo).ToListAsync();
            return View();
        }

        using var transaction = await _contexto.Database.BeginTransactionAsync();
        try
        {
            var detalles = new List<DetalleVenta>();
            decimal subtotal = 0;

            for (int i = 0; i < productoId.Count; i++)
            {
                var producto = await _contexto.Productos.FindAsync(productoId[i]);
                if (producto == null) continue;

                producto.Existencia -= cantidad[i];
                var detalle = new DetalleVenta
                {
                    ProductoId = productoId[i],
                    Cantidad = cantidad[i],
                    PrecioUnitario = precioUnitario[i],
                    Subtotal = cantidad[i] * precioUnitario[i]
                };
                detalles.Add(detalle);
                subtotal += detalle.Subtotal;
            }

            decimal impuesto = subtotal * 0.12m;
            decimal total = subtotal + impuesto - descuento;

            var correlativo = (await _contexto.Ventas.CountAsync()) + 1;
            var venta = new Venta
            {
                NumeroVenta = $"VTA-{correlativo:D6}",
                ClienteId = clienteId,
                UsuarioId = _users.GetUserId(User),
                Subtotal = subtotal,
                Impuesto = impuesto,
                Descuento = descuento,
                Total = total,
                Detalles = detalles
            };

            _contexto.Ventas.Add(venta);
            await _contexto.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Detalle), new { id = venta.Id });
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Error al procesar la venta.");
            ViewBag.Clientes = await _contexto.Clientes.Where(c => c.Activo).ToListAsync();
            ViewBag.Productos = await _contexto.Productos.Where(p => p.Activo).ToListAsync();
            return View();
        }
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _contexto.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return NotFound();
        return View(venta);
    }
}
