using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface IClienteService
    {
        Cliente CrearCliente(string nombre, string documento, string email, string telefono);
        Cliente ObtenerPorId(Guid id);
        IEnumerable<Cliente> ObtenerTodos();
        (List<Cliente> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        Cliente ActualizarCliente(Guid id, string nombre, string documento, string email, string telefono);
        void EliminarCliente(Guid id);
        void ReactivarCliente(Guid id);
    }
}
