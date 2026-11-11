using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblCotizacion
{
    public int IDCotizacion { get; set; }

    public DateTime FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public string EstadoCotizacion { get; set; } = null!;

    public decimal SubtotalBase { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuestos { get; set; }

    public decimal TotalFinal { get; set; }

    public string? Observaciones { get; set; }

    public int IDSolicitud { get; set; }

    public virtual tblSolicitudCotizacion IDSolicitudNavigation { get; set; } = null!;

    public virtual ICollection<tblDetalleCotizacion> tblDetalleCotizacions { get; set; } = new List<tblDetalleCotizacion>();
}
