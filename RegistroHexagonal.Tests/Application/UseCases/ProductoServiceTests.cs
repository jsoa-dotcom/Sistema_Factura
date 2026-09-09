using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using RegistroHexagonal.Application.Ports;
using RegistroHexagonal.Application.UseCases;
using RegistroHexagonal.Domain;
using RegistroHexagonal.Domain.Exceptions;

namespace RegistroHexagonal.Tests.Application.UseCases
{
    public class ProductoServiceTests
    {
        private readonly Mock<IProductoRepository> _repositorioMock;
        private readonly Mock<ICategoriaRepository> _categoriaRepositorioMock;
        private readonly Mock<ILogger<ProductoService>> _loggerMock;
        private readonly ProductoService _service;

        public ProductoServiceTests()
        {
            _repositorioMock = new Mock<IProductoRepository>();
            _categoriaRepositorioMock = new Mock<ICategoriaRepository>();
            _loggerMock = new Mock<ILogger<ProductoService>>();

            _service = new ProductoService(
                _repositorioMock.Object,
                _categoriaRepositorioMock.Object,
                _loggerMock.Object);
        }

        // ---------- CrearProducto ----------

        [Fact]
        public void CrearProducto_CuandoCategoriaNoExiste_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            _categoriaRepositorioMock
                .Setup(r => r.ObtenerPorId(categoriaId))
                .Returns((Categoria?)null);

            // Act
            Action accion = () => _service.CrearProducto("Teclado", "desc", 100000, 19, categoriaId);

            // Assert
            accion.Should().Throw<BusinessRuleException>()
                .WithMessage("*no existe o está inactiva*");
        }

        [Fact]
        public void CrearProducto_CuandoCategoriaInactiva_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoriaInactiva = new Categoria(categoriaId, "Electrónica", activo: false);
            _categoriaRepositorioMock.Setup(r => r.ObtenerPorId(categoriaId)).Returns(categoriaInactiva);

            // Act
            Action accion = () => _service.CrearProducto("Teclado", "desc", 100000, 19, categoriaId);

            // Assert
            accion.Should().Throw<BusinessRuleException>()
                .WithMessage("*no existe o está inactiva*");
            _repositorioMock.Verify(r => r.Agregar(It.IsAny<Producto>()), Times.Never);
        }

        [Fact]
        public void CrearProducto_ConDatosValidos_DebeCrearYLoguear()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoriaActiva = new Categoria(categoriaId, "Electrónica", activo: true);
            _categoriaRepositorioMock.Setup(r => r.ObtenerPorId(categoriaId)).Returns(categoriaActiva);
            _repositorioMock.Setup(r => r.Agregar(It.IsAny<Producto>())).Returns((Producto p) => p);

            // Act
            var resultado = _service.CrearProducto("Mouse", "desc", 50000, 19, categoriaId);

            // Assert
            resultado.Nombre.Should().Be("Mouse");
            resultado.CategoriaId.Should().Be(categoriaId);
            _repositorioMock.Verify(r => r.Agregar(It.IsAny<Producto>()), Times.Once);
        }

        // ---------- ObtenerPorId ----------

        [Fact]
        public void ObtenerPorId_CuandoExiste_DebeRetornarProducto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var producto = new Producto(id, "Teclado", "desc", 100000, 19, Guid.NewGuid(), true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(producto);

            // Act
            var resultado = _service.ObtenerPorId(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nombre.Should().Be("Teclado");
        }

        [Fact]
        public void ObtenerPorId_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Producto?)null);

            // Act
            Action accion = () => _service.ObtenerPorId(id);

            // Assert
            accion.Should().Throw<NotFoundException>()
                .WithMessage($"*{id}*");
        }

        // ---------- ObtenerTodos ----------

        [Fact]
        public void ObtenerTodos_DebeRetornarListaDelRepositorio()
        {
            // Arrange
            var productos = new List<Producto>
            {
                new Producto(Guid.NewGuid(), "A", "descA", 1000, 19, Guid.NewGuid(), true),
                new Producto(Guid.NewGuid(), "B", "descB", 2000, 19, Guid.NewGuid(), true)
            };
            _repositorioMock.Setup(r => r.ObtenerTodos()).Returns(productos);

            // Act
            var resultado = _service.ObtenerTodos();

            // Assert
            resultado.Should().HaveCount(2);
        }

        // ---------- ReactivarProducto ----------

        [Fact]
        public void ReactivarProducto_CuandoExiste_DebeReactivarSinLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Reactivar(id)).Returns(true);

            // Act
            Action accion = () => _service.ReactivarProducto(id);

            // Assert
            accion.Should().NotThrow();
            _repositorioMock.Verify(r => r.Reactivar(id), Times.Once);
        }

        [Fact]
        public void ReactivarProducto_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Reactivar(id)).Returns(false);

            // Act
            Action accion = () => _service.ReactivarProducto(id);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        // ---------- ActualizarProducto ----------

        [Fact]
        public void ActualizarProducto_CuandoCategoriaNoExiste_DebeLanzarBusinessRuleException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            _categoriaRepositorioMock.Setup(r => r.Existe(categoriaId)).Returns(false);

            // Act
            Action accion = () => _service.ActualizarProducto(id, "Nuevo", "desc", 100000, 19, categoriaId);

            // Assert
            accion.Should().Throw<BusinessRuleException>();
            _repositorioMock.Verify(r => r.ObtenerPorId(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public void ActualizarProducto_CuandoProductoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            _categoriaRepositorioMock.Setup(r => r.Existe(categoriaId)).Returns(true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns((Producto?)null);

            // Act
            Action accion = () => _service.ActualizarProducto(id, "Nuevo", "desc", 100000, 19, categoriaId);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void ActualizarProducto_ConDatosValidos_DebeActualizarYRetornarProducto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoriaOriginal = Guid.NewGuid();
            var categoriaNueva = Guid.NewGuid();
            var productoExistente = new Producto(id, "Viejo", "descVieja", 1000, 19, categoriaOriginal, true);

            _categoriaRepositorioMock.Setup(r => r.Existe(categoriaNueva)).Returns(true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(productoExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Producto>())).Returns((Producto p) => p);

            // Act
            var resultado = _service.ActualizarProducto(id, "Nuevo", "descNueva", 5000, 19, categoriaNueva);

            // Assert
            resultado.Nombre.Should().Be("Nuevo");
            resultado.CategoriaId.Should().Be(categoriaNueva);
            _repositorioMock.Verify(r => r.Actualizar(It.IsAny<Producto>()), Times.Once);
        }

        [Fact]
        public void ActualizarProducto_CuandoRepositorioRetornaNull_DebeLanzarInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var productoExistente = new Producto(id, "Viejo", "desc", 1000, 19, categoriaId, true);

            _categoriaRepositorioMock.Setup(r => r.Existe(categoriaId)).Returns(true);
            _repositorioMock.Setup(r => r.ObtenerPorId(id)).Returns(productoExistente);
            _repositorioMock.Setup(r => r.Actualizar(It.IsAny<Producto>())).Returns((Producto?)null);

            // Act
            Action accion = () => _service.ActualizarProducto(id, "Nuevo", "desc", 1000, 19, categoriaId);

            // Assert
            accion.Should().Throw<InvalidOperationException>();
        }

        // ---------- EliminarProducto ----------

        [Fact]
        public void EliminarProducto_CuandoExiste_DebeEliminarSinLanzarExcepcion()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Eliminar(id)).Returns(true);

            // Act
            Action accion = () => _service.EliminarProducto(id);

            // Assert
            accion.Should().NotThrow();
        }

        [Fact]
        public void EliminarProducto_CuandoNoExiste_DebeLanzarNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorioMock.Setup(r => r.Eliminar(id)).Returns(false);

            // Act
            Action accion = () => _service.EliminarProducto(id);

            // Assert
            accion.Should().Throw<NotFoundException>();
        }

        // ---------- ObtenerPaginado ----------

        [Theory]
        [InlineData(0, 10, 1, 10)]      // pagina < 1 -> se corrige a 1
        [InlineData(1, 0, 1, 10)]       // tamanoPagina < 1 -> se corrige a 10
        [InlineData(1, 500, 1, 100)]    // tamanoPagina > 100 -> se corrige a 100
        [InlineData(3, 25, 3, 25)]      // valores válidos -> pasan sin cambios
        public void ObtenerPaginado_DebeNormalizarValoresFueraDeRango(
            int paginaEntrada, int tamanoEntrada, int paginaEsperada, int tamanoEsperado)
        {
            // Arrange
            _repositorioMock
                .Setup(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado))
                .Returns((new List<Producto>(), 0));

            // Act
            _service.ObtenerPaginado(paginaEntrada, tamanoEntrada);

            // Assert
            _repositorioMock.Verify(r => r.ObtenerPaginado(paginaEsperada, tamanoEsperado), Times.Once);
        }
    }
}