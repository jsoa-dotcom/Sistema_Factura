namespace RegistroHexagonal.Application.DTOs
{
    public record ApiResponse<T>(bool Exitoso, string Mensaje, T? Datos);
    public record ApiResponse(bool Exitoso, string Mensaje);
}
