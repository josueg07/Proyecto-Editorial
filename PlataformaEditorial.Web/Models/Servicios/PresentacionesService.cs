using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlataformaEditorial.Web.Data.Negocio;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Entidades;
namespace PlataformaEditorial.Web.Models.Servicios;
public class PresentacionesService(EditorialDbContext db) : ServicioEscritura(db)
{
    public Task<List<tblPresentacionLibro>> ListarAsync() => Db.tblPresentacionLibros.AsNoTracking().Include(p=>p.IDLibroNavigation).Include(p=>p.IDFormatoNavigation).OrderBy(p=>p.IDLibroNavigation.Titulo).ToListAsync();
    public async Task<PresentacionEdicion?> ObtenerAsync(int id)
    {
        var p=await Db.tblPresentacionLibros.AsNoTracking().SingleOrDefaultAsync(p=>p.IDPresentacion==id);
        return p==null ? null : new() { Id=p.IDPresentacion,Isbn=p.isbn,Precio=p.PrecioBase,Existencias=p.Existencias,LibroId=p.IDLibro,FormatoId=p.IDFormato };
    }
    public async Task CompletarAsync(PresentacionEdicion m)
    {
        m.Libros=await Db.tblLibros.AsNoTracking().OrderBy(x=>x.Titulo).Select(x=>new SelectListItem(x.Titulo+(x.Activo?"":" (inactivo)"),x.IDLibro.ToString())).ToListAsync();
        m.Formatos=await Db.tblFormatoLibros.AsNoTracking().OrderBy(x=>x.NombreFormato).Select(x=>new SelectListItem(x.NombreFormato,x.IDFormato.ToString())).ToListAsync();
    }
    public async Task GuardarAsync(PresentacionEdicion m)
    {
        if(!await Db.tblLibros.AnyAsync(l=>l.IDLibro==m.LibroId)) throw new ReglaNegocioException("Seleccione un libro válido.",nameof(m.LibroId));
        if(!await Db.tblFormatoLibros.AnyAsync(f=>f.IDFormato==m.FormatoId)) throw new ReglaNegocioException("Seleccione un formato válido.",nameof(m.FormatoId));
        if(decimal.Round(m.Precio,2)!=m.Precio) throw new ReglaNegocioException("El precio admite como máximo dos decimales.",nameof(m.Precio));
        var p=m.Id==0 ? new tblPresentacionLibro() : await Db.tblPresentacionLibros.FindAsync(m.Id) ?? throw new ReglaNegocioException("La presentación ya no existe.");
        p.isbn=m.Isbn.Trim(); p.PrecioBase=m.Precio; p.Existencias=m.Existencias; p.IDLibro=m.LibroId; p.IDFormato=m.FormatoId;
        if(m.Id==0) Db.Add(p); await GuardarAsync();
    }
    public async Task EliminarAsync(int id)
    {
        if(await Db.tblDetalleCarritos.AnyAsync(x=>x.IDPresentacion==id) || await Db.tblDetallePedidos.AnyAsync(x=>x.IDPresentacion==id)) throw new ReglaNegocioException("La presentación tiene referencias en carritos o pedidos y debe conservarse.");
        Db.Remove(await Db.tblPresentacionLibros.FindAsync(id) ?? throw new ReglaNegocioException("Presentación inexistente."));
        await GuardarAsync();
    }
}
