using Microsoft.Extensions.Logging;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Application.UseCases
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repositorio;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IClienteRepository repositorio, ILogger<ClienteService> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        public Cliente CrearCliente(string nombre, string documento, string email, string telefono)
        {
            if (_repositorio.ExisteDocumento(documento))
                throw new BusinessRuleException($"Ya existe un cliente con el documento {documento}.");

            var cliente = new Cliente(nombre, documento, email, telefono);
            var creado = _repositorio.Agregar(cliente);

            _logger.LogInformation("Cliente creado: {ClienteId} - {Documento}", creado.Id, creado.Documento);
            return creado;
        }

        public Cliente ObtenerPorId(Guid id)
        {
            var cliente = _repositorio.ObtenerPorId(id);
            if (cliente is null)
                throw new NotFoundException($"No se encontró ningún cliente con el id {id}.");

            return cliente;
        }

        public IEnumerable<Cliente> ObtenerTodos() => _repositorio.ObtenerTodos();

        public Cliente ActualizarCliente(Guid id, string nombre, string documento, string email, string telefono)
        {
            var clienteExistente = _repositorio.ObtenerPorId(id);
            if (clienteExistente is null)
                throw new NotFoundException($"No se encontró ningún cliente con el id {id} para actualizar.");

            clienteExistente.Actualizar(nombre, documento, email, telefono);
            var actualizado = _repositorio.Actualizar(clienteExistente);

            if (actualizado is null)
                throw new InvalidOperationException($"Error inesperado: el repositorio no devolvió el cliente actualizado (id: {id}).");

            _logger.LogInformation("Cliente actualizado: {ClienteId}", id);
            return actualizado;
        }

        public void EliminarCliente(Guid id)
        {
            var eliminado = _repositorio.Eliminar(id);
            if (!eliminado)
                throw new NotFoundException($"No se encontró ningún cliente con el id {id} para eliminar.");

            _logger.LogInformation("Cliente eliminado: {ClienteId}", id);
        }

        public void ReactivarCliente(Guid id)
        {
            var reactivado = _repositorio.Reactivar(id);
            if (!reactivado)
                throw new NotFoundException($"No se encontró ningún cliente inactivo con el id {id}.");

            _logger.LogInformation("Cliente reactivado: {ClienteId}", id);
        }

        public (List<Cliente> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1) tamanoPagina = 10;
            if (tamanoPagina > 100) tamanoPagina = 100;
            return _repositorio.ObtenerPaginado(pagina, tamanoPagina);
        }
    }
}