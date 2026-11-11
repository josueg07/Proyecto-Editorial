using PlataformaEditorial.Web.Models.Catalogo;
using PlataformaEditorial.Web.Models.Servicios;
namespace PlataformaEditorial.Web.Controllers;

public class CategoriasController(ReferenciasService servicio) : ReferenciasControllerBase(servicio, TipoReferencia.Categorias);
