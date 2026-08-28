namespace RegistroHexagonal.Application.DTOs
{
    public record PagedResultDto<T>(
        List<T> Items,
        int PaginaActual,
        int TamanoPagina,
        int TotalRegistros,
        int TotalPaginas);
}
