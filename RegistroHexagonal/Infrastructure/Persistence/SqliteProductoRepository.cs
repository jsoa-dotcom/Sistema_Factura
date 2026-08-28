using Microsoft.EntityFrameworkCore;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Infrastructure.Persistence
{
    public class SqliteProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _contexto;

        public SqliteProductoRepository(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public Producto Agregar(Producto producto)
        {
            _contexto.Productos.Add(producto);
            _contexto.SaveChanges();
            return producto;
        }

        public Producto? ObtenerPorId(Guid id)
        {
            return _contexto.Productos.Include(p => p.Categoria).FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Producto> ObtenerTodos()
        {
            return _contexto.Productos.Include(p => p.Categoria).AsNoTracking().ToList();
        }

        public Producto? Actualizar(Producto producto)
        {
            var existente = _contexto.Productos.FirstOrDefault(p => p.Id == producto.Id);
            if (existente is null)
                return null;

            _contexto.Entry(existente).CurrentValues.SetValues(producto);
            _contexto.SaveChanges();
            return existente;
        }

        public bool Eliminar(Guid id)
        {
            var producto = _contexto.Productos.FirstOrDefault(p => p.Id == id && p.Activo);
            if (producto == null) return false;

            producto.Desactivar(); 
            _contexto.Productos.Update(producto);
            _contexto.SaveChanges();
            return true;
        }

        public bool Existe(Guid id)
        {
            return _contexto.Productos.Any(p => p.Id == id);
        }
        public bool Reactivar(Guid id)
        {
            var producto = _contexto.Productos.FirstOrDefault(p => p.Id == id && !p.Activo);
            if (producto is null)
                return false;

            producto.Reactivar();
            _contexto.Productos.Update(producto);
            _contexto.SaveChanges();
            return true;
        }

        public (List<Producto> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            var query = _contexto.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo) 
                .AsNoTracking();

            var total = query.Count();
            var items = query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToList();

            return (items, total);
        }       
    }
}
