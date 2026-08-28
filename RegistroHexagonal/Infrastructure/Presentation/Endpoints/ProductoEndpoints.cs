using RegistroHexagonal.Application.DTOs;
using RegistroHexagonal.Application.Ports;

namespace RegistroHexagonal.Presentation.Endpoints
{
    public static class ProductoEndpoints
    {
        public static void MapProductoEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/productos").WithTags("Productos");

            grupo.MapPost("/", (CrearProductoDto dto, IProductoService service) =>
            {
                var producto = service.CrearProducto(dto.Nombre, dto.Descripcion, dto.PrecioBruto, dto.Iva, dto.CategoriaId);
                var resultado = new ProductoDto(producto.Id, producto.Nombre, producto.Descripcion, producto.PrecioBruto, producto.Iva, producto.CategoriaId, producto.Categoria?.Nombre ?? string.Empty);
                var respuesta = new ApiResponse<ProductoDto>(true, "Producto creado correctamente.", resultado);
                return Results.Created($"/productos/{resultado.Id}", respuesta);
            });

            grupo.MapGet("/", (IProductoService service, int pagina = 1, int tamanoPagina = 10) =>
            {
                var (items, total) = service.ObtenerPaginado(pagina, tamanoPagina);
                var productosDto = items.Select(p => new ProductoDto(p.Id, p.Nombre, p.Descripcion, p.PrecioBruto, p.Iva, p.CategoriaId, p.Categoria?.Nombre ?? string.Empty)).ToList();

                var totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina);
                var resultado = new PagedResultDto<ProductoDto>(productosDto, pagina, tamanoPagina, total, totalPaginas);

                var mensaje = total == 0 ? "No hay productos registrados." : $"Página {pagina} de {totalPaginas} ({total} producto(s) en total).";
                return Results.Ok(new ApiResponse<PagedResultDto<ProductoDto>>(true, mensaje, resultado));
            });

            grupo.MapGet("/{id:guid}", (Guid id, IProductoService service) =>
            {
                var producto = service.ObtenerPorId(id);
                var resultado = new ProductoDto(producto.Id, producto.Nombre, producto.Descripcion, producto.PrecioBruto, producto.Iva, producto.CategoriaId, producto.Categoria?.Nombre ?? string.Empty);
                return Results.Ok(new ApiResponse<ProductoDto>(true, "Producto encontrado.", resultado));
            });

            grupo.MapPut("/{id:guid}", (Guid id, ActualizarProductoDto dto, IProductoService service) =>
            {
                var producto = service.ActualizarProducto(id, dto.Nombre, dto.Descripcion, dto.PrecioBruto, dto.Iva, dto.CategoriaId);
                var resultado = new ProductoDto(producto.Id, producto.Nombre, producto.Descripcion, producto.PrecioBruto, producto.Iva, producto.CategoriaId, producto.Categoria?.Nombre ?? string.Empty);
                return Results.Ok(new ApiResponse<ProductoDto>(true, "Producto actualizado correctamente.", resultado));
            });

            grupo.MapPatch("/{id:guid}/reactivar", (Guid id, IProductoService service) =>
            {
                service.ReactivarProducto(id);
                return Results.Ok(new ApiResponse(true, "Producto reactivado correctamente."));
            });

            grupo.MapDelete("/{id:guid}", (Guid id, IProductoService service) =>
            {
                service.EliminarProducto(id);
                return Results.Ok(new ApiResponse(true, "Producto eliminado correctamente."));
            });
        }
    }
}
