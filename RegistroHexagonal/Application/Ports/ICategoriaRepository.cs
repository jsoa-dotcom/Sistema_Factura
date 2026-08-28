using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface ICategoriaRepository
    {
        Categoria Agregar(Categoria categoria);
        Categoria? ObtenerPorId(Guid id);
        IEnumerable<Categoria> ObtenerTodas();
        (List<Categoria> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        Categoria? Actualizar(Categoria categoria);
        bool Eliminar(Guid id);
        bool Existe(Guid id);
        bool Reactivar(Guid id);
    }
}
