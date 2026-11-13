using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Compras;
using PlataformaEditorial.Web.Models.Servicios.Compras;
namespace PlataformaEditorial.Web.Controllers.Compras;
[Authorize(Roles="ClienteMinorista,ClienteInstitucional"),AutoValidateAntiforgeryToken,FiltroCompra]
public class CarritoController(CarritoService servicio) : Controller
{
    public async Task<IActionResult> Index() => View(await servicio.Ver(ActorCompra.Desde(User)));
    [HttpPost] public async Task<IActionResult> Agregar(CantidadCompra datos)
    {
        if(!ModelState.IsValid) throw new ReglaCompraException("Indica una presentación y una cantidad válidas.");
        await servicio.Agregar(ActorCompra.Desde(User),datos);TempData["Exito"]="Presentación agregada al carrito.";return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Cambiar(int id,int cantidad)
    { await servicio.Cambiar(ActorCompra.Desde(User),id,cantidad);return RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> Quitar(int id)
    { await servicio.Quitar(ActorCompra.Desde(User),id);return RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> Vaciar()
    { await servicio.Vaciar(ActorCompra.Desde(User));return RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> Confirmar(ConfirmarCompra datos)
    {
        if(ModelState.IsValid)
        {
            try { var id=await servicio.Confirmar(ActorCompra.Desde(User),datos);TempData["Exito"]="Compra confirmada y pedido registrado.";return RedirectToAction("Detalles","Pedidos",new{id}); }
            catch(ReglaCompraException ex) { ModelState.AddModelError("",ex.Message); }
        }
        return View("Index",(await servicio.Ver(ActorCompra.Desde(User))) with { Direccion=datos.Direccion });
    }
}
