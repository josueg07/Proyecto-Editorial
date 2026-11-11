using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Entidades;
namespace PlataformaEditorial.Web.Models.Servicios;
public class LibrosService(EditorialDbContext db) : ServicioEscritura(db)
{
    public Task<List<tblLibro>> ListarAsync() => Db.tblLibros.AsNoTracking().Include(l=>l.IDEditorialNavigation).OrderBy(l=>l.Titulo).ToListAsync();
    public async Task<LibroEdicion?> ObtenerAsync(int id)
    {
        var l=await Db.tblLibros.AsNoTracking().Include(l=>l.IDAutors).Include(l=>l.IDCategoria).SingleOrDefaultAsync(l=>l.IDLibro==id);
        return l==null ? null : new() { Id=l.IDLibro,Titulo=l.Titulo,Sinopsis=l.Sinopsis,AnioEdicion=l.AnioEdicion,PortadaUrl=l.portadaURL,EditorialId=l.IDEditorial,Activo=l.Activo,Destacado=l.Destacado,AutoresIds=l.IDAutors.Select(a=>a.IDAutor).ToArray(),CategoriasIds=l.IDCategoria.Select(c=>c.IDCategoria).ToArray() };
    }
    public async Task CompletarAsync(LibroEdicion m)
    {
        m.Editoriales=await Db.tblEditorials.AsNoTracking().OrderBy(x=>x.NombreEditorial).Select(x=>new SelectListItem(x.NombreEditorial,x.IDEditorial.ToString())).ToListAsync();
        m.Autores=await Db.tblAutors.AsNoTracking().OrderBy(x=>x.NombreAutor).Select(x=>new SelectListItem(x.NombreAutor+(x.Activo ? "" : " (inactivo)"),x.IDAutor.ToString())).ToListAsync();
        m.Categorias=await Db.tblCategoria.AsNoTracking().OrderBy(x=>x.NombreCategoria).Select(x=>new SelectListItem(x.NombreCategoria+(x.Activo ? "" : " (inactiva)"),x.IDCategoria.ToString())).ToListAsync();
    }
    public async Task GuardarAsync(LibroEdicion m)
    {
        if(!await Db.tblEditorials.AnyAsync(e=>e.IDEditorial==m.EditorialId)) throw new ReglaNegocioException("Seleccione una editorial válida.",nameof(m.EditorialId));
        var autores=await Db.tblAutors.Where(a=>m.AutoresIds.Contains(a.IDAutor)).ToListAsync();
        var categorias=await Db.tblCategoria.Where(c=>m.CategoriasIds.Contains(c.IDCategoria)).ToListAsync();
        if(autores.Count!=m.AutoresIds.Distinct().Count() || categorias.Count!=m.CategoriasIds.Distinct().Count()) throw new ReglaNegocioException("Hay autores o categorías que ya no existen.");
        var l=m.Id==0 ? new tblLibro { fechaRegistro=DateTime.Now } : await Db.tblLibros.Include(l=>l.IDAutors).Include(l=>l.IDCategoria).SingleOrDefaultAsync(l=>l.IDLibro==m.Id) ?? throw new ReglaNegocioException("El libro ya no existe.");
        l.Titulo=m.Titulo.Trim(); l.Sinopsis=m.Sinopsis.Trim(); l.AnioEdicion=m.AnioEdicion; l.portadaURL=m.PortadaUrl.Trim(); l.IDEditorial=m.EditorialId; l.Activo=m.Activo; l.Destacado=m.Destacado;
        l.IDAutors.Clear(); foreach(var a in autores) l.IDAutors.Add(a);
        l.IDCategoria.Clear(); foreach(var c in categorias) l.IDCategoria.Add(c);
        if(m.Id==0) Db.Add(l);
        await GuardarAsync();
    }
    public async Task EliminarAsync(int id)
    {
        var l=await Db.tblLibros.FindAsync(id) ?? throw new ReglaNegocioException("Libro inexistente.");
        l.Activo=false; await GuardarAsync();
    }
}
