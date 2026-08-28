namespace RegistroHexagonal.Application.DTOs
{
    public record CrearClienteDto(string Nombre, string Documento, string Email, string Telefono);
    public record ActualizarClienteDto(string Nombre, string Documento, string Email, string Telefono);
    public record ClienteDto(Guid Id, string Nombre, string Documento, string Email, string Telefono);
}
