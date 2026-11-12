using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Web.Controllers.Institucional;
[Authorize(Roles="ClienteInstitucional,AsesorVentas,Administrador"),AutoValidateAntiforgeryToken,FiltroRf04]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class CotizacionesInstitucionalesController(CotizacionesInstitucionalesService cotizaciones,CotizacionPdfService pdf) : Controller
{
    [Authorize(Roles="AsesorVentas,Administrador")]
    public async Task<IActionResult> Editar(int id) => View(await cotizaciones.EdicionAsync(ActorRf04.Desde(User),id));
    [HttpPost,Authorize(Roles="AsesorVentas,Administrador")]
    public async Task<IActionResult> Editar(int id,[Bind(Prefix="Datos")] CotizacionInstitucionalEntrada datos,string operacion)
    {
        if(operacion is not ("borrador" or "emitir")) return BadRequest();
        if(ModelState.IsValid)
        {
            try
            {
                await cotizaciones.GuardarAsync(ActorRf04.Desde(User),id,datos,operacion=="emitir");
                TempData["Exito"]=operacion=="emitir" ? "Cotización emitida. Los importes quedaron fijados y el cliente puede responder." : "Borrador guardado.";
                return RedirectToAction("Detalles","SolicitudesInstitucionales",new{id});
            }
            catch(ReglaRf04Exception ex) { ModelState.AddModelError("",ex.Message); }
        }
        var modelo=await cotizaciones.EdicionAsync(ActorRf04.Desde(User),id); modelo.Datos=datos; return View(modelo);
    }
    [HttpPost,Authorize(Roles="ClienteInstitucional")]
    public async Task<IActionResult> Aprobar(int id) => await Responder(id,true);
    [HttpPost,Authorize(Roles="ClienteInstitucional")]
    public async Task<IActionResult> Rechazar(int id) => await Responder(id,false);
    private async Task<IActionResult> Responder(int id,bool aprobar)
    {
        await cotizaciones.ResponderAsync(ActorRf04.Desde(User),id,aprobar);
        TempData["Exito"]=aprobar ? "Cotización aprobada." : "Cotización rechazada.";
        return RedirectToAction("Detalles","SolicitudesInstitucionales",new{id});
    }
    public async Task<IActionResult> Documento(int id) => View(await cotizaciones.DocumentoAsync(ActorRf04.Desde(User),id));
    public async Task<IActionResult> Pdf(int id)
    {
        var documento=await cotizaciones.DocumentoAsync(ActorRf04.Desde(User),id);
        return File(pdf.Generar(documento),"application/pdf",$"Cotizacion-{documento.Cotizacion.Id:D6}.pdf");
    }
}
