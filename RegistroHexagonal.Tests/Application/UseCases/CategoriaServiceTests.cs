using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Application.UseCases;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Tests.Application.UseCases
{
    public class CategoriaServiceTests
    {
        private readonly Mock<ICategoriaRepository> _repositorioMock;
        private readonly Mock<ILogger<CategoriaService>> _loggerMock;
        private readonly CategoriaService _service;

        public CategoriaServiceTests()
        {
            _repositorioMock = new Mock<ICategoriaRepository>();
            _loggerMock = new Mock<ILogger<CategoriaService>>();
            _service = new CategoriaService(_repositorioMock.Object, _loggerMock.Object);
        }

        // ---------- CrearCategoria ----------

        [Fact]
        public void CrearCategoria_ConNombreValido_DebeCrearYLoguear()
        {
            // Arrange
            _repositorioMock.Setup(r => r.Agregar(It.IsAny<Categoria>())).Returns((Categoria c) => c);

            // Act
            var resultado = _service.CrearCategoria("Electrónica");

            // Assert
            resultado.Nombre.Should().Be("Electrónica");
            resultado.Activo.Should().BeTrue();
            _repositorioMock.Verify(r => r.Agregar(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public void CrearCategoria_ConNombreVacio_DebeLanzarArgumentException()
        {
            // Act
            Action accion = () => _service.CrearCategoria("");

            // Assert
            accion.Should().Throw<ArgumentException>();
            _repositorioMock.Verify(r => r.Agregar(It.IsAny<Categoria>()), Times.Never);
        }

        // ---------- ObtenerPorId ----------

        [Fact]
        public void ObtenerPorId_CuandoExiste_DebeRetornarCategoria()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoria = new Categoria(id, "Hogar", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(categoria);

            // Act
            var resultado = _service.ObtenerPorId(id);

            // Assert
            resultado.Nombre.Should().Be("Hogar");
        }

        [Fact]
        public void ObtenerPorId_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Categoria?)null);

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
            var categorias = new List<Categoria>
            {
                new Categoria(Guid.NewGuid(), "Electrónica", true),
                new Categoria(Guid.NewGuid(), "Ropa y Calzado", true)
            };
            _repositorioMock.Setup(r => r.ObtenerTodas()).Returns(categorias);

            // Act
            var resultado = _service.ObtenerTodas();

            // Assert
            resultado.Should().HaveCount(2);
        }

        // ---------- ActualizarCategoria ----------

        [Fact]
        public void ActualizarCategoria_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Categoria?)null);

            // Act
            Action accion = () => _service.ActualizarCategoria(id, "Nuevo nombre");

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void ActualizarCategoria_ConDatosValidos_DebeActualizarYRetornar()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoriaExistente = new Categoria(id, "Viejo nombre", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(categoriaExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Categoria>())).Returns((Categoria c) => c);

            // Act
            var resultado = _service.ActualizarCategoria(id, "Nombre nuevo");

            // Assert
            resultado.Nombre.Should().Be("Nombre nuevo");
            _repositorioMock.Verify(r => r.Actualizar(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public void ActualizarCategoria_CuandoRepositorioRetornaNull_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoriaExistente = new Categoria(id, "Nombre", true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(categoriaExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Categoria>())).Returns((Categoria?)null);

            // Act
            Action accion = () => _service.ActualizarCategoria(id, "Nombre nuevo");

            // Assert
            accion.Should().Throw<InvalidOperationException>();
        }

        // ---------- EliminarCategoria ----------

        [Fact]
        public void EliminarCategoria_CuandoExiste_NoDebeLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Eliminar(id)).Returns(true);

            // Act
            Action accion = () => _service.EliminarCategoria(id);

            // Assert
            accion.Should().NotThrow();
        }

        [Fact]
        public void EliminarCategoria_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Eliminar(id)).Returns(false);

            // Act
            Action accion = () => _service.EliminarCategoria(id);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        // ---------- ReactivarCategoria ----------

        [Fact]
        public void ReactivarCategoria_CuandoExiste_NoDebeLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Reactivar(id)).Returns(true);

            // Act
            Action accion = () => _service.ReactivarCategoria(id);

            // Assert
            accion.Should().NotThrow();
        }

        [Fact]
        public void ReactivarCategoria_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Reactivar(id)).Returns(false);

            // Act
            Action accion = () => _service.ReactivarCategoria(id);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        // ---------- ObtenerPaginado ----------

        [Theory]
        [InlineData(0, 10, 1, 10)]
        [InlineData(1, 0, 1, 10)]
        [InlineData(1, 500, 1, 100)]
        [InlineData(2, 15, 2, 15)]
        public void ObtenerPaginado_DebeNormalizarValoresFueraDeRango(
            int paginaEntrada, int tamanoEntrada, int paginaEsperada, int tamanoEsperado)
        {
            // Arrange
            _repositorioMock
                .Setup(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado))
                .Returns((new List<Categoria>(), 0));

            // Act
            _service.ObtenerPaginado(paginaEntrada, tamanoEntrada);

            // Assert
            _repositorioMock.Verify(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado), Times.Once);
        }
    }
}