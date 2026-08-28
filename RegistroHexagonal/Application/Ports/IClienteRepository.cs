using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface IClienteRepository
    {
        Cliente Agregar(Cliente cliente);
        Cliente? ObtenerPorId(Guid id);
        Cliente? ObtenerPorDocumento(string documento);   
        IEnumerable<Cliente> ObtenerTodos();
        (List<Cliente> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);   
        Cliente? Actualizar(Cliente cliente);
        bool Eliminar(Guid id);
        bool Existe(Guid id);
        bool ExisteDocumento(string documento);
        bool Reactivar(Guid id);
    }
}
