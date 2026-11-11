using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PlataformaEditorial.Web.Data.Negocio;
namespace PlataformaEditorial.Web.Models.Servicios;
public class ReglaNegocioException(string message, string campo = "") : Exception(message)
{
    public string Campo { get; } = campo;
}
public abstract class ServicioEscritura(EditorialDbContext db)
{
    protected EditorialDbContext Db { get; } = db;
    protected async Task GuardarAsync()
    {
        try { await Db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { throw new ReglaNegocioException("El registro cambió o ya no existe. Recargue la página."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        { throw new ReglaNegocioException("Ya existe un registro con ese nombre, ISBN o combinación de libro y formato."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number == 547)
        { throw new ReglaNegocioException("La operación no es válida por sus relaciones o restricciones. Revise los datos; los registros relacionados deben conservarse."); }
    }
}
