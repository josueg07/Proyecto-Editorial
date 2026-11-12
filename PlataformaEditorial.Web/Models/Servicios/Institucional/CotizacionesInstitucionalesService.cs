using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Institucional;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;

public class CotizacionesInstitucionalesService(EditorialDbContext db, SolicitudesInstitucionalesService solicitudes,UnidadRf04 unidad,TiempoRf04 tiempo,IImpuestoInstitucional configuracion)
{
    private static void ExigirComercial(ActorRf04 actor,tblSolicitudCotizacion s)
    {
        if(!actor.Administrador && !(actor.Asesor && s.IDAsesor==actor.UsuarioId)) throw new AccesoRf04Exception();
        if(s.IDAsesor==null) throw new ReglaRf04Exception("El administrador debe asignar un asesor antes de cotizar.");
        if(s.tblCotizacion!=null && s.tblCotizacion.EstadoCotizacion!=EstadosRf04.Borrador)
            throw new ReglaRf04Exception("La cotización ya fue emitida y no admite modificaciones.");
    }
    public async Task<CotizacionEdicionVista> EdicionAsync(ActorRf04 actor,int id)
    {
        var s=await solicitudes.CargarAsync(actor,id);
        ExigirComercial(actor,s);
        var detalle=await solicitudes.DetalleAsync(actor,id);
        var tasa=await configuracion.ObtenerAsync();
        var c=s.tblCotizacion;
        return new() { SolicitudId=id,Institucion=detalle.Institucion.NombreInstitucion,PorcentajeImpuesto=tasa,
            AvisoImpuesto=tasa==null ? "Debe existir exactamente una configuración de impuesto válida en tblConfiguracionSistema. Puede guardar el borrador; no podrá emitirlo." : null,
            Datos=new() { Descuento=c?.Descuento??0,FechaVencimiento=c?.FechaVencimiento??tiempo.Hoy.AddDays(15),Observaciones=c?.Observaciones,
                Detalles=c==null ? s.tblDetalleSolicitudCotizacions.OrderBy(d=>d.IDDetalleSolicitud).Select(d=>new LineaCotizacionEntrada { DetalleSolicitudId=d.IDDetalleSolicitud,Descripcion=d.TituloSolicitado,Cantidad=d.CantidadEstimada }).ToList() :
                c.tblDetalleCotizacions.OrderBy(d=>d.IDDetalleCotizacion).Select(d=>new LineaCotizacionEntrada { DetalleSolicitudId=d.IDDetalleSolicitud??0,Descripcion=d.Descripcion,Cantidad=d.Cantidad,PrecioUnitario=d.PrecioUnitario }).ToList() } };
    }
    public Task<int> GuardarAsync(ActorRf04 actor,int solicitudId,CotizacionInstitucionalEntrada m,bool emitir) => unidad.EjecutarAsync(async ()=>
    {
        var s=await solicitudes.CargarAsync(actor,solicitudId,true);
        ExigirComercial(actor,s);
        ValidacionRf04.Comprobar(m);
        if(m.FechaVencimiento<tiempo.Hoy) throw new ReglaRf04Exception("La fecha de vencimiento no puede estar en el pasado.");
        var esperados=s.tblDetalleSolicitudCotizacions.Select(d=>d.IDDetalleSolicitud).Order().ToArray();
        var recibidos=m.Detalles.Select(d=>d.DetalleSolicitudId).Order().ToArray();
        if(!esperados.SequenceEqual(recibidos)) throw new ReglaRf04Exception("Debe cotizar exactamente las obras de esta solicitud, sin duplicados ni detalles de otra solicitud.");
        var tasa=await configuracion.ObtenerAsync();
        if(emitir && tasa==null) throw new ReglaRf04Exception("No se puede emitir: configure un único porcentaje de impuesto válido en tblConfiguracionSistema. No se asumirá una tasa.");
        // En borrador, cero es un importe provisional si falta configuración; nunca se emite así.
        var totales=CalculoCotizacion.Calcular(m.Detalles,tasa??0,m.Descuento);
        var c=s.tblCotizacion;
        if(c==null) { c=new(){ IDSolicitud=s.IDSolicitud }; db.Add(c); }
        else db.tblDetalleCotizacions.RemoveRange(c.tblDetalleCotizacions);
        c.tblDetalleCotizacions.Clear();
        foreach(var d in m.Detalles) c.tblDetalleCotizacions.Add(new() { IDDetalleSolicitud=d.DetalleSolicitudId,Descripcion=d.Descripcion.Trim(),Cantidad=d.Cantidad,PrecioUnitario=d.PrecioUnitario });
        c.FechaEmision=tiempo.Ahora; c.FechaVencimiento=m.FechaVencimiento!.Value; c.Observaciones=m.Observaciones?.Trim();
        c.EstadoCotizacion=emitir ? EstadosRf04.Emitida : EstadosRf04.Borrador;
        c.SubtotalBase=totales.Subtotal; c.Descuento=m.Descuento; c.Impuestos=totales.Impuestos; c.TotalFinal=totales.Total;
        s.EstadoSolicitud=emitir ? EstadosRf04.Cotizada : EstadosRf04.EnRevision;
        await db.SaveChangesAsync(); return c.IDCotizacion;
    });
    public Task ResponderAsync(ActorRf04 actor,int solicitudId,bool aprobar) => unidad.EjecutarAsync(async ()=>
    {
        await solicitudes.ExigirInstitucionAsync(actor);
        // La propiedad es obligatoria incluso si el usuario también tiene otro rol.
        var s=await solicitudes.CargarAsync(actor,solicitudId,true);
        if(s.IDUsuario!=actor.UsuarioId) throw new NoEncontradoRf04Exception();
        var c=s.tblCotizacion ?? throw new ReglaRf04Exception("La solicitud aún no tiene cotización.");
        if(c.EstadoCotizacion!=EstadosRf04.Emitida) throw new ReglaRf04Exception("Solo se puede responder una cotización emitida una vez.");
        if(c.FechaVencimiento<tiempo.Hoy) throw new ReglaRf04Exception("La cotización está vencida y no admite respuesta.");
        c.EstadoCotizacion=aprobar ? EstadosRf04.Aprobada : EstadosRf04.Rechazada;
        s.EstadoSolicitud=c.EstadoCotizacion;
        await db.SaveChangesAsync(); return true;
    });
    public async Task<DocumentoCotizacion> DocumentoAsync(ActorRf04 actor,int solicitudId)
    {
        var s=await solicitudes.DetalleAsync(actor,solicitudId);
        var c=s.Cotizacion ?? throw new NoEncontradoRf04Exception();
        return new(s,c);
    }
}
