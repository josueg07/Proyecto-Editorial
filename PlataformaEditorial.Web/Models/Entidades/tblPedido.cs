using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblPedido
{
    public int IDPedido { get; set; }

    public DateTime FechaPedido { get; set; }

    public string EstadoPedido { get; set; } = null!;

    public string? DireccionEntrega { get; set; }

    public decimal SubtotalBase { get; set; }

    public decimal DescuentoVolumen { get; set; }

    public decimal DescuentoInstitucional { get; set; }

    public decimal PorcentajeImpuesto { get; set; }

    public decimal Impuestos { get; set; }

    public decimal TotalFinal { get; set; }

    public string IDUsuario { get; set; } = null!;

    public virtual ICollection<tblDetallePedido> tblDetallePedidos { get; set; } = new List<tblDetallePedido>();
}
