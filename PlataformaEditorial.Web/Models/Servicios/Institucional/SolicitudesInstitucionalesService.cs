using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Institucional;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;

public class SolicitudesInstitucionalesService(EditorialDbContext db, UserManager<IdentityUser> usuarios, UnidadRf04 unidad, TiempoRf04 tiempo)
{
    internal async Task ExigirInstitucionAsync(ActorRf04 actor)
    {
        if(!actor.Institucional || !await db.tblPerfilInstitucionals.AnyAsync(p=>p.IDUsuario==actor.UsuarioId && p.EstadoValidacion==EstadosRf04.Aprobado))
            throw new AccesoRf04Exception();
    }
    internal async Task<IQueryable<tblSolicitudCotizacion>> ConsultaPermitidaAsync(ActorRf04 actor)
    {
        var q=db.tblSolicitudCotizacions.AsQueryable();
        if(actor.Administrador) return q;
        if(actor.Asesor) return q.Where(s=>s.IDAsesor==actor.UsuarioId);
        await ExigirInstitucionAsync(actor);
        return q.Where(s=>s.IDUsuario==actor.UsuarioId);
    }
    internal async Task<tblSolicitudCotizacion> CargarAsync(ActorRf04 actor,int id,bool escritura=false)
    {
        var q=(await ConsultaPermitidaAsync(actor)).Include(s=>s.tblDetalleSolicitudCotizacions)
            .Include(s=>s.tblCotizacion!).ThenInclude(c=>c.tblDetalleCotizacions).AsSplitQuery();
        return await (escritura ? q : q.AsNoTracking()).SingleOrDefaultAsync(s=>s.IDSolicitud==id) ?? throw new NoEncontradoRf04Exception();
    }
    public async Task<List<SolicitudFila>> BandejaAsync(ActorRf04 actor)
    {
        var solicitudes=await (await ConsultaPermitidaAsync(actor)).AsNoTracking().Include(s=>s.tblCotizacion).OrderByDescending(s=>s.FechaSolicitud).ThenByDescending(s=>s.IDSolicitud).ToListAsync();
        var ids=solicitudes.Select(s=>s.IDUsuario).Distinct().ToList();
        var perfiles=await db.tblPerfilInstitucionals.AsNoTracking().Where(p=>ids.Contains(p.IDUsuario)).ToDictionaryAsync(p=>p.IDUsuario);
        var asesores=await NombresAsesoresAsync(solicitudes.Select(s=>s.IDAsesor));
        return solicitudes.Select(s=>new SolicitudFila(s.IDSolicitud,s.FechaSolicitud,s.FechaEntregaRequerida,s.EstadoSolicitud,
            perfiles.GetValueOrDefault(s.IDUsuario)?.NombreInstitucion??"Perfil no disponible",
            s.IDAsesor==null ? null : asesores.GetValueOrDefault(s.IDAsesor)??"Asesor no disponible",
            s.tblCotizacion?.IDCotizacion, s.tblCotizacion?.EstadoCotizacion)).ToList();
    }
    public async Task<SolicitudDetalleVista> DetalleAsync(ActorRf04 actor,int id)
    {
        var s=await CargarAsync(actor,id);
        var p=await db.tblPerfilInstitucionals.AsNoTracking().SingleOrDefaultAsync(p=>p.IDUsuario==s.IDUsuario);
        var institucion=p==null ? new PerfilInstitucionalVista(s.IDUsuario,"Perfil no disponible","—","—","SinPerfil",null) : PerfilInstitucionalService.Vista(p);
        var asesores=await NombresAsesoresAsync([s.IDAsesor]);
        CotizacionVista? c=null;
        if(s.tblCotizacion is {} cot && (actor.Administrador || actor.Asesor || cot.EstadoCotizacion!=EstadosRf04.Borrador))
        {
            var obras=s.tblDetalleSolicitudCotizacions.ToDictionary(d=>d.IDDetalleSolicitud);
            c=new(cot.IDCotizacion,s.IDSolicitud,cot.FechaEmision,cot.FechaVencimiento,cot.EstadoCotizacion,cot.SubtotalBase,cot.Descuento,cot.Impuestos,cot.TotalFinal,cot.Observaciones,
                cot.tblDetalleCotizacions.OrderBy(d=>d.IDDetalleCotizacion).Select(d=>new LineaCotizacionVista(d.Descripcion,
                    d.IDDetalleSolicitud.HasValue && obras.TryGetValue(d.IDDetalleSolicitud.Value,out var obra) ? $"Portada: {obra.AcabadoPortada}. Papel: {obra.TipoPapel}." : "",
                    d.Cantidad,d.PrecioUnitario)).ToList()) { Vencida=cot.EstadoCotizacion==EstadosRf04.Emitida && cot.FechaVencimiento<tiempo.Hoy };
        }
        bool editable=s.tblCotizacion==null || s.tblCotizacion.EstadoCotizacion==EstadosRf04.Borrador;
        return new(s.IDSolicitud,s.FechaSolicitud,s.FechaEntregaRequerida,s.EstadoSolicitud,s.Observaciones,institucion,
            s.IDAsesor==null ? null : asesores.GetValueOrDefault(s.IDAsesor),
            s.tblDetalleSolicitudCotizacions.OrderBy(d=>d.IDDetalleSolicitud).Select(d=>new ObraSolicitadaVista(d.IDDetalleSolicitud,d.TituloSolicitado,d.CantidadEstimada,d.AcabadoPortada,d.TipoPapel)).ToList(),c)
        {
            PuedeAsignar=actor.Administrador && editable,
            PuedeCotizar=editable && s.IDAsesor!=null && (actor.Administrador || actor.Asesor && s.IDAsesor==actor.UsuarioId),
            PuedeResponder=actor.Institucional && s.IDUsuario==actor.UsuarioId && c?.Estado==EstadosRf04.Emitida && !c.Vencida
        };
    }
    private async Task<Dictionary<string,string>> NombresAsesoresAsync(IEnumerable<string?> valores)
    {
        var ids=valores.Where(x=>x!=null).Select(x=>x!).Distinct().ToList();
        return await usuarios.Users.AsNoTracking().Where(u=>ids.Contains(u.Id)).ToDictionaryAsync(u=>u.Id,u=>u.UserName??u.Id);
    }
    public Task<int> CrearAsync(ActorRf04 actor,SolicitudInstitucionalEntrada m) => unidad.EjecutarAsync(async ()=>
    {
        await ExigirInstitucionAsync(actor);
        ValidacionRf04.Comprobar(m);
        if(m.FechaRequerida<tiempo.Hoy) throw new ReglaRf04Exception("La fecha requerida no puede estar en el pasado.");
        foreach(var detalle in m.Detalles) ValidacionRf04.Comprobar(detalle);
        var s=new tblSolicitudCotizacion { IDUsuario=actor.UsuarioId,FechaSolicitud=tiempo.Ahora,FechaEntregaRequerida=m.FechaRequerida!.Value,
            EstadoSolicitud=EstadosRf04.Pendiente,Observaciones=m.Observaciones?.Trim() };
        foreach(var d in m.Detalles) s.tblDetalleSolicitudCotizacions.Add(new() { TituloSolicitado=d.Titulo.Trim(),CantidadEstimada=d.Cantidad,AcabadoPortada=d.AcabadoPortada.Trim(),TipoPapel=d.TipoPapel.Trim() });
        db.Add(s); await db.SaveChangesAsync(); return s.IDSolicitud;
    });
    public async Task<AsignacionVista> AsignacionAsync(ActorRf04 actor,int id)
    {
        if(!actor.Administrador) throw new AccesoRf04Exception();
        var s=await CargarAsync(actor,id);
        if(s.tblCotizacion!=null && s.tblCotizacion.EstadoCotizacion!=EstadosRf04.Borrador) throw new ReglaRf04Exception("Una solicitud cotizada ya no se puede reasignar.");
        return new() { SolicitudId=id,Datos=new(){AsesorId=s.IDAsesor??""},Asesores=(await usuarios.GetUsersInRoleAsync("AsesorVentas")).OrderBy(u=>u.UserName).Select(u=>new SelectListItem(u.UserName??u.Id,u.Id)).ToList() };
    }
    public Task AsignarAsync(ActorRf04 actor,int id,AsignacionEntrada m) => unidad.EjecutarAsync(async ()=>
    {
        if(!actor.Administrador) throw new AccesoRf04Exception();
        ValidacionRf04.Comprobar(m);
        var s=await CargarAsync(actor,id,true);
        if(s.tblCotizacion!=null && s.tblCotizacion.EstadoCotizacion!=EstadosRf04.Borrador) throw new ReglaRf04Exception("Solo se puede asignar o reasignar antes de la emisión.");
        var asesor=await usuarios.FindByIdAsync(m.AsesorId);
        if(asesor==null || !await usuarios.IsInRoleAsync(asesor,"AsesorVentas")) throw new ReglaRf04Exception("Seleccione un usuario con el rol AsesorVentas.");
        s.IDAsesor=asesor.Id; s.EstadoSolicitud=EstadosRf04.Asignada;
        await db.SaveChangesAsync(); return true;
    });
}
