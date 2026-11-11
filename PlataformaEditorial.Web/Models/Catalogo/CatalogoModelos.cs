using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlataformaEditorial.Web.Models.Entidades;
namespace PlataformaEditorial.Web.Models.Catalogo;

public class FiltroCatalogo : IValidatableObject
{
    [StringLength(200), Display(Name="Título, autor, ISBN o editorial")] public string? Texto { get; set; }
    [Display(Name="Categoría")] public int? CategoriaId { get; set; }
    [Display(Name="Formato")] public int? FormatoId { get; set; }
    [Range(typeof(decimal), "0", "99999999.99"), Display(Name="Precio mínimo")] public decimal? PrecioMinimo { get; set; }
    [Range(typeof(decimal), "0", "99999999.99"), Display(Name="Precio máximo")] public decimal? PrecioMaximo { get; set; }
    public int Pagina { get; set; } = 1;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (PrecioMinimo > PrecioMaximo) yield return new("El precio máximo debe ser mayor o igual al mínimo.", [nameof(PrecioMaximo)]);
    }
}
public record Opcion(int Id, string Nombre);
public record TarjetaLibro(int Id, string Titulo, string Portada, string Autores, string Editorial, decimal? Precio, string Formatos);
public class CatalogoPagina
{
    public FiltroCatalogo Filtro { get; set; } = new();
    public List<TarjetaLibro> Libros { get; set; } = [];
    public List<Opcion> Categorias { get; set; } = [];
    public List<Opcion> Formatos { get; set; } = [];
    public int Total { get; set; }
    public int Paginas => (int)Math.Ceiling(Total / 12m);
}
public record InicioCatalogo(List<TarjetaLibro> Destacados, List<TarjetaLibro> Novedades);
public class LibroEdicion
{
    public int Id { get; set; }
    [Required(ErrorMessage="Escriba el título."), StringLength(200), Display(Name="Título")] public string Titulo { get; set; } = "";
    [Required, Display(Name="Sinopsis")] public string Sinopsis { get; set; } = "";
    [Range(1, 32767), Display(Name="Año de edición")] public short AnioEdicion { get; set; } = (short)DateTime.Today.Year;
    [Required, StringLength(500), Display(Name="URL de portada"), PortadaSegura] public string PortadaUrl { get; set; } = "/images/sin-portada.svg";
    [Range(1,int.MaxValue), Display(Name="Editorial")] public int EditorialId { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; } = true;
    [Display(Name="Autores")] public int[] AutoresIds { get; set; } = [];
    [Display(Name="Categorías")] public int[] CategoriasIds { get; set; } = [];
    public List<SelectListItem> Editoriales { get; set; } = [];
    public List<SelectListItem> Autores { get; set; } = [];
    public List<SelectListItem> Categorias { get; set; } = [];
}
public class PortadaSeguraAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string url) return true; // Required valida los valores vacíos.
        if (url.StartsWith("/images/", StringComparison.Ordinal))
            return !url.Contains("..") && !url.Contains('\\');
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "https";
    }
    public override string FormatErrorMessage(string name) => "Use una URL https o una ruta local dentro de /images/.";
}
public class PresentacionEdicion
{
    public int Id { get; set; }
    [Range(1,int.MaxValue), Display(Name="Libro")] public int LibroId { get; set; }
    [Range(1,int.MaxValue), Display(Name="Formato")] public int FormatoId { get; set; }
    [Required, StringLength(20), RegularExpression(@"[0-9Xx\- ]{10,20}", ErrorMessage="Use de 10 a 20 caracteres: dígitos, X, espacios o guiones."), Display(Name="ISBN")] public string Isbn { get; set; } = "";
    [Range(typeof(decimal),"0","99999999.99"), Display(Name="Precio base")] public decimal Precio { get; set; }
    [Range(0,int.MaxValue), Display(Name="Existencias (vacío: sin dato)")] public int? Existencias { get; set; }
    public List<SelectListItem> Libros { get; set; } = [];
    public List<SelectListItem> Formatos { get; set; } = [];
}
public enum TipoReferencia { Autores, Categorias, Editoriales, Formatos }
public class ReferenciaEdicion
{
    public int Id { get; set; }
    [Required, StringLength(150), Display(Name="Nombre")] public string Nombre { get; set; } = "";
    public bool Activo { get; set; } = true;
    [Display(Name="Es digital")] public bool EsDigital { get; set; }
}
public record ReferenciaFila(int Id, string Nombre, bool? Activo, bool? EsDigital);
public record ReferenciasPagina(TipoReferencia Tipo, List<ReferenciaFila> Filas);
public record ReferenciaFormulario(TipoReferencia Tipo, ReferenciaEdicion Datos);
