using Microsoft.Extensions.Logging;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Application.UseCases
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repositorio;
        private readonly ICategoriaRepository _categoriaRepositorio;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(
            IProductoRepository repositorio,
            ICategoriaRepository categoriaRepositorio,
            ILogger<ProductoService> logger)
        {
            _repositorio = repositorio;
            _categoriaRepositorio = categoriaRepositorio;
            _logger = logger;
        }

        public Producto CrearProducto(string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId)
        {
            var categoria = _categoriaRepositorio.ObtenerPorId(categoriaId);

            if (categoria == null || !categoria.Activo)
                throw new BusinessRuleException("La categoría especificada no existe o está inactiva.");

            var producto = new Producto(nombre, descripcion, precioBruto, iva, categoriaId);
            var creado = _repositorio.Agregar(producto);

            _logger.LogInformation("Producto creado: {ProductoId} - {Nombre} - Categoría: {CategoriaId}",
                creado.Id, creado.Nombre, categoriaId);
            return creado;
        }

        public Producto ObtenerPorId(Guid id)
        {
            var producto = _repositorio.ObtenerPorId(id);
            if (producto is null)
                throw new NotFoundException($"No se encontró ningún producto con el id {id}.");

            return producto;
        }

        public IEnumerable<Producto> ObtenerTodos() => _repositorio.ObtenerTodos();

        public void ReactivarProducto(Guid id)
        {
            var reactivado = _repositorio.Reactivar(id);
            if (!reactivado)
                throw new NotFoundException($"No se encontró ningún producto inactivo con el id {id}.");

            _logger.LogInformation("Producto reactivado: {ProductoId}", id);
        }

        public Producto ActualizarProducto(Guid id, string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId)
        {
            if (!_categoriaRepositorio.Existe(categoriaId))
                throw new BusinessRuleException($"No existe ninguna categoría con el id {categoriaId}.");

            var productoExistente = _repositorio.ObtenerPorId(id);
            if (productoExistente is null)
                throw new NotFoundException($"No se encontró ningún producto con el id {id} para actualizar.");

            productoExistente.Actualizar(nombre, descripcion, precioBruto, iva, categoriaId);
            var actualizado = _repositorio.Actualizar(productoExistente);

            if (actualizado is null)
                throw new InvalidOperationException($"Error inesperado: el repositorio no devolvió el producto actualizado (id: {id}).");

            _logger.LogInformation("Producto actualizado: {ProductoId}", id);
            return actualizado;
        }

        public void EliminarProducto(Guid id)
        {
            var eliminado = _repositorio.Eliminar(id);
            if (!eliminado)
                throw new NotFoundException($"No se encontró ningún producto con el id {id} para eliminar.");

            _logger.LogInformation("Producto eliminado: {ProductoId}", id);
        }

        public (List<Producto> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1) tamanoPagina = 10;
            if (tamanoPagina > 100) tamanoPagina = 100;

            return _repositorio.ObtenerPaginado(pagina, tamanoPagina);
        }
    }
}