using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblCarrito
{
    public int IDCarrito { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string IDUsuario { get; set; } = null!;

    public virtual ICollection<tblDetalleCarrito> tblDetalleCarritos { get; set; } = new List<tblDetalleCarrito>();
}
