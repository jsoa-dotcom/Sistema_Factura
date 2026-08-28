using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Domain
{
    public class Factura
    {
        public Guid Id { get; private set; }
        public string NumeroFactura { get; private set; }
        public DateTime Fecha { get; private set; }
        public Guid ClienteId { get; private set; }
        public string NombreCliente { get; private set; }      
        public string DocumentoCliente { get; private set; }    
        public decimal Descuento { get; private set; }
        public decimal Total { get; private set; }
        public Cliente? Cliente { get; private set; }
        public List<DetalleFactura> Detalles { get; private set; } = new();
        public bool Anulada { get; private set; } = false;
        public string? Motivo { get; private set; }

        public Factura(string numeroFactura, Guid clienteId, string nombreCliente, string documentoCliente, decimal descuento)
        {
            if (string.IsNullOrWhiteSpace(numeroFactura))
                throw new ArgumentException("El número de factura no puede estar vacío.", nameof(numeroFactura));
            if (descuento < 0 || descuento > 100)
                throw new ArgumentException("El descuento debe ser un porcentaje entre 0 y 100.", nameof(descuento));

            Id = Guid.NewGuid();
            NumeroFactura = numeroFactura;
            Fecha = DateTime.UtcNow;
            ClienteId = clienteId;
            NombreCliente = nombreCliente;
            DocumentoCliente = documentoCliente;
            Descuento = descuento;
            Total = 0;
        }

        public Factura(Guid id, string numeroFactura, DateTime fecha, Guid clienteId, string nombreCliente,
            string documentoCliente, decimal descuento, decimal total, bool anulada, string? motivo)
        {
            Id = id;
            NumeroFactura = numeroFactura;
            Fecha = fecha;
            ClienteId = clienteId;
            NombreCliente = nombreCliente;
            DocumentoCliente = documentoCliente;
            Descuento = descuento;
            Total = total;
            Anulada = anulada;
            Motivo = motivo;
        }

        public void RecalcularTotal(decimal sumaSubtotalesDetalles)
        {
            Total = sumaSubtotalesDetalles < 0 ? 0 : Math.Round(sumaSubtotalesDetalles, 2);
        }

        public void ActualizarDescuento(decimal descuento)
        {
            if (descuento < 0 || descuento > 100)
                throw new ArgumentException("El descuento debe ser un porcentaje entre 0 y 100.", nameof(descuento));

            Descuento = descuento;
        }

        public void Anular(string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de anulación no puede estar vacío.", nameof(motivo));
            if (Anulada)
                throw new InvalidOperationException("La factura ya se encuentra anulada.");

            Anulada = true;
            Motivo = motivo;
        }
    }
}
