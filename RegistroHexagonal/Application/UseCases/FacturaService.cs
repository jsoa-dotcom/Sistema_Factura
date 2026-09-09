using Microsoft.Extensions.Logging;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Application.UseCases
{
    public class FacturaService : IFacturaService
    {
        private readonly IFacturaRepository _facturaRepositorio;
        private readonly IClienteRepository _clienteRepositorio;
        private readonly IProductoRepository _productoRepositorio;
        private readonly ILogger<FacturaService> _logger;

        public FacturaService(
            IFacturaRepository facturaRepositorio,
            IClienteRepository clienteRepositorio,
            IProductoRepository productoRepositorio,
            ILogger<FacturaService> logger)
        {
            _facturaRepositorio = facturaRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _productoRepositorio = productoRepositorio;
            _logger = logger;
        }

        public Factura CrearFactura(string documentoCliente, decimal descuento,
         List<(Guid ProductoId, int Cantidad, decimal Descuento)> lineas)
        {
            var cliente = _clienteRepositorio.ObtenerPorDocumento(documentoCliente);

            if (cliente == null || !cliente.Activo)
                throw new BusinessRuleException($"El cliente con documento {documentoCliente} no existe o se encuentra inactivo.");

            if (lineas is null || lineas.Count == 0)
                throw new BusinessRuleException("La factura debe tener al menos una línea de producto.");

            const int maxIntentos = 5;
            NumeroFacturaDuplicadoException? ultimaExcepcion = null;

            for (int intento = 1; intento <= maxIntentos; intento++)
            {
                var numeroFactura = GenerarSiguienteNumeroFactura();
                var factura = new Factura(numeroFactura, cliente.Id, cliente.Nombre, cliente.Documento, descuento);
                decimal sumaSubtotales = 0;

                foreach (var linea in lineas)
                {
                    var producto = _productoRepositorio.ObtenerPorId(linea.ProductoId);

                    if (producto == null || !producto.Activo)
                        throw new BusinessRuleException($"El producto con el ID {linea.ProductoId} no existe o no está disponible para venta.");

                    var detalle = new DetalleFactura(
                        factura.Id,
                        producto.Id,
                        producto.Nombre,
                        linea.Cantidad,
                        producto.PrecioBruto,
                        producto.Iva,
                        linea.Descuento,
                        descuento);

                    factura.Detalles.Add(detalle);
                    sumaSubtotales += detalle.Subtotal;
                }

                factura.RecalcularTotal(sumaSubtotales);

                try
                {
                    var creada = _facturaRepositorio.Agregar(factura);

                    _logger.LogInformation(
                        "Factura creada: {NumeroFactura} - Cliente: {Documento} - {CantidadLineas} línea(s) - Total: {Total}",
                        creada.NumeroFactura, creada.DocumentoCliente, creada.Detalles.Count, creada.Total);

                    return creada;
                }
                catch (NumeroFacturaDuplicadoException ex)
                {
                    ultimaExcepcion = ex;
                    _logger.LogWarning("Colisión de número de factura ({Numero}), reintentando ({Intento}/{Max})...",
                        numeroFactura, intento, maxIntentos);
                }
            }

            throw new InvalidOperationException(
                "No se pudo generar un número de factura único tras varios intentos.", ultimaExcepcion);
        }

        private string GenerarSiguienteNumeroFactura()
        {
            var siguienteConsecutivo = _facturaRepositorio.ContarFacturas() + 1;
            return $"FAC-{siguienteConsecutivo:D4}";
        }

        public Factura ObtenerPorId(Guid id)
        {
            var factura = _facturaRepositorio.ObtenerPorId(id);
            if (factura is null)
                throw new NotFoundException($"No se encontró ninguna factura con el id {id}.");

            return factura;
        }

        public IEnumerable<Factura> ObtenerTodas() => _facturaRepositorio.ObtenerTodas();

        public void AnularFactura(Guid id, string motivo)
        {
            var factura = _facturaRepositorio.ObtenerPorId(id);
            if (factura is null)
                throw new NotFoundException($"No se encontró ninguna factura con el id {id} para anular.");

            if (factura.Anulada)
                throw new BusinessRuleException($"La factura con el id {id} ya se encuentra anulada.");

            var anulado = _facturaRepositorio.Anular(id, motivo);
            if (!anulado)
                throw new InvalidOperationException($"Error inesperado: no se pudo anular la factura (id: {id}).");

            _logger.LogInformation("Factura anulada: {FacturaId} - Motivo: {Motivo}", id, motivo);
        }

        public (List<Factura> Items, int Total) ObtenerPaginado(int pagina, int tamanoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1) tamanoPagina = 10;
            if (tamanoPagina > 100) tamanoPagina = 100;

            return _facturaRepositorio.ObtenerPaginado(pagina, tamanoPagina);
        }
    }
}