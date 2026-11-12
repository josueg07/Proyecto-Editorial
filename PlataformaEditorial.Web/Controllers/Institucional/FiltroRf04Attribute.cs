using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlataformaEditorial.Web.Models.Institucional;
namespace PlataformaEditorial.Web.Controllers.Institucional;
public class FiltroRf04Attribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        context.Result=context.Exception switch
        {
            AccesoRf04Exception => new ForbidResult(),
            NoEncontradoRf04Exception => new NotFoundResult(),
            ReglaRf04Exception ex => new ViewResult { ViewName="~/Views/Institucional/Error.cshtml",StatusCode=400,
                ViewData=new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<string>(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),context.ModelState) { Model=ex.Message } },
            _=>null
        };
        if(context.Result!=null) context.ExceptionHandled=true;
    }
}
