using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface IProductoService
    {
        Producto CrearProducto(string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId);
        Producto ObtenerPorId(Guid id);
        IEnumerable<Producto> ObtenerTodos();
        (List<Producto> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        Producto ActualizarProducto(Guid id, string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId);
        void EliminarProducto(Guid id);
        void ReactivarProducto(Guid id);
    }
}
