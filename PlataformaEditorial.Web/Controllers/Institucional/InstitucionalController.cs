using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace PlataformaEditorial.Web.Controllers.Institucional;
[Authorize]
public class InstitucionalController : Controller
{
    public IActionResult Index() => View();
}
