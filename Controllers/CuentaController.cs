using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Controllers;

public class CuentaController : Controller
{
    private readonly SignInManager<UsuarioAplicacion> _signIn;
    private readonly UserManager<UsuarioAplicacion> _users;

    public CuentaController(SignInManager<UsuarioAplicacion> signIn, UserManager<UsuarioAplicacion> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Modulos");
        return View(new LoginVista { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVista modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _signIn.PasswordSignInAsync(modelo.Email, modelo.Password, false, false);
        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return Redirect(modelo.ReturnUrl);
            return RedirectToAction("Index", "Modulos");
        }

        ModelState.AddModelError("", "Correo o contraseña incorrectos.");
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult AccesoDenegado() => View();
}
