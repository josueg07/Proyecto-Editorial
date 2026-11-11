using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlataformaEditorial.Web.Models;

namespace PlataformaEditorial.Web.Controllers;

public class HomeController(PlataformaEditorial.Web.Models.Servicios.CatalogoService catalogo) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await catalogo.InicioAsync());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
