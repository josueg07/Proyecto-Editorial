using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Entidades;
namespace PlataformaEditorial.Web.Models.Servicios;
public class ReferenciasService(EditorialDbContext db) : ServicioEscritura(db)
{
    public async Task<List<ReferenciaFila>> ListarAsync(TipoReferencia tipo) => tipo switch
    {
        TipoReferencia.Autores => await Db.tblAutors.AsNoTracking().OrderBy(x=>x.NombreAutor).Select(x=>new ReferenciaFila(x.IDAutor,x.NombreAutor,x.Activo,null)).ToListAsync(),
        TipoReferencia.Categorias => await Db.tblCategoria.AsNoTracking().OrderBy(x=>x.NombreCategoria).Select(x=>new ReferenciaFila(x.IDCategoria,x.NombreCategoria,x.Activo,null)).ToListAsync(),
        TipoReferencia.Editoriales => await Db.tblEditorials.AsNoTracking().OrderBy(x=>x.NombreEditorial).Select(x=>new ReferenciaFila(x.IDEditorial,x.NombreEditorial,null,null)).ToListAsync(),
        _ => await Db.tblFormatoLibros.AsNoTracking().OrderBy(x=>x.NombreFormato).Select(x=>new ReferenciaFila(x.IDFormato,x.NombreFormato,null,x.EsDigital)).ToListAsync()
    };
    public async Task<ReferenciaEdicion?> ObtenerAsync(TipoReferencia tipo,int id)
    {
        var fila=(await ListarAsync(tipo)).SingleOrDefault(x=>x.Id==id);
        return fila==null ? null : new() { Id=fila.Id,Nombre=fila.Nombre,Activo=fila.Activo??true,EsDigital=fila.EsDigital??false };
    }
    public async Task GuardarAsync(TipoReferencia tipo, ReferenciaEdicion m)
    {
        m.Nombre=m.Nombre.Trim();
        var max=tipo==TipoReferencia.Categorias ? 100 : tipo==TipoReferencia.Formatos ? 50 : 150;
        if(m.Nombre.Length==0 || m.Nombre.Length>max) throw new ReglaNegocioException($"El nombre debe tener entre 1 y {max} caracteres.",nameof(m.Nombre));
        switch(tipo)
        {
            case TipoReferencia.Autores:
                var a=m.Id==0 ? new tblAutor() : await Db.tblAutors.FindAsync(m.Id) ?? throw new ReglaNegocioException("Autor inexistente.");
                a.NombreAutor=m.Nombre; a.Activo=m.Activo; if(m.Id==0) Db.Add(a); break;
            case TipoReferencia.Categorias:
                var c=m.Id==0 ? new tblCategorium() : await Db.tblCategoria.FindAsync(m.Id) ?? throw new ReglaNegocioException("Categoría inexistente.");
                c.NombreCategoria=m.Nombre; c.Activo=m.Activo; if(m.Id==0) Db.Add(c); break;
            case TipoReferencia.Editoriales:
                var e=m.Id==0 ? new tblEditorial() : await Db.tblEditorials.FindAsync(m.Id) ?? throw new ReglaNegocioException("Editorial inexistente.");
                e.NombreEditorial=m.Nombre; if(m.Id==0) Db.Add(e); break;
            case TipoReferencia.Formatos:
                var f=m.Id==0 ? new tblFormatoLibro() : await Db.tblFormatoLibros.FindAsync(m.Id) ?? throw new ReglaNegocioException("Formato inexistente.");
                f.NombreFormato=m.Nombre; f.EsDigital=m.EsDigital; if(m.Id==0) Db.Add(f); break;
        }
        await GuardarAsync();
    }
    public async Task EliminarAsync(TipoReferencia tipo,int id)
    {
        switch(tipo)
        {
            case TipoReferencia.Autores:
                var a=await Db.tblAutors.FindAsync(id) ?? throw new ReglaNegocioException("Autor inexistente."); a.Activo=false; break;
            case TipoReferencia.Categorias:
                var c=await Db.tblCategoria.FindAsync(id) ?? throw new ReglaNegocioException("Categoría inexistente."); c.Activo=false; break;
            case TipoReferencia.Editoriales:
                if(await Db.tblLibros.AnyAsync(l=>l.IDEditorial==id)) throw new ReglaNegocioException("La editorial tiene libros relacionados y debe conservarse.");
                Db.Remove(await Db.tblEditorials.FindAsync(id) ?? throw new ReglaNegocioException("Editorial inexistente.")); break;
            case TipoReferencia.Formatos:
                if(await Db.tblPresentacionLibros.AnyAsync(p=>p.IDFormato==id)) throw new ReglaNegocioException("El formato tiene presentaciones relacionadas y debe conservarse.");
                Db.Remove(await Db.tblFormatoLibros.FindAsync(id) ?? throw new ReglaNegocioException("Formato inexistente.")); break;
        }
        await GuardarAsync();
    }
}
