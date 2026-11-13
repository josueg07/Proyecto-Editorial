using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PlataformaEditorial.Web.Localizacion;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using static PlataformaEditorial.Tests.InstitucionalEscenario;
namespace PlataformaEditorial.Tests;
public class LocalizacionTests
{
    [Fact]
    public async Task Formularios_errores_y_confirmacion_sin_correo_son_claros_y_seguros()
    {
        await using var es=await CrearAsync();using var anonimo=es.Cliente();
        var login=WebUtility.HtmlDecode(await anonimo.GetStringAsync("/Identity/Account/Login"));
        Assert.DoesNotContain("\u00ef\u00bb\u00bf",login);Assert.Contains("Contraseña",login);Assert.Contains("Correo electrónico",login);Assert.Contains("es obligatorio",login);
        var invalid=await anonimo.PostAsync("/Identity/Account/Login",Form(Token(login),("Input.Email","no-es-correo"),("Input.Password","abc")));
        var errorHtml=WebUtility.HtmlDecode(await invalid.Content.ReadAsStringAsync());
        var mensajes=string.Join(" | ",System.Text.RegularExpressions.Regex.Matches(errorHtml,"<span[^>]*field-validation-error[^>]*>(.*?)</span>",System.Text.RegularExpressions.RegexOptions.Singleline).Select(m=>m.Groups[1].Value));
        Assert.Contains("válid",mensajes);Assert.DoesNotContain("The ",mensajes);
        var confirmacion=await anonimo.GetStringAsync("/Identity/Account/RegisterConfirmation?email=cualquiera%40example.invalid");
        Assert.DoesNotContain("confirm-link",confirmacion);Assert.DoesNotContain("code=",confirmacion);Assert.Contains("No se envi",confirmacion);
        var olvidada=await anonimo.GetStringAsync("/Identity/Account/ForgotPassword");
        var correo=await anonimo.PostAsync("/Identity/Account/ForgotPassword",Form(Token(olvidada),("Input.Email","cualquiera@example.invalid")));
        Assert.Contains("No se envi",await correo.Content.ReadAsStringAsync());
        var inexistente=await anonimo.GetAsync("/pagina-que-no-existe");Assert.Equal(HttpStatusCode.NotFound,inexistente.StatusCode);Assert.Contains("Página no encontrada",WebUtility.HtmlDecode(await inexistente.Content.ReadAsStringAsync()));
        var errores=new ErroresIdentityEspanol();Assert.Contains("mayúscula",errores.PasswordRequiresUpper().Description);Assert.Contains("Ya existe",errores.DuplicateEmail("prueba@example.invalid").Description);
    }
    [Fact]
    public async Task Navegacion_y_paginas_de_cuenta_muestran_estado_institucional_y_textos_en_espanol()
    {
        await using var es=await CrearAsync();var id=await es.UsuarioAsync("ClienteMinorista");using var cliente=es.Cliente(id,"ClienteMinorista");
        var inicio=WebUtility.HtmlDecode(await cliente.GetStringAsync("/"));Assert.DoesNotContain("\u00ef\u00bb\u00bf",inicio);Assert.Contains("Solicitar perfil institucional",inicio);Assert.Contains("Mis pedidos",inicio);Assert.Contains("Carrito",inicio);
        await es.EnScope(async sp=>{var db=sp.GetRequiredService<EditorialDbContext>();db.Add(new tblPerfilInstitucional{IDUsuario=id,NombreInstitucion="Prueba",NIT="Prueba",Direccion="Prueba",EstadoValidacion="Pendiente"});await db.SaveChangesAsync();return true;});
        inicio=await cliente.GetStringAsync("/");Assert.Contains("Perfil institucional: Pendiente",inicio);Assert.DoesNotContain(">Solicitar perfil institucional<",inicio);
        foreach(var pagina in new[]{"Index","ChangePassword","Email","PersonalData","TwoFactorAuthentication","SetPassword"})
        {
            var response=await cliente.GetAsync("/Identity/Account/Manage/"+pagina);
            if(response.StatusCode==HttpStatusCode.Redirect) continue;
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            var html=WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            Assert.Contains("Administrar mi cuenta",html);Assert.DoesNotContain("Manage your account",html);Assert.DoesNotContain("Please enter",html);
        }
    }
}
