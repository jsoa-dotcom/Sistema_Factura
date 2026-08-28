using RegistroHexagonal.Application.DTOs;
using RegistroHexagonal.Application.Ports;

namespace RegistroHexagonal.Presentation.Endpoints
{
    public static class CategoriaEndpoints
    {
        public static void MapCategoriaEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/categorias").WithTags("Categorías");

            grupo.MapPost("/", (CrearCategoriaDto dto, ICategoriaService service) =>
            {
                var categoria = service.CrearCategoria(dto.Nombre);
                var resultado = new CategoriaDto(categoria.Id, categoria.Nombre);
                var respuesta = new ApiResponse<CategoriaDto>(true, "Categoría creada correctamente.", resultado);
                return Results.Created($"/categorias/{resultado.Id}", respuesta);
            });

            grupo.MapGet("/", (ICategoriaService service, int pagina = 1, int tamanoPagina = 10) =>
            {
                var (items, total) = service.ObtenerPaginado(pagina, tamanoPagina);
                var categoriasDto = items.Select(c => new CategoriaDto(c.Id, c.Nombre)).ToList();

                var totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina);
                var resultado = new PagedResultDto<CategoriaDto>(categoriasDto, pagina, tamanoPagina, total, totalPaginas);

                var mensaje = total == 0 ? "No hay categorías registradas." : $"Página {pagina} de {totalPaginas} ({total} categoría(s) en total).";
                return Results.Ok(new ApiResponse<PagedResultDto<CategoriaDto>>(true, mensaje, resultado));
            });

            grupo.MapGet("/{id:guid}", (Guid id, ICategoriaService service) =>
            {
                var categoria = service.ObtenerPorId(id);
                var resultado = new CategoriaDto(categoria.Id, categoria.Nombre);
                return Results.Ok(new ApiResponse<CategoriaDto>(true, "Categoría encontrada.", resultado));
            });

            grupo.MapPut("/{id:guid}", (Guid id, ActualizarCategoriaDto dto, ICategoriaService service) =>
            {
                var categoria = service.ActualizarCategoria(id, dto.Nombre);
                var resultado = new CategoriaDto(categoria.Id, categoria.Nombre);
                return Results.Ok(new ApiResponse<CategoriaDto>(true, "Categoría actualizada correctamente.", resultado));
            });

            grupo.MapPatch("/{id:guid}/reactivar", (Guid id, ICategoriaService service) =>
            {
                service.ReactivarCategoria(id);
                return Results.Ok(new ApiResponse(true, "Categoría reactivada correctamente."));
            });

            grupo.MapDelete("/{id:guid}", (Guid id, ICategoriaService service) =>
            {
                service.EliminarCategoria(id);
                return Results.Ok(new ApiResponse(true, "Categoría eliminada correctamente."));
            });
        }
    }
}
