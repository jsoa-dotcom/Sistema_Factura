using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Application.UseCases;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Tests.Application.UseCases
{
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _repositorioMock;
        private readonly Mock<ILogger<ClienteService>> _loggerMock;
        private readonly ClienteService _service;

        public ClienteServiceTests()
        {
            _repositorioMock = new Mock<IClienteRepository>();
            _loggerMock = new Mock<ILogger<ClienteService>>();
            _service = new ClienteService(_repositorioMock.Object, _loggerMock.Object);
        }

        // ---------- CrearCliente ----------

        [Fact]
        public void CrearCliente_ConDocumentoDuplicado_DebeLanzarBusinessRuleException()
        {
            // Arrange
            _repositorioMock.Setup(r => r.ExisteDocumento("123456")).Returns(true);

            // Act
            Action accion = () => _service.CrearCliente("Juan", "123456", "juan@mail.com", "3000000000");

            // Assert
            accion.Should().Throw<BusinessRuleException>()
                .WithMessage("*123456*");
            _repositorioMock.Verify(r => r.Agregar(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public void CrearCliente_ConDatosValidos_DebeCrearYLoguear()
        {
            // Arrange
            _repositorioMock.Setup(r => r.ExisteDocumento("123456")).Returns(false);
            _repositorioMock.Setup(r => r.Agregar(It.IsAny<Cliente>())).Returns((Cliente c) => c);

            // Act
            var resultado = _service.CrearCliente("Juan", "123456", "juan@mail.com", "3000000000");

            // Assert
            resultado.Nombre.Should().Be("Juan");
            resultado.Documento.Should().Be("123456");
            _repositorioMock.Verify(r => r.Agregar(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public void CrearCliente_ConNombreVacio_DebeLanzarArgumentException()
        {
            // Arrange
            _repositorioMock.Setup(r => r.ExisteDocumento(It.IsAny<string>())).Returns(false);

            // Act
            Action accion = () => _service.CrearCliente("", "123456", "juan@mail.com", "3000000000");

            // Assert
            accion.Should().Throw<ArgumentException>();
        }

        // ---------- ObtenerPorId ----------

        [Fact]
        public void ObtenerPorId_CuandoExiste_DebeRetornarCliente()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cliente = new Cliente(id, "Juan", "123456", "juan@mail.com", "3000000000", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(cliente);

            // Act
            var resultado = _service.ObtenerPorId(id);

            // Assert
            resultado.Nombre.Should().Be("Juan");
        }

        [Fact]
        public void ObtenerPorId_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Cliente?)null);

            // Act
            Action accion = () => _service.ObtenerPorId(id);

            // Assert
            accion.Should().Throw<NotFoundException>().WithMessage($"*{id}*");
        }

        // ---------- ObtenerTodos ----------

        [Fact]
        public void ObtenerTodos_DebeRetornarListaDelRepositorio()
        {
            // Arrange
            var clientes = new List<Cliente>
            {
                new Cliente(Guid.NewGuid(), "Juan", "111", "a@mail.com", "300", true),
                new Cliente(Guid.NewGuid(), "Ana", "222", "b@mail.com", "301", true)
            };
            _repositorioMock.Setup(r => r.ObtenerTodos()).Returns(clientes);

            // Act
            var resultado = _service.ObtenerTodos();

            // Assert
            resultado.Should().HaveCount(2);
        }

        // ---------- ActualizarCliente ----------

        [Fact]
        public void ActualizarCliente_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Cliente?)null);

            // Act
            Action accion = () => _service.ActualizarCliente(id, "Nuevo", "999", "n@mail.com", "302");

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void ActualizarCliente_ConDatosValidos_DebeActualizarYRetornar()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clienteExistente = new Cliente(id, "Viejo", "111", "viejo@mail.com", "300", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(clienteExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Cliente>())).Returns((Cliente c) => c);

            // Act
            var resultado = _service.ActualizarCliente(id, "Nuevo", "222", "nuevo@mail.com", "301");

            // Assert
            resultado.Nombre.Should().Be("Nuevo");
            resultado.Documento.Should().Be("222");
            _repositorioMock.Verify(r => r.Actualizar(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public void ActualizarCliente_CuandoRepositorioRetornaNull_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clienteExistente = new Cliente(id, "Nombre", "111", "n@mail.com", "300", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(clienteExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Cliente>())).Returns((Cliente?)null);

            // Act
            Action accion = () => _service.ActualizarCliente(id, "Nuevo", "222", "n@mail.com", "300");

            // Assert
            accion.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ActualizarCliente_ConDocumentoDeOtroClienteExistente_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clienteExistente = new Cliente(id, "Juan", "111", "juan@mail.com", "300", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(clienteExistente);
            _repositorioMock.Setup(r => r.ExisteDocumento("999")).Returns(true); // documento de OTRO cliente

            // Act
            Action accion = () => _service.ActualizarCliente(id, "Juan", "999", "juan@mail.com", "300");

            // Assert
            accion.Should().Throw<BusinessRuleException>().WithMessage("*999*");
            _repositorioMock.Verify(r => r.Actualizar(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public void ActualizarCliente_ConMismoDocumentoPropio_NoDebeValidarDuplicado()
        {
            // Arrange: el cliente se actualiza sin cambiar su documento
            var id = Guid.NewGuid();
            var clienteExistente = new Cliente(id, "Juan", "111", "juan@mail.com", "300", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(clienteExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Cliente>())).Returns((Cliente c) => c);

            // Act
            var resultado = _service.ActualizarCliente(id, "Juan Actualizado", "111", "juan@mail.com", "300");

            // Assert
            resultado.Nombre.Should().Be("Juan Actualizado");
            _repositorioMock.Verify(r => r.ExisteDocumento(It.IsAny<string>()), Times.Never); // ni siquiera debe consultarlo
        }

        // ---------- EliminarCliente ----------

        [Fact]
        public void EliminarCliente_CuandoExiste_NoDebeLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Eliminar(id)).Returns(true);

            // Act
            Action accion = () => _service.EliminarCliente(id);

            // Assert
            accion.Should().NotThrow();
        }

        [Fact]
        public void EliminarCliente_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Eliminar(id)).Returns(false);

            // Act
            Action accion = () => _service.EliminarCliente(id);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        // ---------- ReactivarCliente ----------

        [Fact]
        public void ReactivarCliente_CuandoExiste_NoDebeLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Reactivar(id)).Returns(true);

            // Act
            Action accion = () => _service.ReactivarCliente(id);

            // Assert
            accion.Should().NotThrow();
        }

        [Fact]
        public void ReactivarCliente_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Reactivar(id)).Returns(false);

            // Act
            Action accion = () => _service.ReactivarCliente(id);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        // ---------- ObtenerPaginado ----------

        [Theory]
        [InlineData(0, 10, 1, 10)]
        [InlineData(1, 0, 1, 10)]
        [InlineData(1, 500, 1, 100)]
        [InlineData(4, 20, 4, 20)]
        public void ObtenerPaginado_DebeNormalizarValoresFueraDeRango(
            int paginaEntrada, int tamanoEntrada, int paginaEsperada, int tamanoEsperado)
        {
            // Arrange
            _repositorioMock
                .Setup(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado))
                .Returns((new List<Cliente>(), 0));

            // Act
            _service.ObtenerPaginado(paginaEntrada, tamanoEntrada);

            // Assert
            _repositorioMock.Verify(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado), Times.Once);
        }
    }
}