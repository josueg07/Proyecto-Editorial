using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblFormatoLibro
{
    public int IDFormato { get; set; }

    public string NombreFormato { get; set; } = null!;

    public bool EsDigital { get; set; }

    public virtual ICollection<tblPresentacionLibro> tblPresentacionLibros { get; set; } = new List<tblPresentacionLibro>();
}
