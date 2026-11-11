using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblLibro
{
    public int IDLibro { get; set; }

    public string Titulo { get; set; } = null!;

    public string Sinopsis { get; set; } = null!;

    public short AnioEdicion { get; set; }

    public string portadaURL { get; set; } = null!;

    public DateTime fechaRegistro { get; set; }

    public bool Destacado { get; set; }

    public bool Activo { get; set; }

    public int IDEditorial { get; set; }

    public virtual tblEditorial IDEditorialNavigation { get; set; } = null!;

    public virtual ICollection<tblDetalleSolicitudCotizacion> tblDetalleSolicitudCotizacions { get; set; } = new List<tblDetalleSolicitudCotizacion>();

    public virtual ICollection<tblPresentacionLibro> tblPresentacionLibros { get; set; } = new List<tblPresentacionLibro>();

    public virtual ICollection<tblAutor> IDAutors { get; set; } = new List<tblAutor>();

    public virtual ICollection<tblCategorium> IDCategoria { get; set; } = new List<tblCategorium>();
}
