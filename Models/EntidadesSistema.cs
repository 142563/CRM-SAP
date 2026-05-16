using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_ERP_UMG.Models;

public class UsuarioAplicacion : IdentityUser
{
    [Required]
    public string NombreCompleto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public class CampoDinamico
{
    public string NombreCampo { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string TipoCampo { get; set; } = "text";
    public bool Requerido { get; set; } = false;
}

public class ModuloDinamico
{
    public int Id { get; set; }
    [Required]
    public string NombreModulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Dictionary<string, CampoDinamico> EsquemaCampos { get; set; } = new();
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public ICollection<RegistroDinamico> Registros { get; set; } = new List<RegistroDinamico>();
    public ICollection<OperacionDinamica> Operaciones { get; set; } = new List<OperacionDinamica>();
}

public class RegistroDinamico
{
    public int Id { get; set; }
    public int ModuloDinamicoId { get; set; }
    public ModuloDinamico? Modulo { get; set; }
    public Dictionary<string, string> Datos { get; set; } = new();
    public string? UsuarioCreacionId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public class FormulaDinamica
{
    public string NombreResultado { get; set; } = string.Empty;
    public string Expresion { get; set; } = string.Empty;
}

public class OperacionDinamica
{
    public int Id { get; set; }
    public int ModuloDinamicoId { get; set; }
    public ModuloDinamico? Modulo { get; set; }
    [Required]
    public string NombreOperacion { get; set; } = string.Empty;
    public List<string> ColumnasVisibles { get; set; } = new();
    public List<FormulaDinamica> Formulas { get; set; } = new();
}

public class Cliente
{
    public int Id { get; set; }
    [Required]
    public string Nit { get; set; } = string.Empty;
    [Required]
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

public class Producto
{
    public int Id { get; set; }
    [Required]
    public string Codigo { get; set; } = string.Empty;
    [Required]
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioVenta { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Existencia { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}

public class Venta
{
    public int Id { get; set; }
    [Required]
    public string NumeroVenta { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string? UsuarioId { get; set; }
    public UsuarioAplicacion? Usuario { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Impuesto { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Emitida";
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}

public class DetalleVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Cantidad { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
