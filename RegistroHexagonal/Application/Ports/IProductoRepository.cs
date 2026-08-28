using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface IProductoRepository
    {
        Producto Agregar(Producto producto);
        Producto? ObtenerPorId(Guid id);
        IEnumerable<Producto> ObtenerTodos();
        (List<Producto> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        Producto? Actualizar(Producto producto);
        bool Eliminar(Guid id);
        bool Existe(Guid id);
        bool Reactivar(Guid id);
    }
}
