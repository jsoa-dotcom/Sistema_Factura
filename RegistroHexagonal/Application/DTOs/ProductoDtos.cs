namespace RegistroHexagonal.Application.DTOs
{
    public record CrearProductoDto(string Nombre, string Descripcion, decimal PrecioBruto, decimal Iva, Guid CategoriaId);
    public record ActualizarProductoDto(string Nombre, string Descripcion, decimal PrecioBruto, decimal Iva, Guid CategoriaId);
    public record ProductoDto(Guid Id, string Nombre, string Descripcion, decimal PrecioBruto, decimal Iva, Guid CategoriaId, string NombreCategoria);
}
