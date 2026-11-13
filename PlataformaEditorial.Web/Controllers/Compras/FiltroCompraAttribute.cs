using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaEditorial.Web.Models.Compras;
namespace PlataformaEditorial.Web.Controllers.Compras;
public class FiltroCompraAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        context.Result=context.Exception switch
        {
            AccesoCompraException=>new ForbidResult(),CompraNoEncontradaException=>new NotFoundResult(),
            ReglaCompraException ex=>new ViewResult { ViewName="~/Views/Compras/Error.cshtml",StatusCode=400,ViewData=new ViewDataDictionary<string>(new EmptyModelMetadataProvider(),context.ModelState){Model=ex.Message} },_=>null
        };
        context.ExceptionHandled=context.Result!=null;
    }
}
