using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface ICategoriaService
    {
        Categoria CrearCategoria(string nombre);
        Categoria ObtenerPorId(Guid id);
        IEnumerable<Categoria> ObtenerTodas();
        (List<Categoria> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        Categoria ActualizarCategoria(Guid id, string nombre);
        void EliminarCategoria(Guid id);
        void ReactivarCategoria(Guid id);
    }
}
