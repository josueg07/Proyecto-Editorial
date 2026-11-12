using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Web.Controllers.Institucional;
[Authorize(Roles="ClienteInstitucional,AsesorVentas,Administrador"),AutoValidateAntiforgeryToken,FiltroRf04]
public class SolicitudesInstitucionalesController(SolicitudesInstitucionalesService solicitudes,TiempoRf04 tiempo) : Controller
{
    public async Task<IActionResult> Index() => View(await solicitudes.BandejaAsync(ActorRf04.Desde(User)));
    public async Task<IActionResult> Detalles(int id) => View(await solicitudes.DetalleAsync(ActorRf04.Desde(User),id));
    [Authorize(Roles="ClienteInstitucional")]
    public async Task<IActionResult> Nueva()
    {
        await solicitudes.ExigirInstitucionAsync(ActorRf04.Desde(User));
        return View(new SolicitudInstitucionalEntrada { FechaRequerida=tiempo.Hoy.AddDays(15) });
    }
    [HttpPost,Authorize(Roles="ClienteInstitucional")]
    public async Task<IActionResult> Nueva(SolicitudInstitucionalEntrada datos)
    {
        if(ModelState.IsValid)
        {
            try
            {
                var id=await solicitudes.CrearAsync(ActorRf04.Desde(User),datos);
                TempData["Exito"]="Solicitud institucional registrada."; return RedirectToAction(nameof(Detalles),new{id});
            }
            catch(ReglaRf04Exception ex) { ModelState.AddModelError("",ex.Message); }
        }
        return View(datos);
    }
    [Authorize(Roles="Administrador")]
    public async Task<IActionResult> Asignar(int id) => View(await solicitudes.AsignacionAsync(ActorRf04.Desde(User),id));
    [HttpPost,Authorize(Roles="Administrador")]
    public async Task<IActionResult> Asignar(int id,[Bind(Prefix="Datos")] AsignacionEntrada datos)
    {
        if(ModelState.IsValid)
        {
            try { await solicitudes.AsignarAsync(ActorRf04.Desde(User),id,datos); TempData["Exito"]="Asesor asignado."; return RedirectToAction(nameof(Detalles),new{id}); }
            catch(ReglaRf04Exception ex) { ModelState.AddModelError("",ex.Message); }
        }
        var modelo=await solicitudes.AsignacionAsync(ActorRf04.Desde(User),id); modelo.Datos=datos; return View(modelo);
    }
}
