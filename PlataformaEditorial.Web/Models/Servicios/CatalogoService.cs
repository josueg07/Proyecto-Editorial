using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Entidades;
namespace PlataformaEditorial.Web.Models.Servicios;
public class CatalogoService(EditorialDbContext db)
{
    private IQueryable<tblLibro> Consulta() => db.tblLibros.AsNoTracking().Where(l=>l.Activo)
        .Include(l=>l.IDEditorialNavigation).Include(l=>l.IDAutors.Where(a=>a.Activo))
        .Include(l=>l.IDCategoria.Where(c=>c.Activo))
        .Include(l=>l.tblPresentacionLibros).ThenInclude(p=>p.IDFormatoNavigation).AsSplitQuery();
    private static TarjetaLibro Tarjeta(tblLibro l, FiltroCatalogo? f = null)
    {
        var ps = l.tblPresentacionLibros.Where(p=>f == null ||
            (!f.FormatoId.HasValue || p.IDFormato==f.FormatoId) &&
            (!f.PrecioMinimo.HasValue || p.PrecioBase>=f.PrecioMinimo) &&
            (!f.PrecioMaximo.HasValue || p.PrecioBase<=f.PrecioMaximo)).ToList();
        return new(l.IDLibro,l.Titulo,l.portadaURL,string.Join(", ",l.IDAutors.Select(a=>a.NombreAutor)),l.IDEditorialNavigation.NombreEditorial,
            ps.Count==0 ? null : ps.Min(p=>p.PrecioBase),string.Join(", ",ps.Select(p=>p.IDFormatoNavigation.NombreFormato).Distinct()));
    }
    public async Task<CatalogoPagina> BuscarAsync(FiltroCatalogo f, bool valido = true)
    {
        var pagina = new CatalogoPagina { Filtro=f,
            Categorias=await db.tblCategoria.AsNoTracking().Where(c=>c.Activo).OrderBy(c=>c.NombreCategoria).Select(c=>new Opcion(c.IDCategoria,c.NombreCategoria)).ToListAsync(),
            Formatos=await db.tblFormatoLibros.AsNoTracking().OrderBy(c=>c.NombreFormato).Select(c=>new Opcion(c.IDFormato,c.NombreFormato)).ToListAsync() };
        if (!valido) return pagina;
        var q=Consulta();
        if (!string.IsNullOrWhiteSpace(f.Texto))
        {
            var t=f.Texto.Trim();
            q=q.Where(l=>l.Titulo.Contains(t) || l.IDEditorialNavigation.NombreEditorial.Contains(t) ||
                l.IDAutors.Any(a=>a.Activo && a.NombreAutor.Contains(t)) || l.tblPresentacionLibros.Any(p=>p.isbn.Contains(t)));
        }
        if (f.CategoriaId.HasValue) q=q.Where(l=>l.IDCategoria.Any(c=>c.Activo && c.IDCategoria==f.CategoriaId));
        // Todos los filtros comerciales deben cumplirse en la MISMA presentación.
        if (f.FormatoId.HasValue || f.PrecioMinimo.HasValue || f.PrecioMaximo.HasValue)
            q=q.Where(l=>l.tblPresentacionLibros.Any(p=>(!f.FormatoId.HasValue || p.IDFormato==f.FormatoId) &&
                (!f.PrecioMinimo.HasValue || p.PrecioBase>=f.PrecioMinimo) && (!f.PrecioMaximo.HasValue || p.PrecioBase<=f.PrecioMaximo)));
        pagina.Total=await q.CountAsync();
        f.Pagina=Math.Clamp(f.Pagina,1,Math.Max(1,pagina.Paginas));
        pagina.Libros=(await q.OrderBy(l=>l.Titulo).ThenBy(l=>l.IDLibro).Skip((f.Pagina-1)*12).Take(12).ToListAsync()).Select(l=>Tarjeta(l,f)).ToList();
        return pagina;
    }
    public Task<tblLibro?> DetalleAsync(int id) => Consulta().SingleOrDefaultAsync(l=>l.IDLibro==id);
    public async Task<InicioCatalogo> InicioAsync() => new(
        (await Consulta().Where(l=>l.Destacado).OrderByDescending(l=>l.fechaRegistro).ThenByDescending(l=>l.IDLibro).Take(6).ToListAsync()).Select(l=>Tarjeta(l)).ToList(),
        (await Consulta().OrderByDescending(l=>l.fechaRegistro).ThenByDescending(l=>l.IDLibro).Take(6).ToListAsync()).Select(l=>Tarjeta(l)).ToList());
}
