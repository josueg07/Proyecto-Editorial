namespace PlataformaEditorial.Web.Models.Servicios.Compras;

public record ResultadoPedido(
    decimal SubtotalBase,
    decimal DescuentoVolumen,
    decimal DescuentoInstitucional,
    decimal Impuestos,
    decimal TotalFinal
);

public static class CalculoPedido
{
    public static ResultadoPedido Calcular(
        decimal subtotal,
        int unidades,
        bool esInstitucional,
        decimal porcentajeIVA)
    {
        if (subtotal < 0 || unidades <= 0)
            throw new ArgumentException("Pedido inválido.");

        if (porcentajeIVA < 0 || porcentajeIVA > 100)
            throw new ArgumentException("IVA inválido.");

        decimal porcentajeVolumen = unidades switch
        {
            <= 5 => 0m,
            <= 20 => 5m,
            <= 50 => 10m,
            _ => 15m
        };

        decimal descuentoVolumen = Redondear(
            subtotal * porcentajeVolumen / 100m);

        decimal baseConDescuento = subtotal - descuentoVolumen;

        decimal descuentoInstitucional = esInstitucional
            ? Redondear(baseConDescuento * 0.08m)
            : 0m;

        decimal baseImponible =
            baseConDescuento - descuentoInstitucional;

        decimal impuestos = Redondear(
            baseImponible * porcentajeIVA / 100m);

        decimal total = baseImponible + impuestos;

        return new ResultadoPedido(
            subtotal,
            descuentoVolumen,
            descuentoInstitucional,
            impuestos,
            total
        );
    }

    private static decimal Redondear(decimal valor)
    {
        return Math.Round(
            valor, 2, MidpointRounding.AwayFromZero);
    }
}
