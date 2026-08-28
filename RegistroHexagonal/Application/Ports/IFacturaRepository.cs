using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface IFacturaRepository
    {
        Factura Agregar(Factura factura);
        Factura? ObtenerPorId(Guid id);
        IEnumerable<Factura> ObtenerTodas();
        (List<Factura> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        Factura? Actualizar(Factura factura);
        bool Anular(Guid id, string motivo);
        bool ExisteNumeroFactura(string numeroFactura);
        int ContarFacturas();   
    }
}
