using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Servicios;
namespace PlataformaEditorial.Web.Controllers;

[Authorize(Roles = "Administrador")]
[AutoValidateAntiforgeryToken]
public abstract class ReferenciasControllerBase(ReferenciasService servicio, TipoReferencia tipo) : Controller
{
    public async Task<IActionResult> Index() => View("~/Views/Referencias/Index.cshtml", new ReferenciasPagina(tipo, await servicio.ListarAsync(tipo)));
    public IActionResult Crear() => View("~/Views/Referencias/Editar.cshtml", new ReferenciaFormulario(tipo, new()));
    [HttpPost]
    public async Task<IActionResult> Crear([Bind("Nombre,Activo,EsDigital")] ReferenciaEdicion datos) => await Guardar(datos);
    public async Task<IActionResult> Editar(int id)
    {
        var datos = await servicio.ObtenerAsync(tipo, id);
        return datos == null ? NotFound() : View("~/Views/Referencias/Editar.cshtml", new ReferenciaFormulario(tipo, datos));
    }
    [HttpPost]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Nombre,Activo,EsDigital")] ReferenciaEdicion datos)
    {
        if (id != datos.Id) return BadRequest();
        if (await servicio.ObtenerAsync(tipo, id) == null) return NotFound();
        return await Guardar(datos);
    }
    private async Task<IActionResult> Guardar(ReferenciaEdicion datos)
    {
        if (ModelState.IsValid)
        {
            try { await servicio.GuardarAsync(tipo, datos); TempData["Exito"] = "Registro guardado."; return RedirectToAction(nameof(Index)); }
            catch (ReglaNegocioException ex) { ModelState.AddModelError(ex.Campo, ex.Message); }
        }
        return View("~/Views/Referencias/Editar.cshtml", new ReferenciaFormulario(tipo, datos));
    }
    public async Task<IActionResult> Eliminar(int id)
    {
        var datos = await servicio.ObtenerAsync(tipo, id);
        return datos == null ? NotFound() : View("~/Views/Referencias/Eliminar.cshtml", new ReferenciaFormulario(tipo, datos));
    }
    [HttpPost, ActionName("Eliminar")]
    public async Task<IActionResult> ConfirmarEliminar(int id)
    {
        if (await servicio.ObtenerAsync(tipo, id) == null) return NotFound();
        try { await servicio.EliminarAsync(tipo, id); TempData["Exito"] = "Operación realizada."; return RedirectToAction(nameof(Index)); }
        catch (ReglaNegocioException ex) { ModelState.AddModelError("", ex.Message); return await Eliminar(id); }
    }
}
