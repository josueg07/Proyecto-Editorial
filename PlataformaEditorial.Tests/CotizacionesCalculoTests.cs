using PlataformaEditorial.Web.Models.Institucional;
using PlataformaEditorial.Web.Models.Servicios.Institucional;
namespace PlataformaEditorial.Tests;

public class CotizacionesCalculoTests
{
    [Theory]
    [InlineData(10, 25.50, 12, 255, 30.60, 285.60)]
    [InlineData(3, 0.05, 10, 0.15, 0.02, 0.17)]
    [InlineData(5, 10, 0, 50, 0, 50)]
    [InlineData(2, 100, 100, 200, 200, 400)]
    public void Calcula_importes_y_redondeo_monetario(int cantidad, decimal precio, decimal tasa, decimal subtotal, decimal impuestos, decimal total)
    {
        var resultado = CalculoCotizacion.Calcular([new() { DetalleSolicitudId = 1, Descripcion = "Obra", Cantidad = cantidad, PrecioUnitario = precio }], tasa);
        Assert.Equal(subtotal, resultado.Subtotal); Assert.Equal(impuestos, resultado.Impuestos); Assert.Equal(total, resultado.Total);
    }
    [Theory]
    [InlineData(0, 1, 12)]
    [InlineData(-1, 1, 12)]
    [InlineData(1, -1, 12)]
    [InlineData(1, 1, -1)]
    [InlineData(1, 1, 101)]
    [InlineData(1, 0.001, 12)]
    public void Rechaza_cantidades_precios_y_tasas_invalidas(int cantidad, decimal precio, decimal tasa) =>
        Assert.Throws<ReglaRf04Exception>(() => CalculoCotizacion.Calcular([new() { DetalleSolicitudId = 1, Descripcion = "Obra", Cantidad = cantidad, PrecioUnitario = precio }], tasa));
    [Fact]
    public void Rechaza_desbordamiento_de_columnas_SQL_y_detalle_vacio()
    {
        Assert.Throws<ReglaRf04Exception>(() => CalculoCotizacion.Calcular([new() { DetalleSolicitudId = 1, Descripcion = "Obra", Cantidad = int.MaxValue, PrecioUnitario = 99999999.99m }], 12));
        Assert.Throws<ReglaRf04Exception>(() => CalculoCotizacion.Calcular([], 12));
    }

    [Theory]
    [InlineData(25.50, 279.50, 33.54, 313.04)]
    [InlineData(305, 0, 0, 0)]
    public void Aplica_descuento_antes_del_IVA(decimal descuento, decimal baseImponible, decimal iva, decimal total)
    {
        var t = CalculoCotizacion.Calcular([new() { DetalleSolicitudId = 1, Descripcion = "Obra A", Cantidad = 10, PrecioUnitario = 25.50m }, new() { DetalleSolicitudId = 2, Descripcion = "Obra B", Cantidad = 5, PrecioUnitario = 10 }], 12, descuento);
        Assert.Equal(baseImponible, t.Subtotal - descuento); Assert.Equal(iva, t.Impuestos); Assert.Equal(total, t.Total);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(0.001)]
    public void Rechaza_descuento_negativo_excesivo_o_con_demasiados_decimales(decimal descuento) =>
        Assert.Throws<ReglaRf04Exception>(() => CalculoCotizacion.Calcular([new() { DetalleSolicitudId = 1, Descripcion = "Obra", Cantidad = 1, PrecioUnitario = 100 }], 12, descuento));
}
