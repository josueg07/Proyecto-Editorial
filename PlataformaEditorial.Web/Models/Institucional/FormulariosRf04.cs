using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace PlataformaEditorial.Web.Models.Institucional;

public class PerfilInstitucionalEntrada
{
    [Required, StringLength(200), Display(Name="Nombre de la institución")]
    public string NombreInstitucion { get; set; } = "";
    [Required, StringLength(20), Display(Name="NIT")]
    public string NIT { get; set; } = "";
    [Required, StringLength(255), Display(Name="Dirección")]
    public string Direccion { get; set; } = "";
}
public class SolicitudInstitucionalEntrada
{
    [Required, DataType(DataType.Date), Display(Name="Fecha requerida")]
    public DateOnly? FechaRequerida { get; set; }
    [StringLength(500)] public string? Observaciones { get; set; }
    [Required, MinLength(1), MaxLength(20)] public List<ObraSolicitadaEntrada> Detalles { get; set; } = [new()];
}
public class ObraSolicitadaEntrada
{
    [Required, StringLength(200), Display(Name="Título u obra")]
    public string Titulo { get; set; } = "";
    [Range(1,int.MaxValue), Display(Name="Cantidad estimada")]
    public int Cantidad { get; set; } = 1;
    [Required, StringLength(100), Display(Name="Acabado de portada")]
    public string AcabadoPortada { get; set; } = "";
    [Required, StringLength(100), Display(Name="Tipo de papel")]
    public string TipoPapel { get; set; } = "";
}
public class AsignacionEntrada
{
    [Required, Display(Name="Asesor de ventas")] public string AsesorId { get; set; } = "";
}
public class CotizacionInstitucionalEntrada
{
    [Range(typeof(decimal),"0","9999999999.99"), Display(Name="Descuento comercial (Q)")]
    public decimal Descuento { get; set; }
    [Required, DataType(DataType.Date), Display(Name="Válida hasta")]
    public DateOnly? FechaVencimiento { get; set; }
    [StringLength(500)] public string? Observaciones { get; set; }
    [Required, MinLength(1), MaxLength(20)] public List<LineaCotizacionEntrada> Detalles { get; set; } = [];
}
public class LineaCotizacionEntrada
{
    [Range(1,int.MaxValue)] public int DetalleSolicitudId { get; set; }
    [Required, StringLength(255), Display(Name="Descripción")]
    public string Descripcion { get; set; } = "";
    [Range(1,int.MaxValue)] public int Cantidad { get; set; }
    [Range(typeof(decimal),"0","99999999.99"), Display(Name="Precio unitario (Q)")]
    public decimal PrecioUnitario { get; set; }
}
public static class ValidacionRf04
{
    // También protege a los servicios cuando se invocan sin un controlador MVC.
    public static void Comprobar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        if (!Validator.TryValidateObject(modelo,new ValidationContext(modelo),resultados,true))
            throw new ReglaRf04Exception(string.Join(" ",resultados.Select(r=>r.ErrorMessage)));
    }
}
