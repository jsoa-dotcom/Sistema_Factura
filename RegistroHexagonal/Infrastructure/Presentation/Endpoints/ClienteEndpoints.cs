using RegistroHexagonal.Application.DTOs;
using RegistroHexagonal.Application.Ports;

namespace RegistroHexagonal.Presentation.Endpoints
{
    public static class ClienteEndpoints
    {
        public static void MapClienteEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/clientes").WithTags("Clientes");

            grupo.MapPost("/", (CrearClienteDto dto, IClienteService service) =>
            {
                var cliente = service.CrearCliente(dto.Nombre, dto.Documento, dto.Email, dto.Telefono);
                var resultado = new ClienteDto(cliente.Id, cliente.Nombre, cliente.Documento, cliente.Email, cliente.Telefono);
                var respuesta = new ApiResponse<ClienteDto>(true, "Cliente creado correctamente.", resultado);
                return Results.Created($"/clientes/{resultado.Id}", respuesta);
            });

            grupo.MapGet("/", (IClienteService service, int pagina = 1, int tamanoPagina = 10) =>
            {
                var (items, total) = service.ObtenerPaginado(pagina, tamanoPagina);
                var clientesDto = items.Select(c => new ClienteDto(c.Id, c.Nombre, c.Documento, c.Email, c.Telefono)).ToList();

                var totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina);
                var resultado = new PagedResultDto<ClienteDto>(clientesDto, pagina, tamanoPagina, total, totalPaginas);

                var mensaje = total == 0 ? "No hay clientes registrados." : $"Página {pagina} de {totalPaginas} ({total} cliente(s) en total).";
                return Results.Ok(new ApiResponse<PagedResultDto<ClienteDto>>(true, mensaje, resultado));
            });

            grupo.MapGet("/{id:guid}", (Guid id, IClienteService service) =>
            {
                var cliente = service.ObtenerPorId(id);
                var resultado = new ClienteDto(cliente.Id, cliente.Nombre, cliente.Documento, cliente.Email, cliente.Telefono);
                return Results.Ok(new ApiResponse<ClienteDto>(true, "Cliente encontrado.", resultado));
            });

            grupo.MapPut("/{id:guid}", (Guid id, ActualizarClienteDto dto, IClienteService service) =>
            {
                var cliente = service.ActualizarCliente(id, dto.Nombre, dto.Documento, dto.Email, dto.Telefono);
                var resultado = new ClienteDto(cliente.Id, cliente.Nombre, cliente.Documento, cliente.Email, cliente.Telefono);
                return Results.Ok(new ApiResponse<ClienteDto>(true, "Cliente actualizado correctamente.", resultado));
            });

            grupo.MapPatch("/{id:guid}/reactivar", (Guid id, IClienteService service) =>
            {
                service.ReactivarCliente(id);
                return Results.Ok(new ApiResponse(true, "Cliente reactivado correctamente."));
            });

            grupo.MapDelete("/{id:guid}", (Guid id, IClienteService service) =>
            {
                service.EliminarCliente(id);
                return Results.Ok(new ApiResponse(true, "Cliente eliminado correctamente."));
            });
        }
    }
}
