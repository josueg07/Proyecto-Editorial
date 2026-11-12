using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaEditorial.Web.Data;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Entidades;
using PlataformaEditorial.Web.Models.Servicios.Institucional;

namespace PlataformaEditorial.Tests;

internal sealed class InstitucionalEscenario : IAsyncDisposable
{
    public required SqlConnection Conexion { get; init; }
    public required SqlTransaction Transaccion { get; init; }
    public required WebApplicationFactory<Program> App { get; init; }
    public static async Task<InstitucionalEscenario> CrearAsync(bool sinImpuesto = false)
    {
        var conexion = new SqlConnection(Environment.GetEnvironmentVariable("EDITORIAL_TEST_CONNECTION") ?? @"Server=.\SQLEXPRESS;Database=EditorialDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
        await conexion.OpenAsync();
        var tx = (SqlTransaction)await conexion.BeginTransactionAsync(IsolationLevel.Serializable);
        var app = new HostRf04(conexion, tx, sinImpuesto);
        _ = app.Services; // Inicia el host y comprueba los roles existentes.
        return new() { Conexion = conexion, Transaccion = tx, App = app };
    }
    public HttpClient Cliente(string? id = null, string? roles = null)
    {
        var cliente = App.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        if (id != null) cliente.DefaultRequestHeaders.Add("X-Rf04-Usuario", id);
        if (roles != null) cliente.DefaultRequestHeaders.Add("X-Rf04-Roles", roles);
        return cliente;
    }
    public async Task<T> EnScope<T>(Func<IServiceProvider, Task<T>> accion)
    {
        await using var scope = App.Services.CreateAsyncScope(); return await accion(scope.ServiceProvider);
    }
    public Task<string> UsuarioAsync(string rol) => EnScope(async sp =>
    {
        var manager = sp.GetRequiredService<UserManager<IdentityUser>>();
        var user = new IdentityUser { UserName = $"rf04-{Guid.NewGuid():N}@example.invalid", EmailConfirmed = true };
        Assert.True((await manager.CreateAsync(user)).Succeeded);
        Assert.True((await manager.AddToRoleAsync(user, rol)).Succeeded);
        return user.Id;
    });
    public async Task ConfigurarImpuestoAsync(decimal tasa) => await EnScope(async sp =>
    {
        var db = sp.GetRequiredService<EditorialDbContext>();
        var config = await db.tblConfiguracionSistemas.ToListAsync();
        Assert.True(config.Count <= 1, "Use una copia de pruebas si la configuración real es ambigua.");
        if (config.Count == 0) db.Add(new tblConfiguracionSistema { PorcentajeImpuesto = tasa });
        else config[0].PorcentajeImpuesto = tasa;
        await db.SaveChangesAsync(); return true;
    });
    public static string Token(string html)
    {
        var token = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Assert.NotEmpty(token); return token;
    }
    public static FormUrlEncodedContent Form(string token, params (string Key, string Value)[] datos) =>
        new(datos.Append(("__RequestVerificationToken", token)).Select(p => new KeyValuePair<string, string>(p.Item1, p.Item2)));
    public async ValueTask DisposeAsync()
    {
        await App.DisposeAsync();
        await Transaccion.RollbackAsync(); await Transaccion.DisposeAsync(); await Conexion.DisposeAsync();
    }
}

internal sealed class HostRf04(SqlConnection conexion, SqlTransaction transaccion, bool sinImpuesto) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureTestServices(services =>
    {
        if (sinImpuesto) { services.RemoveAll<IImpuestoInstitucional>(); services.AddScoped<IImpuestoInstitucional, _ImpuestoNoConfigurado>(); }
        services.RemoveAll<EditorialDbContext>(); services.RemoveAll<ApplicationDbContext>();
        services.AddScoped(_ =>
        {
            var db = new EditorialDbContext(new DbContextOptionsBuilder<EditorialDbContext>().UseSqlServer(conexion).Options);
            db.Database.UseTransaction(transaccion); return db;
        });
        services.AddScoped(_ =>
        {
            var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(conexion).Options);
            db.Database.UseTransaction(transaccion); return db;
        });
        services.AddAuthentication(o => { o.DefaultAuthenticateScheme = "Rf04Pruebas"; o.DefaultChallengeScheme = "Rf04Pruebas"; o.DefaultForbidScheme = "Rf04Pruebas"; })
            .AddScheme<AuthenticationSchemeOptions, AutenticacionRf04Pruebas>("Rf04Pruebas", _ => { });
    });
}
internal sealed class AutenticacionRf04Pruebas(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var id = Request.Headers["X-Rf04-Usuario"].ToString();
        if (string.IsNullOrEmpty(id)) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, id), new(ClaimTypes.Name, "Usuario de prueba") };
        claims.AddRange(Request.Headers["X-Rf04-Roles"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(r => new Claim(ClaimTypes.Role, r)));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, "Rf04Pruebas")), "Rf04Pruebas")));
    }
}

internal class _ImpuestoNoConfigurado : IImpuestoInstitucional { public Task<decimal?> ObtenerAsync() => Task.FromResult<decimal?>(null); }
