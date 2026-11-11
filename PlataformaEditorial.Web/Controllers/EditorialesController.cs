using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Servicios;
namespace PlataformaEditorial.Web.Controllers;

public class EditorialesController(ReferenciasService servicio) : ReferenciasControllerBase(servicio, TipoReferencia.Editoriales);
