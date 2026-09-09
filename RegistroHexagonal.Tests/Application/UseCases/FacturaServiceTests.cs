using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Application.UseCases;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Tests.Application.UseCases
{
    public class FacturaServiceTests
    {
        private readonly Mock<IFacturaRepository> _facturaRepositorioMock;
        private readonly Mock<IClienteRepository> _clienteRepositorioMock;
        private readonly Mock<IProductoRepository> _productoRepositorioMock;
        private readonly Mock<ILogger<FacturaService>> _loggerMock;
        private readonly FacturaService _service;

        public FacturaServiceTests()
        {
            _facturaRepositorioMock = new Mock<IFacturaRepository>();
            _clienteRepositorioMock = new Mock<IClienteRepository>();
            _productoRepositorioMock = new Mock<IProductoRepository>();
            _loggerMock = new Mock<ILogger<FacturaService>>();

            _service = new FacturaService(
                _facturaRepositorioMock.Object,
                _clienteRepositorioMock.Object,
                _productoRepositorioMock.Object,
                _loggerMock.Object);
        }

        private static Cliente ClienteActivo(string documento = "123456") =>
            new Cliente(Guid.NewGuid(), "Juan Pérez", documento, "juan@mail.com", "3000000000", true);

        private static Producto ProductoActivo(decimal precioBruto = 100000, decimal iva = 19) =>
            new Producto(Guid.NewGuid(), "Teclado", "desc", precioBruto, iva, Guid.NewGuid(), true);

        // ---------- CrearFactura ----------

        [Fact]
        public void CrearFactura_CuandoClienteNoExiste_DebeLanzarBusinessRuleException()
        {
            // Arrange
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento("999")).Returns((Cliente?)null);
            var lineas = new List<(Guid, int, decimal)> { (Guid.NewGuid(), 1, 0) };

            // Act
            Action accion = () => _service.CrearFactura("999", 0, lineas);

            // Assert
            accion.Should().Throw<BusinessRuleException>().WithMessage("*999*");
            _facturaRepositorioMock.Verify(r => r.Agregar(It.IsAny<Factura>()), Times.Never);
        }

        [Fact]
        public void CrearFactura_CuandoClienteInactivo_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var clienteInactivo = new Cliente(Guid.NewGuid(), "Juan", "123456", "j@mail.com", "300", false);
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento("123456")).Returns(clienteInactivo);
            var lineas = new List<(Guid, int, decimal)> { (Guid.NewGuid(), 1, 0) };

            // Act
            Action accion = () => _service.CrearFactura("123456", 0, lineas);

            // Assert
            accion.Should().Throw<BusinessRuleException>();
        }

        [Fact]
        public void CrearFactura_SinLineas_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var cliente = ClienteActivo();
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);

            // Act
            Action accion = () => _service.CrearFactura(cliente.Documento, 0, new List<(Guid, int, decimal)>());

            // Assert
            accion.Should().Throw<BusinessRuleException>().WithMessage("*al menos una línea*");
        }

        [Fact]
        public void CrearFactura_ConLineasNull_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var cliente = ClienteActivo();
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);

            // Act
            Action accion = () => _service.CrearFactura(cliente.Documento, 0, null!);

            // Assert
            accion.Should().Throw<BusinessRuleException>();
        }

        [Fact]
        public void CrearFactura_CuandoProductoNoExiste_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var cliente = ClienteActivo();
            var productoId = Guid.NewGuid();
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);
            _facturaRepositorioMock.Setup(r => r.ContarFacturas()).Returns(0);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(productoId)).Returns((Producto?)null);

            var lineas = new List<(Guid, int, decimal)> { (productoId, 2, 0) };

            // Act
            Action accion = () => _service.CrearFactura(cliente.Documento, 0, lineas);

            // Assert
            accion.Should().Throw<BusinessRuleException>().WithMessage($"*{productoId}*");
            _facturaRepositorioMock.Verify(r => r.Agregar(It.IsAny<Factura>()), Times.Never);
        }

        [Fact]
        public void CrearFactura_CuandoProductoInactivo_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var cliente = ClienteActivo();
            var productoInactivo = new Producto(Guid.NewGuid(), "Mouse", "desc", 50000, 19, Guid.NewGuid(), false);
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);
            _facturaRepositorioMock.Setup(r => r.ContarFacturas()).Returns(0);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(productoInactivo.Id)).Returns(productoInactivo);

            var lineas = new List<(Guid, int, decimal)> { (productoInactivo.Id, 1, 0) };

            // Act
            Action accion = () => _service.CrearFactura(cliente.Documento, 0, lineas);

            // Assert
            accion.Should().Throw<BusinessRuleException>();
        }

        [Fact]
        public void CrearFactura_ConDatosValidos_DebeCalcularTotalYGuardar()
        {
            // Arrange
            var cliente = ClienteActivo();
            var producto = ProductoActivo(precioBruto: 100000, iva: 19); // 1 unidad, sin descuento -> base 100000, iva 19000, subtotal 119000
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);
            _facturaRepositorioMock.Setup(r => r.ContarFacturas()).Returns(0);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(producto.Id)).Returns(producto);
            _facturaRepositorioMock.Setup(r => r.Agregar(It.IsAny<Factura>())).Returns((Factura f) => f);

            var lineas = new List<(Guid, int, decimal)> { (producto.Id, 1, 0) };

            // Act
            var resultado = _service.CrearFactura(cliente.Documento, 0, lineas);

            // Assert
            resultado.NumeroFactura.Should().Be("FAC-0001");
            resultado.DocumentoCliente.Should().Be(cliente.Documento);
            resultado.NombreCliente.Should().Be(cliente.Nombre);
            resultado.Detalles.Should().HaveCount(1);
            resultado.Total.Should().Be(119000m);
            _facturaRepositorioMock.Verify(r => r.Agregar(It.IsAny<Factura>()), Times.Once);
        }

        [Fact]
        public void CrearFactura_ConVariasLineas_DebeSumarTodosLosSubtotales()
        {
            // Arrange
            var cliente = ClienteActivo();
            var producto1 = ProductoActivo(precioBruto: 100000, iva: 19); // subtotal 119000
            var producto2 = ProductoActivo(precioBruto: 50000, iva: 19);  // subtotal 59500

            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);
            _facturaRepositorioMock.Setup(r => r.ContarFacturas()).Returns(0);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(producto1.Id)).Returns(producto1);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(producto2.Id)).Returns(producto2);
            _facturaRepositorioMock.Setup(r => r.Agregar(It.IsAny<Factura>())).Returns((Factura f) => f);

            var lineas = new List<(Guid, int, decimal)>
            {
                (producto1.Id, 1, 0),
                (producto2.Id, 1, 0)
            };

            // Act
            var resultado = _service.CrearFactura(cliente.Documento, 0, lineas);

            // Assert
            resultado.Detalles.Should().HaveCount(2);
            resultado.Total.Should().Be(178500m); // 119000 + 59500
        }

        [Fact]
        public void CrearFactura_CuandoNumeroDuplicadoUnaVez_DebeReintentarYCrearConExito()
        {
            // Arrange
            var cliente = ClienteActivo();
            var producto = ProductoActivo();
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(producto.Id)).Returns(producto);
            _facturaRepositorioMock.SetupSequence(r => r.ContarFacturas())
                .Returns(0)   
                .Returns(1);  

            int intento = 0;
            Factura? facturaCapturada = null;

            _facturaRepositorioMock
                .Setup(r => r.Agregar(It.IsAny<Factura>()))
                .Callback<Factura>(f => facturaCapturada = f)
                .Returns(() =>
                {
                    intento++;
                    if (intento == 1)
                        throw new NumeroFacturaDuplicadoException("Número de factura duplicado.");
                    return facturaCapturada!;
                });

            var lineas = new List<(Guid, int, decimal)> { (producto.Id, 1, 0) };

            // Act
            var resultado = _service.CrearFactura(cliente.Documento, 0, lineas);

            // Assert
            resultado.NumeroFactura.Should().Be("FAC-0002");
            _facturaRepositorioMock.Verify(r => r.Agregar(It.IsAny<Factura>()), Times.Exactly(2));
        }

        [Fact]
        public void CrearFactura_CuandoSiempreDuplicado_DebeLanzarInvalidOperationExceptionTrasAgotarIntentos()
        {
            // Arrange
            var cliente = ClienteActivo();
            var producto = ProductoActivo();
            _clienteRepositorioMock.Setup(r => r.ObtenerPorDocumento(cliente.Documento)).Returns(cliente);
            _productoRepositorioMock.Setup(r => r.ObtenerPorId(producto.Id)).Returns(producto);
            _facturaRepositorioMock.Setup(r => r.ContarFacturas()).Returns(0);
            _facturaRepositorioMock
                .Setup(r => r.Agregar(It.IsAny<Factura>()))
                .Throws(new NumeroFacturaDuplicadoException("Número de factura duplicado."));

            var lineas = new List<(Guid, int, decimal)> { (producto.Id, 1, 0) };

            // Act
            Action accion = () => _service.CrearFactura(cliente.Documento, 0, lineas);

            // Assert
            accion.Should().Throw<InvalidOperationException>();
            _facturaRepositorioMock.Verify(r => r.Agregar(It.IsAny<Factura>()), Times.Exactly(5)); // maxIntentos = 5
        }

        // ---------- ObtenerPorId ----------

        [Fact]
        public void ObtenerPorId_CuandoExiste_DebeRetornarFactura()
        {
            // Arrange
            var id = Guid.NewGuid();
            var factura = new Factura(id, "FAC-0001", DateTime.UtcNow, Guid.NewGuid(), "Juan", "123456", 0, 119000, false, null);
            _facturaRepositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(factura);

            // Act
            var resultado = _service.ObtenerPorId(id);

            // Assert
            resultado.NumeroFactura.Should().Be("FAC-0001");
        }

        [Fact]
        public void ObtenerPorId_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _facturaRepositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Factura?)null);

            // Act
            Action accion = () => _service.ObtenerPorId(id);

            // Assert
            accion.Should().Throw<NotFoundException>().WithMessage($"*{id}*");
        }

        // ---------- ObtenerTodas ----------

        [Fact]
        public void ObtenerTodas_DebeRetornarListaDelRepositorio()
        {
            // Arrange
            var facturas = new List<Factura>
            {
                new Factura(Guid.NewGuid(), "FAC-0001", DateTime.UtcNow, Guid.NewGuid(), "Juan", "111", 0, 100, false, null),
                new Factura(Guid.NewGuid(), "FAC-0002", DateTime.UtcNow, Guid.NewGuid(), "Ana", "222", 0, 200, false, null)
            };
            _facturaRepositorioMock.Setup(r => r.ObtenerTodas()).Returns(facturas);

            // Act
            var resultado = _service.ObtenerTodas();

            // Assert
            resultado.Should().HaveCount(2);
        }

        // ---------- AnularFactura ----------

        [Fact]
        public void AnularFactura_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _facturaRepositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Factura?)null);

            // Act
            Action accion = () => _service.AnularFactura(id, "Error en el pedido");

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void AnularFactura_CuandoYaEstaAnulada_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var facturaAnulada = new Factura(id, "FAC-0001", DateTime.UtcNow, Guid.NewGuid(), "Juan", "111", 0, 100, anulada: true, motivo: "Ya anulada antes");
            _facturaRepositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(facturaAnulada);

            // Act
            Action accion = () => _service.AnularFactura(id, "Nuevo motivo");

            // Assert
            accion.Should().Throw<BusinessRuleException>().WithMessage($"*{id}*");
            _facturaRepositorioMock.Verify(r => r.Anular(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AnularFactura_ConDatosValidos_DebeAnularSinLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            var factura = new Factura(id, "FAC-0001", DateTime.UtcNow, Guid.NewGuid(), "Juan", "111", 0, 100, false, null);
            _facturaRepositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(factura);
            _facturaRepositorioMock.Setup(r => r.Anular(id, "Error en el pedido")).Returns(true);

            // Act
            Action accion = () => _service.AnularFactura(id, "Error en el pedido");

            // Assert
            accion.Should().NotThrow();
            _facturaRepositorioMock.Verify(r => r.Anular(id, "Error en el pedido"), Times.Once);
        }

        [Fact]
        public void AnularFactura_CuandoRepositorioRetornaFalse_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var factura = new Factura(id, "FAC-0001", DateTime.UtcNow, Guid.NewGuid(), "Juan", "111", 0, 100, false, null);
            _facturaRepositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(factura);
            _facturaRepositorioMock.Setup(r => r.Anular(id, It.IsAny<string>())).Returns(false);

            // Act
            Action accion = () => _service.AnularFactura(id, "Motivo cualquiera");

            // Assert
            accion.Should().Throw<InvalidOperationException>();
        }

        // ---------- ObtenerPaginado ----------

        [Theory]
        [InlineData(0, 10, 1, 10)]
        [InlineData(1, 0, 1, 10)]
        [InlineData(1, 500, 1, 100)]
        [InlineData(5, 30, 5, 30)]
        public void ObtenerPaginado_DebeNormalizarValoresFueraDeRango(
            int paginaEntrada, int tamanoEntrada, int paginaEsperada, int tamanoEsperado)
        {
            // Arrange
            _facturaRepositorioMock
                .Setup(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado))
                .Returns((new List<Factura>(), 0));

            // Act
            _service.ObtenerPaginado(paginaEntrada, tamanoEntrada);

            // Assert
            _facturaRepositorioMock.Verify(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado), Times.Once);
        }
    }
}