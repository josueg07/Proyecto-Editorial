using System.Security.Claims;
namespace PlataformaEditorial.Web.Models.Institucional;

public static class EstadosRf04
{
    public const string Pendiente = "Pendiente", Aprobado = "Aprobado", Rechazado = "Rechazado";
    public const string Asignada = "Asignada", EnRevision = "EnRevision", Cotizada = "Cotizada";
    public const string Borrador = "Borrador", Emitida = "Emitida", Aprobada = "Aprobada", Rechazada = "Rechazada";
}
// Se construye exclusivamente desde el principal autenticado, nunca mediante model binding.
public record ActorRf04(string UsuarioId, bool Administrador, bool Asesor, bool Institucional)
{
    public static ActorRf04 Desde(ClaimsPrincipal user) => new(
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException(),
        user.IsInRole("Administrador"), user.IsInRole("AsesorVentas"), user.IsInRole("ClienteInstitucional"));
}
public class AccesoRf04Exception : Exception;
public class NoEncontradoRf04Exception : Exception;
public class ReglaRf04Exception(string message) : Exception(message);

public class TiempoRf04(TimeProvider reloj)
{
    public DateTime Ahora => TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala")).DateTime;
    public DateOnly Hoy => DateOnly.FromDateTime(Ahora);
}
