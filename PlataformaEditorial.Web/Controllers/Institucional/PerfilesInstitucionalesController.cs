using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Web.Controllers.Institucional;
[Authorize(Roles="Administrador"),AutoValidateAntiforgeryToken,FiltroRf04]
public class PerfilesInstitucionalesController(PerfilInstitucionalService perfiles) : Controller
{
    public async Task<IActionResult> Index() => View(await perfiles.PendientesAsync(ActorRf04.Desde(User)));
    [HttpPost]
    public async Task<IActionResult> Aprobar(string usuarioId) => await Resolver(usuarioId,true);
    [HttpPost]
    public async Task<IActionResult> Rechazar(string usuarioId) => await Resolver(usuarioId,false);
    private async Task<IActionResult> Resolver(string usuarioId,bool aprobar)
    {
        if(string.IsNullOrWhiteSpace(usuarioId)) return BadRequest();
        await perfiles.ResolverAsync(ActorRf04.Desde(User),usuarioId,aprobar);
        TempData["Exito"]=aprobar ? "Perfil aprobado y rol institucional asignado. El cliente debe cerrar sesión y volver a entrar." : "Perfil rechazado; el cliente puede corregirlo y reenviarlo.";
        return RedirectToAction(nameof(Index));
    }
}
