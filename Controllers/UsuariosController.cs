using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

[Authorize(Roles = "Admin")]
public class UsuariosController : Controller
{
    private readonly UserManager<UsuarioAplicacion> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public UsuariosController(UserManager<UsuarioAplicacion> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _users.Users.ToListAsync();
        var lista = new List<UsuarioListadoVista>();
        foreach (var u in usuarios)
        {
            lista.Add(new UsuarioListadoVista
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email ?? "",
                Roles = (await _users.GetRolesAsync(u)).ToList()
            });
        }
        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var modelo = new CrearUsuarioVista
        {
            RolesDisponibles = await _roles.Roles.Select(r => r.Name!).ToListAsync()
        };
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioVista modelo)
    {
        modelo.RolesDisponibles = await _roles.Roles.Select(r => r.Name!).ToListAsync();
        if (!ModelState.IsValid) return View(modelo);

        var usuario = new UsuarioAplicacion
        {
            UserName = modelo.Email,
            Email = modelo.Email,
            NombreCompleto = modelo.NombreCompleto,
            EmailConfirmed = true
        };

        var resultado = await _users.CreateAsync(usuario, modelo.Password);
        if (!resultado.Succeeded)
        {
            foreach (var e in resultado.Errors)
                ModelState.AddModelError("", e.Description);
            return View(modelo);
        }

        await _users.AddToRoleAsync(usuario, modelo.Rol);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string userId, string nuevoRol)
    {
        var usuario = await _users.FindByIdAsync(userId);
        if (usuario == null) return NotFound();

        var rolesActuales = await _users.GetRolesAsync(usuario);
        await _users.RemoveFromRolesAsync(usuario, rolesActuales);
        await _users.AddToRoleAsync(usuario, nuevoRol);
        return RedirectToAction(nameof(Index));
    }
}
