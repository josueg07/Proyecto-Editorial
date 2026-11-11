using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblDetalleCotizacion
{
    public int IDDetalleCotizacion { get; set; }

    public string Descripcion { get; set; } = null!;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public int IDCotizacion { get; set; }

    public int? IDDetalleSolicitud { get; set; }

    public virtual tblCotizacion IDCotizacionNavigation { get; set; } = null!;

    public virtual tblDetalleSolicitudCotizacion? IDDetalleSolicitudNavigation { get; set; }
}
