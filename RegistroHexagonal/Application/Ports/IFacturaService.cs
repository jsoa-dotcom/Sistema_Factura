using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Application.Ports
{
    public interface IFacturaService
    {
        Factura CrearFactura(string documentoCliente, decimal descuento, List<(Guid ProductoId, int Cantidad, decimal Descuento)> lineas);
        Factura ObtenerPorId(Guid id);
        IEnumerable<Factura> ObtenerTodas();
        (List<Factura> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina);
        void AnularFactura(Guid id, string motivo);
    }
}
