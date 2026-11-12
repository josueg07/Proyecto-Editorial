using PlataformaEditorial.Web.Models.Institucional;
namespace PlataformaEditorial.Web.Models.Servicios.Institucional;
public record TotalesCotizacion(decimal Subtotal,decimal Impuestos,decimal Total);
public static class CalculoCotizacion
{
    public static TotalesCotizacion Calcular(IReadOnlyCollection<LineaCotizacionEntrada> lineas,decimal porcentaje,decimal descuento=0)
    {
        if(lineas.Count is <1 or >20 || porcentaje is <0 or >100) throw new ReglaRf04Exception("Detalle o porcentaje de impuesto inválido.");
        decimal subtotal=0;
        foreach(var linea in lineas)
        {
            ValidacionRf04.Comprobar(linea);
            if(decimal.Round(linea.PrecioUnitario,2)!=linea.PrecioUnitario)
                throw new ReglaRf04Exception("El precio unitario admite hasta dos decimales.");
            subtotal+=linea.Cantidad*linea.PrecioUnitario;
        }
        if(descuento<0 || descuento>subtotal || decimal.Round(descuento,2)!=descuento)
            throw new ReglaRf04Exception("El descuento debe tener hasta dos decimales y estar entre cero y el subtotal.");
        var baseImponible=subtotal-descuento;
        var impuestos=decimal.Round(baseImponible*porcentaje/100m,2,MidpointRounding.AwayFromZero);
        var total=baseImponible+impuestos;
        if(subtotal>9999999999.99m || total>9999999999.99m) throw new ReglaRf04Exception("El total excede la capacidad de las columnas monetarias de SQL Server.");
        return new(subtotal,impuestos,total);
    }
}
