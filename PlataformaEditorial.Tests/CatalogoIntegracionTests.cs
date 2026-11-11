using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Servicios;

namespace PlataformaEditorial.Tests;

public class CatalogoIntegracionTests
{
    private static string Conexion => Environment.GetEnvironmentVariable("EDITORIAL_TEST_CONNECTION") ?? @"Server=.\SQLEXPRESS;Database=EditorialDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
    private static EditorialDbContext Contexto() => new(new DbContextOptionsBuilder<EditorialDbContext>().UseSqlServer(Conexion).Options);

    [Fact]
    public async Task Servicios_CRUD_relaciones_filtros_y_eliminacion_logica_en_SQL_Server()
    {
        await using var db = Contexto();
        await using var tx = await db.Database.BeginTransactionAsync();
        // Dispose revierte la transacción incluso si falla una aserción. No hay migraciones.
        var sufijo = Guid.NewGuid().ToString("N");
        var referencias = new ReferenciasService(db);
        await referencias.GuardarAsync(TipoReferencia.Autores, new() { Nombre = "Autor " + sufijo });
        await referencias.GuardarAsync(TipoReferencia.Categorias, new() { Nombre = "Categoría " + sufijo });
        await referencias.GuardarAsync(TipoReferencia.Editoriales, new() { Nombre = "Editorial " + sufijo });
        await referencias.GuardarAsync(TipoReferencia.Formatos, new() { Nombre = "Impreso " + sufijo });
        await referencias.GuardarAsync(TipoReferencia.Formatos, new() { Nombre = "Digital " + sufijo, EsDigital = true });
        var autor = (await referencias.ListarAsync(TipoReferencia.Autores)).Single(x => x.Nombre.EndsWith(sufijo));
        var categoria = (await referencias.ListarAsync(TipoReferencia.Categorias)).Single(x => x.Nombre.EndsWith(sufijo));
        var editorial = (await referencias.ListarAsync(TipoReferencia.Editoriales)).Single(x => x.Nombre.EndsWith(sufijo));
        var formatos = (await referencias.ListarAsync(TipoReferencia.Formatos)).Where(x => x.Nombre.EndsWith(sufijo)).ToList();
        var fisico = formatos.Single(x => x.EsDigital == false); var digital = formatos.Single(x => x.EsDigital == true);
        var libros = new LibrosService(db);
        var m = new LibroEdicion { Titulo = "Arquitectura " + sufijo, Sinopsis = "Sinopsis de integración", EditorialId = editorial.Id, AutoresIds = [autor.Id], CategoriasIds = [categoria.Id], Destacado = true };
        await libros.GuardarAsync(m);
        var id = (await libros.ListarAsync()).Single(x => x.Titulo == m.Titulo).IDLibro;
        var presentaciones = new PresentacionesService(db);
        await presentaciones.GuardarAsync(new() { LibroId = id, FormatoId = fisico.Id, Isbn = "9780000000001", Precio = 20, Existencias = 7 });
        await presentaciones.GuardarAsync(new() { LibroId = id, FormatoId = digital.Id, Isbn = "9780000000002", Precio = 100 });
        var catalogo = new CatalogoService(db);
        foreach (var texto in new[] { m.Titulo, autor.Nombre, editorial.Nombre, "9780000000001" })
            Assert.Contains((await catalogo.BuscarAsync(new() { Texto = texto })).Libros, x => x.Id == id);
        Assert.DoesNotContain((await catalogo.BuscarAsync(new() { Texto = sufijo, FormatoId = digital.Id, PrecioMaximo = 30 })).Libros, x => x.Id == id);
        var resultado = await catalogo.BuscarAsync(new() { Texto = sufijo, CategoriaId = categoria.Id, FormatoId = fisico.Id, PrecioMinimo = 15, PrecioMaximo = 30 });
        Assert.Equal(id, Assert.Single(resultado.Libros).Id); Assert.Equal(20, resultado.Libros[0].Precio);
        var detalle = await catalogo.DetalleAsync(id); Assert.NotNull(detalle); Assert.Equal(2, detalle.tblPresentacionLibros.Count);
        Assert.Single(detalle.IDAutors); Assert.Single(detalle.IDCategoria);
        Assert.Contains((await catalogo.InicioAsync()).Destacados, x => x.Id == id);
        Assert.Contains((await catalogo.InicioAsync()).Novedades, x => x.Id == id);
        await Assert.ThrowsAsync<ReglaNegocioException>(() => referencias.EliminarAsync(TipoReferencia.Editoriales, editorial.Id));
        await Assert.ThrowsAsync<ReglaNegocioException>(() => referencias.EliminarAsync(TipoReferencia.Formatos, fisico.Id));
        var edicion = (await libros.ObtenerAsync(id))!; edicion.Titulo += " editado"; await libros.GuardarAsync(edicion);
        Assert.EndsWith("editado", (await catalogo.DetalleAsync(id))!.Titulo);
        var pres = (await presentaciones.ListarAsync()).Single(x => x.IDLibro == id && x.IDFormato == fisico.Id);
        var pe = (await presentaciones.ObtenerAsync(pres.IDPresentacion))!; pe.Precio = 25.50m; pe.Existencias = 0; await presentaciones.GuardarAsync(pe);
        Assert.Equal(25.50m, (await presentaciones.ObtenerAsync(pe.Id))!.Precio);
        await referencias.EliminarAsync(TipoReferencia.Autores, autor.Id);
        await referencias.EliminarAsync(TipoReferencia.Categorias, categoria.Id);
        Assert.Empty((await catalogo.DetalleAsync(id))!.IDAutors); Assert.Empty((await catalogo.DetalleAsync(id))!.IDCategoria);
        Assert.DoesNotContain((await catalogo.BuscarAsync(new() { Texto = sufijo, CategoriaId = categoria.Id })).Libros, x => x.Id == id);
        await presentaciones.EliminarAsync(pe.Id); Assert.Null(await presentaciones.ObtenerAsync(pe.Id));
        await libros.EliminarAsync(id); Assert.Null(await catalogo.DetalleAsync(id)); Assert.NotNull(await libros.ObtenerAsync(id));
        edicion.Activo = true; await libros.GuardarAsync(edicion); Assert.NotNull(await catalogo.DetalleAsync(id));
        // Restricción única traducida a un error comprensible.
        await Assert.ThrowsAsync<ReglaNegocioException>(() => referencias.GuardarAsync(TipoReferencia.Editoriales, new() { Nombre = editorial.Nombre }));
        await tx.RollbackAsync();
    }

    [Fact]
    public async Task HTTP_publico_autorizacion_vistas_formularios_y_antifalsificacion()
    {
        await using var connection = new SqlConnection(Conexion); await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        await using var app = new AplicacionPruebas(connection, transaction);
        using var client = app.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        foreach (var url in new[] { "/", "/Catalogo", "/Catalogo?Texto=prueba&PrecioMinimo=1&PrecioMaximo=200", "/Catalogo?PrecioMinimo=200&PrecioMaximo=1" })
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Catalogo/Detalles/2147483647")).StatusCode);
        var administracion = new[] { "Administracion", "Libros", "Autores", "Categorias", "Editoriales", "Formatos", "Presentaciones" };
        foreach (var controller in administracion) Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/" + controller)).StatusCode);
        client.DefaultRequestHeaders.Add("X-Test-Role", "ClienteMinorista");
        foreach (var controller in administracion) Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/" + controller)).StatusCode);
        client.DefaultRequestHeaders.Remove("X-Test-Role"); client.DefaultRequestHeaders.Add("X-Test-Role", "Administrador");
        foreach (var controller in administracion) Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/" + controller)).StatusCode);
        foreach (var controller in administracion.Skip(1))
        {
            var response = await client.GetAsync($"/{controller}/Crear"); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync(); Assert.Contains("__RequestVerificationToken", html);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/{controller}/Crear", new FormUrlEncodedContent([]))).StatusCode);
            var token = Token(html);
            var invalido = await client.PostAsync($"/{controller}/Crear", Form(("__RequestVerificationToken", token)));
            Assert.Equal(HttpStatusCode.OK, invalido.StatusCode);
            Assert.Contains("validation-summary-errors", await invalido.Content.ReadAsStringAsync());
        }
        // Verifica binding del formulario compartido, creación, edición y baja lógica real.
        var crear = await client.GetStringAsync("/Autores/Crear");
        var nombre = "Autor HTTP " + Guid.NewGuid().ToString("N");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Autores/Crear", Form(("__RequestVerificationToken", Token(crear)), ("Nombre", nombre), ("Activo", "true")))).StatusCode);
        await using var db = new EditorialDbContext(new DbContextOptionsBuilder<EditorialDbContext>().UseSqlServer(connection).Options);
        await db.Database.UseTransactionAsync(transaction);
        var autor = await db.tblAutors.AsNoTracking().SingleAsync(x => x.NombreAutor == nombre);
        var editar = await client.GetStringAsync($"/Autores/Editar/{autor.IDAutor}");
        Assert.Contains(nombre, editar);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Autores/Editar/{autor.IDAutor}", Form(("__RequestVerificationToken", Token(editar)), ("Id", autor.IDAutor.ToString()), ("Nombre", nombre + " editado"), ("Activo", "true")))).StatusCode);
        var eliminar = await client.GetStringAsync($"/Autores/Eliminar/{autor.IDAutor}");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Autores/Eliminar/{autor.IDAutor}", Form(("__RequestVerificationToken", Token(eliminar))))).StatusCode);
        Assert.False((await db.tblAutors.AsNoTracking().SingleAsync(x => x.IDAutor == autor.IDAutor)).Activo);
        // Prueba los formularios de libro y presentación, y el detalle Razor con datos.
        var editorial = new tblEditorial { NombreEditorial = "Editorial HTTP " + Guid.NewGuid().ToString("N") };
        var formato = new tblFormatoLibro { NombreFormato = "Formato HTTP " + Guid.NewGuid().ToString("N") };
        db.AddRange(editorial, formato); await db.SaveChangesAsync();
        var libroForm = await client.GetStringAsync("/Libros/Crear");
        var titulo = "Libro HTTP " + Guid.NewGuid().ToString("N");
        var libroPost = await client.PostAsync("/Libros/Crear", Form(("__RequestVerificationToken", Token(libroForm)), ("Titulo", titulo), ("Sinopsis", "Sinopsis HTTP de prueba"), ("AnioEdicion", "2026"), ("PortadaUrl", "/images/sin-portada.svg"), ("EditorialId", editorial.IDEditorial.ToString()), ("Activo", "true"), ("Destacado", "true"), ("AutoresIds", autor.IDAutor.ToString())));
        Assert.Equal(HttpStatusCode.Redirect, libroPost.StatusCode);
        var libro = await db.tblLibros.AsNoTracking().SingleAsync(x => x.Titulo == titulo);
        var presentacionForm = await client.GetStringAsync("/Presentaciones/Crear");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Presentaciones/Crear", Form(("__RequestVerificationToken", Token(presentacionForm)), ("LibroId", libro.IDLibro.ToString()), ("FormatoId", formato.IDFormato.ToString()), ("Isbn", "9780000000999"), ("Precio", "45.25"), ("Existencias", "3")))).StatusCode);
        var detalle = await client.GetStringAsync($"/Catalogo/Detalles/{libro.IDLibro}");
        Assert.Contains("Sinopsis HTTP de prueba", detalle); Assert.Contains("9780000000999", detalle);
        var editarLibro = await client.GetStringAsync($"/Libros/Editar/{libro.IDLibro}");
        Assert.Contains(titulo, editarLibro);
        var presentacion = await db.tblPresentacionLibros.AsNoTracking().SingleAsync(x => x.IDLibro == libro.IDLibro);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/Presentaciones/Editar/{presentacion.IDPresentacion}")).StatusCode);
        var eliminarPresentacion = await client.GetStringAsync($"/Presentaciones/Eliminar/{presentacion.IDPresentacion}");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Presentaciones/Eliminar/{presentacion.IDPresentacion}", Form(("__RequestVerificationToken", Token(eliminarPresentacion))))).StatusCode);
        var eliminarLibro = await client.GetStringAsync($"/Libros/Eliminar/{libro.IDLibro}");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Libros/Eliminar/{libro.IDLibro}", Form(("__RequestVerificationToken", Token(eliminarLibro))))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/Catalogo/Detalles/{libro.IDLibro}")).StatusCode);
        await transaction.RollbackAsync();
    }
    private static string Token(string html) => WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    private static FormUrlEncodedContent Form(params (string Key, string Value)[] campos) => new(campos.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
}

// Autenticación de prueba registrada SOLO en el host de pruebas, nunca en la aplicación.
internal class AplicacionPruebas(SqlConnection connection, SqlTransaction transaction) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<EditorialDbContext>();
            services.AddScoped(_ =>
            {
                var db = new EditorialDbContext(new DbContextOptionsBuilder<EditorialDbContext>().UseSqlServer(connection).Options);
                db.Database.UseTransaction(transaction); return db;
            });
            services.AddAuthentication(o => { o.DefaultAuthenticateScheme = "Pruebas"; o.DefaultChallengeScheme = "Pruebas"; o.DefaultForbidScheme = "Pruebas"; })
                .AddScheme<AuthenticationSchemeOptions, AutenticacionPruebas>("Pruebas", _ => { });
        });
    }
}
internal class AutenticacionPruebas(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers["X-Test-Role"].ToString();
        if (string.IsNullOrEmpty(role)) return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Prueba"), new Claim(ClaimTypes.NameIdentifier, "prueba"), new Claim(ClaimTypes.Role, role) }, "Pruebas");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Pruebas")));
    }
}
