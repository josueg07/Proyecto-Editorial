using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Tests;

public class CotizacionPdfTests
{
    [Fact]
    public void Genera_documento_multipagina_con_textos_largos_y_importes()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var lineas = Enumerable.Range(1, 20).Select(i => new LineaCotizacionVista($"Obra de demostración {i}: " + new string('A', 180), "Portada: acabado mate con solapas. Papel: bond de uso académico. Edición institucional de demostración.", 10, 25.50m)).ToList();
        var perfil = new PerfilInstitucionalVista("muestra", "Instituto académico de demostración — documento ficticio", "1234567-8", "Dirección ficticia de prueba, Guatemala", EstadosRf04.Aprobado, DateTime.Today);
        var cotizacion = new CotizacionVista(1, 1, DateTime.Today, DateOnly.FromDateTime(DateTime.Today).AddDays(15), EstadosRf04.Emitida, 5100, 100, 600, 5600,
            "MUESTRA DE VERIFICACIÓN. Datos ficticios; no representa una propuesta comercial real.", lineas);
        var solicitud = new SolicitudDetalleVista(1, DateTime.Today, DateOnly.FromDateTime(DateTime.Today).AddDays(30), EstadosRf04.Cotizada, null, perfil, "Asesor de demostración", [], cotizacion);
        var pdf = new CotizacionPdfService().Generar(new(solicitud, cotizacion));
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(pdf)); Assert.True(pdf.Length > 5000);
        var carpeta = Path.Combine(Path.GetTempPath(), "PlataformaEditorial-RF04"); Directory.CreateDirectory(carpeta);
        File.WriteAllBytes(Path.Combine(carpeta, "cotizacion-muestra.pdf"), pdf);
    }
}
