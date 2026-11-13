using Microsoft.AspNetCore.Identity.UI.Services;
namespace PlataformaEditorial.Web.Localizacion;
// Fallar explícitamente evita que un consumidor nuevo anuncie un envío inexistente.
public class CorreoNoConfigurado : IEmailSender
{
    public Task SendEmailAsync(string email,string subject,string htmlMessage) => throw new NotSupportedException("El envío de correo electrónico no está configurado.");
}
