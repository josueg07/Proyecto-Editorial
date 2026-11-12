using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Web.Controllers.Institucional;
[Authorize, AutoValidateAntiforgeryToken, FiltroRf04]
public class PerfilInstitucionalController(PerfilInstitucionalService perfiles) : Controller
{
    public async Task<IActionResult> Index() => View(await perfiles.MiPerfilAsync(ActorRf04.Desde(User)));
    [Authorize(Roles="ClienteMinorista,ClienteInstitucional")]
    public async Task<IActionResult> Solicitar()
    {
        var perfil=await perfiles.MiPerfilAsync(ActorRf04.Desde(User));
        if(perfil!=null && perfil.Estado!=EstadosRf04.Rechazado) return RedirectToAction(nameof(Index));
        return View(perfil==null ? new PerfilInstitucionalEntrada() : new PerfilInstitucionalEntrada { NombreInstitucion=perfil.NombreInstitucion,NIT=perfil.NIT,Direccion=perfil.Direccion });
    }
    [HttpPost, Authorize(Roles="ClienteMinorista,ClienteInstitucional")]
    public async Task<IActionResult> Solicitar(PerfilInstitucionalEntrada datos)
    {
        if(ModelState.IsValid)
        {
            try { await perfiles.SolicitarAsync(ActorRf04.Desde(User),datos); TempData["Exito"]="Solicitud de perfil enviada al administrador."; return RedirectToAction(nameof(Index)); }
            catch(ReglaRf04Exception ex) { ModelState.AddModelError("",ex.Message); }
        }
        return View(datos);
    }
}
