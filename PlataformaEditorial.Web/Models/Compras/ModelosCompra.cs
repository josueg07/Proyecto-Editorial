using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using PlataformaEditorial.Web.Models.Servicios.Compras;
namespace PlataformaEditorial.Web.Models.Compras;
public record ActorCompra(string Id,bool Cliente,bool Institucional,bool Administrador)
{
    public static ActorCompra Desde(ClaimsPrincipal p) => new(p.FindFirstValue(ClaimTypes.NameIdentifier)??throw new UnauthorizedAccessException(),p.IsInRole("ClienteMinorista")||p.IsInRole("ClienteInstitucional"),p.IsInRole("ClienteInstitucional"),p.IsInRole("Administrador"));
}
public class ReglaCompraException(string mensaje) : Exception(mensaje);
public class AccesoCompraException : Exception;
public class CompraNoEncontradaException : Exception;
public class CantidadCompra
{
    [Range(1,int.MaxValue)] public int PresentacionId { get; set; }
    [Range(1,100000,ErrorMessage="La cantidad debe estar entre 1 y 100000.")] public int Cantidad { get; set; }=1;
}
public class ConfirmarCompra
{
    [Required] public string Confirmacion { get; set; }="";
    [StringLength(255),Display(Name="Dirección de entrega")] public string? Direccion { get; set; }
}
public record LineaCarrito(int Id,int PresentacionId,string Titulo,string Formato,string Isbn,int Cantidad,decimal Precio,bool Digital,int? Existencias,bool Activo)
{
    public decimal Importe => Cantidad*Precio;
}
public record CarritoVista(List<LineaCarrito> Lineas,ResultadoPedido? Totales,decimal? IVA,bool Institucional,string? Confirmacion,string? Aviso)
{
    public string? Direccion { get; init; }
    public bool RequiereDireccion => Lineas.Any(l=>!l.Digital);
}
public record PedidoFila(int Id,DateTime Fecha,string Estado,decimal Total);
public record PedidoVista(int Id,DateTime Fecha,string Estado,string? Direccion,List<LineaCarrito> Lineas,ResultadoPedido Totales,decimal IVA,List<string> EstadosPermitidos);
