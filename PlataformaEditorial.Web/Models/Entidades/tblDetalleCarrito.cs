using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblDetalleCarrito
{
    public int IDDetalleCarrito { get; set; }

    public int Cantidad { get; set; }

    public int IDPresentacion { get; set; }

    public int IDCarrito { get; set; }

    public virtual tblCarrito IDCarritoNavigation { get; set; } = null!;

    public virtual tblPresentacionLibro IDPresentacionNavigation { get; set; } = null!;
}
