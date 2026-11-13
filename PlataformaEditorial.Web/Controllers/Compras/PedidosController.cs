using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Compras;
using PlataformaEditorial.Web.Models.Servicios.Compras;
namespace PlataformaEditorial.Web.Controllers.Compras;
[Authorize(Roles="ClienteMinorista,ClienteInstitucional,Administrador"),AutoValidateAntiforgeryToken,FiltroCompra]
public class PedidosController(PedidosService servicio) : Controller
{
    public async Task<IActionResult> Index() => View(await servicio.Listar(ActorCompra.Desde(User)));
    public async Task<IActionResult> Detalles(int id) => View(await servicio.Detalle(ActorCompra.Desde(User),id));
    [HttpPost,Authorize(Roles="Administrador")]
    public async Task<IActionResult> Estado(int id,string estado)
    { await servicio.CambiarEstado(ActorCompra.Desde(User),id,estado);TempData["Exito"]="Estado del pedido actualizado.";return RedirectToAction(nameof(Detalles),new{id}); }
}
