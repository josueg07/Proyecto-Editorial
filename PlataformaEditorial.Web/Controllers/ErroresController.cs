using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace PlataformaEditorial.Web.Controllers;
[AllowAnonymous]
public class ErroresController : Controller
{
    public IActionResult Estado(int id)
    {
        Response.StatusCode=id is >=400 and <=599 ? id:404;
        return View(Response.StatusCode);
    }
}
