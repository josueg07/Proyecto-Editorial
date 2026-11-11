using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblPerfilInstitucional
{
    public string IDUsuario { get; set; } = null!;

    public string NombreInstitucion { get; set; } = null!;

    public string NIT { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string EstadoValidacion { get; set; } = null!;

    public DateTime? FechaValidacion { get; set; }
}
