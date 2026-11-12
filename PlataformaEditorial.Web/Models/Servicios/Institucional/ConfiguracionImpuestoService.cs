using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;
public interface IImpuestoInstitucional
{
    Task<decimal?> ObtenerAsync();
}
public class ConfiguracionImpuestoService(EditorialDbContext db) : IImpuestoInstitucional
{
    public async Task<decimal?> ObtenerAsync()
    {
        var configuraciones=await db.tblConfiguracionSistemas.AsNoTracking().Take(2).ToListAsync();
        if(configuraciones.Count!=1) return null;
        var tasa=configuraciones[0].PorcentajeImpuesto;
        return tasa is >=0 and <=100 ? tasa : null;
    }
}
