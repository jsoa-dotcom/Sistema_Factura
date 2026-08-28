namespace RegistroHexagonal.Application.DTOs
{
    public record CrearCategoriaDto(string Nombre);
    public record ActualizarCategoriaDto(string Nombre);
    public record CategoriaDto(Guid Id, string Nombre);
}
