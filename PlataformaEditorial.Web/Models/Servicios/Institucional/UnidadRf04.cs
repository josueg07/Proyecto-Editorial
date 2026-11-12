using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Institucional;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;

// Serializa las transiciones: no se sobrescribe una cotización emitida o respondida.
public class UnidadRf04(EditorialDbContext db)
{
    public async Task<T> EjecutarAsync<T>(Func<Task<T>> accion)
    {
        try
        {
            // Las pruebas usan una transacción externa que siempre se revierte.
            if (db.Database.CurrentTransaction != null) return await accion();
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var resultado = await accion();
            await tx.CommitAsync();
            return resultado;
        }
        catch (DbUpdateConcurrencyException) { throw new ReglaRf04Exception("El registro cambió. Recargue la página."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        { throw new ReglaRf04Exception("Ya existe el perfil o la cotización de esta solicitud. Recargue la página."); }
        catch (Exception ex) when ((ex is SqlException sql && sql.Number == 1205) || (ex.InnerException is SqlException inner && inner.Number == 1205))
        { throw new ReglaRf04Exception("Otro usuario está actualizando la solicitud. Recargue e intente de nuevo."); }
    }
}
