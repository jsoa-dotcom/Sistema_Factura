namespace RegistroHexagonal.Application.DTOs
{
    public record CrearFacturaDto(string DocumentoCliente, decimal Descuento, List<LineaFacturaDto> Lineas);
    public record AnularFacturaDto(string Motivo);
    public record FacturaDto(
        Guid Id,
        string NumeroFactura,
        DateTime Fecha,
        Guid ClienteId,
        string NombreCliente,
        string DocumentoCliente,
        decimal Descuento,
        decimal TotalBaseGravable,
        decimal TotalIva,
        decimal Total,
        bool Anulada,
        string? Motivo,
        List<DetalleFacturaDto> Detalles);
}
