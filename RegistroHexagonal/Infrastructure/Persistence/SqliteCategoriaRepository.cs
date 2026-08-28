using Microsoft.EntityFrameworkCore;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Infrastructure.Persistence
{
    public class SqliteCategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _contexto;

        public SqliteCategoriaRepository(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public Categoria Agregar(Categoria categoria)
        {
            _contexto.Categorias.Add(categoria);
            _contexto.SaveChanges();
            return categoria;
        }

        public Categoria? ObtenerPorId(Guid id)
        {
            return _contexto.Categorias.FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<Categoria> ObtenerTodas()
        {
            return _contexto.Categorias.AsNoTracking().ToList();
        }

        public Categoria? Actualizar(Categoria categoria)
        {
            var existente = _contexto.Categorias.FirstOrDefault(c => c.Id == categoria.Id);
            if (existente is null)
                return null;

            _contexto.Entry(existente).CurrentValues.SetValues(categoria);
            _contexto.SaveChanges();
            return existente;
        }

        public bool Eliminar(Guid id)
        {
            var categoria = _contexto.Categorias.FirstOrDefault(c => c.Id == id && c.Activo);
            if (categoria == null) return false;

            categoria.Desactivar(); 
            _contexto.Categorias.Update(categoria);
            _contexto.SaveChanges();
            return true;
        }

        public bool Existe(Guid id)
        {
            return _contexto.Categorias.Any(c => c.Id == id);
        }

        public bool Reactivar(Guid id)
        {
            var categoria = _contexto.Categorias.FirstOrDefault(c => c.Id == id && !c.Activo);
            if (categoria is null) return false;

            categoria.Reactivar();
            _contexto.Categorias.Update(categoria);
            _contexto.SaveChanges();
            return true;
        }

        public (List<Categoria> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            var query = _contexto.Categorias
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
