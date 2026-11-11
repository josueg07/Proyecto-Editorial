using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblPresentacionLibro
{
    public int IDPresentacion { get; set; }

    public string isbn { get; set; } = null!;

    public decimal PrecioBase { get; set; }

    public int? Existencias { get; set; }

    public int IDLibro { get; set; }

    public int IDFormato { get; set; }

    public virtual tblFormatoLibro IDFormatoNavigation { get; set; } = null!;

    public virtual tblLibro IDLibroNavigation { get; set; } = null!;

    public virtual ICollection<tblDetalleCarrito> tblDetalleCarritos { get; set; } = new List<tblDetalleCarrito>();

    public virtual ICollection<tblDetallePedido> tblDetallePedidos { get; set; } = new List<tblDetallePedido>();
}
