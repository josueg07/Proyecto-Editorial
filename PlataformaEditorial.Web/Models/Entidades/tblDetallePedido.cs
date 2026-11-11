using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblDetallePedido
{
    public int IDDetallePedido { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public int IDPedido { get; set; }

    public int IDPresentacion { get; set; }

    public virtual tblPedido IDPedidoNavigation { get; set; } = null!;

    public virtual tblPresentacionLibro IDPresentacionNavigation { get; set; } = null!;
}
