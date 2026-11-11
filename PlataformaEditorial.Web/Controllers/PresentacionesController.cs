using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Servicios;
namespace PlataformaEditorial.Web.Controllers;

[Authorize(Roles = "Administrador")]
[AutoValidateAntiforgeryToken]
public class PresentacionesController(PresentacionesService servicio) : Controller
{
    public async Task<IActionResult> Index() => View(await servicio.ListarAsync());
    public async Task<IActionResult> Crear()
    {
        var m = new PresentacionEdicion(); await servicio.CompletarAsync(m); return View("Editar", m);
    }
    [HttpPost]
    public async Task<IActionResult> Crear([Bind("LibroId,FormatoId,Isbn,Precio,Existencias")] PresentacionEdicion m) => await Guardar(m);
    public async Task<IActionResult> Editar(int id)
    {
        var m = await servicio.ObtenerAsync(id); if (m == null) return NotFound();
        await servicio.CompletarAsync(m); return View(m);
    }
    [HttpPost]
    public async Task<IActionResult> Editar(int id, [Bind("Id,LibroId,FormatoId,Isbn,Precio,Existencias")] PresentacionEdicion m)
    {
        if (id != m.Id) return BadRequest();
        if (await servicio.ObtenerAsync(id) == null) return NotFound();
        return await Guardar(m);
    }
    private async Task<IActionResult> Guardar(PresentacionEdicion m)
    {
        if (ModelState.IsValid)
        {
            try { await servicio.GuardarAsync(m); TempData["Exito"] = "Registro guardado."; return RedirectToAction(nameof(Index)); }
            catch (ReglaNegocioException ex) { ModelState.AddModelError(ex.Campo, ex.Message); }
        }
        await servicio.CompletarAsync(m); return View("Editar", m);
    }
    public async Task<IActionResult> Eliminar(int id)
    {
        var m = await servicio.ObtenerAsync(id); return m == null ? NotFound() : View(m);
    }
    [HttpPost, ActionName("Eliminar")]
    public async Task<IActionResult> ConfirmarEliminar(int id)
    {
        if (await servicio.ObtenerAsync(id) == null) return NotFound();
        try { await servicio.EliminarAsync(id); TempData["Exito"] = "Operación realizada."; return RedirectToAction(nameof(Index)); }
        catch (ReglaNegocioException ex) { ModelState.AddModelError("", ex.Message); return await Eliminar(id); }
    }
}
