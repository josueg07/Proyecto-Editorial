using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
using static PlataformaEditorial.Tests.InstitucionalEscenario;

namespace PlataformaEditorial.Tests;

public class InstitucionalIntegracionTests
{
    private static ActorRf04 Actor(string id, string rol) => new(id, rol == "Administrador", rol == "AsesorVentas", rol == "ClienteInstitucional");
    private static async Task<string> AprobarCliente(InstitucionalEscenario es, string admin)
    {
        var id = await es.UsuarioAsync("ClienteMinorista");
        await es.EnScope(async sp =>
        {
            var perfiles = sp.GetRequiredService<PerfilInstitucionalService>();
            await perfiles.SolicitarAsync(Actor(id, "ClienteMinorista"), new() { NombreInstitucion = "Instituto de prueba " + id, NIT = "1234567-8", Direccion = "Dirección académica de prueba" });
            await perfiles.ResolverAsync(Actor(admin, "Administrador"), id, true); return true;
        });
        return id;
    }
    private static async Task<int> NuevaSolicitud(InstitucionalEscenario es, string propietario, string titulo = "Obra de prueba") => await es.EnScope(async sp =>
    {
        var tiempo = sp.GetRequiredService<TiempoRf04>();
        return await sp.GetRequiredService<SolicitudesInstitucionalesService>().CrearAsync(Actor(propietario, "ClienteInstitucional"), new()
        {
            FechaRequerida = tiempo.Hoy.AddDays(30),
            Detalles = [new() { Titulo = titulo, Cantidad = 10, AcabadoPortada = "Mate", TipoPapel = "Bond" }]
        });
    });

    [Fact]
    public async Task Perfil_rechazo_reenvio_aprobacion_y_rol_controlado_por_servidor()
    {
        await using var es = await CrearAsync();
        var usuario = await es.UsuarioAsync("ClienteMinorista"); var adminId = await es.UsuarioAsync("Administrador");
        using var cliente = es.Cliente(usuario, "ClienteMinorista"); using var admin = es.Cliente(adminId, "Administrador");
        var token = Token(await cliente.GetStringAsync("/PerfilInstitucional/Solicitar"));
        var datos = new[] { ("NombreInstitucion", "Instituto HTTP"), ("NIT", "1234567-8"), ("Direccion", "Dirección HTTP"), ("EstadoValidacion", "Aprobado"), ("IDUsuario", adminId), ("Rol", "ClienteInstitucional") };
        Assert.Equal(HttpStatusCode.Redirect, (await cliente.PostAsync("/PerfilInstitucional/Solicitar", Form(token, datos))).StatusCode);
        await es.EnScope(async sp =>
        {
            var db = sp.GetRequiredService<EditorialDbContext>(); var manager = sp.GetRequiredService<UserManager<IdentityUser>>();
            var perfil = await db.tblPerfilInstitucionals.SingleAsync(p => p.IDUsuario == usuario);
            Assert.Equal(EstadosRf04.Pendiente, perfil.EstadoValidacion);
            Assert.False(await manager.IsInRoleAsync((await manager.FindByIdAsync(usuario))!, "ClienteInstitucional")); return true;
        });
        Assert.Equal(HttpStatusCode.OK, (await cliente.PostAsync("/PerfilInstitucional/Solicitar", Form(token, datos))).StatusCode); // Error de duplicado en formulario.
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/SolicitudesInstitucionales/Nueva")).StatusCode);
        using var rolSinPerfil = es.Cliente(usuario, "ClienteInstitucional");
        Assert.Equal(HttpStatusCode.Forbidden, (await rolSinPerfil.GetAsync("/SolicitudesInstitucionales/Nueva")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsync("/PerfilesInstitucionales/Aprobar", Form(token, ("usuarioId", usuario)))).StatusCode);
        var adminToken = Token(await admin.GetStringAsync("/PerfilesInstitucionales"));
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync("/PerfilesInstitucionales/Aprobar", new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("usuarioId", usuario) }))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync("/PerfilesInstitucionales/Rechazar", Form(adminToken, ("usuarioId", usuario)))).StatusCode);
        Assert.Contains("Instituto HTTP", await cliente.GetStringAsync("/PerfilInstitucional/Solicitar"));
        Assert.Equal(HttpStatusCode.Redirect, (await cliente.PostAsync("/PerfilInstitucional/Solicitar", Form(token, datos))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync("/PerfilesInstitucionales/Aprobar", Form(adminToken, ("usuarioId", usuario)))).StatusCode);
        await es.EnScope(async sp =>
        {
            var manager = sp.GetRequiredService<UserManager<IdentityUser>>(); var u = (await manager.FindByIdAsync(usuario))!;
            Assert.True(await manager.IsInRoleAsync(u, "ClienteInstitucional")); Assert.False(await manager.IsInRoleAsync(u, "ClienteMinorista"));
            var p = await sp.GetRequiredService<EditorialDbContext>().tblPerfilInstitucionals.AsNoTracking().SingleAsync(p => p.IDUsuario == usuario);
            Assert.Equal(EstadosRf04.Aprobado, p.EstadoValidacion); Assert.NotNull(p.FechaValidacion); return true;
        });
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync("/PerfilesInstitucionales/Rechazar", Form(adminToken, ("usuarioId", usuario)))).StatusCode);
    }

    [Fact]
    public async Task Flujo_HTTP_completo_propiedad_asignacion_borrador_emision_PDF_y_aprobacion()
    {
        await using var es = await CrearAsync();
        await es.ConfigurarImpuestoAsync(12); // Solo en esta transacción, que se revierte.
        var adminId = await es.UsuarioAsync("Administrador");
        var propietario = await AprobarCliente(es, adminId); var ajeno = await AprobarCliente(es, adminId);
        var asesorId = await es.UsuarioAsync("AsesorVentas"); var otroAsesorId = await es.UsuarioAsync("AsesorVentas");
        using var cliente = es.Cliente(propietario, "ClienteInstitucional"); using var otroCliente = es.Cliente(ajeno, "ClienteInstitucional");
        using var admin = es.Cliente(adminId, "Administrador"); using var asesor = es.Cliente(asesorId, "AsesorVentas"); using var otroAsesor = es.Cliente(otroAsesorId, "AsesorVentas");
        var token = Token(await cliente.GetStringAsync("/SolicitudesInstitucionales/Nueva"));
        var fecha = DateOnly.FromDateTime(DateTime.Today).AddDays(30).ToString("yyyy-MM-dd");
        var crear = await cliente.PostAsync("/SolicitudesInstitucionales/Nueva", Form(token, ("FechaRequerida", fecha), ("IDUsuario", ajeno), ("IDAsesor", asesorId), ("EstadoSolicitud", "Aprobada"),
            ("Detalles[0].Titulo", "Obra HTTP A"), ("Detalles[0].Cantidad", "10"), ("Detalles[0].AcabadoPortada", "Mate"), ("Detalles[0].TipoPapel", "Bond"),
            ("Detalles[1].Titulo", "Obra HTTP B"), ("Detalles[1].Cantidad", "5"), ("Detalles[1].AcabadoPortada", "Brillante"), ("Detalles[1].TipoPapel", "Couché")));
        Assert.Equal(HttpStatusCode.Redirect, crear.StatusCode);
        var solicitudId = await es.EnScope(async sp => (await sp.GetRequiredService<EditorialDbContext>().tblSolicitudCotizacions.AsNoTracking().SingleAsync(s => s.IDUsuario == propietario)).IDSolicitud);
        await es.EnScope(async sp =>
        {
            var s = await sp.GetRequiredService<EditorialDbContext>().tblSolicitudCotizacions.AsNoTracking().SingleAsync(s => s.IDSolicitud == solicitudId);
            Assert.Null(s.IDAsesor); Assert.Equal(EstadosRf04.Pendiente, s.EstadoSolicitud); return true;
        });
        Assert.Equal(HttpStatusCode.NotFound, (await otroCliente.GetAsync($"/SolicitudesInstitucionales/Detalles/{solicitudId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asesor.GetAsync($"/SolicitudesInstitucionales/Detalles/{solicitudId}")).StatusCode);
        var asignacion = await admin.GetStringAsync($"/SolicitudesInstitucionales/Asignar/{solicitudId}"); var adminToken = Token(asignacion);
        var malAsesor = await admin.PostAsync($"/SolicitudesInstitucionales/Asignar/{solicitudId}", Form(adminToken, ("Datos.AsesorId", ajeno)));
        Assert.Equal(HttpStatusCode.OK, malAsesor.StatusCode); Assert.Contains("Seleccione un usuario", await malAsesor.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync($"/SolicitudesInstitucionales/Asignar/{solicitudId}", Form(adminToken, ("Datos.AsesorId", asesorId)))).StatusCode);
        var edicion = await asesor.GetStringAsync($"/CotizacionesInstitucionales/Editar/{solicitudId}"); var asesorToken = Token(edicion);
        var lineas = await es.EnScope(async sp => await sp.GetRequiredService<EditorialDbContext>().tblDetalleSolicitudCotizacions.AsNoTracking().Where(d => d.IDSolicitud == solicitudId).OrderBy(d => d.IDDetalleSolicitud).Select(d => d.IDDetalleSolicitud).ToListAsync());
        var campos = new[]{("Datos.FechaVencimiento",fecha),("Datos.Descuento","25.50"),("Datos.Detalles[0].DetalleSolicitudId",lineas[0].ToString()),("Datos.Detalles[0].Descripcion","Obra HTTP A"),("Datos.Detalles[0].Cantidad","10"),("Datos.Detalles[0].PrecioUnitario","25.50"),
            ("Datos.Detalles[1].DetalleSolicitudId",lineas[1].ToString()),("Datos.Detalles[1].Descripcion","Obra HTTP B"),("Datos.Detalles[1].Cantidad","5"),("Datos.Detalles[1].PrecioUnitario","10.00"),("TotalFinal","0.01"),("Impuestos","0")};
        Assert.Equal(HttpStatusCode.Redirect, (await asesor.PostAsync($"/CotizacionesInstitucionales/Editar/{solicitudId}", Form(asesorToken, campos.Append(("operacion", "borrador")).ToArray()))).StatusCode);
        var cotizacionId = await es.EnScope(async sp =>
        {
            var c = await sp.GetRequiredService<EditorialDbContext>().tblCotizacions.AsNoTracking().SingleAsync(c => c.IDSolicitud == solicitudId);
            Assert.Equal(EstadosRf04.Borrador, c.EstadoCotizacion); Assert.Equal(305, c.SubtotalBase); Assert.Equal(25.50m, c.Descuento);
            Assert.Equal(33.54m, c.Impuestos); Assert.Equal(313.04m, c.TotalFinal); return c.IDCotizacion;
        });
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/CotizacionesInstitucionales/Pdf/{solicitudId}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsync($"/CotizacionesInstitucionales/Aprobar/{solicitudId}", Form(token))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await asesor.PostAsync($"/CotizacionesInstitucionales/Editar/{solicitudId}", Form(asesorToken, campos.Append(("operacion", "emitir")).ToArray()))).StatusCode);
        await es.EnScope(async sp =>
        {
            var cs = await sp.GetRequiredService<EditorialDbContext>().tblCotizacions.AsNoTracking().Where(c => c.IDSolicitud == solicitudId).ToListAsync();
            var c = Assert.Single(cs); Assert.Equal(cotizacionId, c.IDCotizacion); Assert.Equal(EstadosRf04.Emitida, c.EstadoCotizacion); return true;
        });
        // El documento usa los importes guardados; una modificación posterior de la tasa no recalcula la cotización emitida.
        await es.ConfigurarImpuestoAsync(5);
        var documento = await cliente.GetStringAsync($"/CotizacionesInstitucionales/Documento/{solicitudId}");
        Assert.Contains("313.04", documento); Assert.Contains("NIT:", documento); Assert.Contains("Obra HTTP A", documento);
        var pdf = await cliente.GetAsync($"/CotizacionesInstitucionales/Pdf/{solicitudId}"); Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType?.MediaType); Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(await pdf.Content.ReadAsByteArrayAsync()));
        Assert.Equal(HttpStatusCode.NotFound, (await otroCliente.GetAsync($"/CotizacionesInstitucionales/Pdf/{solicitudId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await otroAsesor.GetAsync($"/CotizacionesInstitucionales/Pdf/{solicitudId}")).StatusCode);
        var otroToken = Token(await otroCliente.GetStringAsync("/SolicitudesInstitucionales/Nueva"));
        Assert.Equal(HttpStatusCode.NotFound, (await otroCliente.PostAsync($"/CotizacionesInstitucionales/Aprobar/{solicitudId}", Form(otroToken))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await asesor.PostAsync($"/CotizacionesInstitucionales/Editar/{solicitudId}", Form(asesorToken, campos.Append(("operacion", "emitir")).ToArray()))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await cliente.PostAsync($"/CotizacionesInstitucionales/Aprobar/{solicitudId}", Form(token))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsync($"/CotizacionesInstitucionales/Rechazar/{solicitudId}", Form(token))).StatusCode);
        await es.EnScope(async sp =>
        {
            var s = await sp.GetRequiredService<EditorialDbContext>().tblSolicitudCotizacions.AsNoTracking().Include(s => s.tblCotizacion).SingleAsync(s => s.IDSolicitud == solicitudId);
            Assert.Equal(EstadosRf04.Aprobada, s.EstadoSolicitud); Assert.Equal(EstadosRf04.Aprobada, s.tblCotizacion!.EstadoCotizacion); return true;
        });
    }

    [Fact]
    public async Task Permisos_visitante_minorista_y_asesor_ajeno_incluyen_operaciones_de_escritura()
    {
        await using var es = await CrearAsync(); using var anonimo = es.Cliente();
        foreach (var ruta in new[] { "/Institucional", "/PerfilInstitucional", "/PerfilesInstitucionales", "/SolicitudesInstitucionales", "/CotizacionesInstitucionales/Documento/1" })
            Assert.Equal(HttpStatusCode.Unauthorized, (await anonimo.GetAsync(ruta)).StatusCode);
        var minorista = await es.UsuarioAsync("ClienteMinorista"); using var cliente = es.Cliente(minorista, "ClienteMinorista");
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/PerfilInstitucional/Solicitar")).StatusCode);
        foreach (var ruta in new[] { "/PerfilesInstitucionales", "/SolicitudesInstitucionales", "/CotizacionesInstitucionales/Editar/1", "/CotizacionesInstitucionales/Pdf/1" })
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync(ruta)).StatusCode);
        var admin = await es.UsuarioAsync("Administrador"); var propietario = await AprobarCliente(es, admin); var ajeno = await AprobarCliente(es, admin);
        var asesor = await es.UsuarioAsync("AsesorVentas"); var otro = await es.UsuarioAsync("AsesorVentas");
        var s1 = await NuevaSolicitud(es, propietario, "Obra propia"); var s2 = await NuevaSolicitud(es, ajeno, "Obra ajena");
        await es.EnScope(async sp =>
        {
            var srv = sp.GetRequiredService<SolicitudesInstitucionalesService>();
            await srv.AsignarAsync(Actor(admin, "Administrador"), s1, new() { AsesorId = asesor });
            await srv.AsignarAsync(Actor(admin, "Administrador"), s2, new() { AsesorId = otro });
            Assert.Single(await srv.BandejaAsync(Actor(asesor, "AsesorVentas")));
            Assert.Single(await srv.BandejaAsync(Actor(propietario, "ClienteInstitucional")));
            await Assert.ThrowsAsync<NoEncontradoRf04Exception>(() => srv.DetalleAsync(Actor(otro, "AsesorVentas"), s1));
            await Assert.ThrowsAsync<AccesoRf04Exception>(() => srv.AsignarAsync(Actor(asesor, "AsesorVentas"), s1, new() { AsesorId = otro }));
            return true;
        });
        using var asesorAjeno = es.Cliente(otro, "AsesorVentas");
        var token = Token(await asesorAjeno.GetStringAsync($"/CotizacionesInstitucionales/Editar/{s2}"));
        Assert.Equal(HttpStatusCode.NotFound, (await asesorAjeno.PostAsync($"/CotizacionesInstitucionales/Editar/{s1}", Form(token, ("operacion", "emitir")))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await asesorAjeno.PostAsync($"/SolicitudesInstitucionales/Asignar/{s1}", Form(token, ("Datos.AsesorId", otro)))).StatusCode);
    }

    [Fact]
    public async Task Validacion_fechas_detalles_ajenos_duplicados_y_cotizacion_vencida()
    {
        await using var es = await CrearAsync(); await es.ConfigurarImpuestoAsync(12);
        var admin = await es.UsuarioAsync("Administrador"); var propietario = await AprobarCliente(es, admin); var asesor = await es.UsuarioAsync("AsesorVentas");
        var s1 = await NuevaSolicitud(es, propietario); var s2 = await NuevaSolicitud(es, propietario);
        await es.EnScope(async sp =>
        {
            var solicitudes = sp.GetRequiredService<SolicitudesInstitucionalesService>(); var cotizaciones = sp.GetRequiredService<CotizacionesInstitucionalesService>(); var hoy = sp.GetRequiredService<TiempoRf04>().Hoy;
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => solicitudes.CrearAsync(Actor(propietario, "ClienteInstitucional"), new() { FechaRequerida = hoy.AddDays(-1), Detalles = [new() { Titulo = "Obra", Cantidad = 1, AcabadoPortada = "Mate", TipoPapel = "Bond" }] }));
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => solicitudes.CrearAsync(Actor(propietario, "ClienteInstitucional"), new() { FechaRequerida = hoy, Detalles = [] }));
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => cotizaciones.EdicionAsync(Actor(admin, "Administrador"), s1));
            await solicitudes.AsignarAsync(Actor(admin, "Administrador"), s1, new() { AsesorId = asesor });
            var m = (await cotizaciones.EdicionAsync(Actor(asesor, "AsesorVentas"), s1)).Datos;
            m.FechaVencimiento = hoy.AddDays(-1);
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => cotizaciones.GuardarAsync(Actor(asesor, "AsesorVentas"), s1, m, true));
            m.FechaVencimiento = hoy.AddDays(1); var original = m.Detalles[0].DetalleSolicitudId;
            var db = sp.GetRequiredService<EditorialDbContext>();
            m.Detalles[0].DetalleSolicitudId = await db.tblDetalleSolicitudCotizacions.Where(d => d.IDSolicitud == s2).Select(d => d.IDDetalleSolicitud).SingleAsync();
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => cotizaciones.GuardarAsync(Actor(asesor, "AsesorVentas"), s1, m, true));
            m.Detalles[0].DetalleSolicitudId = original; m.Detalles.Add(m.Detalles[0]);
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => cotizaciones.GuardarAsync(Actor(asesor, "AsesorVentas"), s1, m, true));
            m.Detalles.RemoveAt(1); m.Detalles[0].PrecioUnitario = 50;
            await cotizaciones.GuardarAsync(Actor(asesor, "AsesorVentas"), s1, m, true);
            var c = await db.tblCotizacions.SingleAsync(c => c.IDSolicitud == s1); c.FechaVencimiento = hoy.AddDays(-1); await db.SaveChangesAsync();
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => cotizaciones.ResponderAsync(Actor(propietario, "ClienteInstitucional"), s1, true));
            Assert.True((await solicitudes.DetalleAsync(Actor(propietario, "ClienteInstitucional"), s1)).Cotizacion!.Vencida);
            return true;
        });
    }

    [Fact]
    public async Task Sin_configuracion_no_emite_y_conserva_un_solo_borrador()
    {
        await using var es = await CrearAsync(sinImpuesto: true);
        var admin = await es.UsuarioAsync("Administrador"); var cliente = await AprobarCliente(es, admin); var asesor = await es.UsuarioAsync("AsesorVentas"); var id = await NuevaSolicitud(es, cliente);
        await es.EnScope(async sp =>
        {
            await sp.GetRequiredService<SolicitudesInstitucionalesService>().AsignarAsync(Actor(admin, "Administrador"), id, new() { AsesorId = asesor });
            var srv = sp.GetRequiredService<CotizacionesInstitucionalesService>(); var m = await srv.EdicionAsync(Actor(asesor, "AsesorVentas"), id);
            Assert.Null(m.PorcentajeImpuesto); Assert.NotNull(m.AvisoImpuesto); m.Datos.Detalles[0].PrecioUnitario = 50;
            var c = await srv.GuardarAsync(Actor(asesor, "AsesorVentas"), id, m.Datos, false);
            m.Datos.Detalles[0].PrecioUnitario = 60;
            Assert.Equal(c, await srv.GuardarAsync(Actor(asesor, "AsesorVentas"), id, m.Datos, false));
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => srv.GuardarAsync(Actor(asesor, "AsesorVentas"), id, m.Datos, true));
            var db = sp.GetRequiredService<EditorialDbContext>(); Assert.Single(await db.tblCotizacions.Where(c => c.IDSolicitud == id).ToListAsync());
            return true;
        });
    }

    [Fact]
    public async Task Rechazo_definitivo_y_reasignacion_quitan_acceso_al_asesor_anterior()
    {
        await using var es = await CrearAsync(); await es.ConfigurarImpuestoAsync(12);
        var admin = await es.UsuarioAsync("Administrador"); var cliente = await AprobarCliente(es, admin);
        var anterior = await es.UsuarioAsync("AsesorVentas"); var actual = await es.UsuarioAsync("AsesorVentas"); var id = await NuevaSolicitud(es, cliente);
        await es.EnScope(async sp =>
        {
            var srv = sp.GetRequiredService<SolicitudesInstitucionalesService>(); var cot = sp.GetRequiredService<CotizacionesInstitucionalesService>();
            await srv.AsignarAsync(Actor(admin, "Administrador"), id, new() { AsesorId = anterior });
            var edicion = await cot.EdicionAsync(Actor(anterior, "AsesorVentas"), id);
            edicion.Datos.Detalles[0].PrecioUnitario = 50;
            await cot.GuardarAsync(Actor(anterior, "AsesorVentas"), id, edicion.Datos, false);
            await srv.AsignarAsync(Actor(admin, "Administrador"), id, new() { AsesorId = actual });
            await Assert.ThrowsAsync<NoEncontradoRf04Exception>(() => cot.EdicionAsync(Actor(anterior, "AsesorVentas"), id));
            var edicionActual = await cot.EdicionAsync(Actor(actual, "AsesorVentas"), id);
            await cot.GuardarAsync(Actor(actual, "AsesorVentas"), id, edicionActual.Datos, true);
            await cot.ResponderAsync(Actor(cliente, "ClienteInstitucional"), id, false);
            Assert.Equal(EstadosRf04.Rechazada, (await srv.DetalleAsync(Actor(cliente, "ClienteInstitucional"), id)).Estado);
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => cot.ResponderAsync(Actor(cliente, "ClienteInstitucional"), id, true));
            await Assert.ThrowsAsync<ReglaRf04Exception>(() => srv.AsignarAsync(Actor(admin, "Administrador"), id, new() { AsesorId = anterior }));
            return true;
        });
    }
}
