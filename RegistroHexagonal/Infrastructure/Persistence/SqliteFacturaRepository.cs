using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RegistroHexagonal.Domain.Exceptions;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Infrastructure.Persistence
{
    public class SqliteFacturaRepository : IFacturaRepository
    {
        private readonly AppDbContext _contexto;

        public SqliteFacturaRepository(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public Factura Agregar(Factura factura)
        {
            _contexto.Facturas.Add(factura);
            try
            {
                _contexto.SaveChanges();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 } sqliteEx
                && sqliteEx.Message.Contains("Facturas.NumeroFactura"))
            {
                _contexto.Entry(factura).State = EntityState.Detached;
                throw new NumeroFacturaDuplicadoException(factura.NumeroFactura);
            }
            return factura;
        }

        public Factura? ObtenerPorId(Guid id)
        {
            return _contexto.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(f => f.Id == id);
        }

        public IEnumerable<Factura> ObtenerTodas()
        {
            return _contexto.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .ToList();
        }

        public Factura? Actualizar(Factura factura)
        {
            var existente = _contexto.Facturas.FirstOrDefault(f => f.Id == factura.Id);
            if (existente is null)
                return null;

            _contexto.Entry(existente).CurrentValues.SetValues(factura);
            _contexto.SaveChanges();
            return existente;
        }

        public bool Anular(Guid id, string motivo)
        {
            var existente = _contexto.Facturas.FirstOrDefault(f => f.Id == id);
            if (existente is null || existente.Anulada)
                return false;

            existente.Anular(motivo);
            _contexto.SaveChanges();
            return true;
        }

        public bool ExisteNumeroFactura(string numeroFactura)
        {
            return _contexto.Facturas.Any(f => f.NumeroFactura == numeroFactura);
        }

        public int ContarFacturas()
        {
            return _contexto.Facturas.Count();
        }

        public (List<Factura> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            var total = _contexto.Facturas.Count();
            var items = _contexto.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles).ThenInclude(d => d.Producto)
                .AsNoTracking()
                .OrderByDescending(f => f.Fecha)
                .Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();
            return (items, total);
        }
    }
}
