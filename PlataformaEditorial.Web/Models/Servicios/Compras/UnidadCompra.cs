using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Compras;
namespace PlataformaEditorial.Web.Models.Servicios.Compras;
public class UnidadCompra(EditorialDbContext db)
{
    public async Task<T> Ejecutar<T>(Func<Task<T>> accion)
    {
        try
        {
            if(db.Database.CurrentTransaction!=null) return await accion();
            await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var resultado=await accion(); await tx.CommitAsync(); return resultado;
        }
        catch(DbUpdateConcurrencyException) { throw new ReglaCompraException("El carrito cambió. Revísalo e intenta nuevamente."); }
        catch(DbUpdateException ex) when(ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        { throw new ReglaCompraException("El carrito fue actualizado por otra solicitud. Recarga la página."); }
        catch(Exception ex) when(ex is SqlException { Number:1205 } || ex.InnerException is SqlException { Number:1205 })
        { throw new ReglaCompraException("Otra compra está actualizando las existencias. Revisa el carrito e intenta de nuevo."); }
    }
}
