using System;
using System.Collections.Generic;

namespace PlataformaEditorial.Web.Models.Entidades;

public partial class tblBanner
{
    public int IDBanner { get; set; }

    public string Titulo { get; set; } = null!;

    public string ImagenURL { get; set; } = null!;

    public string? EnlaceURL { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public int OrdenVisualizacion { get; set; }
}
