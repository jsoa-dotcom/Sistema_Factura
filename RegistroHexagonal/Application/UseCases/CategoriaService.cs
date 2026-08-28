using Microsoft.Extensions.Logging;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Application.UseCases
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repositorio;
        private readonly ILogger<CategoriaService> _logger;

        public CategoriaService(ICategoriaRepository repositorio, ILogger<CategoriaService> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        public Categoria CrearCategoria(string nombre)
        {
            var categoria = new Categoria(nombre);
            var creada = _repositorio.Agregar(categoria);

            _logger.LogInformation("Categoría creada: {CategoriaId} - {Nombre}", creada.Id, creada.Nombre);
            return creada;
        }

        public Categoria ObtenerPorId(Guid id)
        {
            var categoria = _repositorio.ObtenerPorId(id);
            if (categoria is null)
                throw new NotFoundException($"No se encontró ninguna categoría con el id {id}.");

            return categoria;
        }

        public IEnumerable<Categoria> ObtenerTodas() => _repositorio.ObtenerTodas();

        public Categoria ActualizarCategoria(Guid id, string nombre)
        {
            var categoriaExistente = _repositorio.ObtenerPorId(id);
            if (categoriaExistente is null)
                throw new NotFoundException($"No se encontró ninguna categoría con el id {id} para actualizar.");

            categoriaExistente.Actualizar(nombre);
            var actualizada = _repositorio.Actualizar(categoriaExistente);

            if (actualizada is null)
                throw new InvalidOperationException($"Error inesperado: el repositorio no devolvió la categoría actualizada (id: {id}).");

            _logger.LogInformation("Categoría actualizada: {CategoriaId}", id);
            return actualizada;
        }

        public void EliminarCategoria(Guid id)
        {
            var eliminado = _repositorio.Eliminar(id);
            if (!eliminado)
                throw new NotFoundException($"No se encontró ninguna categoría con el id {id} para eliminar.");

            _logger.LogInformation("Categoría eliminada: {CategoriaId}", id);
        }

        public void ReactivarCategoria(Guid id)
        {
            var reactivado = _repositorio.Reactivar(id);
            if (!reactivado)
                throw new NotFoundException($"No se encontró ninguna categoría inactiva con el id {id}.");

            _logger.LogInformation("Categoría reactivada: {CategoriaId}", id);
        }

        public (List<Categoria> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1) tamanoPagina = 10;
            if (tamanoPagina > 100) tamanoPagina = 100;

            return _repositorio.ObtenerPaginado(pagina, tamanoPagina);
        }
    }
}