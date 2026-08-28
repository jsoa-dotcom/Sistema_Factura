using Microsoft.EntityFrameworkCore;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Infrastructure.Persistence
{
    public class SqliteClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _contexto;

        public SqliteClienteRepository(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public Cliente Agregar(Cliente cliente)
        {
            _contexto.Clientes.Add(cliente);
            _contexto.SaveChanges();
            return cliente;
        }

        public Cliente? ObtenerPorId(Guid id)
        {
            return _contexto.Clientes.FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<Cliente> ObtenerTodos()
        {
            return _contexto.Clientes.AsNoTracking().ToList();
        }

        public Cliente? Actualizar(Cliente cliente)
        {
            var existente = _contexto.Clientes.FirstOrDefault(c => c.Id == cliente.Id);
            if (existente is null)
                return null;

            _contexto.Entry(existente).CurrentValues.SetValues(cliente);
            _contexto.SaveChanges();
            return existente;
        }

        public bool Eliminar(Guid id)
        {
            var cliente = _contexto.Clientes.FirstOrDefault(c => c.Id == id && c.Activo);
            if (cliente == null) return false;

            cliente.Desactivar(); 
            _contexto.Clientes.Update(cliente);
            _contexto.SaveChanges();
            return true;
        }

        public bool Existe(Guid id)
        {
            return _contexto.Clientes.Any(c => c.Id == id);
        }

        public bool Reactivar(Guid id)
        {
            var cliente = _contexto.Clientes.FirstOrDefault(c => c.Id == id && !c.Activo);
            if (cliente is null) return false;

            cliente.Reactivar();
            _contexto.Clientes.Update(cliente);
            _contexto.SaveChanges();
            return true;
        }

        public bool ExisteDocumento(string documento)
        {
            return _contexto.Clientes.Any(c => c.Documento == documento);
        }

        public Cliente? ObtenerPorDocumento(string documento)
        {
            return _contexto.Clientes.FirstOrDefault(c => c.Documento == documento);
        }

        public (List<Cliente> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            var query = _contexto.Clientes
                .Where(c => c.Activo) 
                .AsNoTracking();

            var total = query.Count();
            var items = query
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToList();

            return (items, total);
        }
    }
}
