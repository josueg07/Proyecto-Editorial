using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PlataformaEditorial.Web.Data;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Institucional;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;

public class PerfilInstitucionalService(EditorialDbContext db, ApplicationDbContext identidad,
    UserManager<IdentityUser> usuarios, UnidadRf04 unidad, TiempoRf04 tiempo)
{
    internal static PerfilInstitucionalVista Vista(tblPerfilInstitucional p) => new(p.IDUsuario,p.NombreInstitucion,p.NIT,p.Direccion,p.EstadoValidacion,p.FechaValidacion);
    public async Task<PerfilInstitucionalVista?> MiPerfilAsync(ActorRf04 actor)
    {
        var p=await db.tblPerfilInstitucionals.AsNoTracking().SingleOrDefaultAsync(p=>p.IDUsuario==actor.UsuarioId);
        return p==null ? null : Vista(p);
    }
    public Task SolicitarAsync(ActorRf04 actor,PerfilInstitucionalEntrada m) => unidad.EjecutarAsync(async ()=>
    {
        if (actor.Administrador || actor.Asesor) throw new AccesoRf04Exception();
        ValidacionRf04.Comprobar(m);
        var p=await db.tblPerfilInstitucionals.SingleOrDefaultAsync(p=>p.IDUsuario==actor.UsuarioId);
        if(p!=null && p.EstadoValidacion!=EstadosRf04.Rechazado)
            throw new ReglaRf04Exception("Su perfil ya está pendiente o aprobado. Solo puede reenviar un perfil rechazado.");
        if(!await identidad.Users.AnyAsync(u=>u.Id==actor.UsuarioId)) throw new AccesoRf04Exception();
        if(p==null) { p=new() { IDUsuario=actor.UsuarioId }; db.Add(p); }
        p.NombreInstitucion=m.NombreInstitucion.Trim(); p.NIT=m.NIT.Trim(); p.Direccion=m.Direccion.Trim();
        p.EstadoValidacion=EstadosRf04.Pendiente; p.FechaValidacion=null;
        await db.SaveChangesAsync(); return true;
    });
    public async Task<List<PerfilInstitucionalVista>> PendientesAsync(ActorRf04 actor)
    {
        if(!actor.Administrador) throw new AccesoRf04Exception();
        return (await db.tblPerfilInstitucionals.AsNoTracking().OrderBy(p=>p.EstadoValidacion).ThenBy(p=>p.NombreInstitucion).ToListAsync()).Select(Vista).ToList();
    }
    public Task ResolverAsync(ActorRf04 actor,string usuarioId,bool aprobar) => unidad.EjecutarAsync(async ()=>
    {
        if(!actor.Administrador) throw new AccesoRf04Exception();
        var p=await db.tblPerfilInstitucionals.SingleOrDefaultAsync(p=>p.IDUsuario==usuarioId) ?? throw new NoEncontradoRf04Exception();
        if(p.EstadoValidacion!=EstadosRf04.Pendiente) throw new ReglaRf04Exception("Solo se pueden resolver perfiles pendientes.");
        // Ambos contextos usan la MISMA conexión y transacción SQL. No requiere MSDTC.
        if(identidad.Database.GetDbConnection()!=db.Database.GetDbConnection())
            identidad.Database.SetDbConnection(db.Database.GetDbConnection());
        var transaccion=db.Database.CurrentTransaction!.GetDbTransaction();
        if(identidad.Database.CurrentTransaction?.GetDbTransaction()!=transaccion)
            await identidad.Database.UseTransactionAsync(transaccion);
        var usuario=await usuarios.FindByIdAsync(usuarioId) ?? throw new NoEncontradoRf04Exception();
        if(aprobar)
        {
            if(!await usuarios.IsInRoleAsync(usuario,"ClienteInstitucional")) Comprobar(await usuarios.AddToRoleAsync(usuario,"ClienteInstitucional"));
            if(await usuarios.IsInRoleAsync(usuario,"ClienteMinorista")) Comprobar(await usuarios.RemoveFromRoleAsync(usuario,"ClienteMinorista"));
        }
        else
        {
            if(await usuarios.IsInRoleAsync(usuario,"ClienteInstitucional")) Comprobar(await usuarios.RemoveFromRoleAsync(usuario,"ClienteInstitucional"));
            if(!await usuarios.IsInRoleAsync(usuario,"ClienteMinorista")) Comprobar(await usuarios.AddToRoleAsync(usuario,"ClienteMinorista"));
        }
        // Invalida sesiones anteriores. El cliente debe volver a entrar para cargar el nuevo rol.
        Comprobar(await usuarios.UpdateSecurityStampAsync(usuario));
        p.EstadoValidacion=aprobar ? EstadosRf04.Aprobado : EstadosRf04.Rechazado;
        p.FechaValidacion=tiempo.Ahora;
        await db.SaveChangesAsync(); return true;
    });
    private static void Comprobar(IdentityResult resultado)
    {
        if(!resultado.Succeeded) throw new ReglaRf04Exception("No se pudo actualizar el rol: "+string.Join(" ",resultado.Errors.Select(e=>e.Description)));
    }
}
