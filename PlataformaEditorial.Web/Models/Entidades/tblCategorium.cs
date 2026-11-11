using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblCategorium
{
    public int IDCategoria { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<tblLibro> IDLibros { get; set; } = new List<tblLibro>();
}
