using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Compras;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Web.Models.Servicios.Compras;
public class CarritoService(EditorialDbContext db,UnidadCompra unidad,IImpuestoInstitucional impuestos,TiempoRf04 tiempo,IDataProtectionProvider proteccion)
{
    private readonly ITimeLimitedDataProtector protector=proteccion.CreateProtector("PlataformaEditorial.RF03.Confirmacion.v1").ToTimeLimitedDataProtector();
    internal static void ExigirCliente(ActorCompra actor) { if(!actor.Cliente) throw new AccesoCompraException(); }
    private async Task<tblCarrito?> Cargar(ActorCompra actor,bool bloquear)
    {
        ExigirCliente(actor);
        return bloquear ? await db.tblCarritos.FromSqlInterpolated($"SELECT * FROM dbo.tblCarrito WITH (UPDLOCK,HOLDLOCK) WHERE IDUsuario={actor.Id}").SingleOrDefaultAsync() :
            await db.tblCarritos.AsNoTracking().SingleOrDefaultAsync(c=>c.IDUsuario==actor.Id);
    }
    internal Task<tblCarrito?> Bloquear(ActorCompra actor) => Cargar(actor,true);
    internal async Task<bool> DescuentoInstitucional(ActorCompra actor) => actor.Institucional && await db.tblPerfilInstitucionals.AsNoTracking().AnyAsync(p=>p.IDUsuario==actor.Id && p.EstadoValidacion==EstadosRf04.Aprobado);
    internal async Task<List<tblDetalleCarrito>> Detalles(int carritoId) => await db.tblDetalleCarritos.Where(d=>d.IDCarrito==carritoId)
        .Include(d=>d.IDPresentacionNavigation).ThenInclude(p=>p.IDLibroNavigation)
        .Include(d=>d.IDPresentacionNavigation).ThenInclude(p=>p.IDFormatoNavigation).OrderBy(d=>d.IDPresentacion).ToListAsync();
    private static List<LineaCarrito> Lineas(List<tblDetalleCarrito> detalles) => detalles.Select(d=>new LineaCarrito(d.IDDetalleCarrito,d.IDPresentacion,d.IDPresentacionNavigation.IDLibroNavigation.Titulo,d.IDPresentacionNavigation.IDFormatoNavigation.NombreFormato,d.IDPresentacionNavigation.isbn,d.Cantidad,d.IDPresentacionNavigation.PrecioBase,d.IDPresentacionNavigation.IDFormatoNavigation.EsDigital,d.IDPresentacionNavigation.Existencias,d.IDPresentacionNavigation.IDLibroNavigation.Activo)).ToList();
    internal static void ValidarDisponibilidad(List<LineaCarrito> lineas)
    {
        if(lineas.Count==0) throw new ReglaCompraException("El carrito está vacío.");
        foreach(var l in lineas)
        {
            if(!l.Activo) throw new ReglaCompraException($"El libro «{l.Titulo}» ya no está disponible.");
            if(l.Cantidad is <1 or >100000) throw new ReglaCompraException("Hay una cantidad inválida en el carrito.");
            if(!l.Digital && (!l.Existencias.HasValue || l.Existencias<l.Cantidad)) throw new ReglaCompraException($"No hay suficientes existencias físicas de «{l.Titulo}».");
        }
    }
    internal static ResultadoPedido Calcular(List<LineaCarrito> lineas,bool institucional,decimal iva)
    {
        long unidades=lineas.Sum(l=>(long)l.Cantidad);
        var subtotal=lineas.Sum(l=>l.Importe);
        if(unidades>int.MaxValue || subtotal>9999999999.99m) throw new ReglaCompraException("El pedido excede los límites permitidos.");
        var resultado=CalculoPedido.Calcular(subtotal,(int)unidades,institucional,iva);
        if(resultado.TotalFinal>9999999999.99m) throw new ReglaCompraException("El total excede la capacidad del pedido.");
        return resultado;
    }
    private static string Huella(ActorCompra actor,int carrito,List<LineaCarrito> lineas,decimal iva,bool institucional) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { actor.Id,carrito,iva,institucional,Detalles=lineas.Select(l=>new{l.Id,l.PresentacionId,l.Cantidad,l.Precio}).ToArray() }))));
    public async Task<CarritoVista> Ver(ActorCompra actor)
    {
        var c=await Cargar(actor,false);
        var lineas=c==null ? [] : Lineas(await Detalles(c.IDCarrito));
        var iva=await impuestos.ObtenerAsync(); var institucional=await DescuentoInstitucional(actor);
        if(lineas.Count==0) return new(lineas,null,iva,institucional,null,null);
        string? aviso=null; ResultadoPedido? totales=null;
        try { ValidarDisponibilidad(lineas); if(iva==null) throw new ReglaCompraException("La configuración de IVA no está disponible. No se puede confirmar la compra.");totales=Calcular(lineas,institucional,iva.Value); }
        catch(ReglaCompraException ex) { aviso=ex.Message; }
        var token=aviso==null ? protector.Protect(Huella(actor,c!.IDCarrito,lineas,iva!.Value,institucional),TimeSpan.FromMinutes(30)) : null;
        return new(lineas,totales,iva,institucional,token,aviso);
    }
    public Task Agregar(ActorCompra actor,CantidadCompra entrada) => unidad.Ejecutar(async ()=>
    {
        ExigirCliente(actor);
        if(entrada.Cantidad is <1 or >100000) throw new ReglaCompraException("La cantidad debe estar entre 1 y 100000.");
        var carrito=await Cargar(actor,true);
        if(carrito==null) { carrito=new(){IDUsuario=actor.Id,FechaCreacion=tiempo.Ahora};db.Add(carrito);await db.SaveChangesAsync(); }
        var p=await db.tblPresentacionLibros.Include(p=>p.IDLibroNavigation).Include(p=>p.IDFormatoNavigation).SingleOrDefaultAsync(p=>p.IDPresentacion==entrada.PresentacionId) ?? throw new CompraNoEncontradaException();
        var detalle=await db.tblDetalleCarritos.SingleOrDefaultAsync(d=>d.IDCarrito==carrito.IDCarrito && d.IDPresentacion==p.IDPresentacion);
        long cantidad=(long)(detalle?.Cantidad??0)+entrada.Cantidad;
        if(cantidad>100000) throw new ReglaCompraException("La cantidad acumulada supera el máximo permitido.");
        ValidarDisponibilidad([new(0,p.IDPresentacion,p.IDLibroNavigation.Titulo,p.IDFormatoNavigation.NombreFormato,p.isbn,(int)cantidad,p.PrecioBase,p.IDFormatoNavigation.EsDigital,p.Existencias,p.IDLibroNavigation.Activo)]);
        if(detalle==null) db.Add(new tblDetalleCarrito { IDCarrito=carrito.IDCarrito,IDPresentacion=p.IDPresentacion,Cantidad=(int)cantidad });else detalle.Cantidad=(int)cantidad;
        await db.SaveChangesAsync();return true;
    });
    public Task Cambiar(ActorCompra actor,int detalleId,int cantidad) => unidad.Ejecutar(async ()=>
    {
        var carrito=await Cargar(actor,true) ?? throw new CompraNoEncontradaException();
        var detalle=(await Detalles(carrito.IDCarrito)).SingleOrDefault(d=>d.IDDetalleCarrito==detalleId) ?? throw new CompraNoEncontradaException();
        var lineas=Lineas([detalle]);lineas[0]=lineas[0] with { Cantidad=cantidad };ValidarDisponibilidad(lineas);
        detalle.Cantidad=cantidad;await db.SaveChangesAsync();return true;
    });
    public Task Quitar(ActorCompra actor,int detalleId) => unidad.Ejecutar(async ()=>
    {
        var c=await Cargar(actor,true) ?? throw new CompraNoEncontradaException();
        var d=await db.tblDetalleCarritos.SingleOrDefaultAsync(d=>d.IDCarrito==c.IDCarrito && d.IDDetalleCarrito==detalleId) ?? throw new CompraNoEncontradaException();
        db.Remove(d);await db.SaveChangesAsync();return true;
    });
    public Task Vaciar(ActorCompra actor) => unidad.Ejecutar(async ()=>
    {
        var c=await Cargar(actor,true);
        if(c!=null) { db.RemoveRange(await db.tblDetalleCarritos.Where(d=>d.IDCarrito==c.IDCarrito).ToListAsync());await db.SaveChangesAsync(); }return true;
    });
    public Task<int> Confirmar(ActorCompra actor,ConfirmarCompra entrada) => unidad.Ejecutar(async ()=>
    {
        var c=await Cargar(actor,true) ?? throw new ReglaCompraException("El carrito está vacío.");
        // Bloqueo ordenado de presentaciones: dos compradores no pueden consumir el mismo stock.
        var detalles=await db.tblDetalleCarritos.Where(d=>d.IDCarrito==c.IDCarrito).OrderBy(d=>d.IDPresentacion).ToListAsync();
        foreach(var d in detalles)
            d.IDPresentacionNavigation=await db.tblPresentacionLibros.FromSqlInterpolated($"SELECT * FROM dbo.tblPresentacionLibro WITH (UPDLOCK,HOLDLOCK) WHERE IDPresentacion={d.IDPresentacion}")
                .Include(p=>p.IDLibroNavigation).Include(p=>p.IDFormatoNavigation).SingleAsync();
        var lineas=Lineas(detalles);ValidarDisponibilidad(lineas);
        if(entrada.Direccion?.Length>255 || lineas.Any(l=>!l.Digital) && string.IsNullOrWhiteSpace(entrada.Direccion)) throw new ReglaCompraException("Escribe una dirección de entrega de hasta 255 caracteres para los libros físicos.");
        var iva=await impuestos.ObtenerAsync() ?? throw new ReglaCompraException("No hay una configuración válida de IVA.");
        var institucional=await DescuentoInstitucional(actor);
        string huella;
        try { huella=protector.Unprotect(entrada.Confirmacion); }
        catch(CryptographicException) { throw new ReglaCompraException("La confirmación venció o no es válida. Revisa el carrito."); }
        if(huella!=Huella(actor,c.IDCarrito,lineas,iva,institucional)) throw new ReglaCompraException("El carrito, los precios o las condiciones cambiaron. Revisa el resumen antes de confirmar.");
        var t=Calcular(lineas,institucional,iva);
        var pedido=new tblPedido { IDUsuario=actor.Id,FechaPedido=tiempo.Ahora,EstadoPedido="Pendiente",DireccionEntrega=lineas.Any(l=>!l.Digital)?entrada.Direccion!.Trim():null,
            SubtotalBase=t.SubtotalBase,DescuentoVolumen=t.DescuentoVolumen,DescuentoInstitucional=t.DescuentoInstitucional,PorcentajeImpuesto=iva,Impuestos=t.Impuestos,TotalFinal=t.TotalFinal };
        foreach(var d in detalles)
        {
            var presentacion=d.IDPresentacionNavigation;
            if(!presentacion.IDFormatoNavigation.EsDigital) presentacion.Existencias-=d.Cantidad;
            pedido.tblDetallePedidos.Add(new(){IDPresentacion=d.IDPresentacion,Cantidad=d.Cantidad,PrecioUnitario=presentacion.PrecioBase});
        }
        db.Add(pedido);db.RemoveRange(detalles);await db.SaveChangesAsync();return pedido.IDPedido;
    });
}
