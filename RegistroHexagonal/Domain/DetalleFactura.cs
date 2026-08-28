using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Domain
{
    public class DetalleFactura
    {
        public Guid Id { get; private set; }
        public Guid FacturaId { get; private set; }
        public Guid ProductoId { get; private set; }
        public string NombreProducto { get; private set; }  
        public int Cantidad { get; private set; }
        public decimal PrecioUnitarioBruto { get; private set; }
        public decimal Iva { get; private set; }
        public decimal Descuento { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal BaseGravable { get; private set; }
        public decimal ValorIva { get; private set; }
        public Producto? Producto { get; private set; }

        public DetalleFactura(Guid facturaId, Guid productoId, string nombreProducto, int cantidad,
    decimal precioUnitarioBruto, decimal iva, decimal descuento, decimal descuentoFactura)
        {
            ValidarDatos(cantidad, precioUnitarioBruto, iva, descuento);

            Id = Guid.NewGuid();
            FacturaId = facturaId;
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Cantidad = cantidad;
            PrecioUnitarioBruto = precioUnitarioBruto;
            Iva = iva;
            Descuento = descuento;

            BaseGravable = CalcularBaseGravable(cantidad, precioUnitarioBruto, descuento, descuentoFactura);
            ValorIva = CalcularValorIva(BaseGravable, iva);
            Subtotal = BaseGravable + ValorIva;
        }

        public DetalleFactura(Guid id, Guid facturaId, Guid productoId, string nombreProducto, int cantidad,
            decimal precioUnitarioBruto, decimal iva, decimal descuento, decimal subtotal)
        {
            Id = id;
            FacturaId = facturaId;
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Cantidad = cantidad;
            PrecioUnitarioBruto = precioUnitarioBruto;
            Iva = iva;
            Descuento = descuento;
            Subtotal = subtotal;
        }

        private static void ValidarDatos(int cantidad, decimal precioUnitarioBruto, decimal iva, decimal descuento)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));
            if (precioUnitarioBruto < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo.", nameof(precioUnitarioBruto));
            if (iva < 0)
                throw new ArgumentException("El IVA no puede ser negativo.", nameof(iva));
            if (descuento < 0 || descuento > 100)
                throw new ArgumentException("El descuento debe ser un porcentaje entre 0 y 100.", nameof(descuento));
        }

        private static decimal CalcularBaseGravable(int cantidad, decimal precioUnitarioBruto, decimal descuentoLinea, decimal descuentoFactura)
        {
            var descuentoEfectivo = 1 - (1 - descuentoLinea / 100m) * (1 - descuentoFactura / 100m);
            var baseSinDescuento = precioUnitarioBruto * cantidad;
            var baseConDescuento = baseSinDescuento * (1 - descuentoEfectivo);
            return baseConDescuento < 0 ? 0 : Math.Round(baseConDescuento, 2);
        }

        private static decimal CalcularValorIva(decimal baseGravable, decimal iva)
        {
            return Math.Round(baseGravable * (iva / 100m), 2);
        }
    }
}
