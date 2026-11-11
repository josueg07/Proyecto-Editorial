using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PlataformaEditorial.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministracionController : Controller
    {
        public IActionResult Index()
        {
            return Content("Bienvenido al panel de administración.");
        }
    }
}
