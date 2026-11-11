using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblAutor
{
    public int IDAutor { get; set; }

    public string NombreAutor { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<tblLibro> IDLibros { get; set; } = new List<tblLibro>();
}
