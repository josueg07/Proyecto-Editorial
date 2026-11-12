using Microsoft.AspNetCore.Mvc.Rendering;
namespace PlataformaEditorial.Web.Models.Institucional;
public record PerfilInstitucionalVista(string UsuarioId,string NombreInstitucion,string NIT,string Direccion,string Estado,DateTime? FechaValidacion);
public record SolicitudFila(int Id,DateTime Fecha,DateOnly FechaRequerida,string Estado,string Institucion,string? Asesor,int? CotizacionId,string? EstadoCotizacion);
public record ObraSolicitadaVista(int Id,string Titulo,int Cantidad,string AcabadoPortada,string TipoPapel);
public record LineaCotizacionVista(string Descripcion,string Especificaciones,int Cantidad,decimal PrecioUnitario)
{
    public decimal Importe => Cantidad * PrecioUnitario;
}
public record CotizacionVista(int Id,int SolicitudId,DateTime FechaEmision,DateOnly FechaVencimiento,string Estado,
    decimal Subtotal,decimal Descuento,decimal Impuestos,decimal Total,string? Observaciones,List<LineaCotizacionVista> Detalles)
{
    public bool Vencida { get; init; }
}
public record SolicitudDetalleVista(int Id,DateTime Fecha,DateOnly FechaRequerida,string Estado,string? Observaciones,
    PerfilInstitucionalVista Institucion,string? Asesor,List<ObraSolicitadaVista> Detalles,CotizacionVista? Cotizacion)
{
    public bool PuedeAsignar { get; init; }
    public bool PuedeCotizar { get; init; }
    public bool PuedeResponder { get; init; }
}
public class AsignacionVista
{
    public int SolicitudId { get; set; }
    public AsignacionEntrada Datos { get; set; } = new();
    public List<SelectListItem> Asesores { get; set; } = [];
}
public class CotizacionEdicionVista
{
    public int SolicitudId { get; set; }
    public string Institucion { get; set; } = "";
    public CotizacionInstitucionalEntrada Datos { get; set; } = new();
    public decimal? PorcentajeImpuesto { get; set; }
    public string? AvisoImpuesto { get; set; }
}
public record DocumentoCotizacion(SolicitudDetalleVista Solicitud,CotizacionVista Cotizacion);
