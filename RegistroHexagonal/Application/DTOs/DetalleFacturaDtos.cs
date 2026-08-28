namespace RegistroHexagonal.Application.DTOs
{
    public record LineaFacturaDto(Guid ProductoId, int Cantidad, decimal Descuento);
    public record DetalleFacturaDto(
    Guid Id,
    Guid ProductoId,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitarioBruto,
    decimal Iva,
    decimal Descuento,
    decimal BaseGravable,
    decimal ValorIva,
    decimal Subtotal);
}
