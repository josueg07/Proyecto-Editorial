using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblSolicitudCotizacion
{
    public int IDSolicitud { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateOnly FechaEntregaRequerida { get; set; }

    public string EstadoSolicitud { get; set; } = null!;

    public string? Observaciones { get; set; }

    public string IDUsuario { get; set; } = null!;

    public string? IDAsesor { get; set; }

    public virtual tblCotizacion? tblCotizacion { get; set; }

    public virtual ICollection<tblDetalleSolicitudCotizacion> tblDetalleSolicitudCotizacions { get; set; } = new List<tblDetalleSolicitudCotizacion>();
}
