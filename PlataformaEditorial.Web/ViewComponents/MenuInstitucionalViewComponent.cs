using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PlataformaEditorial.Web.Data.Negocio;
namespace PlataformaEditorial.Web.ViewComponents;
public class MenuInstitucionalViewComponent(EditorialDbContext db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var id=HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? estado=id==null ? null : await db.tblPerfilInstitucionals.AsNoTracking().Where(p=>p.IDUsuario==id).Select(p=>p.EstadoValidacion).SingleOrDefaultAsync();
        return View("Default",estado??"");
    }
}
