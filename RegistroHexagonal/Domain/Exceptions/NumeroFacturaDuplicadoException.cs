namespace RegistroHexagonal.Domain.Exceptions
{
    public class NumeroFacturaDuplicadoException : Exception
    {
        public NumeroFacturaDuplicadoException(string numeroFactura)
            : base($"El número de factura {numeroFactura} ya existe (colisión de concurrencia).") { }
    }
}