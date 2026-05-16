using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CRM_ERP_UMG.Models;

namespace CRM_ERP_UMG.Data;

public class ContextoSistema : IdentityDbContext<UsuarioAplicacion>
{
    public ContextoSistema(DbContextOptions<ContextoSistema> options) : base(options) { }

    public DbSet<ModuloDinamico> ModulosDinamicos => Set<ModuloDinamico>();
    public DbSet<RegistroDinamico> RegistrosDinamicos => Set<RegistroDinamico>();
    public DbSet<OperacionDinamica> OperacionesDinamicas => Set<OperacionDinamica>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ModuloDinamico>()
            .Property(m => m.EsquemaCampos)
            .HasColumnType("jsonb");

        builder.Entity<RegistroDinamico>()
            .Property(r => r.Datos)
            .HasColumnType("jsonb");

        builder.Entity<OperacionDinamica>()
            .Property(o => o.ColumnasVisibles)
            .HasColumnType("jsonb");

        builder.Entity<OperacionDinamica>()
            .Property(o => o.Formulas)
            .HasColumnType("jsonb");

        builder.Entity<RegistroDinamico>()
            .HasOne(r => r.Modulo)
            .WithMany(m => m.Registros)
            .HasForeignKey(r => r.ModuloDinamicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OperacionDinamica>()
            .HasOne(o => o.Modulo)
            .WithMany(m => m.Operaciones)
            .HasForeignKey(o => o.ModuloDinamicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Venta>()
            .HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
