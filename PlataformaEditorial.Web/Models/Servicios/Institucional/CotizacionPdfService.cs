using System.Globalization;
using PlataformaEditorial.Web.Models.Institucional;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;

public class CotizacionPdfService
{
    private static string Dinero(decimal valor) => "Q "+valor.ToString("N2",CultureInfo.GetCultureInfo("es-GT"));
    public byte[] Generar(DocumentoCotizacion modelo)
    {
        var s=modelo.Solicitud; var c=modelo.Cotizacion;
        return Document.Create(doc=>doc.Page(page=>
        {
            page.Size(PageSizes.A4); page.Margin(36); page.DefaultTextStyle(x=>x.FontSize(10));
            page.Header().PaddingBottom(16).Column(col=>
            {
                col.Item().Text("PLATAFORMA EDITORIAL").FontSize(19).Bold().FontColor(Colors.Blue.Darken3);
                col.Item().Text($"Cotización institucional COT-{c.Id:D6}").FontSize(14);
                col.Item().Text($"Solicitud SOL-{s.Id:D6} | Estado: {c.Estado}{(c.Vencida?" (vencida)":"")}");
                if(c.Estado==EstadosRf04.Borrador) col.Item().Text("BORRADOR - SIN VALIDEZ COMERCIAL").Bold().FontColor(Colors.Red.Darken2);
            });
            page.Content().Column(col=>
            {
                col.Spacing(10);
                col.Item().Background(Colors.Grey.Lighten4).Padding(12).Column(datos=>
                {
                    datos.Item().Text(s.Institucion.NombreInstitucion).Bold();
                    datos.Item().Text($"NIT: {s.Institucion.NIT}");
                    datos.Item().Text($"Dirección: {s.Institucion.Direccion}");
                    datos.Item().Text($"Asesor: {s.Asesor??"Sin asignar"}");
                });
                col.Item().Text($"Emisión: {c.FechaEmision:dd/MM/yyyy} | Válida hasta: {c.FechaVencimiento:dd/MM/yyyy}");
                col.Item().Text($"Fecha de entrega requerida: {s.FechaRequerida:dd/MM/yyyy}");
                col.Item().Table(tabla=>
                {
                    tabla.ColumnsDefinition(cols=>{cols.RelativeColumn();cols.ConstantColumn(70);cols.ConstantColumn(90);cols.ConstantColumn(100);});
                    tabla.Header(h=>
                    {
                        foreach(var texto in new[]{"Descripción / especificaciones","Cant.","Precio unitario","Importe"})
                            h.Cell().Background(Colors.Blue.Darken3).Padding(6).Text(texto).FontColor(Colors.White).Bold();
                    });
                    foreach(var linea in c.Detalles)
                    {
                        tabla.Cell().ShowEntire().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(det=>
                        {
                            det.Item().Text(linea.Descripcion);
                            det.Item().Text(linea.Especificaciones).FontSize(8).FontColor(Colors.Grey.Darken2);
                        });
                        tabla.Cell().ShowEntire().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(linea.Cantidad.ToString());
                        tabla.Cell().ShowEntire().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(Dinero(linea.PrecioUnitario));
                        tabla.Cell().ShowEntire().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(Dinero(linea.Importe));
                    }
                });
                col.Item().ShowEntire().AlignRight().Width(280).Column(totales=>
                {
                    totales.Spacing(5);
                    totales.Item().Text($"Subtotal: {Dinero(c.Subtotal)}").AlignRight();
                    totales.Item().Text($"Descuento: {Dinero(c.Descuento)}").AlignRight();
                    totales.Item().Text($"Base imponible: {Dinero(c.Subtotal-c.Descuento)}").AlignRight();
                    totales.Item().Text($"IVA: {Dinero(c.Impuestos)}").AlignRight();
                    totales.Item().BorderTop(1).PaddingTop(6).Text($"TOTAL: {Dinero(c.Total)}").FontSize(13).Bold().AlignRight();
                });
                if(!string.IsNullOrWhiteSpace(c.Observaciones)) col.Item().Text("Observaciones: "+c.Observaciones);
                col.Item().Text("Importes expresados en quetzales. Esta cotización no constituye factura ni pedido.").FontSize(9);
                if(c.Estado==EstadosRf04.Borrador) col.Item().Text("Los importes del borrador son provisionales; los impuestos se fijan al emitir.").FontSize(9);
            });
            page.Footer().PaddingTop(12).AlignCenter().Text(text=>
            {
                text.Span($"COT-{c.Id:D6} | Página ");text.CurrentPageNumber();text.Span(" de ");text.TotalPages();
            });
        })).GeneratePdf();
    }
}
