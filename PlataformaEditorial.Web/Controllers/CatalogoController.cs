using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Servicios;
namespace PlataformaEditorial.Web.Controllers;

[AllowAnonymous]
public class CatalogoController(CatalogoService servicio) : Controller
{
    public async Task<IActionResult> Index([FromQuery] FiltroCatalogo filtro) => View(await servicio.BuscarAsync(filtro, ModelState.IsValid));
    public async Task<IActionResult> Detalles(int id)
    {
        var libro = await servicio.DetalleAsync(id);
        return libro == null ? NotFound() : View(libro);
    }
}
