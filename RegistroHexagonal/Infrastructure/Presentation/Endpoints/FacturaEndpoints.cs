using RegistroHexagonal.Application.DTOs;
using RegistroHexagonal.Application.Ports;

namespace RegistroHexagonal.Presentation.Endpoints
{
    public static class FacturaEndpoints
    {
        public static void MapFacturaEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/facturas").WithTags("Facturas");

            grupo.MapPost("/", (CrearFacturaDto dto, IFacturaService service) =>
            {
                var lineas = dto.Lineas
                    .Select(l => (l.ProductoId, l.Cantidad, l.Descuento))
                    .ToList();

                var factura = service.CrearFactura(dto.DocumentoCliente, dto.Descuento, lineas);

                var resultado = MapearAFacturaDto(factura);
                var respuesta = new ApiResponse<FacturaDto>(true, "Factura creada correctamente.", resultado);
                return Results.Created($"/facturas/{resultado.Id}", respuesta);
            });

            grupo.MapGet("/", (IFacturaService service, int pagina = 1, int tamanoPagina = 10) =>
            {
                var (items, total) = service.ObtenerPaginado(pagina, tamanoPagina);
                var facturasDto = items.Select(MapearAFacturaDto).ToList();

                var totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina);
                var resultado = new PagedResultDto<FacturaDto>(facturasDto, pagina, tamanoPagina, total, totalPaginas);

                var mensaje = total == 0 ? "No hay facturas registradas." : $"Página {pagina} de {totalPaginas} ({total} factura(s) en total).";
                return Results.Ok(new ApiResponse<PagedResultDto<FacturaDto>>(true, mensaje, resultado));
            });

            grupo.MapGet("/{id:guid}", (Guid id, IFacturaService service) =>
            {
                var factura = service.ObtenerPorId(id);
                return Results.Ok(new ApiResponse<FacturaDto>(true, "Factura encontrada.", MapearAFacturaDto(factura)));
            });

            grupo.MapPatch("/{id:guid}/anular", (Guid id, AnularFacturaDto dto, IFacturaService service) =>
            {
                service.AnularFactura(id, dto.Motivo);
                return Results.Ok(new ApiResponse(true, "Factura anulada correctamente."));
            });
        }

        private static readonly TimeZoneInfo ZonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

        private static DateTime ConvertirAHoraLocal(DateTime fechaUtc)
        {
            var utc = DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, ZonaHorariaColombia);
        }
        private static FacturaDto MapearAFacturaDto(Domain.Factura factura)
        {
            var detalles = factura.Detalles.Select(d => new DetalleFacturaDto(
                d.Id,
                d.ProductoId,
                d.NombreProducto,
                d.Cantidad,
                d.PrecioUnitarioBruto,
                d.Iva,
                d.Descuento,
                d.BaseGravable,
                d.ValorIva,
                d.Subtotal
            )).ToList();

            return new FacturaDto(
                factura.Id,
                factura.NumeroFactura,
                ConvertirAHoraLocal(factura.Fecha),
                factura.ClienteId,
                factura.NombreCliente,
                factura.DocumentoCliente,
                factura.Descuento,
                detalles.Sum(d => d.BaseGravable),
                detalles.Sum(d => d.ValorIva),
                factura.Total,
                factura.Anulada,
                factura.Motivo,
                detalles
            );
        }
    }
}
