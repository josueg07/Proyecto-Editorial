using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblDetalleSolicitudCotizacion
{
    public int IDDetalleSolicitud { get; set; }

    public string TituloSolicitado { get; set; } = null!;

    public int CantidadEstimada { get; set; }

    public string AcabadoPortada { get; set; } = null!;

    public string TipoPapel { get; set; } = null!;

    public int IDSolicitud { get; set; }

    public int? IDLibro { get; set; }

    public virtual tblLibro? IDLibroNavigation { get; set; }

    public virtual tblSolicitudCotizacion IDSolicitudNavigation { get; set; } = null!;

    public virtual ICollection<tblDetalleCotizacion> tblDetalleCotizacions { get; set; } = new List<tblDetalleCotizacion>();
}
