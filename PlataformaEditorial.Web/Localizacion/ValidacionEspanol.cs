using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
namespace PlataformaEditorial.Web.Localizacion;
public class ValidacionEspanol : IValidationMetadataProvider, IDisplayMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach(var atributo in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if(atributo.ErrorMessageResourceType!=null) continue;
            if(atributo.ErrorMessage is {} mensaje && !mensaje.StartsWith("The ",StringComparison.Ordinal) && !mensaje.Contains("do not match",StringComparison.Ordinal)) continue;
            atributo.ErrorMessage=atributo switch
            {
                RequiredAttribute=>"El campo {0} es obligatorio.",EmailAddressAttribute=>"El correo electrónico no es válido.",
                StringLengthAttribute=>"El campo {0} debe tener entre {2} y {1} caracteres.",RangeAttribute=>"El campo {0} debe estar entre {1} y {2}.",
                CompareAttribute=>"Los campos {0} y {1} no coinciden.",MinLengthAttribute=>"El campo {0} debe contener al menos {1} elemento(s).",MaxLengthAttribute=>"El campo {0} admite como máximo {1} elemento(s).",
                PhoneAttribute=>"El teléfono no es válido.",RegularExpressionAttribute=>"El formato de {0} no es válido.",_=>null
            };
        }
    }
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if(context.Attributes.OfType<DisplayAttribute>().Any()) return;
        var nombre=context.Key.Name switch { "Email"=>"Correo electrónico","Password"=>"Contraseña","ConfirmPassword"=>"Confirmar contraseña","UserName"=>"Nombre de usuario","Username"=>"Nombre de usuario","PhoneNumber"=>"Teléfono","Code"=>"Código","RecoveryCode"=>"Código de recuperación",_=>null };
        if(nombre!=null) context.DisplayMetadata.DisplayName=()=>nombre;
    }
}
