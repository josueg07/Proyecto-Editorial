using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblEditorial
{
    public int IDEditorial { get; set; }

    public string NombreEditorial { get; set; } = null!;

    public virtual ICollection<tblLibro> tblLibros { get; set; } = new List<tblLibro>();
}
