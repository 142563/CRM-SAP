using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Data;

public static class SembradorDatos
{
    public static async Task CargarDatosIniciales(IServiceProvider servicios)
    {
        using var scope = servicios.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoSistema>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioAplicacion>>();

        string[] roles = { "Admin", "Editor", "Viewer", "Vendedor" };
        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        if (await userManager.FindByEmailAsync("admin@umg.com") == null)
        {
            var admin = new UsuarioAplicacion
            {
                UserName = "admin@umg.com",
                Email = "admin@umg.com",
                NombreCompleto = "Administrador del Sistema",
                EmailConfirmed = true
            };
            var resultado = await userManager.CreateAsync(admin, "Admin123*");
            if (resultado.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!await contexto.ModulosDinamicos.AnyAsync())
        {
            contexto.ModulosDinamicos.Add(new ModuloDinamico
            {
                NombreModulo = "Ventas Dinámicas",
                Descripcion = "Módulo de ejemplo para gestión dinámica",
                EsquemaCampos = new Dictionary<string, CampoDinamico>
                {
                    ["descripcion"] = new CampoDinamico { NombreCampo = "descripcion", Etiqueta = "Descripción", TipoCampo = "text", Requerido = true },
                    ["monto"] = new CampoDinamico { NombreCampo = "monto", Etiqueta = "Monto", TipoCampo = "number", Requerido = true }
                }
            });
        }

        if (!await contexto.Clientes.AnyAsync())
        {
            contexto.Clientes.Add(new Cliente
            {
                Nit = "CF",
                Nombre = "Consumidor Final",
                Telefono = "00000000",
                Direccion = "Ciudad",
                Activo = true
            });
        }

        if (!await contexto.Productos.AnyAsync())
        {
            contexto.Productos.AddRange(
                new Producto
                {
                    Codigo = "P001",
                    Nombre = "Producto 001",
                    Descripcion = "Producto de ejemplo 1",
                    PrecioVenta = 100.00m,
                    Existencia = 50,
                    Activo = true
                },
                new Producto
                {
                    Codigo = "P002",
                    Nombre = "Producto 002",
                    Descripcion = "Producto de ejemplo 2",
                    PrecioVenta = 75.00m,
                    Existencia = 30,
                    Activo = true
                }
            );
        }

        await contexto.SaveChangesAsync();
    }
}
